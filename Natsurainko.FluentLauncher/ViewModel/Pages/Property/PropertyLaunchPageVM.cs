using Natsurainko.FluentCore.Class.Model.Launch;
using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Shared.Class.Model;
using Natsurainko.FluentLauncher.Shared.Mapping;
using ReactiveUI.Fody.Helpers;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.ViewModel.Pages.Property;

public class PropertyLaunchPageVM : ViewModelBase<Page>
{
    public PropertyLaunchPageVM(Page control) : base(control)
    {
    }

    public GameCore GameCore { get; set; }

    public CustomLaunchSetting CustomLaunchSetting { get; set; }

    [Reactive]
    public bool Enable { get; set; }

    [Reactive]
    public string GameWindowTitle { get; set; }

    [Reactive]
    public int GameWindowWidth { get; set; } = 854;

    [Reactive]
    public int GameWindowHeight { get; set; } = 480;

    [Reactive]
    public string GameServerAddress { get; set; }

    [Reactive]
    public bool EnableFullScreen { get; set; }

    [Reactive]
    public bool EnableIndependencyCore { get; set; }

    public async void Set(GameCore core)
    {
        this.GameCore = core;

        CustomLaunchSetting = await core.GetCustomLaunchSetting();

        this.Enable = CustomLaunchSetting.Enable;
        this.GameWindowTitle = CustomLaunchSetting.GameWindowTitle;
        this.GameWindowWidth = CustomLaunchSetting.GameWindowWidth;
        this.GameWindowHeight = CustomLaunchSetting.GameWindowHeight;
        this.GameServerAddress = CustomLaunchSetting.GameServerAddress;
        this.EnableFullScreen = CustomLaunchSetting.EnableFullScreen;
        this.EnableIndependencyCore = CustomLaunchSetting.EnableIndependencyCore;
    }

    public override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        _ = GameCoreLocator.SaveGameCoreCustomLaunchSetting(this.GameCore.Root.FullName, this.GameCore.Id, new()
        {
            Enable = this.Enable,
            LastLaunchTime = this.CustomLaunchSetting.LastLaunchTime,
            GameWindowTitle = this.GameWindowTitle,
            GameWindowWidth = this.GameWindowWidth,
            GameWindowHeight = this.GameWindowHeight,
            GameServerAddress = this.GameServerAddress,
            EnableFullScreen = this.EnableFullScreen,
            EnableIndependencyCore = this.EnableIndependencyCore
        });
    }
}
