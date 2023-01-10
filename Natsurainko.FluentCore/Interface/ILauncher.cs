using Natsurainko.FluentCore.Class.Model.Launch;
using Natsurainko.FluentCore.Event;
using Natsurainko.FluentCore.Module.Launcher;
using System;
using System.Threading.Tasks;

namespace Natsurainko.FluentCore.Interface;

public interface ILauncher
{
    LaunchSetting LaunchSetting { get; }

    ArgumentsBuilder ArgumentsBuilder { get; }

    IAuthenticator Authenticator { get; set; }

    IGameCoreLocator GameCoreLocator { get; set; }

    IResourceDownloader ResourceDownloader { get; set; }

    Task<LaunchResponse> LaunchMinecraftAsync(string id, Action<LaunchProgressChangedEventArgs> action);

    LaunchResponse LaunchMinecraft(string id, Action<LaunchProgressChangedEventArgs> action);
}
