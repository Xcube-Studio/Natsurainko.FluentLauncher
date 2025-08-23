using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.OOBE;

namespace Natsurainko.FluentLauncher.Views.OOBE;

public sealed partial class LanguagePage : Page
{
    OOBEViewModel VM => (OOBEViewModel)DataContext;

    public LanguagePage()
    {
        InitializeComponent();
    }
}
