using Natsurainko.FluentCore.Interface;
using System.Collections.Generic;

namespace Natsurainko.FluentCore.Class.Model.Download;

public class ResourceDownloadResponse
{
    public int Total { get; set; }

    public int SuccessCount { get; set; }

    public List<IResource> FailedResources { get; set; }
}
