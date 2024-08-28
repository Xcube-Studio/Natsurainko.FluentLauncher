using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Nrk.FluentCore.Experimental.GameManagement;
using Nrk.FluentCore.Experimental.GameManagement.Installer.Data;
using Nrk.FluentCore.Experimental.GameManagement.Instances;
using System;
using System.Collections.Generic;

#nullable disable
namespace Natsurainko.FluentLauncher.Services.UI;

internal partial class SearchProviderService : ObservableObject
{
    private AutoSuggestBox _autoSuggestBox;

    private Dictionary<object, Func<string, IEnumerable<Suggestion>>> SuggestionProviders = new Dictionary<object, Func<string, IEnumerable<Suggestion>>>();

    public object QueryReceiverOwner { get; private set; }
    private Action<string> QueryReceiver;

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
            suggestion.InvokeAction();
        }
    }

    private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.ChosenSuggestion is Suggestion suggestion)
        {
            return;
        }

        if (QueryReceiverOwner != null)
            QueryReceiver(sender.Text);
    }

    #endregion

    public void RegisterSuggestionProvider<TProvider>(TProvider provider, Func<string, IEnumerable<Suggestion>> func)
    {
        SuggestionProviders.Add(provider, func);
    }

    public void UnregisterSuggestionProvider<TProvider>(TProvider provider)
    {
        SuggestionProviders.Remove(provider);
    }

    public bool ContainsSuggestionProvider<TProvider>(TProvider provider)
        => SuggestionProviders.ContainsKey(typeof(TProvider));

    public void OccupyQueryReceiver<TReceiver>(TReceiver provider, Action<string> action)
    {
        QueryReceiverOwner = provider;
        QueryReceiver = action;
    }

    public class Suggestion
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public SuggestionIconType SuggestionIconType { get; set; } = SuggestionIconType.Glyph;

        public string Icon { get; set; } = "\ue721";

        public Action InvokeAction { get; set; }

        public object Parameter { get; set; }
    }

    public enum SuggestionIconType
    {
        Glyph = 0,
        UriIcon = 1,
        WebUrlIcon = 2
    }
}

internal static class SuggestionHelper
{
    public static SearchProviderService.Suggestion FromMinecraftInstance(MinecraftInstance MinecraftInstance, string description, Action action)
    {
        return new SearchProviderService.Suggestion
        {
            Title = MinecraftInstance.InstanceId,
            Description = description,
            SuggestionIconType = SearchProviderService.SuggestionIconType.UriIcon,
            Icon = string.Format("ms-appx:///Assets/Icons/{0}.png", MinecraftInstance.Version.Type switch
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

    public static SearchProviderService.Suggestion FromVersionManifestItem(VersionManifestItem item, string description, Action action)
    {
        return new SearchProviderService.Suggestion
        {
            Title = item.Id,
            Description = description,
            SuggestionIconType = SearchProviderService.SuggestionIconType.UriIcon,
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
}