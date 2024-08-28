using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.XamlHelpers.Converters;
using Nrk.FluentCore.GameManagement.Instances;
using System.Collections.ObjectModel;
using System.Linq;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Cores.Manage;

public partial class NavigationViewModel : ObservableObject, INavigationAware
{
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<string> routes;

    public MinecraftInstance MinecraftInstance { get; private set; }

    public string InstanceId { get; private set; } // 缓存游戏名称，防止昵称修改后名称对不上

    public NavigationViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        MinecraftInstance = parameter as MinecraftInstance;
        InstanceId = MinecraftInstance.InstanceId;

        Routes = [];
        _navigationService.NavigateTo("CoreManage/Default", MinecraftInstance);
    }

    public void NavigateTo(string pageKey, object parameter = null)
    {
        _navigationService.NavigateTo(pageKey, parameter);

        if (pageKey == "CoreManage/Default")
        {
            Routes = new(["CoreManage", InstanceId]);
        }
        else
        {
            Routes = new(pageKey.Split('/'));
            Routes.Insert(1, InstanceId);
        }
    }

    [RelayCommand]
    public void ItemClickedEvent(object args)
    {
        var breadcrumbBarItemClickedEventArgs = args.As<BreadcrumbBar, BreadcrumbBarItemClickedEventArgs>().args;

        if (breadcrumbBarItemClickedEventArgs.Item.ToString() == "CoreManage")
            _navigationService.Parent.NavigateTo("CoresPage");
        else if (breadcrumbBarItemClickedEventArgs.Item.ToString() == InstanceId)
            NavigateTo("CoreManage/Default", MinecraftInstance);
        else NavigateTo(string.Join('/', Routes.ToArray()[..^1]).Replace($"/{InstanceId}/", "/"), MinecraftInstance);
    }

    [RelayCommand]
    public void BreadcrumbBarLoadingEvent(object args)
    {
        var breadcrumbBar = args.As<BreadcrumbBar, object>().sender;
        var converter = breadcrumbBar.Resources["BreadcrumbBarLocalizationConverter"] as BreadcrumbBarLocalizationConverter;

        converter.IgnoredText.Add(InstanceId);
    }
}
