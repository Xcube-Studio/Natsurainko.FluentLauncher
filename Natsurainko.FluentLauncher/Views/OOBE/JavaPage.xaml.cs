using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.OOBE;

namespace Natsurainko.FluentLauncher.Views.OOBE;

public sealed partial class JavaPage : Page
{
    OOBEViewModel VM => (OOBEViewModel)DataContext;

    public JavaPage()
    {
        InitializeComponent();
    }
}
