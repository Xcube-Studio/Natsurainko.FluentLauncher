using FluentCore.Extend.Service.Component.Authenticator;
using FluentCore.Model;
using FluentCore.Model.Auth;
using FluentCore.Model.Auth.Microsoft;
using FluentCore.Model.Auth.Yggdrasil;
using FluentCore.Model.Launch;
using FluentCore.Service.Component.Authenticator;
using FluentCore.Service.Component.DependencesResolver;
using FluentCore.Service.Component.Launch;
using FluentCore.Service.Local;
using FluentCore.Service.Network;
using FluentCore.Wrapper;
using System;
using System.Diagnostics;
using System.IO;

namespace FluentCore.Console
{
    class Program
    {
        static string client_id = "0844e754-1d2e-4861-8e2b-18059609badb";
        static string redirect_uri = "http://localhost:5001/fluentlauncher/auth-response";

        static void Main(string[] args)
        {
            #region Forge Installer
            /*
            var forgeInstaller = new ModernForgeInstaller
            (new CoreLocator(System.Console.ReadLine()), "1.17.1", "1.17.1", System.Console.ReadLine(), System.Console.ReadLine());

            var result = forgeInstaller.InstallAsync().GetAwaiter().GetResult();
            System.Console.ReadLine();
            return;
            */
            #endregion

            #region Vanllia Installer

            //var install = new VanlliaInstaller(new CoreLocator(System.Console.ReadLine()));
            //string version = System.Console.ReadLine();
            //install.InstallAsync(version).Wait();

            //System.Console.WriteLine($"Install Version:{version} Successfully");
            //System.Console.ReadLine();

            //return;
            #endregion

            HttpHelper.SetTimeout(60000);

            System.Console.Write("Minecraft Path:");
            CoreLocator coreLocator = new CoreLocator(System.Console.ReadLine());
            System.Console.Write("Java Path:");
            string javaPath = System.Console.ReadLine();
            System.Console.Write("Version:");
            string id = System.Console.ReadLine();
            GameCore core = coreLocator.GetGameCoreFromId(id);

            #region Authlib-Injector

            /*
            System.Console.WriteLine("Loading AuthlibInjector...");
            var injector = new AuthlibInjector("https://skin.greenspray.cn/api/yggdrasil", "C:\\Users\\Admin\\AppData\\Roaming\\.hmcl\\authlib-injector.jar");
            var vs = await injector.GetArgumentsAsync();
            */

            #endregion

            System.Console.WriteLine("Loading Authenticator...");

            #region YggdrasilAuthenticator
            /*
            
            System.Console.Write("Email:");
            string email = System.Console.ReadLine();
            System.Console.Write("Password:");
            string password = System.Console.ReadLine();

            using var auth = new YggdrasilAuthenticator(email, password);
            var res = (YggdrasilResponseModel)auth.AuthenticateAsync().GetAwaiter().GetResult().Item1;

            */
            #endregion

            var auth = new MicrosoftAuthenticator(default, client_id, redirect_uri);
            auth.Code = auth.GetAuthorizationCodeFromBrower().GetAwaiter().GetResult();
            var res = (MicrosoftAuthenticationResponse)auth.Authenticate().Item1;

            System.Console.WriteLine("Loading DependencesCompleter...");

            var completer = new DependencesCompleter(core);
            completer.SingleDownloadedEvent += Completer_SingleDownloadedEvent;
            completer.CompleteAsync().Wait();

            LaunchConfig launchConfig = new LaunchConfig
            {
                MoreBehindArgs = string.Empty,
                MoreFrontArgs = string.Empty,
                JavaPath = javaPath,
                MaximumMemory = 2048,
                NativesFolder = $"{PathHelper.GetVersionFolder(coreLocator.Root, id)}{PathHelper.X}natives",
                AuthDataModel = new AuthDataModel
                {
                    AccessToken = res.AccessToken,
                    UserName = res.Name,
                    Uuid = Guid.Parse(res.Id)
                }
            };

            #region Authlib-Injector

            /*
            foreach (string value in vs)
                launchConfig.MoreFrontArgs += $" {value}";
            */

            #endregion

            var launcher = new MinecraftLauncher(coreLocator, launchConfig);
            launcher.Launch(id);
            System.Console.WriteLine($"[FluentCore.MinecraftLauncher]Process Start [{launcher.ProcessContainer.Process.Id}]");

            launcher.ProcessContainer.OutputDataReceived += ProcessContainer_OutputDataReceived;
            launcher.ProcessContainer.Exited += ProcessContainer_Exited;

            System.Console.WriteLine(launcher.ProcessContainer.Process.StartInfo.Arguments);
            System.Console.ReadLine();
        }

        private static void ProcessContainer_Exited(object sender, Event.Process.ProcessExitedEventArgs e)
        {
            System.Console.WriteLine($"[FluentCore.MinecraftLauncher]Process Exited [ExitCode:{e.ExitCode}][Running time:{e.RunTime}]");
        }

        private static void ProcessContainer_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            System.Console.WriteLine($"[FluentCore.MinecraftLauncher]{e.Data}");
        }

        private static void Completer_SingleDownloadedEvent(object sender, HttpDownloadResponse e)
        {
            System.Console.WriteLine($"[{e.HttpStatusCode}][{e.Message}][{e.FileInfo}]");
        }
    }
}
