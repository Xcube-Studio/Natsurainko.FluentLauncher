using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Pages.Mods;

public partial class CurseForgeModInfo : ObservableObject
{
    public CurseForge.Resource Resource { get; set; }

    public CurseForgeModInfo(CurseForge.Resource resource) 
    {
        Resource = resource;
        Icon = resource.Data.Logo.Url;

        Images = resource.Data.Screenshots.Select(x => x.Url);
        Versions = resource.Data.LatestFilesIndexes.Select(x => x.SupportedVersion);
        Categories = resource.Data.Categories.Select(x => x.Name);
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
