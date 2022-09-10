using Natsurainko.FluentCore.Class.Model.Launch;
using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Class.ViewData;
using Natsurainko.FluentLauncher.Shared.Class.Model;
using Natsurainko.FluentLauncher.Shared.Mapping;
using Natsurainko.FluentLauncher.View.Pages.Property;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;

namespace Natsurainko.FluentLauncher.ViewModel.Pages.Property;

public class PropertyModPageVM : ViewModelBase<PropertyModPage>
{
    public PropertyModPageVM(PropertyModPage control) : base(control)
    {
        Mods = new();
    }

    public GameCore GameCore { get; set; }

    [Reactive]
    public ObservableCollection<LocalModInformationViewData> Mods { get; set; }

    [Reactive]
    public Visibility TipsVisibility { get; set; } = Visibility.Collapsed;

    [Reactive]
    public Visibility ModTipsVisibility { get; set; } = Visibility.Collapsed;

    public async void Set(GameCore core)
    {
        GameCore = core;
        var setting = await core.GetCustomLaunchSetting();

        Mods.Clear();
        (await LocalModManager.GetModsOfGameCore(GameCore.Root.FullName, GameCore.Id, GameCore.GetEnableIndependencyCore(setting)))
            .CreateCollectionViewData<LocalModInformation, LocalModInformationViewData>()
            .ToList()
            .ForEach(x => Mods.Add(x));

        if (Mods.Any())
        {
            ModTipsVisibility = GameCore.GetEnableIndependencyCore(setting)
                ? Visibility.Collapsed
                : Visibility.Visible;

            TipsVisibility = Visibility.Collapsed;
        }
        else TipsVisibility = Visibility.Visible;

        Control.ToggledEnable = true;
    }

    public void UpdateMods()
    {
        DispatcherHelper.RunAsync(async () =>
        {
            var customLaunchSetting = await GameCore.GetCustomLaunchSetting();
            var enableIndependencyCore = GameCore.GetEnableIndependencyCore(customLaunchSetting);

            Mods.Clear();
            (await LocalModManager.GetModsOfGameCore(GameCore.Root.FullName, GameCore.Id, enableIndependencyCore))
                .CreateCollectionViewData<LocalModInformation, LocalModInformationViewData>()
                .ToList()
                .ForEach(x => Mods.Add(x));

            if (Mods != null)
            {
                ModTipsVisibility = enableIndependencyCore
                    ? Visibility.Collapsed
                    : Visibility.Visible;

                TipsVisibility = Visibility.Collapsed;
            }
            else TipsVisibility = Visibility.Visible;
        });
    }
}
