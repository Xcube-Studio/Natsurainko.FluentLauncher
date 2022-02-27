using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel;
using Windows.Foundation.Collections;
using System.IO;
using Windows.Graphics.Display;
using Windows.Storage;

namespace FluentLauncher.DesktopBridge
{
    public class Program
    {
        public static AppServiceConnection Connection { get; set; }

        public static bool IsDisconnceted { get; set; } = false;

        public static string StorageFolder = ApplicationData.Current.LocalFolder.Path;

        public const string Client_id = "0844e754-1d2e-4861-8e2b-18059609badb";

        public const string Redirect_uri = "http://localhost:5001/fluentlauncher/auth-response";

        public static async Task Main(string[] args)
        {
            InitializeAppServiceConnection();

            while (!IsDisconnceted)
                await Task.Delay(1000);
        }

        private async static void InitializeAppServiceConnection()
        {
            Connection = new AppServiceConnection();
            Connection.AppServiceName = "FluentLauncher.DesktopBridge";
            Connection.PackageFamilyName = Package.Current.Id.FamilyName;
            Connection.RequestReceived += Connection_RequestReceived;
            Connection.ServiceClosed += Connection_ServiceClosed;
            MethodHandler.LanguageResource = new LanguageResource("English");

            var status = await Connection.OpenAsync();
            if (status == AppServiceConnectionStatus.Success)
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}][Application Started]");
        }

        private static void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args) => IsDisconnceted = true;

        private static async void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            string header = (string)args.Request.Message["Header"];
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}][Connection_RequestReceived][Method:{header}]");

            var response = new ValueSet
            {
                { "Header", header }
            };

            switch (header)
            {
                case "SetDownloadOptitions":
                    MethodHandler.SetDownloadOptitions(Convert.ToInt32(args.Request.Message["MaxThreads"]), (string)args.Request.Message["DownloadSource"]);
                    response.Add("Message", "Download Settings Has Updated");
                    break;
                case "SetLanguage":
                    MethodHandler.SetLanguage((string)args.Request.Message["Message"]);
                    response.Add("Message", "Language Settings Has Updated");
                    break;
                case "NavigateWebUrl":
                    MethodHandler.NavigateWebUrl((string)args.Request.Message["Url"]);
                    response.Add("Message", "Url Has Opened in the WebBrowser");
                    break; 
                case "GetMinecraftCoreList":
                    response.Add("Response", MethodHandler.GetMinecraftCoreList((string)args.Request.Message["Folder"]));
                    break;
                case "GetMicrosoftAuthenticatorResult":
                    MethodHandler.GetMicrosoftAuthenticatorResult(ref response, (string)args.Request.Message["Code"]);
                    break;
                case "GetRefreshMicrosoftAuthenticatorResult":
                    MethodHandler.GetRefreshMicrosoftAuthenticatorResult(ref response, (string)args.Request.Message["RefreshToken"]);
                    break;
                case "GetOfflineAuthenticatorResult":
                    MethodHandler.GetOfflineAuthenticatorResult(ref response, (string)args.Request.Message["Name"]);
                    break;
                case "GetVersionManifest":
                    response.Add("Response", MethodHandler.GetVersionManifest());
                    break;
                case "NavigateFolder":
                    MethodHandler.NavigateFolder((string)args.Request.Message["Message"]);
                    response.Add("Message", "Folder Has Opened");
                    break;
                case "LaunchMinecraft":
                    MethodHandler.LaunchMinecraft
                        (JsonConvert.DeserializeObject<LaunchModel>(JsonConvert.SerializeObject(args.Request.Message)));
                    response.Add("Message", "Launch Task Has Started");
                    break;
                case "GetLaunchArguments":
                    response.Add("Response", MethodHandler.GetLaunchArguments
                        (JsonConvert.DeserializeObject<LaunchModel>(JsonConvert.SerializeObject(args.Request.Message))));
                    break;
                case "DeleteMinecraftCore":
                    MethodHandler.DeleteMinecraftCore((string)args.Request.Message["Folder"], (string)args.Request.Message["Name"]);
                    response.Add("Message", $"{(string)args.Request.Message["Name"]} Has Deleted");
                    break;
                case "RenameMinecraftCore":
                    MethodHandler.RenameMinecraftCore((string)args.Request.Message["Folder"], (string)args.Request.Message["McVersionId"], (string)args.Request.Message["NewId"]);
                    response.Add("Message", $"{(string)args.Request.Message["McVersionId"]} Has Renamed");
                    break;
                case "GetJavaRuntimeEnvironmentInfo":
                    response.Add("Response", MethodHandler.GetJavaRuntimeEnvironmentInfo((string)args.Request.Message["Path"]));
                    break;
                case "InstallMinecraft":
                    var model = JsonConvert.DeserializeObject<InstallInfomation>(JsonConvert.SerializeObject(args.Request.Message));
                    response.Add("Message", MethodHandler.InstallMinecraft(model).ToString());
                    break;
                case "InstallJavaRuntime":
                    MethodHandler.InstallJavaRuntime(ref response);
                    break;
                case "SearchJavaRuntime":
                    MethodHandler.SearchJavaRuntime(ref response);
                    break; 
                case "GetRequiredJavaVersion":
                    response.Add("Response", MethodHandler.GetRequiredJavaVersion((string)args.Request.Message["Path"], (string)args.Request.Message["McVersion"]));
                    break;
                case "FileExist":
                    response.Add("Response", File.Exists((string)args.Request.Message["Message"]).ToString());
                    break;
                case "FolderExist":
                    response.Add("Response", Directory.Exists((string)args.Request.Message["Message"]).ToString());
                    break;
                default:
                    break;
            }

            await args.Request.SendResponseAsync(response);

            string debug = string.Empty;
            if (response.ContainsKey("Message"))
                debug = (string)response["Message"];
            if (response.ContainsKey("Response"))
                debug = (string)response["Response"];
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}][SendResponseAsync][Method:{header}][{debug}]");
        }
    }
}
