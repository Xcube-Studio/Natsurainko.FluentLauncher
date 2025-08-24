using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.GameManagement;
using Nrk.FluentCore.GameManagement.Installer;
using Nrk.FluentCore.GameManagement.Instances;
using Nrk.FluentCore.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using StringComparer = Natsurainko.FluentLauncher.Utils.StringComparer;

namespace Natsurainko.FluentLauncher.Services.UI;

public partial class SearchProviderService : ObservableObject
{
    private AutoSuggestBox _autoSuggestBox = null!;

    private readonly Dictionary<object, Func<string, IEnumerable<Suggestion>>> SuggestionProviders = [];

    public object? QueryReceiverOwner { get; private set; }

    public Action<string>? QueryReceiver { get; private set; }

    public void BindingSearchBox(AutoSuggestBox autoSuggestBox)
    {
        _autoSuggestBox = autoSuggestBox;
        _autoSuggestBox.SuggestionChosen += AutoSuggestBox_SuggestionChosen;
        _autoSuggestBox.TextChanged += AutoSuggestBox_TextChanged;
        _autoSuggestBox.QuerySubmitted += AutoSuggestBox_QuerySubmitted;

        _autoSuggestBox.UpdateTextOnSelect = false;
    }

    #region AutoSuggestBox Events

    private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (string.IsNullOrEmpty(sender.Text))
        {
            _autoSuggestBox.ItemsSource = null;
            return;
        }

        var suggestions = new List<Suggestion>();

        foreach (var provider in SuggestionProviders.Values)
            suggestions.AddRange(provider(sender.Text));

        _autoSuggestBox.ItemsSource = suggestions;
    }

    private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if (args.SelectedItem is Suggestion suggestion)
        {
            suggestion.InvokeAction?.Invoke();
        }
    }

    private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.ChosenSuggestion is Suggestion suggestion)
        {
            return;
        }

        if (QueryReceiverOwner != null)
            QueryReceiver!(sender.Text);
    }

    #endregion

    public void RegisterSuggestionProvider<TProvider>(TProvider provider, Func<string, IEnumerable<Suggestion>> func)
        where TProvider : notnull
    {
        SuggestionProviders.Add(provider, func);
    }

    public void UnregisterSuggestionProvider<TProvider>(TProvider provider) where TProvider : notnull
    {
        SuggestionProviders.Remove(provider);
    }

    public bool ContainsSuggestionProvider<TProvider>(TProvider _) where TProvider : notnull
        => SuggestionProviders.ContainsKey(typeof(TProvider));

    public void OccupyQueryReceiver<TReceiver>(TReceiver provider, Action<string> action)
    {
        QueryReceiverOwner = provider;
        QueryReceiver = action;
    }

    public void ClearSearchBox() => _autoSuggestBox.Text = string.Empty;
}

public class Suggestion
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public SuggestionIconType SuggestionIconType { get; set; } = SuggestionIconType.Glyph;

    public string Icon { get; set; } = "\ue721";

    public Action? InvokeAction { get; set; }

    public object? Parameter { get; set; }
}

public enum SuggestionIconType
{
    Glyph = 0,
    UriIcon = 1,
    WebUrlIcon = 2
}

internal static class SuggestionHelper
{
    public static List<(string, string)>? CurseforgeModSearchSlugs;
    public static List<(string, string)>? ModrinthModSearchSlugs;

    static SuggestionHelper()
    {
        Task.Run(async () => CurseforgeModSearchSlugs = await LoadSearchSlugs("curseforge-mod-slugs.json")).Forget();
        Task.Run(async () => ModrinthModSearchSlugs = await LoadSearchSlugs("modrinth-mod-slugs.json")).Forget();
    }

    public static Suggestion FromMinecraftInstance(MinecraftInstance instance, string description, Action action)
    {
        return new Suggestion
        {
            Title = instance.InstanceId,
            Description = description,
            SuggestionIconType = SuggestionIconType.UriIcon,
            Icon = string.Format("ms-appx:///Assets/Icons/{0}.png", instance.Version.Type switch
            {
                MinecraftVersionType.Release => "grass_block_side",
                MinecraftVersionType.Snapshot => "crafting_table_front",
                MinecraftVersionType.OldBeta => "dirt_path_side",
                MinecraftVersionType.OldAlpha => "dirt_path_side",
                _ => "grass_block_side"
            }),
            InvokeAction = action
        };
    }

    public static Suggestion FromVersionManifestItem(VersionManifestItem item, string description, Action action)
    {
        return new Suggestion
        {
            Title = item.Id,
            Description = description,
            SuggestionIconType = SuggestionIconType.UriIcon,
            Icon = string.Format("ms-appx:///Assets/Icons/{0}.png", item.Type switch
            {
                "release" => "grass_block_side",
                "snapshot" => "crafting_table_front",
                "old_beta" => "dirt_path_side",
                "old_alpha" => "dirt_path_side",
                _ => "grass_block_side"
            }),
            InvokeAction = action
        };
    }

    public static IEnumerable<Suggestion> GetSearchModSuggestions(string query, int source, Action<string> action)
    {
        List<(string, string)>? slugs = source switch
        {
            0 => CurseforgeModSearchSlugs,
            1 => ModrinthModSearchSlugs,
            _ => null
        };

        if (slugs is null) return [];

        return slugs.Where(i => i.Item1.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(i => i.Item1.StartsWith(query))
            .ThenBy(i => StringComparer.LevenshteinDistance(i.Item1, query))
            .Select(i => new Suggestion
            {
                Title = i.Item1,
                Description = i.Item2,
                InvokeAction = () => action(i.Item2)
            });
    }

    private static async Task<List<(string, string)>> LoadSearchSlugs(string slugFileName)
    {
        string json = await File.ReadAllTextAsync(Path.Combine(Package.Current.InstalledLocation.Path, $"Assets\\Strings\\{slugFileName}"));

        var array = JsonNode.Parse(json)!.AsArray();
        var list = new List<(string, string)>(array.Count);

        foreach (var item in array)
        {
            list.Add((
                item!["name"]!.GetValue<string>(),
                item!["slug"]!.GetValue<string>()
            ));
        }

        return list;
    }
}