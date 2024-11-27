using Natsurainko.FluentLauncher.Models.Launch;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.Launch.Data;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.GameManagement.Installer;
using System.Text.Json.Serialization;

namespace Natsurainko.FluentLauncher;

// News
[JsonSerializable(typeof(NewsData))]
[JsonSerializable(typeof(PatchNoteData))]

// Game management
[JsonSerializable(typeof(ForgeInstallData[]))]
[JsonSerializable(typeof(OptiFineInstallData[]))]
[JsonSerializable(typeof(FabricInstallData[]))]
[JsonSerializable(typeof(QuiltInstallData[]))]
[JsonSerializable(typeof(VersionManifestJsonObject))]

// Accounts
[JsonSerializable(typeof(Account[]))]
[JsonSerializable(typeof(OfflineAccount))]
[JsonSerializable(typeof(MicrosoftAccount))]
[JsonSerializable(typeof(YggdrasilAccount))]

// Settings
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(uint))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(float))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(Windows.UI.Color))]
[JsonSerializable(typeof(WinUIEx.WindowState))]
internal partial class FLSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(InstanceConfig))]
[JsonSourceGenerationOptions(
    IncludeFields = false,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class InstanceConfigSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(QuickLaunchArgumentsData))]
[JsonSourceGenerationOptions(
    IncludeFields = false,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class QuickLaunchArgumentsDataSerializerContext : JsonSerializerContext { }