namespace Natsurainko.FluentLauncher.Services.Data;

internal class CurseForgeCategoryData
{
    public CurseForgeCategoryData(string name, int id, string iconUrl)
    {
        Name = name;
        Id = id;
        IconUrl = iconUrl;
    }

    public string Name { get; }

    public int Id { get; }

    public string IconUrl { get; }
}
