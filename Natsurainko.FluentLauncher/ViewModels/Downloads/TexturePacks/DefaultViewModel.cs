using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Services.UI;
using Nrk.FluentCore.Resources;
using System.Collections.Generic;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads.TexturePacks;

internal partial class DefaultViewModel(
    CurseForgeClient curseForgeClient,
    ModrinthClient modrinthClient,
    INavigationService navigationService,
    SearchProviderService searchProviderService) : ResourceDefaultViewModel(curseForgeClient, modrinthClient, navigationService, searchProviderService)
{
    protected override CurseForgeResourceType CurseForgeResourceType => CurseForgeResourceType.TexturePack;

    protected override ModrinthResourceType ModrinthResourceType => ModrinthResourceType.Resourcepack;

    protected override string[] ModrinthCategories { get; } = [
        "all", 
    ];

    protected override Dictionary<string, int> CurseForgeCategories { get; } = new()
    {
        { "All", 0 }, 
    };
}