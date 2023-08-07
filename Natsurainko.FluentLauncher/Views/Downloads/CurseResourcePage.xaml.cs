using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Services.Storage;
using Nrk.FluentCore.Classes.Datas.Download;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Views.Downloads;

public sealed partial class CurseResourcePage : Page
{
    private readonly InterfaceCacheService _interfaceCacheService = App.GetService<InterfaceCacheService>();

    public CurseResourcePage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        var resource = e.Parameter as CurseResource;

        base.OnNavigatedTo(e);
        this.DataContext = resource;

        if (!(resource.ScreenshotUrls?.ToList().Any()).GetValueOrDefault())
            stackPanel.Children.Remove(ScreenshotsBorder);

        Task.Run(() =>
        {
            var des = _interfaceCacheService.CurseForgeClient.GetResourceDescription(resource.Id);

            App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
            {
                await webView2.EnsureCoreWebView2Async();

                webView2.NavigationCompleted += WebView2_NavigationCompleted;
                webView2.NavigateToString($"<div id='container'>{des}</div>");
            });
        }).ContinueWith(task =>
        {
            if (task.IsFaulted)
                App.MainWindow.DispatcherQueue.TryEnqueue(() => stackPanel.Children.Remove(descriptionBorder));
        });
    }

    private async void WebView2_NavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
    {
        var script = "eval(document.getElementById('container').getBoundingClientRect().height.toString());";
        var heightString = await webView2.ExecuteScriptAsync(script);

        if (double.TryParse(heightString, out double height))
            sender.Height = height + 50;
    }
}
