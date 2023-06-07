using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.OOBE;

namespace Natsurainko.FluentLauncher.Views.OOBE;

public sealed partial class LanguagePage : Page
{
    public LanguagePage()
    {
        InitializeComponent();
        DataContext = App.Services.GetService<LanguageViewModel>();
    }
}
