using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Nrk.FluentCore.GameManagement;
using Nrk.FluentCore.GameManagement.Instances;
using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI.StartScreen;

namespace Natsurainko.FluentLauncher.Services.Launch;

internal class QuickLaunchService
{
    private readonly LaunchService _launchService;
    private readonly SettingsService _settingsService;
    private readonly NotificationService _notificationService;

    public const string PinnedUri = "ms-resource:///Resources/JumpList__Pinned";
    public const string LatestUri = "ms-resource:///Resources/JumpList__Latest";

    public QuickLaunchService(LaunchService launchService, SettingsService settingsService, NotificationService notificationService)
    {
        _launchService = launchService;
        _settingsService = settingsService;
        _notificationService = notificationService;
    }

    public void LaunchFromActivatedEventArgs(string[] args)
    {
        var parseResult = Program.BuildSubCommand().Parse(args);

        string? minecraftFolder = parseResult.GetValueForOption(Program.MinecraftFolderOption);
        string? instanceId = parseResult.GetValueForOption(Program.InstanceIdOption);

        if (minecraftFolder == null || instanceId == null)
            return;

        try
        {
            MinecraftInstanceParser minecraftInstanceParser = new(minecraftFolder);
            MinecraftInstance instance = minecraftInstanceParser.ParseAllInstances()
                .FirstOrDefault(x => (x?.InstanceId.Equals(instanceId)).GetValueOrDefault(false), null)
                ?? throw new Exception("The target Minecraft instance could not be found");

            _launchService.LaunchFromUI(instance);
        }
        catch (Exception ex)
        {
            _notificationService.NotifyWithoutContent(ex.Message);
        }
    }

    public async Task LaunchFromArguments(string minecraftFolder, string instanceId)
    {
        try
        {
            var appInstance = AppInstance.GetCurrent();
            var appActivationArguments = appInstance.GetActivatedEventArgs();
            var mainInstance = AppInstance.FindOrRegisterForKey("Main");

            if (!mainInstance.IsCurrent)
            {
                await mainInstance.RedirectActivationToAsync(appActivationArguments);
                return;
            }

            MinecraftInstanceParser minecraftInstanceParser = new(minecraftFolder);
            MinecraftInstance instance = minecraftInstanceParser.ParseAllInstances()
                .FirstOrDefault(x => (x?.InstanceId.Equals(instanceId)).GetValueOrDefault(false), null)
                ?? throw new Exception("The target Minecraft instance could not be found");

            QuickLaunchProgressViewModel progressViewModel = new(instance);
            AppNotificationManager.Default.Show(progressViewModel.AppNotification);

            using var process = await _launchService.LaunchAsync(instance, progress: progressViewModel);
            await process.Process.WaitForExitAsync();
            await AppNotificationManager.Default.RemoveAllAsync();

            if (process.Process.ExitCode != 0)
            {
                AppNotificationManager.Default.Show(new AppNotificationBuilder()
                    .AddText($"Minecraft: {instance.GetDisplayName} Crashed")
                    .AddText("Quick Launch cannot provide further error information")
                    .BuildNotification());
            }
        }
        catch (Exception ex)
        {
            var title = "Quick Launch Failed";
            var content = $"An exception occurred during the quick start process\r\n{ex}";

            MessageBox.Show(content, title, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            await AppNotificationManager.Default.RemoveAllAsync();
        }
    }

    public async Task AddLatestMinecraftInstance(MinecraftInstance instance)
    {
        JumpList jumpList = await JumpList.LoadCurrentAsync();
        var item = JumpListItem.CreateWithArguments(GenerateCommandLineArguments(instance), instance.GetDisplayName());
        item.GroupName = LatestUri;
        item.Logo = GetItemIcon(instance);

        if (IsExisted(jumpList, instance, out var existedItem))
            jumpList.Items.Remove(existedItem);

        GetStartIndexOfGroups(jumpList, out var pinStartIndex, out var latestStartIndex);

        if (latestStartIndex != -1)
            jumpList.Items.Insert(latestStartIndex, item);
        else jumpList.Items.Add(item);

        GetStartIndexOfGroups(jumpList, out pinStartIndex, out latestStartIndex);

        if (jumpList.Items.Count - latestStartIndex > _settingsService.MaxQuickLaunchLatestItem)
        {
            jumpList.Items.Skip(_settingsService.MaxQuickLaunchLatestItem)
                .ToList()
                .ForEach(item => jumpList.Items.Remove(item));
        }

        await jumpList.SaveAsync();
    }

    public async Task AddPinMinecraftInstance(MinecraftInstance instance)
    {
        JumpList jumpList = await JumpList.LoadCurrentAsync();
        var item = JumpListItem.CreateWithArguments(GenerateCommandLineArguments(instance), instance.GetDisplayName());
        item.GroupName = PinnedUri;
        item.Logo = GetItemIcon(instance);

        if (IsExisted(jumpList, instance, out var existedItem, PinnedUri))
            jumpList.Items.Remove(existedItem);

        GetStartIndexOfGroups(jumpList, out var pinStartIndex, out _);
        jumpList.Items.Insert(pinStartIndex == -1 ? 0 : pinStartIndex, item);

        await jumpList.SaveAsync();
    }

    public bool IsExisted(JumpList jumpList, MinecraftInstance instance, out JumpListItem? jumpListItem, string groupName = LatestUri)
    {
        string args = GenerateCommandLineArguments(instance);
        jumpListItem = null;

        foreach (var item in jumpList.Items)
        {
            if (item.Arguments == args && item.GroupName == groupName)
            {
                jumpListItem = item;
                return true;
            }
        }

        return false;
    }

    public async void CleanRemovedJumpListItem()
    {
        try
        {
            JumpList jumpList = await JumpList.LoadCurrentAsync();

            jumpList.Items.Where(x => x.RemovedByUser)
                .ToList()
                .ForEach(x => jumpList.Items.Remove(x));

            await jumpList.SaveAsync();
        }
        catch (Exception)
        {
            // Write into logs
        }
    }

    private static string GenerateCommandLineArguments(MinecraftInstance instance)
    {
        List<string> argumentsList =
        [
            "quickLaunch",
            "--minecraftFolder",
            instance.MinecraftFolderPath.ToPathParameter(),
            "--instanceId",
            instance.InstanceId.ToPathParameter()
        ];

        return string.Join(" ", argumentsList);
    }

    private static Uri GetItemIcon(MinecraftInstance instance) =>
        new(string.Format("ms-appx:///Assets/Icons/{0}.png", !instance.IsVanilla ? "furnace_front" : instance.Version.Type switch
        {
            MinecraftVersionType.Release => "grass_block_side",
            MinecraftVersionType.Snapshot => "crafting_table_front",
            MinecraftVersionType.OldBeta => "dirt_path_side",
            MinecraftVersionType.OldAlpha => "dirt_path_side",
            _ => "grass_block_side"
        }), UriKind.RelativeOrAbsolute);

    private static void GetStartIndexOfGroups(JumpList jumpList, out int pinStartIndex, out int latestStartIndex)
    {
        pinStartIndex = -1;
        latestStartIndex = -1;

        for (int i = 0; i < jumpList.Items.Count; i++)
        {
            if (jumpList.Items[i].GroupName == PinnedUri && pinStartIndex == -1)
                pinStartIndex = i;
            else if (jumpList.Items[i].GroupName == LatestUri && latestStartIndex == -1)
                latestStartIndex = i;
        }
    }
}