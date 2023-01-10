using System.Collections.Generic;

namespace Natsurainko.Toolkits.Network.Model
{
    public class MultithreadedDownloadResponse
    {
        public Dictionary<HttpDownloadRequest, HttpDownloadResponse> FailedDownloadRequests { get; set; }

        public bool IsAllSuccess { get; set; }
    }
}
