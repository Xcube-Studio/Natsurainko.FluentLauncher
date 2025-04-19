using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.ViewModels.Dialogs;

namespace Natsurainko.FluentLauncher.Views.Dialogs;

public sealed partial class UpdateDialog : ContentDialog
{
    UpdateDialogViewModel VM => (UpdateDialogViewModel)DataContext;

    public MarkdownConfig MarkdownConfig { get; set; } = new MarkdownConfig();

    public UpdateDialog(SettingsService _settingsService)
    {
        InitializeComponent();
        RequestedTheme = (ElementTheme)_settingsService.DisplayTheme;
    }
}
