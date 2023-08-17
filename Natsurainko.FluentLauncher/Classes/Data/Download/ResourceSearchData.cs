namespace Natsurainko.FluentLauncher.Classes.Data.Download;

internal record ResourceSearchData
{
    public string SearchInput { get; set; }

    public int ResourceType { get; set; }

    public string Version { get; set; }

    public int Source { get; set; }
}
