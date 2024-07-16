using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.Common;
using Nrk.FluentCore.Resources;
using System;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Downloads;

internal partial class ResourceItemViewModel : ObservableObject, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly InterfaceCacheService _interfaceCacheService;

    public ResourceItemViewModel(INavigationService navigationService, InterfaceCacheService interfaceCacheService)
    {
        _navigationService = navigationService;
        _interfaceCacheService = interfaceCacheService;
    }

    [ObservableProperty]
    private Visibility descriptionBorderVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility screenshotsBorderVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private object resource;

    void INavigationAware.OnNavigatedTo(object? parameter)
    {
        Resource = parameter;

        bool isCurse = parameter is CurseForgeResource;

        var urls = isCurse ? ((CurseForgeResource)parameter).ScreenshotUrls : ((ModrinthResource)parameter).ScreenshotUrls;

        if (urls.Any())
            ScreenshotsBorderVisibility = Visibility.Visible;
    }

    [RelayCommand]
    public async Task Download()
    {
        await new ResourceItemFilesDialog()
        {
            DataContext = new ResourceItemFilesDialogViewModel(Resource, _navigationService)
        }.ShowAsync();
    }

    [RelayCommand]
    public void LoadedEvent(object args)
    {
        var sender = args.As<MarkdownTextBlock, object>().sender;
        bool isCurse = Resource is CurseForgeResource;
        object id = isCurse ? ((CurseForgeResource)Resource).Id : ((ModrinthResource)Resource).Id;

        Task.Run(async () =>
        {
            string markdown = default;

            if (!isCurse)
            {
                markdown = await _interfaceCacheService.ModrinthClient.GetResourceDescriptionAsync((string)id);
            }
            else
            {
                var des = await _interfaceCacheService.CurseForgeClient.GetResourceDescriptionAsync((int)id);
                markdown = new ReverseMarkdown.Converter().Convert(des);
            }

            App.DispatcherQueue.TryEnqueue(() =>
            {
                sender.Config = new MarkdownConfig();
                sender.Text = markdown;
                DescriptionBorderVisibility = Visibility.Visible;
            });
        });
    }
}
