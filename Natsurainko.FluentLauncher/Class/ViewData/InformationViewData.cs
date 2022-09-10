using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Natsurainko.FluentLauncher.Class.ViewData;

public class InformationViewData : ViewDataBase
{
    public bool Removed { get; set; } = false;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ButtonBase Button { get; set; }

    public int Delay { get; set; } = 5000;

    public InfoBarSeverity Severity { get; set; } = InfoBarSeverity.Informational;
}
