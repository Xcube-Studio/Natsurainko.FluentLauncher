namespace Natsurainko.Toolkits.Network.Downloader.Model
{
    public interface IDownloaderProgressChangedEventArgs
    {
        public double Progress { get; }
    }

    public class SimpleDownloaderProgressChangedEventArgs : IDownloaderProgressChangedEventArgs
    {
        public double Progress => CompletedLength / (double)TotleLength;

        public long TotleLength { get; set; }

        public long CompletedLength { get; set; }
    }
}
