using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentCore.Model.Launch;
using Natsurainko.FluentLauncher.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Natsurainko.FluentLauncher.ViewModels.Pages;

public partial class LoggerViewModel : ObservableObject
{
    public LoggerViewModel(List<GameProcessOutput> processOutputs, LaunchResponse launchResponse, Views.LoggerPage view)
    {
        View = view;

        LoggerItems = new(processOutputs);
        LoggerItems.CollectionChanged += LoggerItems_CollectionChanged;

        if (!launchResponse.Disposed)
        {
            void GameProcessOutput(object sender, FluentCore.Event.GameProcessOutputArgs e)
                => App.MainWindow.DispatcherQueue.TryEnqueue(() => LoggerItems.Add(e.GameProcessOutput));

            View.Loaded += (_, e) => launchResponse.GameProcessOutput += GameProcessOutput;
            View.Unloaded += (_, e) => launchResponse.GameProcessOutput -= GameProcessOutput;
        }

        this.OnPropertyChanged(nameof(LoggerItems));
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

    [ObservableProperty]
    private ObservableCollection<LoggerItem> filterLoggerItems = new();

    public ObservableCollection<GameProcessOutput> LoggerItems;

    public Views.LoggerPage View { get; set; }

    public string Title { get; set; }

    private void LoggerItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        var enums = new List<GameProcessOutputLevel>();

        if (Info)
            enums.Add(GameProcessOutputLevel.Info);

        if (Warn)
            enums.Add(GameProcessOutputLevel.Warn);

        if (Error)
            enums.Add(GameProcessOutputLevel.Error);

        if (Fatal)
            enums.Add(GameProcessOutputLevel.Fatal);

        if (Debug)
            enums.Add(GameProcessOutputLevel.Debug);

        foreach (var item in e.NewItems)
            if (item is GameProcessOutput gameProcessOutput && enums.Contains(gameProcessOutput.Level))
                FilterLoggerItems.Add(new LoggerItem(gameProcessOutput));

        if (EnableAutoScroll)
            ScrollToEnd();
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName != nameof(FilterLoggerItems))
        {
            var enums = new List<GameProcessOutputLevel>();

            if (Info)
                enums.Add(GameProcessOutputLevel.Info);

            if (Warn)
                enums.Add(GameProcessOutputLevel.Warn);

            if (Error)
                enums.Add(GameProcessOutputLevel.Error);

            if (Fatal)
                enums.Add(GameProcessOutputLevel.Fatal);

            if (Debug)
                enums.Add(GameProcessOutputLevel.Debug);

            FilterLoggerItems = new();

            foreach (var item in LoggerItems)
                if (enums.Contains(item.Level))
                    FilterLoggerItems.Add(new LoggerItem(item));
        }

        if (EnableAutoScroll)
            ScrollToEnd();
    }

    private void ScrollToEnd()
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            View.ListView.SelectedIndex = View.ListView.Items.Count - 1;

            try { View.ListView.ScrollIntoView(View.ListView.Items.Last()); } catch { }
        });
    }
}
