﻿using Microsoft.UI.Xaml.Data;
using Nrk.FluentCore.GameManagement.Installer;
using System;

namespace Natsurainko.FluentLauncher.XamlHelpers.Converters;

internal partial class InstallDataLoaderVersionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is ForgeInstallData forgeInstallData)
            return $"{forgeInstallData.Version}{(string.IsNullOrEmpty(forgeInstallData.Branch) ? string.Empty : $"-{forgeInstallData.Branch}")}";
        else if (value is OptiFineInstallData optiFineInstallData)
            return $"{optiFineInstallData.Type}_{optiFineInstallData.Patch}";
        else if (value is FabricInstallData fabricInstallData)
            return $"{fabricInstallData.Loader.Version}";
        else if (value is QuiltInstallData quiltInstallData)
            return $"{quiltInstallData.Loader.Version}";

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
