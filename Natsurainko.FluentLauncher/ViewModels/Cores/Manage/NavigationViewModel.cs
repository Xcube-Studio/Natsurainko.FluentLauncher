using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Management;
using System.Collections.ObjectModel;
using System.Linq;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Cores.Manage;

public partial class NavigationViewModel : ObservableObject, INavigationAware
{
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<string> routes;

    public GameInfo GameInfo { get; private set; }

    public NavigationViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        GameInfo = parameter as GameInfo;

        Routes = [];
        _navigationService.NavigateTo("CoreManage/Default", GameInfo);
    }

    public void NavigateTo(string pageKey, object parameter = null)
    {
        _navigationService.NavigateTo(pageKey, parameter);

        if (pageKey == "CoreManage/Default")
        {
            Routes = new(["CoreManage", GameInfo.Name]);
        }
        else
        {
            Routes = new(pageKey.Split('/'));
            Routes.Insert(1, GameInfo.Name);
        }
    }

    [RelayCommand]
    public void ItemClickedEvent(object args)
    {
        var breadcrumbBarItemClickedEventArgs = args.As<BreadcrumbBar, BreadcrumbBarItemClickedEventArgs>().args;

        if (breadcrumbBarItemClickedEventArgs.Item.ToString() == "CoreManage")
            _navigationService.Parent.NavigateTo("CoresPage");
        else if (breadcrumbBarItemClickedEventArgs.Item.ToString() == GameInfo.Name)
            NavigateTo("CoreManage/Default", GameInfo);
        else NavigateTo(string.Join('/', Routes.ToArray()[..^1]).Replace($"/{GameInfo.Name}/", "/"), GameInfo);
    }
}
