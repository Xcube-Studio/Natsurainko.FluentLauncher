using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Classes.Data.Launch;
using Natsurainko.FluentLauncher.Components.Launch;
using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Classes.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Natsurainko.FluentLauncher.ViewModels.Pages;

internal partial class LoggerViewModel : ObservableObject
{
    private readonly ObservableCollection<GameLoggerOutput> _gameLoggerOutputs;

    public LoggerViewModel(LaunchProcess launchProcess, Views.LoggerPage view)
    {
        _gameLoggerOutputs = launchProcess._gameLoggerOutputs;
        _gameLoggerOutputs.CollectionChanged += LoggerItems_CollectionChanged;

        View = view;

        if (!launchProcess.McProcess.HasExited)
            View.Unloaded += (_, e) => _gameLoggerOutputs.CollectionChanged -= LoggerItems_CollectionChanged;

        OnPropertyChanged();
    }

    [ObservableProperty]
    private bool info = true;

    [ObservableProperty]
    private bool warn = true;

    [ObservableProperty]
    private bool error = true;

    [ObservableProperty]
    private bool fatal = true;

    [ObservableProperty]
    private bool debug = true;

    [ObservableProperty]
    private bool enableAutoScroll = true;

    public ObservableCollection<LoggerItem> FilterLoggerItems { get; } = new();

    public Views.LoggerPage View { get; set; }

    public string Title { get; set; }

    private void LoggerItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        var enums = new List<GameLoggerOutputLevel>();

        if (Info)
            enums.Add(GameLoggerOutputLevel.Info);

        if (Warn)
            enums.Add(GameLoggerOutputLevel.Warn);

        if (Error)
            enums.Add(GameLoggerOutputLevel.Error);

        if (Fatal)
            enums.Add(GameLoggerOutputLevel.Fatal);

        if (Debug)
            enums.Add(GameLoggerOutputLevel.Debug);

        foreach (var item in e.NewItems)
            if (item is GameLoggerOutput GameLoggerOutput && enums.Contains(GameLoggerOutput.Level))
                App.DispatcherQueue.TryEnqueue(() => FilterLoggerItems.Add(new LoggerItem(GameLoggerOutput)));

        if (EnableAutoScroll)
            App.DispatcherQueue.TryEnqueue(ScrollToEnd);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName != nameof(FilterLoggerItems))
        {
            var enums = new List<GameLoggerOutputLevel>();

            if (Info)
                enums.Add(GameLoggerOutputLevel.Info);

            if (Warn)
                enums.Add(GameLoggerOutputLevel.Warn);

            if (Error)
                enums.Add(GameLoggerOutputLevel.Error);

            if (Fatal)
                enums.Add(GameLoggerOutputLevel.Fatal);

            if (Debug)
                enums.Add(GameLoggerOutputLevel.Debug);

            FilterLoggerItems.Clear();

            foreach (var item in _gameLoggerOutputs)
                if (enums.Contains(item.Level))
                    FilterLoggerItems.Add(new LoggerItem(item));
        }

        if (EnableAutoScroll)
            ScrollToEnd();
    }

    private void ScrollToEnd()
    {
        View.ListView.SelectedIndex = View.ListView.Items.Count - 1;

        try { View.ListView.ScrollIntoView(View.ListView.Items.Last()); } catch { }
    }
}
