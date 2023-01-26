using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Models;

public class GuideNavigationMessage
{
    public bool CanNext { get; set; }

    public Type NextPage { get; set; }
}
