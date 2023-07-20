using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nrk.FluentCore.Classes.Datas;

public record JavaInfo
{
    public Version Version { get; set; }

    public string Company { get; set; }

    public string ProductName { get; set; }

    public string Architecture { get; set; }

    public string Name { get; set; }
}
