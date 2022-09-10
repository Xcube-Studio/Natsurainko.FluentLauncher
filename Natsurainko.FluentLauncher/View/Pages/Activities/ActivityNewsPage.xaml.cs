using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.ViewModel.Pages.Activities;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.View.Pages.Activities;

public sealed partial class ActivityNewsPage : Page
{
    public ActivityNewsPage()
    {
        this.InitializeComponent();

        ViewModelBuilder.Build<ActivityNewsPageVM, Page>(this);
    }
}
