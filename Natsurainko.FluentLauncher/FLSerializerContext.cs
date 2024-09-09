using Nrk.FluentCore.GameManagement.Installer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher;

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