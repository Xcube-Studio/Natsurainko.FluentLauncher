using Microsoft.Extensions.DependencyInjection;
using Natsurainko.FluentLauncher.ViewModels.Pages.Activities;
using Natsurainko.FluentLauncher.ViewModels.Pages.Mods;

namespace Natsurainko.FluentLauncher.Components.Mvvm;

internal class ViewModelLocator
{
    public ActivitiesLayer Activities { get; } = new();

    public ModsLayer Mods { get; } = new();

    public class ActivitiesLayer
    {
        public News News => App.Services.GetService<News>();
    }

    public class ModsLayer
    {
        public CurseForge CurseForge => App.Services.GetService<CurseForge>();
    }
}
