using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.OOBE;
using System.Collections.Generic;

namespace Natsurainko.FluentLauncher.Views.OOBE;

public sealed partial class LanguagePage : Page
{
    OOBEViewModel VM => (OOBEViewModel)DataContext;

    public string Version => App.Version.GetVersionString();

#if DEBUG 
    public string Edition => ResourceUtils.GetValue("Settings", "AboutPage", "_Debug");
#else 
    public string Edition => ResourceUtils.GetValue("Settings", "AboutPage", "_Release");
#endif

    public List<string> Languages => ResourceUtils.Languages;

    public LanguagePage()
    {
        InitializeComponent();
    }
}
