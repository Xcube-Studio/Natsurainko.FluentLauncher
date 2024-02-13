using Natsurainko.FluentLauncher.Services.Launch;
using Nrk.FluentCore.Launch;
using Nrk.FluentCore.Utils;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.UI.StartScreen;

namespace Natsurainko.FluentLauncher.Services.SystemServices;

internal class JumpListService
{
    private readonly LaunchService _launchService;

    public JumpListService(LaunchService launchService)
    {
        _launchService = launchService;
    }

    private static async Task AddItem(GameInfo gameInfo)
    {
        var itemArguments = JsonSerializer.Serialize(gameInfo).ConvertToBase64();
        var jumpListItem = JumpListItem.CreateWithArguments($"/quick-launch {itemArguments}", gameInfo.Name);

        jumpListItem.GroupName = "Latest";
        jumpListItem.Logo = new Uri(string.Format("ms-appx:///Assets/Icons/{0}.png", !gameInfo.IsVanilla ? "furnace_front" : gameInfo.Type switch
        {
            "release" => "grass_block_side",
            "snapshot" => "crafting_table_front",
            "old_beta" => "dirt_path_side",
            "old_alpha" => "dirt_path_side",
            _ => "grass_block_side"
        }), UriKind.RelativeOrAbsolute);

        var list = await JumpList.LoadCurrentAsync();
        list.Items.Add(jumpListItem);

        await list.SaveAsync();
    }

    private static async Task RemoveItem(GameInfo gameInfo)
    {
        var list = await JumpList.LoadCurrentAsync();
        var jumpListItem = list.Items.Where(item =>
            gameInfo == JsonSerializer.Deserialize<GameInfo>
                (item.Arguments.Replace("/quick-launch ", string.Empty).ConvertFromBase64())).FirstOrDefault();

        if (jumpListItem != null)
            list.Items.Remove(jumpListItem);

        await list.SaveAsync();
    }

    private static async Task MoveToFirst(GameInfo gameInfo)
    {
        var list = await JumpList.LoadCurrentAsync();
        var jumpListItem = list.Items.Where(item =>
            gameInfo == JsonSerializer.Deserialize<GameInfo>
                (item.Arguments.Replace("/quick-launch ", string.Empty).ConvertFromBase64())).FirstOrDefault();

        if (jumpListItem != null)
        {
            list.Items.Remove(jumpListItem);
            list.Items.Insert(0, jumpListItem);
        }

        await list.SaveAsync();
    }

    public void LaunchFromJumpList(string arguments)
    {
        //TODO: Implement launch from jump list
        var _gameInfo = JsonSerializer.Deserialize<GameInfo>(arguments.Replace("/quick-launch ", string.Empty).ConvertFromBase64());

        //new ToastContentBuilder()
        //    .AddHeader(_gameInfo.Name, $"正在尝试启动游戏: {_gameInfo.Name}", string.Empty)
        //    .AddText("这可能需要一点时间，请稍后")
        //    .Show();

        //var process = CreateLaunchSession(_gameInfo);
        //_launchProcesses.Insert(0, process);

        //Task.Run(process.Start);

        //process.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
        //{
        //    if (e.PropertyName == "DisplayState")
        //    {
        //        if (!(process.State == MinecraftSessionState.GameRunning ||
        //            process.State == MinecraftSessionState.Faulted ||
        //            process.State == MinecraftSessionState.GameExited ||
        //            process.State == MinecraftSessionState.GameCrashed))
        //            return;

        //        var builder = new ToastContentBuilder();

        //        switch (process.State)
        //        {
        //            case MinecraftSessionState.GameRunning:
        //                builder.AddHeader(Guid.NewGuid().ToString(), $"启动游戏成功: {_gameInfo.Name}", string.Empty);
        //                break;
        //            case MinecraftSessionState.Faulted:
        //                builder.AddHeader(Guid.NewGuid().ToString(), $"启动游戏失败: {_gameInfo.Name}", string.Empty);
        //                break;
        //            case MinecraftSessionState.GameExited:
        //                builder.AddHeader(Guid.NewGuid().ToString(), $"启动进程消息: {_gameInfo.Name}", string.Empty);
        //                break;
        //            case MinecraftSessionState.GameCrashed:
        //                builder.AddHeader(Guid.NewGuid().ToString(), $"启动进程消息: {_gameInfo.Name}", string.Empty);
        //                break;
        //            default:
        //                break;
        //        }

        //        builder.AddText(ResourceUtils.GetValue("Converters", $"_LaunchState_{process.State}"));
        //        builder.Show();

        //        switch (process.State)
        //        {
        //            case MinecraftSessionState.Faulted:
        //            case MinecraftSessionState.GameExited:
        //                Process.GetCurrentProcess().Kill();
        //                break;
        //            case MinecraftSessionState.GameCrashed:
        //                App.DispatcherQueue.TryEnqueue(() => process.LoggerButton());
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //};
    }

    public static void UpdateJumpList(GameInfo gameInfo)
    {

    }

}
