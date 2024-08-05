#nullable disable
namespace Natsurainko.FluentLauncher.Models.UI;

internal class SearchOptions
{
    public int ResourceType { get; set; } = 0;

    public int ResourceSource { get; set; } = 0;

    public string SearchText { get; set; } = string.Empty;
}
