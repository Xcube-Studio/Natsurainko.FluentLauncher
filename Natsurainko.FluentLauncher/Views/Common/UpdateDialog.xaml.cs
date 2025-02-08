using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Settings;

namespace Natsurainko.FluentLauncher.Views.Common;

public sealed partial class UpdateDialog : ContentDialog
{
    public MarkdownConfig MarkdownConfig { get; set; } = new MarkdownConfig();

    public UpdateDialog(SettingsService _settingsService)
    {
        InitializeComponent();
        RequestedTheme = (ElementTheme)_settingsService.DisplayTheme;
    }
}
