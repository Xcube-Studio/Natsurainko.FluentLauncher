using Natsurainko.FluentLauncher.Models;
using System.Collections.Generic;

namespace Natsurainko.FluentLauncher.Components;

static class GlobalActivitiesCache
{
    public static List<LaunchArrangement> LaunchArrangements { get; private set; } = new List<LaunchArrangement>();

    public static List<DownloadArrangement> DownloadArrangements { get; private set; } = new List<DownloadArrangement>();
}
