using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Services.Data;
using System.Collections.Generic;
using System.Linq;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads;

internal partial class CurseForgeModInfoViewModel : ObservableObject
{
    public CurseForgeResourceData Resource { get; }

    public CurseForgeModInfoViewModel(CurseForgeResourceData resource) 
    {
        Resource = resource;
        Icon = resource.InnerData.Logo.Url;

        Images = resource.InnerData.Screenshots.Select(x => x.Url);
        Versions = resource.InnerData.LatestFilesIndexes.Select(x => x.SupportedVersion);
        Categories = resource.InnerData.Categories.Select(x => x.Name);
        ImageShow = Images.Any() ? Visibility.Visible : Visibility.Collapsed;
    }

    [ObservableProperty]
    private string icon;

    [ObservableProperty]
    private IEnumerable<string> images;

    [ObservableProperty]
    private IEnumerable<string> versions;

    [ObservableProperty]
    private IEnumerable<string> categories;

    [ObservableProperty]
    private Visibility imageShow;
}
