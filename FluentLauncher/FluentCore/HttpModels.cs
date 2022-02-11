using System.IO;
using System.Net;

namespace FluentCore.Model
{
    /// <summary>
    /// Http下载请求模型
    /// </summary>
    public class HttpDownloadRequest
    {
        /// <summary>
        /// 下载文件存放目录
        /// </summary>
        public DirectoryInfo Directory { get; set; }

        /// <summary>
        /// 下载文件Url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 目标文件大小(仅供验证文件)
        /// </summary>
        public int? Size { get; set; }

        /// <summary>
        /// 目标文件Sha1(仅供验证文件)
        /// </summary>
        public string Sha1 { get; set; }
    }

    /// <summary>
    /// Http下载返回模型
    /// </summary>
    public class HttpDownloadResponse
    {
        /// <summary>
        /// Http返回
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Http状态码
        /// </summary>
        public HttpStatusCode HttpStatusCode { get; set; }

        /// <summary>
        /// 下载实际文件
        /// </summary>
        public FileInfo FileInfo { get; set; }
    }
}
