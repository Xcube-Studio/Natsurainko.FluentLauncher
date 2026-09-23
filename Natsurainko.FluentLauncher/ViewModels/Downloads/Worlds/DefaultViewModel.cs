using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Nrk.FluentCore.Resources;
using System.Collections.Generic;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads.Worlds;

internal partial class DefaultViewModel(
    CurseForgeClient curseForgeClient,
    ModrinthClient modrinthClient,
    SettingsService settingsService,
    INavigationService navigationService,
    SearchProviderService searchProviderService) : ResourceDefaultViewModel(
        curseForgeClient, modrinthClient, settingsService, navigationService, searchProviderService)
{
    public override bool EnableModrinthSource => false;

    protected override CurseForgeResourceType CurseForgeResourceType => CurseForgeResourceType.World;

    protected override ModrinthResourceType ModrinthResourceType => ModrinthResourceType.DataPack;

    protected override string[] ModrinthCategories { get; } = [
        "all",
    ];

    protected override Dictionary<string, int> CurseForgeCategories { get; } = new()
    {
        { "All", 0 },
    };
}
