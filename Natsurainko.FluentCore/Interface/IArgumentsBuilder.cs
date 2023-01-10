using Natsurainko.FluentCore.Class.Model.Launch;
using System.Collections.Generic;

namespace Natsurainko.FluentCore.Interface;

public interface IArgumentsBuilder
{
    GameCore GameCore { get; }

    LaunchSetting LaunchSetting { get; }

    IEnumerable<string> Build();
}
