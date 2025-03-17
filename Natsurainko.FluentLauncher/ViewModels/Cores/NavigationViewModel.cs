using FluentLauncher.Infra.UI.Navigation;
using Nrk.FluentCore.GameManagement.Instances;

namespace Natsurainko.FluentLauncher.ViewModels.Cores;

internal partial class NavigationViewModel(INavigationService navigationService) 
    : NavigationPageVM<MinecraftInstance>(navigationService), INavigationAware
{
    protected override string RootPageKey => "Cores";

    protected override string ParameterRouteKey => "Instance";

    protected override string GetRouteOfParameter(MinecraftInstance instance) => instance.InstanceId;

    void INavigationAware.OnNavigatedTo(object? parameter)
    {
        if (parameter is MinecraftInstance instance)
        {
            Parameter = instance;
            NavigateTo("Cores/Instance", instance);
        }
        else if (parameter is string pageKey)
        {
            NavigateTo(pageKey);
        }
        else
        {
            NavigateTo("Cores/Default");
        }
    }
}
