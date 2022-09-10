using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Shared.Mapping;
using Natsurainko.FluentLauncher.ViewModel.Pages.Settings;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Natsurainko.FluentLauncher.View.Pages.Settings;

public sealed partial class SettingDownloadPage : Page
{
    public SettingDownloadPageVM ViewModel { get; set; }

    public SettingDownloadPage()
    {
        this.InitializeComponent();

        ViewModel = ViewModelBuilder.Build<SettingDownloadPageVM, Page>(this);
        Slider.AddHandler(PointerReleasedEvent, new PointerEventHandler((s, e) => DefaultSettings.SetMaxDownloadThreads(ViewModel.MaxDownloadThreads)), true);
    }
}
