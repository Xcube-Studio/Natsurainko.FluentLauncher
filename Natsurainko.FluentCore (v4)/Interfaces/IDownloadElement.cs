using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nrk.FluentCore.Interfaces;

public interface IDownloadElement
{
    public string AbsolutePath { get; set; }

    public string Url { get; set; }

    public string Checksum { get; set; }
}
