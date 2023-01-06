using Natsurainko.FluentLauncher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Components;

public static class GlobalActivitiesCache
{
    public static List<LaunchArrangement> LaunchArrangements { get; private set; } = new List<LaunchArrangement>();
}
