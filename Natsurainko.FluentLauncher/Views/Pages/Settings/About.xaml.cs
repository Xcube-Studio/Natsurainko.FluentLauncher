using Microsoft.UI.Xaml.Controls;
using System;

namespace Natsurainko.FluentLauncher.Views.Pages.Settings;

public sealed partial class About : Page
{
    public About()
    {
        InitializeComponent();
        throw new Exception("Test exception handler");
    }
}
