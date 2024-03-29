﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Services.UI.Navigation;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views;
using Natsurainko.FluentLauncher.Views.Common;
using Nrk.FluentCore.Resources;
using System.Linq;
using System.Threading.Tasks;

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
    private string markdownText;

    [ObservableProperty]
    private Visibility descriptionBorderVisbility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility screenshotsBorderVisbility = Visibility.Collapsed;

    [ObservableProperty]
    private object resource;

    void INavigationAware.OnNavigatedTo(object? parameter)
    {
        Resource = parameter;

        bool isCurse = parameter is CurseForgeResource;

        var urls = isCurse ? ((CurseForgeResource)parameter).ScreenshotUrls : ((ModrinthResource)parameter).ScreenshotUrls;
        object id = isCurse ? ((CurseForgeResource)parameter).Id : ((ModrinthResource)parameter).Id;

        if (urls.Any())
            ScreenshotsBorderVisbility = Visibility.Visible;

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
                MarkdownText = markdown;
                DescriptionBorderVisbility = Visibility.Visible;
            });
        });
    }

    [RelayCommand]
    public void Download()
    {
        _ = new ResourceItemFilesDialog()
        {
            XamlRoot = ShellPage._XamlRoot,
            DataContext = new ResourceItemFilesDialogViewModel(Resource, _navigationService)
        }.ShowAsync();
    }
}
