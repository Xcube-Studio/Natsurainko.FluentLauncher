using FluentLauncher.Infra.UI.Navigation;
using Nrk.FluentCore.GameManagement.Instances;

namespace Natsurainko.FluentLauncher.ViewModels.Instances;

internal partial class NavigationViewModel(INavigationService navigationService)
    : NavigationPageVM<MinecraftInstance>(navigationService), INavigationAware
{
    protected override string RootPageKey => "Instances";

    protected override string ParameterRouteKey => "Instance";

    protected override string GetRouteOfParameter(MinecraftInstance instance) => instance.InstanceId;

    void INavigationAware.OnNavigatedTo(object? parameter)
    {
        if (parameter is MinecraftInstance instance)
        {
            Parameter = instance;
            NavigateTo("Instances/Instance", instance);
        }
        else if (parameter is string pageKey)
        {
            NavigateTo(pageKey);
        }
        else
        {
            NavigateTo("Instances/Default");
        }
    }
}
