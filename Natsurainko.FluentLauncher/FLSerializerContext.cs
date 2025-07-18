using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Models.Launch;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.GameManagement.Installer;
using System.Text.Json.Serialization;
using static Nrk.FluentCore.Utils.PlayerTextureHelper;

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

// Textures
[JsonSerializable(typeof(PlayerTextureProfile))]

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
[JsonSerializable(typeof(Windows.Win32.UI.WindowsAndMessaging.WINDOWPLACEMENT))]
internal partial class FLSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(InstanceConfig))]
[JsonSourceGenerationOptions(
    IncludeFields = false,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class InstanceConfigSerializerContext : JsonSerializerContext { }