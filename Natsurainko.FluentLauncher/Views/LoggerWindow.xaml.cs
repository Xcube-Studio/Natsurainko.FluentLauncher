using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.ViewModels;
using Natsurainko.FluentLauncher.ViewModels.Pages;
using Nrk.FluentCore.Launch;
using System.ComponentModel;
using System.Windows.Forms;
using WinUIEx;

namespace Natsurainko.FluentLauncher.Views;

internal sealed partial class LoggerWindow : WindowEx, INotifyPropertyChanged
{
    private static readonly LoggerColorLightLanguage lightLanguage = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    LoggerViewModel? VM { get; set; }

    public LoggerWindow(SettingsService settingsService)
    {
        this.ConfigureTitleBarTheme();
        this.ConfigureElementTheme();

        InitializeComponent();

        if (MicaController.IsSupported())
        {
            SystemBackdrop = new MicaBackdrop() { Kind = (MicaKind)settingsService.MicaKind };
        }
        else if (DesktopAcrylicController.IsSupported())
        {
            SystemBackdrop = new DesktopAcrylicBackdrop();
        }
    }

    public void Initialize(LaunchTaskViewModel launchTaskViewModel)
    {
        Title = launchTaskViewModel.Title;

        VM = new()
        {
            Outputs = launchTaskViewModel.ProcessLogger,
            Title = Title,
            ScrollToEnd = ScrollToEnd,
            IsActive = true,
            Info = true,
            Warn = true,
            Error = true,
            Fatal = true
        };

        PropertyChanged?.Invoke(this, new(nameof(VM)));
    }

    private void Grid_Unloaded(object sender, RoutedEventArgs e)
    {
        if (VM is not null)
        {
            VM.IsActive = false;
        }
    }

    internal static UIElement ConvertLoggerOutput(GameLoggerOutput output)
    {
        RichTextBlock richTextBlock = new() 
        {
            FontWeight = FontWeights.SemiBold
        };

        LoggerColorLightLanguage.Formatter.FormatRichTextBlock(output.FullData, lightLanguage, richTextBlock);
        return richTextBlock;
    }

    private void ScrollToEnd()
    {
        // Memory issue and scroll behavior issue
        // View.ListView.SmoothScrollIntoViewWithIndexAsync(View.ListView.Items.Count - 1);

        // Crash with layout recycle detected
        // ScrollView scrollView = View.ItemsView.ScrollView;
        // scrollView?.ScrollTo(0, scrollView.ScrollableHeight);

        // ScrollViewer behavior issue when ItemsRepeater has many item
        // View.ScrollViewer.ScrollTo(0, View.ScrollViewer.ScrollableHeight);

        try
        {
            ListBox.ScrollIntoView(ListBox.Items[^1]);
        }
        catch { }
    }
}
