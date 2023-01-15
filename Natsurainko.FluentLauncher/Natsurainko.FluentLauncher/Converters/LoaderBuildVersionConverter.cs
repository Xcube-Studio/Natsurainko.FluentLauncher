using Microsoft.UI.Xaml.Data;
using Natsurainko.FluentCore.Model.Install.Fabric;
using Natsurainko.FluentCore.Model.Install.Forge;
using Natsurainko.FluentCore.Model.Install.OptiFine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Converters;

public class LoaderBuildVersionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is ForgeInstallBuild forge)
            return forge.ForgeVersion;
        else if (value is OptiFineInstallBuild optiFine)
            return optiFine.Patch;
        else if (value is FabricInstallBuild fabric)
            return fabric.Loader.Version;

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
