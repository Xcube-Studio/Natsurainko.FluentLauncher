using Natsurainko.FluentLauncher.Models;
using Natsurainko.Toolkits.Network;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Components;

public static class GlobalActivitiesCache
{
    public static List<LaunchArrangement> LaunchArrangements { get; private set; } = new List<LaunchArrangement>();

    public static List<DownloadArrangement> DownloadArrangements { get; private set; } = new List<DownloadArrangement>();
}
