using System;

namespace Natsurainko.FluentLauncher.Services.Launch.Data;

public class QuickLaunchArgumentsData
{
    public required string MinecraftInstanceId { get; set; }

    public required string MinecraftFolder { get; set; }

    public required DateTime PinnedDate { get; set; }
}
