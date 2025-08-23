using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Downloads;
using System;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Views.Downloads;

internal record ResourceAuthor(string Name, string WebLink);

public sealed partial class ResourcePage : Page, IBreadcrumbBarAware
{
    string IBreadcrumbBarAware.Route => "Resource";

    ResourceViewModel VM => (ResourceViewModel)DataContext;

    public MarkdownConfig MarkdownConfig { get; set; } = new MarkdownConfig();

    public ResourcePage()
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

    internal static string GetPreferredFolderOptionText(string folder, int index)
    {
        if (folder == null) return string.Empty;

        return index switch
        {
            0 => LocalizedStrings.Downloads_ResourcePage__M2.Replace("${folder}", folder),
            1 => LocalizedStrings.Downloads_ResourcePage__M3.Replace("${folder}", folder),
            _ => throw new NotImplementedException()
        };
    }
}
