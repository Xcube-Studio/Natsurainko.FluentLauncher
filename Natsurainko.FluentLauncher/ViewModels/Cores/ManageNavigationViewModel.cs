using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views;
using Natsurainko.FluentLauncher.Views.Common;
using Nrk.FluentCore.Management;
using System;

namespace Natsurainko.FluentLauncher.ViewModels.Cores;

internal partial class ManageNavigationViewModel : ObservableObject, INavigationAware
{
    public readonly INavigationService _navigationService;
    public GameInfo _gameInfo;

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

    void INavigationAware.OnNavigatedTo(object? parameter)
    {
        if (parameter is not GameInfo gameInfo)
            throw new ArgumentException("Invalid parameter type");

        _gameInfo = gameInfo;

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
