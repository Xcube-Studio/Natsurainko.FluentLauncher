using Natsurainko.FluentCore.Class.Model.Launch;
using Natsurainko.FluentLauncher.Shared.Class.Model;
using Natsurainko.FluentLauncher.Shared.Desktop;
using Natsurainko.Toolkits.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#if WINDOWS_UWP

using Natsurainko.FluentLauncher.Class.AppData;
using Natsurainko.FluentLauncher.Class.Component;

#endif

namespace Natsurainko.FluentLauncher.Shared.Mapping;

public static class GameCoreLocator
{
#if WINDOWS_UWP
    public static async Task<List<GameCore>> GetGameCores(string folder)
    {
        var builder = MethodRequestBuilder.Create()
            .AddParameter(folder)
            .SetMethod("GetGameCores");

        return (await DesktopServiceManager.Service.SendAsync<List<GameCore>>(builder.Build())).Response;
    }

    public static async Task<GameCore> GetGameCore(string folder, string id)
    {
        foreach (var gameCore in await GetGameCores(folder))
            if (gameCore.Id.Equals(id))
                return gameCore;

        return null;
    }

    public static async Task<Dictionary<string, DateTime?>> GetGameCoresLastLaunchTime(string folder)
    {
        var builder = MethodRequestBuilder.Create()
            .AddParameter(folder)
            .SetMethod("GetGameCoresLastLaunchTime");

        return (await DesktopServiceManager.Service.SendAsync<Dictionary<string, DateTime?>>(builder.Build())).Response;
    }

    public static async Task<DateTime?> GetGameCoreLastLaunchTime(string folder, string id)
    {
        var builder = MethodRequestBuilder.Create()
            .AddParameter(folder)
            .SetMethod("GetGameCoresLastLaunchTime");

        return (await DesktopServiceManager.Service.SendAsync<Dictionary<string, DateTime?>>(builder.Build())).Response.First(x => x.Key.Equals(id)).Value;
    }

    public static async Task<GameCoreInformation> GetGameCoreInformation(string folder, string id)
    {
        var builder = MethodRequestBuilder.Create()
            .AddParameter(folder)
            .AddParameter(id)
            .SetMethod("GetGameCoreInformation");

        return (await DesktopServiceManager.Service.SendAsync<GameCoreInformation>(builder.Build())).Response;
    }

    public static async Task<CustomLaunchSetting> GetGameCoreCustomLaunchSetting(string folder, string id)
    {
        var builder = MethodRequestBuilder.Create()
            .AddParameter(folder)
            .AddParameter(id)
            .SetMethod("GetGameCoreCustomLaunchSetting");

        return (await DesktopServiceManager.Service.SendAsync<CustomLaunchSetting>(builder.Build())).Response;
    }

    public static async Task SaveGameCoreCustomLaunchSetting(string folder, string id, CustomLaunchSetting customLaunchSetting)
    {
        var builder = MethodRequestBuilder.Create()
            .AddParameter(folder)
            .AddParameter(id)
            .AddParameter(customLaunchSetting)
            .SetMethod("SaveGameCoreCustomLaunchSetting");

        await DesktopServiceManager.Service.SendAsyncWithoutResponse(builder.Build());
    }

#endif

#if NETCOREAPP
    public static List<GameCore> GetGameCores(string folder)
        => new Natsurainko.FluentCore.Module.Launcher.GameCoreLocator(folder).GetGameCores().ToList();

    public static Dictionary<string, DateTime?> GetGameCoresLastLaunchTime(string folder)
    {
        return new Natsurainko.FluentCore.Module.Launcher.GameCoreLocator(folder).GetGameCores().ToDictionary(x => x.Id, x =>
        {
            var customSettingFile = new FileInfo(Path.Combine(folder, "versions", x.Id, "fluentlauncher.json"));

            if (!customSettingFile.Exists)
                return DateTime.MinValue;

            var jobject = JObject.Parse(File.ReadAllText(customSettingFile.FullName));
            return (DateTime?)(jobject.ContainsKey("LastLaunchTime") ? DateTime.Parse(jobject["LastLaunchTime"].ToString()) : null);
        });
    }

    public static object GetGameCoreInformation(string folder, string id)
        => GameCoreInformation.CreateFromGameCore(new Natsurainko.FluentCore.Module.Launcher.GameCoreLocator(folder).GetGameCore(id)).GetAwaiter().GetResult();

    public static CustomLaunchSetting GetGameCoreCustomLaunchSetting(string folder, string id)
    {
        var core = new Natsurainko.FluentCore.Module.Launcher.GameCoreLocator(folder).GetGameCore(id);

        var customSettingFile = new FileInfo(Path.Combine(core.Root.FullName, "versions", core.Id, "fluentlauncher.json"));

        if (!customSettingFile.Exists)
            return new CustomLaunchSetting();
        else return JsonConvert.DeserializeObject<CustomLaunchSetting>(File.ReadAllText(customSettingFile.FullName));
    }

    public static void SaveGameCoreCustomLaunchSetting(string folder, string id, CustomLaunchSetting customLaunchSetting)
    {
        var core = new Natsurainko.FluentCore.Module.Launcher.GameCoreLocator(folder).GetGameCore(id);

        var customSettingFile = new FileInfo(Path.Combine(core.Root.FullName, "versions", core.Id, "fluentlauncher.json"));
        if (!customSettingFile.Directory.Exists)
            customSettingFile.Directory.Create();

        File.WriteAllText(customSettingFile.FullName, customLaunchSetting.ToJson());
    }
#endif
}
