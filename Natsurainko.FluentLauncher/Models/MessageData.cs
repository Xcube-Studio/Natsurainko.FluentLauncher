using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Natsurainko.FluentLauncher.Models;

public class MessageData
{
    public string Message { get; set; }

    public string Title { get; set; }

    public ButtonBase Button { get; set; }

    public InfoBarSeverity Severity { get; set; }

    public bool Removed { get; set; }
}
