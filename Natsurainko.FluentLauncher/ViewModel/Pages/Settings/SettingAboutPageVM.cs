using ReactiveUI.Fody.Helpers;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.ViewModel.Pages.Settings;

public class SettingAboutPageVM : ViewModelBase<Page>
{
    public SettingAboutPageVM(Page control) : base(control)
    {
        Version = string.Format("{0}.{1}.{2}.{3}",
            Package.Current.Id.Version.Major,
            Package.Current.Id.Version.Minor,
            Package.Current.Id.Version.Build,
            Package.Current.Id.Version.Revision);

#if DEBUG
        Edition = "开发版";
#else
        Edition = "正式版";
#endif
    }

    [Reactive]
    public string Version { get; set; }

    [Reactive]
    public string Edition { get; set; }
}
