using Natsurainko.FluentLauncher.Models.Launch;
using Natsurainko.FluentLauncher.Services.Launch;
using Nrk.FluentCore.GameManagement.Instances;

namespace Natsurainko.FluentLauncher.Utils.Extensions;

static class InstanceConfigExtension
{
    private static InstanceConfigService s_instanceConfigService = App.GetService<InstanceConfigService>();

    public static InstanceConfig GetConfig(this MinecraftInstance instance)
    {
        return s_instanceConfigService.GetConfig(instance);
    }
}