using Natsurainko.FluentLauncher.Class.AppData;
using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Class.ViewData;
using Natsurainko.FluentLauncher.Shared.Class.Model;
using Natsurainko.FluentLauncher.Shared.Mapping;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Language = Natsurainko.FluentLauncher.Class.ViewData.Language;

namespace Natsurainko.FluentLauncher.ViewModel.Pages.Settings;

public class SettingLaunchPageVM : ViewModelBase<Page>
{
    public SettingLaunchPageVM(Page control) : base(control)
    {
        GameFolders = new ObservableCollection<string>(ConfigurationManager.AppSettings.GameFolders);
        CurrentGameFolder = ConfigurationManager.AppSettings.CurrentGameFolder;

        JavaRuntimes = new ObservableCollection<string>(ConfigurationManager.AppSettings.JavaRuntimes);
        CurrentJavaRuntime = ConfigurationManager.AppSettings.CurrentJavaRuntime;
        JavaVirtualMachineMemory = ConfigurationManager.AppSettings.JavaVirtualMachineMemory ?? (int)AppSettings.Default.JavaVirtualMachineMemory;
        EnableAutoMemory = ConfigurationManager.AppSettings.EnableAutoMemory ?? (bool)AppSettings.Default.EnableAutoMemory;

        GameWindowTitle = ConfigurationManager.AppSettings.GameWindowTitle;
        GameServerAddress = ConfigurationManager.AppSettings.GameServerAddress;
        GameWindowHeight = ConfigurationManager.AppSettings.GameWindowHeight ?? (int)AppSettings.Default.GameWindowHeight;
        GameWindowWidth = ConfigurationManager.AppSettings.GameWindowWidth ?? (int)AppSettings.Default.GameWindowWidth;
        EnableFullScreen = ConfigurationManager.AppSettings.EnableFullScreen ?? (bool)AppSettings.Default.EnableFullScreen;
        EnableIndependencyCore = ConfigurationManager.AppSettings.EnableIndependencyCore ?? (bool)AppSettings.Default.EnableIndependencyCore;

        SupportedLanguages = GlobalResources.SupportedLanguages.CreateCollectionViewData<Language, LanguageViewData>();
        CurrentLanguage = ConfigurationManager.AppSettings.CurrentLanguage.CreateViewData<Language, LanguageViewData>() ?? AppSettings.Default.CurrentLanguage.CreateViewData<Language, LanguageViewData>();

        UpdataJavaInformation();
    }

    [Reactive]
    public ObservableCollection<string> GameFolders { get; set; }

    [Reactive]
    public string CurrentGameFolder { get; set; }

    [Reactive]
    public ObservableCollection<string> JavaRuntimes { get; set; }

    [Reactive]
    public string CurrentJavaRuntime { get; set; }

    [Reactive]
    public JavaInformation CurrentJavaInformation { get; set; }

    [Reactive]
    public int JavaVirtualMachineMemory { get; set; }

    [Reactive]
    public bool EnableAutoMemory { get; set; }

    [Reactive]
    public string GameWindowTitle { get; set; }

    [Reactive]
    public int GameWindowWidth { get; set; }

    [Reactive]
    public int GameWindowHeight { get; set; }

    [Reactive]
    public string GameServerAddress { get; set; }

    [Reactive]
    public bool EnableFullScreen { get; set; }

    [Reactive]
    public bool EnableIndependencyCore { get; set; }

    [Reactive]
    public LanguageViewData CurrentLanguage { get; set; }

    [Reactive]
    public ObservableCollection<LanguageViewData> SupportedLanguages { get; set; }

    public override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CurrentJavaRuntime))
            UpdataJavaInformation();

        if (e.PropertyName == nameof(CurrentGameFolder))
            UpdataGameCore();

        if (e.PropertyName == nameof(CurrentLanguage))
            CurrentLanguage.SetCurrentLanguage();

        DispatcherHelper.RunAsync(() =>
        {
            ConfigurationManager.AppSettings.GameFolders = GameFolders.ToList();
            ConfigurationManager.AppSettings.CurrentGameFolder = CurrentGameFolder;

            ConfigurationManager.AppSettings.JavaRuntimes = JavaRuntimes.ToList();
            ConfigurationManager.AppSettings.CurrentJavaRuntime = CurrentJavaRuntime;
            ConfigurationManager.AppSettings.JavaVirtualMachineMemory = JavaVirtualMachineMemory;
            ConfigurationManager.AppSettings.EnableAutoMemory = EnableAutoMemory;

            ConfigurationManager.AppSettings.GameWindowTitle = GameWindowTitle;
            ConfigurationManager.AppSettings.GameServerAddress = GameServerAddress;
            ConfigurationManager.AppSettings.GameWindowHeight = GameWindowHeight;
            ConfigurationManager.AppSettings.GameWindowWidth = GameWindowWidth;
            ConfigurationManager.AppSettings.EnableFullScreen = EnableFullScreen;
            ConfigurationManager.AppSettings.EnableIndependencyCore = EnableIndependencyCore;

            ConfigurationManager.AppSettings.CurrentLanguage = CurrentLanguage.Data;

            ConfigurationManager.Configuration.Save();
        });
    }

    private void UpdataJavaInformation() => DispatcherHelper.RunAsync(async () => CurrentJavaInformation = !string.IsNullOrEmpty(CurrentJavaRuntime) ? await JavaHelper.GetJavaInformation(CurrentJavaRuntime) : null);

    private void UpdataGameCore()
    {
        DispatcherHelper.RunAsync(async () =>
        {
            if (!string.IsNullOrEmpty(CurrentGameFolder))
            {
                var res = await GameCoreLocator.GetGameCores(CurrentGameFolder);
                ConfigurationManager.AppSettings.CurrentGameCore = res.Any() ? res[0] : null;
            }
            else ConfigurationManager.AppSettings.CurrentGameCore = null;

            ConfigurationManager.Configuration.Save();
        });
    }
}
