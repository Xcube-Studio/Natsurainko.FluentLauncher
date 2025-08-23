using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Services.UI;
using Nrk.FluentCore.Resources;
using System.Collections.Generic;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads.Mods;

internal partial class DefaultViewModel(
    CurseForgeClient curseForgeClient,
    ModrinthClient modrinthClient,
    INavigationService navigationService,
    SearchProviderService searchProviderService) : ResourceDefaultViewModel(curseForgeClient, modrinthClient, navigationService, searchProviderService)
{
    protected override CurseForgeResourceType CurseForgeResourceType => CurseForgeResourceType.McMod;

    protected override ModrinthResourceType ModrinthResourceType => ModrinthResourceType.McMod;

    protected override string[] ModrinthCategories { get; } = [
        "all", "adventure", "cursed", "decoration", "economy", "equipment", "food",
        "game-mechanics", "library", "magic", "management", "minigame", "mobs", "optimization",
        "social", "storage", "technology", "transportation", "utility", "worldgen"
    ];

    protected override Dictionary<string, int> CurseForgeCategories { get; } = new()
    {
        { "All", 0 }, { "Map and Information", 423 }, { "Armor, Tools, and Weapons", 434 }, { "API and Library", 421 }, { "Adventure and RPG", 422 }, { "Processing", 413 },
        { "Utility & QoL", 5191 }, { "Education", 5299 }, { "Miscellaneous", 425 }, { "Server Utility", 435 }, { "Technology", 412 }, { "Food", 436 }, { "World Gen", 406 },
        { "Storage", 420 }, { "Structures", 409 }, { "Addons", 426 }, { "Tinker's Construct", 428 }, { "Blood Magic", 4485 }, { "Bug Fixes", 6821 }, { "Industrial Craft", 429 },
        { "Galacticraft", 5232 }, { "Farming", 416 }, { "Magic", 419 }, { "Automation", 4843 }, { "Applied Energistics 2", 4545 }, { "Twitch Integration", 4671 },
        { "CraftTweaker", 4773 }, { "Integrated Dynamics", 6954 }, { "Create", 6484 }, { "Mobs", 411 }, { "Skyblock", 6145 }, { "MCreator", 4906 }, { "Cosmetic", 424 },
        { "KubeJS", 5314 }, { "Redstone", 4558 }, { "Performance", 6814 }, { "Player Transport", 414 }, { "Biomes", 407 }, { "Ores and Resources", 408 },
        { "Energy, Fluid, and Item Transport", 415 }, { "Buildcraft", 432 }, { "Thaumcraft", 430 }, { "Thermal Expansion", 427 }, { "Dimensions", 410 }, { "Energy", 417 },
        { "Twilight Forest", 7669 }, { "Genetics", 418 }, { "Forestry", 433 }
    };
}