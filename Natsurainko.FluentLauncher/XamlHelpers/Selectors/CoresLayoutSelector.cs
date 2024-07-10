using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Settings;

#nullable disable
namespace Natsurainko.FluentLauncher.XamlHelpers.Selectors;

internal class CoresLayoutSelector : DataTemplateSelector
{
    private readonly SettingsService settingsService = App.GetService<SettingsService>();

    public DataTemplate StackTemplate { get; set; }

    public DataTemplate CardTemplate { get; set; }

    public DataTemplate TileTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item) => settingsService.CoresLayoutIndex switch
    {
        1 => CardTemplate,
        2 => TileTemplate,
        _ => StackTemplate,
    };
}
