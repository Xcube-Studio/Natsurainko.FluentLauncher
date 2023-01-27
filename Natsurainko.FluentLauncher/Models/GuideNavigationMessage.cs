using System;

namespace Natsurainko.FluentLauncher.Models;

public class GuideNavigationMessage
{
    public bool CanNext { get; set; }

    public Type NextPage { get; set; }
}
