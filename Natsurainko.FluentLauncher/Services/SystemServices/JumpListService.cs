using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Natsurainko.FluentLauncher.Components.Launch;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.UI.Windows;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Activities;
using Nrk.FluentCore.Launch;
using Nrk.FluentCore.Management;
using Nrk.FluentCore.Utils;
using System;
using System.ComponentModel;
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
        var list = await JumpList.LoadCurrentAsync();
        var itemArguments = JsonSerializer.Serialize(gameInfo).ConvertToBase64();

        var jumpListItem = list.Items.Where(item =>
            gameInfo == JsonSerializer.Deserialize<GameInfo>
                (item.Arguments.Replace("/quick-launch ", string.Empty).ConvertFromBase64())).FirstOrDefault();

        if (jumpListItem != null)
        {
            list.Items.Remove(jumpListItem);
            list.Items.Insert(0, jumpListItem);
        }
        else
        {
            jumpListItem = JumpListItem.CreateWithArguments($"/quick-launch {itemArguments}", gameInfo.Name);

            jumpListItem.GroupName = "Latest";
            jumpListItem.Logo = new Uri(string.Format("ms-appx:///Assets/Icons/{0}.png", !gameInfo.IsVanilla ? "furnace_front" : gameInfo.Type switch
            {
                "release" => "grass_block_side",
                "snapshot" => "crafting_table_front",
                "old_beta" => "dirt_path_side",
                "old_alpha" => "dirt_path_side",
                _ => "grass_block_side"
            }), UriKind.RelativeOrAbsolute);

            list.Items.Add(jumpListItem);
        }

        await list.SaveAsync();
    }

    public async void LaunchFromJumpList(string arguments)
    {
        #region Init Launch & Display Elements

        var gameInfo = JsonSerializer.Deserialize<GameInfo>(arguments.Replace("/quick-launch ", string.Empty).ConvertFromBase64())
            ?? throw new InvalidOperationException();

        string name = gameInfo.Name ?? "Minecraft";
        string icon = string.Format("ms-appx:///Assets/Icons/{0}.png", !gameInfo.IsVanilla ? "furnace_front" : gameInfo.Type switch
        {
            "release" => "grass_block_side",
            "snapshot" => "crafting_table_front",
            "old_beta" => "dirt_path_side",
            "old_alpha" => "dirt_path_side",
            _ => "grass_block_side"
        });

        #endregion

        #region Init Notification

        var guid = Guid.NewGuid();

        var appNotification = new AppNotificationBuilder()
            .AddArgument("guid", guid.ToString())
            //.SetAppLogoOverride(new Uri(icon), AppNotificationImageCrop.Default)
            .AddText($"Launching Game: {name}")
            .AddText("This may take some time, please wait")
            .AddProgressBar(new AppNotificationProgressBar()
                .BindTitle()
                .BindValue()
                .BindValueStringOverride()
                .BindStatus())
            //.AddButton(new AppNotificationButton("Open Launcher")
            //    .AddArgument("action", "OpenApp"))
            .BuildNotification();

        appNotification.Tag = guid.ToString();
        appNotification.Group = guid.ToString();
        /*
        void NotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
        {
            if (args.Arguments["guid"] != guid.ToString())
                return;

            if (args.Arguments["action"] == "OpenApp")
            {
                try { App.GetService<IActivationService>().ActivateWindow("MainWindow"); }
                catch (Exception e) { App.ProcessException(e); }
            }

            AppNotificationManager.Default.NotificationInvoked -= NotificationInvoked;
        }

        AppNotificationManager.Default.NotificationInvoked += NotificationInvoked;*/
        AppNotificationManager.Default.Show(appNotification);

        #endregion

        #region Create Launch Session

        var minecraftSession = _launchService.CreateMinecraftSessionFromGameInfo(gameInfo, null);
        var sessionViewModel = new LaunchSessionViewModel(minecraftSession);
        App.GetService<LaunchSessions>().SessionViewModels.Insert(0, sessionViewModel);

        gameInfo.UpdateLastLaunchTimeToNow();
        UpdateJumpList(gameInfo);

        minecraftSession.StateChanged += MinecraftSession_StateChanged;

        void MinecraftSession_StateChanged(object? sender, MinecraftSessionStateChagnedEventArgs e)
        {
            switch (e.NewState)
            {
                case MinecraftSessionState.GameExited:
                    Task.Delay(1500).ContinueWith(task => System.Diagnostics.Process.GetCurrentProcess().Kill());
                    break;
                case MinecraftSessionState.Faulted:
                case MinecraftSessionState.GameCrashed:
                    try 
                    { 
                        App.DispatcherQueue.TryEnqueue(() =>
                        {
                            App.GetService<IActivationService>().ActivateWindow("MainWindow");
                            App.MainWindow.NavigateToLaunchTasksPage();
                        }); 
                    }
                    catch (Exception ex) { App.ProcessException(ex); }
                    break;
            }
        }

        #endregion

        #region Progress Update

        uint sequence = 1;
        sessionViewModel.PropertyChanged += SessionViewModel_PropertyChanged;

        void SessionViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            //if (e.PropertyName != "Progress" && e.PropertyName != "SessionState")
            //    return;

            var data = new AppNotificationProgressData(sequence);
            data.Title = gameInfo.Name;
            data.Value = sessionViewModel.Progress;
            data.ValueStringOverride = sessionViewModel.ProgressText;
            data.Status = ResourceUtils.GetValue("Converters", $"_LaunchState_{sessionViewModel.SessionState}");

            appNotification.Progress = data;
            var result = AppNotificationManager.Default.UpdateAsync(data, guid.ToString(), guid.ToString())
                .GetAwaiter().GetResult();

            sequence++;
        }

        #endregion

        await minecraftSession.StartAsync();
    }

    public async void UpdateJumpList(GameInfo gameInfo)
    {
        await AddItem(gameInfo);

        var list = await JumpList.LoadCurrentAsync();

        if (list.Items.Count > 10)
            list.Items.Remove(list.Items.Last());
    }
}
