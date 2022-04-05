using FluentCore.Service.Network;
using FluentLauncher.Classes;
using FluentLauncher.Models;
using FluentLauncher.Pages;
using FluentLauncher.Strings;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace FluentLauncher
{
    public static class ShareResource
    {
        public static MainContainer MainContainer { get; set; }

        public static SettingPage SettingPage { get; set; }

        public static InfoBar InfoBar { get; set; }

        public static MojangNews News { get; set; }

        public static BackgroundLanguageResource LanguageResource { get; set; }

        public static VersionManifestModel VersionManifest { get; set; }

        public static List<MinecraftCoreInfo> MinecraftCores { get; set; }

        public static MicrosoftAuthenticationResponse AuthenticationResponse { get; set; }

        public static ObservableCollection<ProcessOutput> ProcessOutputs { get; set; } = new ObservableCollection<ProcessOutput>();

        public static List<ProcessOutput> ErrorProcessOutputs { set; get; } = new List<ProcessOutput>();

        public static bool NavigateToLaunch { get; set; }

        public static bool WebBrowserNavigateBack { get; set; }

        public static string WebBrowserLoginCode { get; set; }

        public static string Version => string.Format($"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}");

        public static JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public static MinecraftProcessContainer MinecraftProcess { get; set; } = new MinecraftProcessContainer();

        public static List<string> DownloadSources
            => new List<string>() { "Official Source", "BmclApi Source", "Mcbbs Source" };

        public static List<string> WorkingFolders
            => new List<string>() { "Standard [.minecraft/]", "Independent [.minecraft/versions/{version}/]" };

        public static List<string> AccountTypes = new List<string>() { "MicrosoftAccount", "OfflineAccount", "Authlib-injector Account" };

        public static List<string> Languages => new List<string>() { "中文", "English" };

        public static Task DownloadNews { get; set; }

        public static Task InitializeNewsPicture { get; set; }

        public static Task FirstNewsInitialized { get; set; }

        public static Task DownloadVersionManifest { get; set; }

        public static Task RunForFirstTimeTask { get; set; }

        #region Setttings
        private static List<MinecraftFolder> _minecraftFolders;
        public static List<MinecraftFolder> MinecraftFolders
        { get => _minecraftFolders; set { _minecraftFolders = value; App.Settings.Values["MinecraftFolders"] = JsonConvert.SerializeObject(value, JsonSerializerSettings); } }

        private static MinecraftFolder _selectedFolder;
        public static MinecraftFolder SelectedFolder
        { get => _selectedFolder; set { _selectedFolder = value; App.Settings.Values["SelectedFolder"] = JsonConvert.SerializeObject(value, JsonSerializerSettings); } }

        private static List<JavaRuntimeEnvironment> _javaRuntimeEnvironments;
        public static List<JavaRuntimeEnvironment> JavaRuntimeEnvironments
        { get => _javaRuntimeEnvironments; set { _javaRuntimeEnvironments = value; App.Settings.Values["JavaRuntimeEnvironments"] = JsonConvert.SerializeObject(value, JsonSerializerSettings); } }

        private static JavaRuntimeEnvironment _selectedJava;
        public static JavaRuntimeEnvironment SelectedJava
        { get => _selectedJava; set { _selectedJava = value; App.Settings.Values["SelectedJava"] = JsonConvert.SerializeObject(value, JsonSerializerSettings); } }

        private static MinecraftCoreInfo _selectedCore;
        public static MinecraftCoreInfo SelectedCore
        { get => _selectedCore; set { _selectedCore = value; App.Settings.Values["SelectedCore"] = JsonConvert.SerializeObject(value, JsonSerializerSettings); } }

        private static List<ThemeModel> _themes;
        public static List<ThemeModel> Themes
        { get => _themes; set { _themes = value; App.Settings.Values["Themes"] = JsonConvert.SerializeObject(value, JsonSerializerSettings); } }

        private static ThemeModel _selectedTheme;
        public static ThemeModel SelectedTheme
        { get => _selectedTheme; set { _selectedTheme = value; App.Settings.Values["SelectedTheme"] = JsonConvert.SerializeObject(value, JsonSerializerSettings); } }

        private static int _maxMemory;
        public static int MaxMemory
        { get => _maxMemory; set { _maxMemory = value; App.Settings.Values["MaxMemory"] = value; } }

        private static int _minMemory;
        public static int MinMemory
        { get => _minMemory; set { _minMemory = value; App.Settings.Values["MinMemory"] = value; } }

        private static int _maxThreads;
        public static int MaxThreads
        { get => _maxThreads; set { _maxThreads = value; App.Settings.Values["MaxThreads"] = value; } }

        private static string _workingFolder;
        public static string WorkingFolder
        { get => _workingFolder; set { _workingFolder = value; App.Settings.Values["WorkingFolder"] = value; } }

        private static string _downloadSource;
        public static string DownloadSource
        { get => _downloadSource; set { _downloadSource = value; App.Settings.Values["DownloadSource"] = value; } }

        private static string _language;
        public static string Language
        { get => _language; set { _language = value; App.Settings.Values["Language"] = value; } }

        private static string _shownUpdate;
        public static string ShownUpdate
        { get => _shownUpdate; set { _shownUpdate = value; App.Settings.Values["ShownUpdate"] = value; } }

        private static bool _runForFirstTime;
        public static bool RunForFirstTime
        { get => _runForFirstTime; set { _runForFirstTime = value; App.Settings.Values["RunForFirstTime"] = value; } }

        private static List<MinecraftAccount> _minecraftAccounts;
        public static List<MinecraftAccount> MinecraftAccounts
        { get => _minecraftAccounts; set { _minecraftAccounts = value; App.Settings.Values["MinecraftAccounts"] = JsonConvert.SerializeObject(value, JsonSerializerSettings); } }

        private static MinecraftAccount _selectedAccount;
        public static MinecraftAccount SelectedAccount
        { get => _selectedAccount; set { _selectedAccount = value; App.Settings.Values["SelectedAccount"] = JsonConvert.SerializeObject(value, JsonSerializerSettings); } }

        private static bool _mainPageNewsVisibility;
        public static bool MainPageNewsVisibility
        { get => _mainPageNewsVisibility; set { _mainPageNewsVisibility = value; App.Settings.Values["MainPageNewsVisibility"] = value; } }

        #endregion

        public static async Task ShowInfoAsync(string title = "Title", string message = "", int time = 3000, InfoBarSeverity severity = InfoBarSeverity.Informational)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async delegate
            {
                InfoBar.Title = title;
                InfoBar.Message = message;
                InfoBar.IsOpen = true;
                InfoBar.Severity = severity;
                await Task.Delay(time);
                InfoBar.IsOpen = false;
            });
        }

        public static async Task BeginInitializeNewsPicture()
        {
            FirstNewsInitialized = Task.Run(async delegate
            {
                await DownloadNews;

                double time = 0;
                while (News.Entries[0].BitmapImage == null)
                {
                    await Task.Delay(100);
                    time += 0.1;

                    if (time > 15)
                        break;
                }
            });

            await DownloadNews;
            News.Entries = News.Entries.Take(15).ToList();

            var manyBlock = new TransformManyBlock<List<MojangNewsItem>, MojangNewsItem>(x => x);
            var blockOptions = new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 15,
                MaxDegreeOfParallelism = 15
            };
            var actionBlock = new ActionBlock<MojangNewsItem>(async x =>
            {
                using var res = await HttpHelper.HttpGetAsync(x.PlayPageImage.Url);
                var stream = await res.Content.ReadAsStreamAsync();

                await CoreApplication.MainView.Dispatcher.RunAsync(default, async delegate
                {
                    var image = new BitmapImage();
                    await image.SetSourceAsync(stream.AsRandomAccessStream());
                    x.BitmapImage = image;
                });

                stream.Dispose();
            }, blockOptions);

            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            var dis = manyBlock.LinkTo(actionBlock, linkOptions);
            _ = manyBlock.Post(News.Entries);
            manyBlock.Complete();

            await actionBlock.Completion;
            dis.Dispose();
        }

        public static async Task BeginDownloadVersionManifest()
        {
            var res = await App.DesktopBridge.SendAsync<StandardResponseModel>(new StandardRequestModel()
            {
                Header = "GetVersionManifest"
            });

            VersionManifest = JsonConvert.DeserializeObject<VersionManifestModel>(res.Response);
        }

        public static async Task UpdateMinecraftCoresAsync()
        {
            await App.DesktopBridge_Init;
            if (SelectedFolder != null)
            {
                var res = await App.DesktopBridge.SendAsync<StandardResponseModel>
                    (new GetCoreListRequest() { Folder = SelectedFolder.Path });

                MinecraftCores = JsonConvert.DeserializeObject<List<MinecraftCoreInfo>>(res.Response);
                if (!MinecraftCores.Contains(SelectedCore))
                    SelectedCore = null;
            }
            else
            {
                MinecraftCores = null;
                SelectedCore = null;
            }
        }

        #region Installer

        public static List<string> ForgeSupportMcVersionList { get; set; }

        public static List<string> OptiFineSupportMcVersionList { get; set; }

        #region Forge
        public static async Task<ForgeBuild> GetLatestForgeVersion(string mcVersion)
        {
            using var res = await HttpHelper.HttpGetAsync("https://bmclapi2.bangbang93.com/forge/promos");

            var list = JsonConvert.DeserializeObject<List<RromosForgeItem>>(await res.Content.ReadAsStringAsync());

            foreach (var item in list)
                if (item.Name == $"{mcVersion}-latest")
                    return item.Build;

            return null;
        }

        public static async Task<List<string>> GetForgeSupportMcVersionList()
        {
            using var res = await HttpHelper.HttpGetAsync("https://bmclapi2.bangbang93.com/forge/minecraft");

            return JsonConvert.DeserializeObject<List<string>>(await res.Content.ReadAsStringAsync());
        }
        #endregion

        #region OptiFine
        public static async Task<OptiFineBuild> GetLatestOptiFineVersion(string mcVersion)
        {
            using var res = await HttpHelper.HttpGetAsync($"https://bmclapi2.bangbang93.com/optifine/{mcVersion}");

            var list = JsonConvert.DeserializeObject<List<OptiFineBuild>>(await res.Content.ReadAsStringAsync());

            var nList = new List<OptiFineBuild>();
            list.ForEach(item =>
            {
                if (!item.Patch.Contains("pre"))
                    nList.Add(item);
            });

            char maxChar = (char)0;
            foreach (var item in nList)
            {
                char c = Convert.ToChar(item.Patch.Substring(0, 1));
                if (c > maxChar)
                    maxChar = c;
            }

            var vList = list.Select(x =>
            {
                if (x.Patch.Contains(maxChar.ToString()))
                    return x;

                return null;
            });
            int index = 0;
            foreach (var item in vList)
            {
                if (item == null)
                    continue;

                int i = Convert.ToInt32(item.Patch.Substring(1, 1));
                if (i > index)
                    index = i;
            }

            foreach (var item in nList)
                if (item.Patch == $"{maxChar}{index}")
                    return item;

            return null;
        }

        public static List<string> GetOptiFineSupportMcVersionList() => new List<string>
        {
            "1.7.2", "1.7.10", "1.8.0", "1.8.8", "1.8.9", "1.9.0", "1.9.2", "1.9.4", "1.10", "1.10.2",
            "1.11", "1.11.2", "1.12", "1.12.1", "1.12.2", "1.13", "1.13.1", "1.13.2", "1.14.2", "1.14.3",
            "1.14.4", "1.15.2", "1.16.1", "1.16.2", "1.16.3", "1.16.4", "1.16.5", "1.17.1", "1.18", "1.18.1"
        };
        #endregion

        #endregion

        public static bool GetBasicSettingsProblem()
        {
            if (MinecraftFolders.Count == 0)
                return true;

            if (SelectedFolder == null)
                return true;

            if (JavaRuntimeEnvironments.Count == 0)
                return true;

            if (SelectedJava == null)
                return true;

            return false;
        }

        public static bool GetAccountProblem()
        {
            if (MinecraftAccounts.Count == 0)
                return true;

            if (SelectedAccount == null)
                return true;

            return false;
        }

        public static Task SetDownloadSource() => App.DesktopBridge.SendAsync<StandardResponseModel>(new SetDownloadOptitionsRequest());

        public static async Task<bool> SetSuitableJavaRuntime()
        {
            if (SelectedJava == null)
                return false;

            var res = await App.DesktopBridge.SendAsync<StandardResponseModel>(new GetRequiredJavaVersionRequest());
            int major = Convert.ToInt32(res.Response);

            int current = await SelectedJava.GetJavaVersionAsync();
            if (current < major)
            {
                bool switched = false;
                foreach (var item in JavaRuntimeEnvironments)
                {
                    var version = await item.GetJavaVersionAsync();
                    if (version >= major)
                    {
                        SelectedJava = item;
                        _ = ShowInfoAsync(LanguageResource.Background_SetSuitableJavaRuntime_True.Replace("{major}", major.ToString()));
                        switched = true;
                    }
                }

                return switched;
            }
            else return true;
        }
    }
}
