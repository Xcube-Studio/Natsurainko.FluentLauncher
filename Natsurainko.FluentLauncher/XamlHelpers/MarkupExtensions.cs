using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;
using Natsurainko.FluentLauncher.Utils;
using System.Linq;

#nullable disable
namespace Natsurainko.FluentLauncher.XamlHelpers;

[MarkupExtensionReturnType(ReturnType = typeof(Microsoft.UI.Xaml.Controls.FontIcon))]
public partial class FontIcon : MarkupExtension
{
    public string Glyph { get; set; }

    protected override object ProvideValue()
    {
        return new Microsoft.UI.Xaml.Controls.FontIcon { Glyph = Glyph };
    }
}

[MarkupExtensionReturnType(ReturnType = typeof(string[]))]
public partial class ComboBox : MarkupExtension
{
    public string ResourceKey { get; set; }

    protected override object ProvideValue(IXamlServiceProvider serviceProvider)
    {
        var uriContext = serviceProvider.GetService(typeof(IUriContext)) as IUriContext;
        var path = uriContext.BaseUri.AbsolutePath.ToString()
            .Replace("/Views/", string.Empty)
            .Replace(".xaml", string.Empty)
            .Split('/')
            .Append(ResourceKey)
            .ToArray();
        return LocalizedStrings.GetStrings(string.Join('_', path));
    }
}