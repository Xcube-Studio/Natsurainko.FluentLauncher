using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Natsurainko.FluentLauncher.Views.Pages.Mods;

public sealed partial class CurseForge : Page
{
    public CurseForge()
    {
        this.InitializeComponent();
    }

    private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width < 800)
        {
            SplitView.DisplayMode = SplitViewDisplayMode.Overlay;
            SplitViewGrid.Margin = new Thickness(0, 0, 0, 0);
        }
        else
        {
            SplitView.DisplayMode = SplitViewDisplayMode.Inline;
            SplitViewGrid.Margin = new Thickness(0, 0, 10, 0);
        }
    }
}
