using FluentLauncher.Infra.UI.Windows;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.ViewModels.Tasks;
using Nrk.FluentCore.Experimental.GameManagement;
using Nrk.FluentCore.Experimental.GameManagement.Instances;
using Nrk.FluentCore.Launch;
using Nrk.FluentCore.Management;
using Nrk.FluentCore.Utils;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Windows.UI.StartScreen;

namespace Natsurainko.FluentLauncher.Services.SystemServices;

//internal class JumpListService
//{
//    private readonly LaunchService _launchService;

//    public JumpListService(LaunchService launchService)
//    {
//        _launchService = launchService;
//    }

//    #region Jumplist argument and MinecraftInstance conversion

//    private static string InstanceToJumplistArg(MinecraftInstance instance)
//    {
//        // Do not change property names (compatibility with legacy GameInfo)
//        var argJson = new JsonObject();
//        argJson["AbsoluteId"] = instance.InstanceId;
//        argJson["MinecraftFolderPath"] = instance.MinecraftFolderPath;
//        return "/quick-launch " + argJson.ToJsonString().ConvertToBase64();
//    }

//    private static (string mcFolderPath, string instanceId) ParseJumplistArg(string argument)
//    {
//        string argJson = argument["/quick-launch ".Length..];
//        argJson = argJson.ConvertFromBase64();
//        string? mcFolderPath = null, instanceId = null;
//        try
//        {
//            var argJsonNode = JsonNode.Parse(argJson)
//                ?? throw new JsonException();
//            mcFolderPath = argJsonNode["MinecraftFolderPath"]?.GetValue<string>();
//            instanceId = argJsonNode["AbsoluteId"]?.GetValue<string>();
//            if (mcFolderPath is null || instanceId is null)
//                throw new FormatException();
//        }
//        catch (Exception e) when (e is JsonException || e is FormatException)
//        {
//            throw new InvalidDataException($"Invalid jumplist argument: {argJson}");
//        }
//        return (mcFolderPath, instanceId);
//    }

//    private static MinecraftInstance JumplistArgToInstance(string argument)
//    {
//        var (mcFolderPath, instanceId) = ParseJumplistArg(argument);
//        var instanceDir = new DirectoryInfo(Path.Combine(mcFolderPath, "versions", instanceId));
//        return MinecraftInstance.Parse(instanceDir);
//    }

//    #endregion

//    private static async Task AddItem(MinecraftInstance minecraftInstance)
//    {
//        var list = await JumpList.LoadCurrentAsync();
//        var itemArguments = InstanceToJumplistArg(minecraftInstance);

//        var jumpListItem = list.Items.Where(item =>
//        {
//            var (mcFolderPath, instanceId) = ParseJumplistArg(item.Arguments);
//            return minecraftInstance.MinecraftFolderPath == mcFolderPath &&
//                minecraftInstance.InstanceId == instanceId;
//        }).FirstOrDefault();

//        if (jumpListItem != null)
//        {
//            list.Items.Remove(jumpListItem);
//            list.Items.Insert(0, jumpListItem);
//        }
//        else
//        {
//            jumpListItem = JumpListItem.CreateWithArguments(itemArguments, minecraftInstance.GetConfig().NickName);

//            jumpListItem.GroupName = "Latest";
//            jumpListItem.Logo = new Uri(string.Format("ms-appx:///Assets/Icons/{0}.png", !minecraftInstance.IsVanilla ? "furnace_front" : minecraftInstance.Version.Type switch
//            {
//                MinecraftVersionType.Release => "grass_block_side",
//                MinecraftVersionType.Snapshot => "crafting_table_front",
//                MinecraftVersionType.OldBeta => "dirt_path_side",
//                MinecraftVersionType.OldAlpha => "dirt_path_side",
//                _ => "grass_block_side"
//            }), UriKind.RelativeOrAbsolute);

//            list.Items.Add(jumpListItem);
//        }

//        await list.SaveAsync();
//    }

//    public async Task LaunchFromJumpListAsync(string arguments)
//    {
//        #region Init Launch & Display Elements

//        var minecraftInstance = JumplistArgToInstance(arguments);

//        string name = minecraftInstance.GetConfig().NickName ?? "Minecraft";
//        string icon = string.Format("ms-appx:///Assets/Icons/{0}.png", !minecraftInstance.IsVanilla ? "furnace_front" : minecraftInstance.Version.Type switch
//        {
//            MinecraftVersionType.Release => "grass_block_side",
//            MinecraftVersionType.Snapshot => "crafting_table_front",
//            MinecraftVersionType.OldBeta => "dirt_path_side",
//            MinecraftVersionType.OldAlpha => "dirt_path_side",
//            _ => "grass_block_side"
//        });

//        #endregion

//        #region Init Notification

//        var guid = Guid.NewGuid();

//        var appNotification = new AppNotificationBuilder()
//            .AddArgument("guid", guid.ToString())
//            //.SetAppLogoOverride(new Uri(icon), AppNotificationImageCrop.Default)
//            .AddText($"Launching Game: {name}")
//            .AddText("This may take some time, please wait")
//            .AddProgressBar(new AppNotificationProgressBar()
//                .BindTitle()
//                .BindValue()
//                .BindValueStringOverride()
//                .BindStatus())
//            //.AddButton(new AppNotificationButton("Open Launcher")
//            //    .AddArgument("action", "OpenApp"))
//            .BuildNotification();

//        appNotification.Tag = guid.ToString();
//        appNotification.Group = guid.ToString();
//        /*
//        void NotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
//        {
//            if (args.Arguments["guid"] != guid.ToString())
//                return;

//            if (args.Arguments["action"] == "OpenApp")
//            {
//                try { App.GetService<IActivationService>().ActivateWindow("MainWindow"); }
//                catch (Exception e) { App.ProcessException(e); }
//            }

//            AppNotificationManager.Default.NotificationInvoked -= NotificationInvoked;
//        }

//        AppNotificationManager.Default.NotificationInvoked += NotificationInvoked;*/
//        AppNotificationManager.Default.Show(appNotification);

//        #endregion

//        #region Create Launch Session

//        var viewModel = new LaunchSessionViewModel(minecraftInstance);
//        _launchService.LaunchSessions.Insert(0, viewModel);
//        await _launchService.LaunchAsync(minecraftInstance, viewModel, viewModel.LaunchCancellationToken);

//        #endregion

//        // TODO: handle progress update
//        //minecraftSession.StateChanged += MinecraftSession_StateChanged;

//        #region Progress Update

//        void MinecraftSession_StateChanged(object? sender, MinecraftSessionStateChagnedEventArgs e)
//        {
//            switch (e.NewState)
//            {
//                case MinecraftSessionState.GameExited:
//                    Task.Delay(1500).ContinueWith(task => System.Diagnostics.Process.GetCurrentProcess().Kill());
//                    break;
//                case MinecraftSessionState.Faulted:
//                case MinecraftSessionState.GameCrashed:
//                    try
//                    {
//                        App.DispatcherQueue.TryEnqueue(() =>
//                        {
//                            App.GetService<IActivationService>().ActivateWindow("MainWindow");
//                            App.MainWindow.NavigateToLaunchTasksPage();
//                        });
//                    }
//                    catch (Exception ex) { App.ProcessException(ex); }
//                    break;
//            }
//        }

//        //uint sequence = 1;
//        //sessionViewModel.PropertyChanged += SessionViewModel_PropertyChanged;

//        //void SessionViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
//        //{
//        //    //if (e.PropertyName != "Progress" && e.PropertyName != "SessionState")
//        //    //    return;

//        //    var data = new AppNotificationProgressData(sequence);
//        //    data.Title = minecraftInstance.GetConfig().NickName;
//        //    data.Value = sessionViewModel.Progress;
//        //    data.ValueStringOverride = sessionViewModel.ProgressText;
//        //    data.Status = ResourceUtils.GetValue("Converters", $"_LaunchState_{sessionViewModel.SessionState}");

//        //    appNotification.Progress = data;
//        //    var result = AppNotificationManager.Default.UpdateAsync(data, guid.ToString(), guid.ToString())
//        //        .GetAwaiter().GetResult();

//        //    sequence++;
//        //}

//        #endregion
//    }

//    public static async Task UpdateJumpListAsync(MinecraftInstance MinecraftInstance)
//    {
//        await AddItem(MinecraftInstance);

//        var list = await JumpList.LoadCurrentAsync();

//        if (list.Items.Count > 10)
//            list.Items.Remove(list.Items.Last());
//    }
//}
