using ABI.System;
using FluentCore.Event.Process;
using FluentCore.Extend.Service.Component.Authenticator;
using FluentCore.Model;
using FluentCore.Model.Auth;
using FluentCore.Model.Auth.Microsoft;
using FluentCore.Model.Auth.Yggdrasil;
using FluentCore.Model.Launch;
using FluentCore.Service.Component.Authenticator;
using FluentCore.Service.Component.DependencesResolver;
using FluentCore.Service.Component.Installer;
using FluentCore.Service.Component.Installer.ForgeInstaller;
using FluentCore.Service.Component.Launch;
using FluentCore.Service.Local;
using FluentCore.Service.Network;
using FluentCore.Service.Network.Api;
using FluentCore.Wrapper;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Windows.Foundation.Collections;

namespace FluentLauncher.DesktopBridge
{
    public class MethodHandler
    {
        public static MinecraftLauncher Launcher { get; set; }

        public static LanguageResource LanguageResource { get; set; }

        public static void NavigateWebUrl(string url)
        {
            using var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    FileName = "cmd"
                },
            };
            process.Start();
            process.StandardInput.WriteLine($"start \"\" \"{url}\"");
            process.StandardInput.WriteLine("exit");
            process.WaitForExit();
        }

        public static string GetMinecraftCoreList(string folder)
        {
            var locator = new CoreLocator(folder);

            var list = locator.GetAllGameCores().Select(x =>
            {
                if (x.MainClass == "net.minecraft.client.main.Main")
                    return new MinecraftCoreInfo() { Id = x.Id, Tag = $"{x.Type},{x.Id}" };
                else return new MinecraftCoreInfo() { Id = x.Id, Tag = $"{x.Type},modded,{x.Id}" };
            });

            return JsonConvert.SerializeObject(list);
        }

        public static void SetLanguage(string language) => LanguageResource = new LanguageResource(language);   

        public static void GetMicrosoftAuthenticatorResult(ref ValueSet values, string code)
        {
            var authenticator = new MicrosoftAuthenticator(code, Program.Client_id, Program.Redirect_uri);

            static async void Authenticator_SingleStepBeginning(object sender, string e)
            {
                var res = new ValueSet
                {
                    { "Header", "MicrosoftAuthenticateProcessing" },
                    { "Message", e }
                };

                await Program.Connection.SendMessageAsync(res);
            }
            authenticator.SingleStepBeginning += Authenticator_SingleStepBeginning;

            var result = authenticator.Authenticate();
            authenticator.SingleStepBeginning -= Authenticator_SingleStepBeginning;

            switch (result.Item2)
            {
                case AuthResponseType.Succeeded:
                    var model = (MicrosoftAuthenticationResponse)result.Item1;
                    values.Add("Response", "Succeeded");
                    values.Add("Id", model.Id);
                    values.Add("Name", model.Name);
                    values.Add("AccessToken", model.AccessToken);
                    values.Add("RefreshToken", model.RefreshToken);
                    values.Add("ExpiresIn", 86400);
                    values.Add("Time", model.Time);
                    break;
                case AuthResponseType.Failed:
                    values.Add("Response", "Failed");
                    break;
                default:
                    break;
            }
        }

        public static void GetRefreshMicrosoftAuthenticatorResult(ref ValueSet values, string refreshtoken)
        {
            var authenticator = new MicrosoftAuthenticator(string.Empty, Program.Client_id, Program.Redirect_uri);
            var result = authenticator.Refresh(refreshtoken);

            switch (result.Item2)
            {
                case AuthResponseType.Succeeded:
                    var model = (MicrosoftAuthenticationResponse)result.Item1;
                    values.Add("Response", "Succeeded");
                    values.Add("Id", model.Id);
                    values.Add("Name", model.Name);
                    values.Add("AccessToken", model.AccessToken);
                    values.Add("RefreshToken", model.RefreshToken);
                    values.Add("ExpiresIn", 86400);
                    values.Add("Time", model.Time);
                    break;
                case AuthResponseType.Failed:
                    values.Add("Response", "Failed");
                    break;
                default:
                    break;
            }
        }

        public static void GetOfflineAuthenticatorResult(ref ValueSet values, string name)
        {
            using var authenticator = new OfflineAuthenticator(name);
            var result = authenticator.Authenticate();
            var model = (YggdrasilResponseModel)result.Item1;

            values.Add("Response", "Succeeded");
            values.Add("Id", model.SelectedProfile.Id);
            values.Add("Name", model.SelectedProfile.Name);
            values.Add("AccessToken", model.AccessToken);
        }

        public static string GetVersionManifest() => JsonConvert.SerializeObject(SystemConfiguration.Api.GetVersionManifest().GetAwaiter().GetResult());

        public static string GetRequiredJavaVersion(string path, string version)
        {
            var locator = new CoreLocator(path);
            var model = locator.GetGameCoreFromId(version);

            return model.JavaVersion.MajorVersion.ToString();
        }

        public static void DeleteMinecraftCore(string folder, string mcVersionId)
        {
            var directory = new DirectoryInfo(Path.Combine(PathHelper.GetVersionsFolder(folder), mcVersionId));
            FileHelper.DeleteAllFiles(directory);
            directory.Delete();
        }

        public static void RenameMinecraftCore(string folder, string mcVersionId, string newId)
        {
            var oldDirectory = new DirectoryInfo(Path.Combine(PathHelper.GetVersionsFolder(folder), mcVersionId));

            var json = new FileInfo(Path.Combine(PathHelper.GetVersionsFolder(folder), mcVersionId, $"{mcVersionId}.json"));
            var jar = new FileInfo(Path.Combine(PathHelper.GetVersionsFolder(folder), mcVersionId, $"{mcVersionId}.jar"));

            var newDirectory = new DirectoryInfo(Path.Combine(PathHelper.GetVersionsFolder(folder), newId));
            if (!newDirectory.Exists)
                newDirectory.Create();

            var stream = json.OpenRead();
            var reader = new StreamReader(stream);

            var jobject = JObject.Parse(reader.ReadToEnd());

            jobject.Remove("id");
            jobject.Add("id", newId);

            File.WriteAllText(Path.Combine(PathHelper.GetVersionsFolder(folder), newId, $"{newId}.json"), jobject.ToString(Formatting.Indented));

            reader.Close();
            reader.Dispose();
            stream.Close();
            stream.Dispose();

            json.Delete();

            if (jar.Exists)
            {
                jar.MoveTo(Path.Combine(PathHelper.GetVersionsFolder(folder), newId, $"{newId}.jar"));
                jar.Delete();
            }

            FileHelper.DeleteAllFiles(oldDirectory);
            oldDirectory.Delete();
        }

        public static void LaunchMinecraft(LaunchModel model)
        {
            void SendDetails(string title,string message)
            {
                var res = new ValueSet() { { "Header", "MinecraftLauncherDetails" }, { "Message", title }, { "Response", message } };
                _ = Program.Connection.SendMessageAsync(res);
            }

            #region Launch Arrangements

            SendDetails("Info", LanguageResource.Launching_Info_1);

            var locator = new CoreLocator(model.GameFolder);
            var config = new LaunchConfig
            {
                AuthDataModel = new AuthDataModel
                {
                    AccessToken = model.AccessToken,
                    UserName = model.UserName,
                    Uuid = Guid.Parse(model.Uuid)
                },
                JavaPath = model.JavaPath,
                MaximumMemory = model.MaximumMemory,
                MinimumMemory = model.MinimumMemory
            };
            Launcher = new MinecraftLauncher(locator, config);

            #endregion

            #region DependencesCompleter

            SendDetails("Info", LanguageResource.Launching_Info_2);
            var completer = new DependencesCompleter(locator.GetGameCoreFromId(model.Id));

            int done = 0;
            void SingleDownloadedEvent(object sender, HttpDownloadResponse e)
            {
                done += 1;

                if (e.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    SendDetails("DownloadInfo", string.Format(LanguageResource.Launching_DownloadInfo_1, e.FileInfo.Name, done, completer.NeedDownloadDependencesCount));
            }

            completer.SingleDownloadedEvent += SingleDownloadedEvent;
            completer.CompleteAsync().Wait();

            if (completer.ErrorDownloadResponses.Count > 0)
                SendDetails("DownloadInfo", LanguageResource.Launching_DownloadInfo_2);
            else SendDetails("DownloadInfo", string.Empty);

            #endregion

            #region Launch Process
            SendDetails("Info", LanguageResource.Launching_Info_3);

            Launcher.Launch(model.Id);
            Task.Run(async () =>
            {
                try { Launcher.ProcessContainer.Process.WaitForInputIdle(); }
                catch { }

                SendDetails("Process", $"[{Launcher.ProcessContainer.Process.Id}] {model.JavaPath}");
                SendDetails("Event", "WaitForInputIdle");

                await Task.Delay(1000);

                var result = await Launcher.WaitForResult();
                SendDetails("Event", "Exited");

                #region Dispose

                Launcher.ProcessContainer.OutputDataReceived -= OutputDataReceived;
                Launcher.ProcessContainer.ErrorDataReceived -= ErrorDataReceived;
                Launcher.ProcessContainer.Crashed -= Crashed;

                Launcher.Dispose();
                Launcher = null;

                GC.Collect();
                #endregion
            });

            void OutputDataReceived(object sender, DataReceivedEventArgs e) => SendDetails("OutputReceived", e.Data);
            void ErrorDataReceived(object sender, DataReceivedEventArgs e) => SendDetails("ErrorOutputReceived", e.Data);
            void Crashed(object sender, ProcessCrashedEventArgs e) => SendDetails("Event", "Crashed");

            Launcher.ProcessContainer.OutputDataReceived += OutputDataReceived;
            Launcher.ProcessContainer.ErrorDataReceived += ErrorDataReceived;
            Launcher.ProcessContainer.Crashed += Crashed;

            #endregion
        }

        public static string GetLaunchArguments(LaunchModel model)
        {
            var javaw = new FileInfo(model.JavaPath);
            if (!string.IsNullOrWhiteSpace(FileHelper.FindFile(javaw.Directory, "java.exe")))
                model.JavaPath = FileHelper.FindFile(javaw.Directory, "java.exe");

            var locator = new CoreLocator(model.GameFolder);
            var config = new LaunchConfig
            {
                AuthDataModel = new AuthDataModel
                {
                    AccessToken = model.AccessToken,
                    UserName = model.UserName,
                    Uuid = Guid.Parse(model.Uuid)
                },
                JavaPath = model.JavaPath,
                MaximumMemory = model.MaximumMemory,
                MinimumMemory = model.MinimumMemory
            };

            if (string.IsNullOrEmpty(config.NativesFolder))
                config.NativesFolder = $"{PathHelper.GetVersionFolder(locator.Root, model.Id)}{PathHelper.X}natives";

            var builder = new ArgumentsBuilder(locator.GetGameCoreFromId(model.Id), config);
            return builder.BulidArguments(true);
        }

        public static void NavigateFolder(string folder)
        {
            var process = new ProcessContainer(new ProcessStartInfo
            {
                FileName = "Explorer.exe",
                Arguments = $"\"{folder}\""
            });

            process.Start();

            Task.Run(async () =>
            {
                await process.Process.WaitForExitAsync();
                process.Dispose();
            });
        }

        public static void SetDownloadOptitions(int threads, string source)
        {
            DependencesCompleter.MaxThread = threads;

            switch (source)
            {
                case "Official Source":
                    SystemConfiguration.Api = new Mojang();
                    break;
                case "BmclApi Source":
                    SystemConfiguration.Api = new Bmclapi();
                    break;
                case "Mcbbs Source":
                    SystemConfiguration.Api = new Mcbbs();
                    break;
                default:
                    break;
            }
        }

        public static string GetJavaRuntimeEnvironmentInfo(string path)
        {
            var process = new ProcessContainer
                (new ProcessStartInfo()
                {
                    FileName = path,
                    Arguments = $"-jar \"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Libraries", "JavaDetails.jar")}\""
                });

            process.Start();
            process.Process.WaitForExit();

            string value = process.OutputData.Count > 0 ? process.OutputData[0] : string.Empty;
            process.Dispose();

            return value;
        }

        public static bool InstallMinecraft(InstallInfomation installInfomation)
        {
            void SendProgress(string message, double progress)
            {
                var res = new ValueSet() { { "Header", "InstallMinecraftProgress" }, { "Message", message }, { "Progress", progress } };
                _ = Program.Connection.SendMessageAsync(res);
            }

            var modLoader = JsonConvert.DeserializeObject<AbstractModLoader>(installInfomation.ModLoader);
            var locator = new CoreLocator(installInfomation.Folder);
            var downloadPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            #region InstallVanllia

            SendProgress("InstallVanllia", 10);
            var installer = new VanlliaInstaller(locator);
            var vanlliaInstallResult = installer.Install(installInfomation.McVersion);

            if (modLoader == null)
            {
                SendProgress("InstallVanllia", 100);
                return vanlliaInstallResult;
            }

            #endregion

            #region InstallModLoader

            if (!vanlliaInstallResult)
                return false;

            SendProgress("InstallVanllia", 25);

            switch (modLoader.Type)
            {
                case "Forge":
                    var forgeBuild = JsonConvert.DeserializeObject<ForgeBuild>(JsonConvert.SerializeObject(modLoader.Build));
                    InstallerBase forgeInstaller;

                    var res = HttpHelper.HttpDownloadAsync($"{new Bmclapi().Url}/forge/download/{forgeBuild.Build}", downloadPath, "ForgeInstaller.jar").GetAwaiter().GetResult();
                    SendProgress("DownloadJar", 50);
                    if (res.HttpStatusCode != System.Net.HttpStatusCode.OK)
                        return false;

                    if (Convert.ToInt32(modLoader.McVersion.Split('.')[1]) < 13)
                    {
                        forgeInstaller = new LegacyForgeInstaller(locator, res.FileInfo.FullName);
                        ((LegacyForgeInstaller)forgeInstaller).Install();
                    }
                    else
                    {
                        forgeInstaller = new ModernForgeInstaller(locator, installInfomation.McVersion, installInfomation.McVersion, installInfomation.JavaPath, res.FileInfo.FullName);
                        ((ModernForgeInstaller)forgeInstaller).Install();
                    }
                    SendProgress("InstallForge", 80);

                    res.FileInfo.Delete();
                    SendProgress("InstallForge", 100);
                    return true;
                case "Fabric":
                    break;
                case "OptiFine":
                    var optiFineBuild = JsonConvert.DeserializeObject<OptiFineBuild>(JsonConvert.SerializeObject(modLoader.Build));
                    OptiFineInstaller optifineInstaller;

                    res = HttpHelper.HttpDownloadAsync($"{new Bmclapi().Url}/optifine/{modLoader.McVersion}/{optiFineBuild.Type}/{optiFineBuild.Patch}", downloadPath, optiFineBuild.FileName).GetAwaiter().GetResult();
                    SendProgress("DownloadJar", 50);
                    if (res.HttpStatusCode != System.Net.HttpStatusCode.OK)
                        return false;

                    optifineInstaller = new OptiFineInstaller(locator, modLoader.McVersion, modLoader.McVersion, installInfomation.JavaPath, res.FileInfo.FullName);
                    optifineInstaller.Install();

                    SendProgress("InstallForge", 80);

                    res.FileInfo.Delete();
                    SendProgress("InstallForge", 100);
                    return true;
                default:
                    break;
            }
            #endregion

            return false;
        }

        public static void InstallJavaRuntime(ref ValueSet values)
        {
            void Send(string title, string message)
            {
                var res = new ValueSet() { { "Header", "InstallJavaProgress" }, { "Response", title }, { "Message", message } };
                _ = Program.Connection.SendMessageAsync(res);
            }

            var timer = new Timer(1000);

            FileInfo fileInfo = default;

            string downloadfolder = Path.Combine(Program.StorageFolder, "Downloads");
            string runtimefolder = Path.Combine(Program.StorageFolder, "Runtimes");
            string url = "https://d6.injdk.cn/openjdk/openjdk/17/openjdk-17.0.1_windows-x64_bin.zip";

            bool result = true;
            string resultPath = string.Empty;

            try
            {
                #region Download
                Send("Progress_Title", LanguageResource.InstallJava_Downloding);
                Send("Progress_SubTitle", "https://aka.ms/download-jdk/microsoft-jdk-17.0.2.8.1-windows-x64.zip");

                using var responseMessage = HttpHelper.HttpGetAsync(url, default, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();

                if (!responseMessage.IsSuccessStatusCode)
                    result = false;

                if (responseMessage.Content.Headers != null && responseMessage.Content.Headers.ContentDisposition != null)
                    fileInfo = new FileInfo(Path.Combine(downloadfolder, responseMessage.Content.Headers.ContentDisposition.FileName.Trim('\"')));
                else fileInfo = new FileInfo(Path.Combine(downloadfolder, Path.GetFileName(responseMessage.RequestMessage.RequestUri.AbsoluteUri)));

                if (!Directory.Exists(downloadfolder))
                    Directory.CreateDirectory(downloadfolder);

                using var fileStream = new FileStream(fileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                using var stream = responseMessage.Content.ReadAsStreamAsync().GetAwaiter().GetResult();

                timer.Elapsed += delegate
                {
                    Send("Progress_Text", $"{fileStream.Length.LengthToMb()}/{((long)responseMessage.Content.Headers.ContentLength).LengthToMb()}");
                    Send("ProgressBar_Value", ((double)fileStream.Length / ((long)responseMessage.Content.Headers.ContentLength) * 100.0).ToString());
                };
                timer.Start();

                int bufferSize = 1024 * 1024;
                byte[] bytes = new byte[bufferSize];
                int read = stream.Read(bytes, 0, bufferSize);

                while (read > 0)
                {
                    fileStream.Write(bytes, 0, read);
                    read = stream.Read(bytes, 0, bufferSize);
                }

                fileStream.Flush();
                fileStream.Close();
                stream.Close();

                GC.Collect();

                timer.Stop();
                timer.Dispose();
                #endregion

                #region Extract Zip

                if (!Directory.Exists(runtimefolder))
                    Directory.CreateDirectory(runtimefolder);

                FileHelper.DeleteAllFiles(new DirectoryInfo(runtimefolder));

                Send("Progress_Title", LanguageResource.InstallJava_Extracting);
                Send("Progress_SubTitle", $"{fileInfo.FullName}");

                ZipFile.ExtractToDirectory(fileInfo.FullName, runtimefolder, true);

                resultPath = FileHelper.FindFile(new DirectoryInfo(runtimefolder), "javaw.exe");
                #endregion

                #region Dispose
                fileInfo.Delete();
                #endregion

                result = true;
            }
            catch (HttpRequestException e)
            {
                #region Error Dispose

                GC.Collect();

                if (timer.Enabled)
                    timer.Stop();

                timer.Dispose();
                #endregion

                result = false;
            }

            values.Add("Response", resultPath);
            values.Add("Message", result.ToString());
        }

        public static void SearchJavaRuntime(ref ValueSet values)
        {
            var paths = new List<string>();

            #region Cmd

            var process = new ProcessContainer
                (new ProcessStartInfo()
                {
                    FileName = "cmd",
                    CreateNoWindow = true
                });

            process.Start();
            process.Process.StandardInput.WriteLine("where javaw");
            process.Process.StandardInput.WriteLine("exit");
            process.Process.WaitForExit();

            for(int i = 0; i < process.OutputData.Count; i++)
                if (process.OutputData[i].Contains(">"))
                    process.OutputData.Remove(process.OutputData[i]);

            if (process.OutputData.Count > 0)
                process.OutputData.Skip(2).ToList().ForEach(x => paths.Add(x));
            process.Dispose();

            #endregion

            #region Regedit

            var javaHomePaths = new List<string>();

            List<string> ForRegistryKey(RegistryKey registryKey,string keyName)
            {
                var result = new List<string>();

                foreach (string valueName in registryKey.GetValueNames())
                    if (valueName == keyName)
                        result.Add((string)registryKey.GetValue(valueName));

                foreach (string registrySubKey in registryKey.GetSubKeyNames())
                    ForRegistryKey(registryKey.OpenSubKey(registrySubKey), keyName).ForEach(x => result.Add(x));

                return result;
            };

            using var reg = Registry.LocalMachine.OpenSubKey("SOFTWARE");

            if (reg.GetSubKeyNames().Contains("JavaSoft"))
            {
                using var registryKey = reg.OpenSubKey("JavaSoft");
                ForRegistryKey(registryKey, "JavaHome").ForEach(x => javaHomePaths.Add(x));
            }

            if (reg.GetSubKeyNames().Contains("WOW6432Node"))
            {
                using var registryKey = reg.OpenSubKey("WOW6432Node");
                if (registryKey.GetSubKeyNames().Contains("JavaSoft"))
                {
                    using var registrySubKey = reg.OpenSubKey("JavaSoft");
                    ForRegistryKey(registrySubKey, "JavaHome").ForEach(x => javaHomePaths.Add(x));
                }
            }

            javaHomePaths.ForEach(x =>
            {
                if (Directory.Exists(x))
                {
                    var directory = new DirectoryInfo(x);

                    paths.Add(FileHelper.FindFile(directory, "javaw.exe"));
                }
            });

            #endregion

            #region Special Folders

            var folders = new List<string>()
            {
                Path.Combine(Environment.GetEnvironmentVariable("APPDATA"),".minecraft\\cache\\java"),
                Path.Combine(Program.StorageFolder,"Runtimes"),
                Environment.GetEnvironmentVariable("JAVA_HOME"),
                "C:\\Program Files\\Java"
            };

            folders.ForEach(x =>
            {
                if (Directory.Exists(x))
                {
                    var directory = new DirectoryInfo(x);

                    paths.Add(FileHelper.FindFile(directory, "javaw.exe"));
                }
            });

            #endregion

            var results = paths.Distinct().Select(path =>
            {
                var model = JsonConvert.DeserializeObject<JreInfo>(GetJavaRuntimeEnvironmentInfo(path));
                return new JavaRuntimeEnvironment
                {
                    Path = path,
                    Title = $"{model.JAVA_VM_NAME} {model.JAVA_VERSION}"
                };
            });

            values.Add("Response", JsonConvert.SerializeObject(results));
        }
    }
}
