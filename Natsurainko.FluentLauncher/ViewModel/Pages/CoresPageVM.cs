using Natsurainko.FluentCore.Class.Model.Launch;
using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Class.ViewData;
using Natsurainko.FluentLauncher.Shared.Mapping;
using Natsurainko.FluentLauncher.View.Pages.Resources;
using Natsurainko.FluentLauncher.View.Pages.Settings;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.ViewModel.Pages;

public class CoresPageVM : ViewModelBase<Page>
{
    public CoresPageVM(Page control) : base(control)
    {
        GameFolders = new ObservableCollection<string>(ConfigurationManager.AppSettings.GameFolders);
        CurrentGameFolder = ConfigurationManager.AppSettings.CurrentGameFolder;

        CoreSortBy = ConfigurationManager.AppSettings.CoreSortBy.GetValueOrDefault();
        CoreSortByItemSource = ConfigurationManager.AppSettings.CurrentLanguage.GetString("CP_CB1_IS").Split(":").ToList();

        CoreVisibility = ConfigurationManager.AppSettings.CoreVisibility.GetValueOrDefault();
        CoreVisibilityItemSource = ConfigurationManager.AppSettings.CurrentLanguage.GetString("CP_CB2_IS").Split(":").ToList();

        UpdateGameCores();
    }

    public ListBox CoresList { get; set; }

    [Reactive]
    public ObservableCollection<string> GameFolders { get; set; }

    [Reactive]
    public string CurrentGameFolder { get; set; }

    [Reactive]
    public ObservableCollection<GameCoreViewData> GameCores { get; set; }

    [Reactive]
    public GameCoreViewData CurrentGameCore { get; set; }

    [Reactive]
    public Visibility TipsVisibility { get; set; }

    [Reactive]
    public string TipsTitle { get; set; }

    [Reactive]
    public string TipsLink { get; set; }

    [Reactive]
    public int CoreSortBy { get; set; }

    [Reactive]
    public List<string> CoreSortByItemSource { get; set; }

    [Reactive]
    public int CoreVisibility { get; set; }

    [Reactive]
    public List<string> CoreVisibilityItemSource { get; set; }

    public Action TipsAction { get; private set; }

    public override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CurrentGameFolder) || e.PropertyName == nameof(GameCores))
            UpdateTips();

        if (e.PropertyName == nameof(CurrentGameFolder) || e.PropertyName == nameof(CoreSortBy) || e.PropertyName == nameof(CoreVisibility))
            UpdateGameCores();

        if (e.PropertyName == nameof(CurrentGameCore))
            DispatcherHelper.RunAsync(() => ConfigurationManager.AppSettings.CurrentGameCore = CurrentGameCore?.Data);

        DispatcherHelper.RunAsync(() =>
        {
            ConfigurationManager.AppSettings.GameFolders = GameFolders.ToList();
            ConfigurationManager.AppSettings.CurrentGameFolder = CurrentGameFolder;

            ConfigurationManager.AppSettings.CoreSortBy = CoreSortBy;
            ConfigurationManager.AppSettings.CoreVisibility = CoreVisibility;

            ConfigurationManager.Configuration.Save();
        });
    }

    private void UpdateGameCores()
    {
        DispatcherHelper.RunAsync(async () =>
        {
            if (!string.IsNullOrEmpty(CurrentGameFolder))
            {
                var gameCores = (await GameCoreLocator.GetGameCores(CurrentGameFolder)).Where(x =>
                {
                    if (CoreVisibility == 0)
                        return true;
                    else if (CoreVisibility == 1 && x.Type == "release")
                        return true;
                    else if (CoreVisibility == 2 && x.Type == "snapshot")
                        return true;
                    else if (CoreVisibility == 3 && x.Type.StartsWith("old"))
                        return true;

                    return false;
                }).ToList();

                if (CoreSortBy == 0)
                    gameCores.Sort((a, b) => a.Id.CompareTo(b.Id));
                else
                {
                    var cache = (await GameCoreLocator.GetGameCoresLastLaunchTime(CurrentGameFolder))
                    .Select(x => (gameCores.FirstOrDefault(y => y.Id.Equals(x.Key)), x.Value))
                    .Where(x => x.Item1 != null)
                    .ToList();

                    cache.Sort((a, b) => a.Value.GetValueOrDefault().CompareTo(b.Value.GetValueOrDefault()));

                    gameCores = cache.Select(x => x.Item1).ToList();
                    gameCores.Reverse();
                }

                GameCores = gameCores.CreateCollectionViewData<GameCore, GameCoreViewData>();
            }

            CurrentGameCore = ConfigurationManager.AppSettings.CurrentGameCore?.CreateViewData<GameCore, GameCoreViewData>();

            ConfigurationManager.AppSettings.CurrentGameCore = CurrentGameCore?.Data;
            ConfigurationManager.Configuration.Save();

            UpdateTips();
            CoresList.ScrollIntoView(CurrentGameCore);
        });
    }

    private void UpdateTips()
    {
        TipsVisibility = GameFolders != null && GameFolders.Any() && (!string.IsNullOrEmpty(CurrentGameFolder)) && GameCores != null && GameCores.Any()
            ? Visibility.Collapsed : Visibility.Visible;

        if (!(GameFolders != null && GameFolders.Any()))
        {
            TipsTitle = ConfigurationManager.AppSettings.CurrentLanguage.GetString("CP_T1");
            TipsLink = ConfigurationManager.AppSettings.CurrentLanguage.GetString("CP_L1");

            TipsAction = () => MainContainer.ContentFrame.Navigate(typeof(SettingsPage));
            return;
        }

        if (string.IsNullOrEmpty(CurrentGameFolder))
        {
            TipsTitle = ConfigurationManager.AppSettings.CurrentLanguage.GetString("CP_T2");
            TipsLink = ConfigurationManager.AppSettings.CurrentLanguage.GetString("CP_L2");

            TipsAction = () => MainContainer.ContentFrame.Navigate(typeof(SettingsPage));
            return;
        }

        if (!(GameCores != null && GameCores.Any()))
        {
            TipsTitle = ConfigurationManager.AppSettings.CurrentLanguage.GetString("CP_T3");
            TipsLink = ConfigurationManager.AppSettings.CurrentLanguage.GetString("CP_L3");

            TipsAction = () => MainContainer.ContentFrame.Navigate(typeof(ResourcesPage));
            return;
        }
    }

}
