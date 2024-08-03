using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.Services.UI;

internal partial class SearchProviderService : ObservableObject
{
    private AutoSuggestBox _autoSuggestBox;

    private Dictionary<Type, Func<string, IEnumerable<Suggestion>>> SuggestionProviders = new Dictionary<Type, Func<string, IEnumerable<Suggestion>>>();

    private Type QueryReceiverOwner;
    private Action<string> QueryReceiver;

    public void BindingSearchBox(AutoSuggestBox autoSuggestBox)
    {
        _autoSuggestBox = autoSuggestBox;
        _autoSuggestBox.SuggestionChosen += AutoSuggestBox_SuggestionChosen;
        _autoSuggestBox.TextChanged += AutoSuggestBox_TextChanged;
        _autoSuggestBox.QuerySubmitted += AutoSuggestBox_QuerySubmitted;
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
            suggestion.InvokeAction(sender.Text);
    }

    private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (QueryReceiverOwner != null)
            QueryReceiver(sender.Text);
    }

    #endregion

    public void RegisterSuggestionProvider<TProvider>(TProvider provider, Func<string, IEnumerable<Suggestion>> func)
    {
        SuggestionProviders.Add(typeof(TProvider), func);
    }

    public void UnregisterSuggestionProvider<TProvider>(TProvider provider)
    {
        SuggestionProviders.Remove(typeof(TProvider));
    }

    public void RegisterQueryReceiver<TReceiver>(TReceiver provider, Action<string> action)
    {
        if (QueryReceiverOwner != null)
            throw new InvalidOperationException();

        QueryReceiverOwner = typeof(TReceiver);
        QueryReceiver = action;
    }

    public void UnregisterQueryReceiver<TReceiver>(TReceiver provider)
    {
        if (QueryReceiverOwner != typeof(TReceiver)) 
            throw new InvalidOperationException();

        QueryReceiverOwner = null;
        QueryReceiver = null;


    }

    public class Suggestion
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public SuggestionIconType SuggestionIconType { get; set; } = SuggestionIconType.Glyph;

        public string Icon { get; set; } = "\ue721";

        public Action<string> InvokeAction { get; set; }
    }

    public enum SuggestionIconType
    {
        Glyph = 0,
        UriIcon = 1,
        WebUrlIcon = 2
    }
}
