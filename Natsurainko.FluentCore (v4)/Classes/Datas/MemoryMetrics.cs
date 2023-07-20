using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nrk.FluentCore.Classes.Datas;

public record MemoryMetrics
{
    public double Total { get; set; }

    public double Used { get; set; }

    public double Free { get; set; }
}
