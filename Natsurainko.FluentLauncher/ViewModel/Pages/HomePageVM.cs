using Natsurainko.FluentCore.Class.Model.Auth;
using Natsurainko.FluentCore.Class.Model.Launch;
using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Class.ViewData;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using GameCoreLocator = Natsurainko.FluentLauncher.Shared.Mapping.GameCoreLocator;

namespace Natsurainko.FluentLauncher.ViewModel.Pages;

public class HomePageVM : ViewModelBase<Page>
{
    public HomePageVM(Page control) : base(control)
    {
        CurrentGameFolder = ConfigurationManager.AppSettings.CurrentGameFolder;
        CurrentAccount = ConfigurationManager.AppSettings.CurrentAccount;

        LaunchButtonTag = CurrentGameCore?.Data.Id ?? ConfigurationManager.AppSettings.CurrentLanguage.GetString("HP_Converter_NoCore");

        DispatcherHelper.RunAsync(async () =>
        {
            if (!string.IsNullOrEmpty(CurrentGameFolder))
            {
                CurrentGameCore = ConfigurationManager.AppSettings.CurrentGameCore?.CreateViewData<GameCore, GameCoreViewData>();
                GameCores = (await GameCoreLocator.GetGameCores(CurrentGameFolder)).CreateCollectionViewData<GameCore, GameCoreViewData>();
                CurrentGameCore = ConfigurationManager.AppSettings.CurrentGameCore?.CreateViewData<GameCore, GameCoreViewData>();
            }
            else CurrentGameCore = null;
        });

        UpdateAccountDisplay();
    }

    [Reactive]
    public string LaunchButtonTag { get; set; }

    [Reactive]
    public string CurrentGameFolder { get; set; }

    [Reactive]
    public ObservableCollection<GameCoreViewData> GameCores { get; set; }

    [Reactive]
    public GameCoreViewData CurrentGameCore { get; set; }

    [Reactive]
    public Account CurrentAccount { get; set; }

    [Reactive]
    public string AccountButtonTitle { get; set; }

    [Reactive]
    public string AccountButtonTag { get; set; }

    [Reactive]
    public Visibility AccountButtonTagVisibility { get; set; }

    [Reactive]
    public List<NewsViewData> NewsViews { get; set; }

    [Reactive]
    public Visibility ShowNews { get; set; } = Visibility.Collapsed;

    [Reactive]
    public string NewsButtonIcon { get; set; } = "\ue70d";

    [Reactive]
    public string NewsButtonText { get; set; } = ConfigurationManager.AppSettings.CurrentLanguage.GetString("HomePage_NewsShow");

    public override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(LaunchButtonTag))
            LaunchButtonTag = CurrentGameCore?.Data.Id ?? ConfigurationManager.AppSettings.CurrentLanguage.GetString("HP_Converter_NoCore");

        if (e.PropertyName != nameof(NewsButtonIcon))
            NewsButtonIcon = ShowNews == Visibility.Collapsed ? "\ue70d" : "\ue70e";

        if (e.PropertyName != nameof(NewsButtonIcon))
            NewsButtonText = ConfigurationManager.AppSettings.CurrentLanguage.GetString($"HomePage_News{(ShowNews == Visibility.Collapsed ? "Show" : "Hide")}");

        if (e.PropertyName == nameof(CurrentAccount))
            UpdateAccountDisplay();

        DispatcherHelper.RunAsync(() =>
        {
            ConfigurationManager.AppSettings.CurrentGameCore = CurrentGameCore?.Data;
            ConfigurationManager.AppSettings.ShowNews = (int)ShowNews;

            ConfigurationManager.Configuration.Save();
        });
    }

    private void UpdateAccountDisplay()
    {
        AccountButtonTitle = CurrentAccount?.Name ?? ConfigurationManager.AppSettings.CurrentLanguage.GetString("HP_Converter_NoAccount");
        AccountButtonTag = CurrentAccount == null ? null : ConfigurationManager.AppSettings.CurrentLanguage.GetString($"SAP_Converter_{CurrentAccount.Type}");
        AccountButtonTagVisibility = CurrentAccount != null ? Visibility.Visible : Visibility.Collapsed;
    }
}
