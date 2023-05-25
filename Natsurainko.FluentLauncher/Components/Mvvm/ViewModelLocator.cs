using Microsoft.Extensions.DependencyInjection;
using Natsurainko.FluentLauncher.ViewModels.Activities;
using Natsurainko.FluentLauncher.ViewModels.Downloads;

namespace Natsurainko.FluentLauncher.Components.Mvvm;

internal class ViewModelLocator
{
    public ActivitiesLayer Activities { get; } = new();

    public ModsLayer Mods { get; } = new();

    public class ActivitiesLayer
    {
        public NewsViewModel News => App.Services.GetService<NewsViewModel>();
    }

    public class ModsLayer
    {
        public CurseForgeViewModel CurseForge => App.Services.GetService<CurseForgeViewModel>();
    }
}
