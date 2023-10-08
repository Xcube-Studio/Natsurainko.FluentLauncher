using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Classes.Data.Launch;
using Natsurainko.FluentLauncher.Services.UI.Navigation;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views;
using Natsurainko.FluentLauncher.Views.Common;

namespace Natsurainko.FluentLauncher.ViewModels.Cores;

internal partial class ManageNavigationViewModel : ObservableObject, INavigationAware
{
    public readonly INavigationService _navigationService;
    public ExtendedGameInfo _gameInfo;

    public ManageNavigationViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    [ObservableProperty]
    private string[] breadcrumbBarItemsSource;

    [RelayCommand]
    void BreadcrumbBarClicked(object args)
    {
        var e = args.As<BreadcrumbBar, BreadcrumbBarItemClickedEventArgs>();

        if (e.args.Index.Equals(0))
            _navigationService.Parent.GoBack();
    }

    [RelayCommand]
    void DeleteGame() => _ = new DeleteGameDialog() { XamlRoot = ShellPage._XamlRoot, DataContext = new DeleteGameDialogViewModel(_gameInfo, _navigationService) }.ShowAsync();

    #region Navigation

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        _gameInfo = parameter as ExtendedGameInfo;

        BreadcrumbBarItemsSource = new string[]
        {
            ResourceUtils.GetValue("Cores", "ManageNavigationPage", "_BreadcrumbBar_First"),
            _gameInfo.Name
        };

        _navigationService.NavigateTo("CoreSettingsPage", _gameInfo);
    }

    public void NavigateTo(string pageKey, object? parameter = null)
        => _navigationService.NavigateTo(pageKey, parameter);

    #endregion
}
