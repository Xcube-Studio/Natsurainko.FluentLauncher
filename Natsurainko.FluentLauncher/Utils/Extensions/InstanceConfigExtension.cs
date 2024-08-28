using Natsurainko.FluentLauncher.Models.Launch;
using Natsurainko.FluentLauncher.Services.Launch;
using Nrk.FluentCore.GameManagement.Instances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Utils.Extensions;

static class InstanceConfigExtension
{
    private static InstanceConfigService s_instanceConfigService = App.GetService<InstanceConfigService>();

    public static GameConfig GetConfig(this MinecraftInstance instance)
    {
        return s_instanceConfigService.GetConfig(instance);
    }
}