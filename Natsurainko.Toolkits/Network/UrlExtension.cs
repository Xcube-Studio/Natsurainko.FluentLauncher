namespace Natsurainko.Toolkits.Network
{
    public static class UrlExtension
    {
        public static string Combine(params string[] paths)
            => string.Join("/", paths);
    }
}
