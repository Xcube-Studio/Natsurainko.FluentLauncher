using System;

namespace Natsurainko.FluentLauncher.Shared.Class.Model;

public class CustomLaunchSetting
{
    public bool Enable { get; set; } = false;

    public DateTime? LastLaunchTime { get; set; } = null;

    public string GameWindowTitle { get; set; } = string.Empty;

    public int GameWindowWidth { get; set; } = 854;

    public int GameWindowHeight { get; set; } = 480;

    public string GameServerAddress { get; set; } = string.Empty;

    public bool EnableFullScreen { get; set; } = false;

    public bool EnableIndependencyCore { get; set; } = false;
}
