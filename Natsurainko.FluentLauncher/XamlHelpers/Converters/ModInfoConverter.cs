using Microsoft.UI.Xaml.Data;
using Nrk.FluentCore.GameManagement.Mods;
using System;
using System.Collections.Generic;
using System.IO;

#nullable disable
namespace Natsurainko.FluentLauncher.XamlHelpers.Converters;

internal partial class ModInfoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is MinecraftMod mod)
        {
            if (mod.DisplayName.Equals(Path.GetFileNameWithoutExtension(mod.AbsolutePath)))
                return "Unable to parse mod details";

            var strings = new List<string>();

            if (mod.SupportedModLoaders != null && mod.SupportedModLoaders.Length != 0) 
                strings.Add(string.Join(",", mod.SupportedModLoaders));

            if (!string.IsNullOrEmpty(mod.Version)) 
                strings.Add(mod.Version);

            if (!string.IsNullOrEmpty(mod.Description)) 
                strings.Add(mod.Description);

            return string.Join(" | ", strings);
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
