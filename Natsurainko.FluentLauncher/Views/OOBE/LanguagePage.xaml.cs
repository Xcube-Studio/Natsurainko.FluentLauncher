using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.OOBE;
using System.Collections.Generic;

namespace Natsurainko.FluentLauncher.Views.OOBE;

public sealed partial class LanguagePage : Page
{
    OOBEViewModel VM => (OOBEViewModel)DataContext;

    public LanguagePage()
    {
        InitializeComponent();
    }
}
