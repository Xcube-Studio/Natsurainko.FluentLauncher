using System.IO;
using System.Net;

namespace Natsurainko.Toolkits.Network.Model
{
    public class HttpDownloadRequest
    {
        public DirectoryInfo Directory { get; set; }

        public string Url { get; set; }

        public int? Size { get; set; }

        public string Sha1 { get; set; }

        public string FileName { get; set; }
    }

    public class HttpDownloadResponse
    {
        public string Message { get; set; }

        public HttpStatusCode HttpStatusCode { get; set; }

        public FileInfo FileInfo { get; set; }
    }
}
