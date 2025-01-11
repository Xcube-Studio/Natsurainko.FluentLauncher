using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.News;

namespace Natsurainko.FluentLauncher.Views.News;

public sealed partial class NotePage : Page, IBreadcrumbBarAware
{
    public string Route => "Note";

    NoteViewModel VM => (NoteViewModel)DataContext;

    public NotePage()
    {
        InitializeComponent();
    }
}
