using Natsurainko.FluentLauncher.Models.Launch;
using Natsurainko.FluentLauncher.Models.UI;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.GameManagement.Installer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher;

// News
[JsonSerializable(typeof(NewsData))]
[JsonSerializable(typeof(PatchNoteData))]

// Game management
[JsonSerializable(typeof(ForgeInstallData[]))]
[JsonSerializable(typeof(OptiFineInstallData[]))]
[JsonSerializable(typeof(FabricInstallData[]))]
[JsonSerializable(typeof(QuiltInstallData[]))]
[JsonSerializable(typeof(InstanceConfig))]
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
internal partial class FLSerializerContext : JsonSerializerContext
{
}