﻿using Natsurainko.FluentLauncher.Models.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.Storage;
using Nrk.FluentCore.GameManagement;
using Nrk.FluentCore.GameManagement.Installer;
using Nrk.FluentCore.GameManagement.Instances;
using Nrk.FluentCore.Management;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Natsurainko.FluentLauncher.Utils.Extensions;

internal static class MinecraftInstanceExtensions
{
    public static bool IsSupportMod(this MinecraftInstance instance)
    {
        if (instance.IsVanilla) return false;

        var loaders = instance.GetModLoaders().Select(x => x.Type).ToArray();

        if (!(loaders.Contains(ModLoaderType.Forge) ||
            loaders.Contains(ModLoaderType.Fabric) ||
            loaders.Contains(ModLoaderType.NeoForge) ||
            loaders.Contains(ModLoaderType.Quilt) ||
            loaders.Contains(ModLoaderType.LiteLoader)))
            return false;

        return true;
    }

    public static string GetGameDirectory(this MinecraftInstance instance)
    {
        var config = instance.GetConfig();

        if (config.EnableSpecialSetting)
        {
            if (config.EnableIndependencyCore)
                return Path.Combine(instance.MinecraftFolderPath, "versions", instance.InstanceId);
            else return instance.MinecraftFolderPath;
        }

        if (App.GetService<SettingsService>().EnableIndependencyCore)
            return Path.Combine(instance.MinecraftFolderPath, "versions", instance.InstanceId);

        return instance.MinecraftFolderPath;
    }

    public static string GetModsDirectory(this MinecraftInstance instance) => Path.Combine(GetGameDirectory(instance), "mods");

    public static string GetSavesDirectory(this MinecraftInstance instance) => Path.Combine(GetGameDirectory(instance), "saves");

    public static void UpdateLastLaunchTimeToNow(this MinecraftInstance instance)
    {
        var config = instance.GetConfig();

        // Update launch time
        var launchTime = DateTime.Now;
        config.LastLaunchTime = launchTime;
    }
}
