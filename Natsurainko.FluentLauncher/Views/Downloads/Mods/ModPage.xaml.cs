using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Natsurainko.FluentLauncher.ViewModels.Downloads.Mods;
using System;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Views.Downloads.Mods;

public sealed partial class ModPage : Page, IBreadcrumbBarAware
{
    string IBreadcrumbBarAware.Route => "Mod";

    ModViewModel VM => (ModViewModel)DataContext;

    public MarkdownConfig MarkdownConfig { get; set; } = new MarkdownConfig();

    public ModPage()
    {
        this.InitializeComponent();
    }

    private void FilesItemsView_SelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs args) => VM.SelectedFile = sender.SelectedItem;

    private void FilesItemsView_Unloaded(object sender, RoutedEventArgs e) => VM.SelectedFile = null;

    private void DescriptionMarkdown_Loaded(object sender, RoutedEventArgs e) => DescriptionMarkdown.Text = VM.DescriptionContent;

    private async void DescriptionWebView2_Loaded(object sender, RoutedEventArgs e)
    {
        WebView2 webView2 = DescriptionWebView2;
        await webView2.EnsureCoreWebView2Async();

        webView2.CoreWebView2.Profile.PreferredColorScheme = webView2.ActualTheme == ElementTheme.Dark
            ? CoreWebView2PreferredColorScheme.Dark
            : CoreWebView2PreferredColorScheme.Light;

        string body = $"<meta name=\"color-scheme\" content=\"{(webView2.ActualTheme == ElementTheme.Dark ? "dark light" : "light dark")}\">\r\n"
            + "<style>img{width:auto;height:auto;max-width:100%;max-height:100%;}</style>\r\n"
            + $"<div id='container'>{VM.DescriptionContent}</div>";

        webView2.MinHeight = VM.DescriptionContent.Length / (webView2.ActualWidth / 8) * 14;
        webView2.NavigateToString(body);

        await Task.Delay(2000);

        var script = "eval(document.getElementById('container').getBoundingClientRect().height.toString());";
        var heightString = await webView2.ExecuteScriptAsync(script);

        if (double.TryParse(heightString, out double height))
            webView2.MinHeight = height + 30;
    }
}
