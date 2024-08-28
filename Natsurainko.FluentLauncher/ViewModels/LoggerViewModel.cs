using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Natsurainko.FluentLauncher.Classes.Data.Launch;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Nrk.FluentCore.Launch;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Pages;

internal partial class LoggerViewModel : ObservableObject
{
    private readonly ObservableCollection<GameLoggerOutput> _gameLoggerOutputs;

    public LoggerViewModel(LaunchSessionViewModel launchProcess, Views.LoggerPage view)
    {
        _gameLoggerOutputs = launchProcess._gameLoggerOutputs;
        _gameLoggerOutputs.CollectionChanged += LoggerItems_CollectionChanged;

        View = view;

        if (!(launchProcess.SessionState == LaunchSessionState.GameExited ||
            launchProcess.SessionState == LaunchSessionState.Killed ||
            launchProcess.SessionState == LaunchSessionState.GameCrashed))
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

    [RelayCommand]
    void ExportLog()
    {
        var saveFileDialog = new SaveFileDialog { FileName = "latest.log" };

        if (saveFileDialog.ShowDialog().GetValueOrDefault())
            File.WriteAllLines(saveFileDialog.FileName, _gameLoggerOutputs.Select(x => x.FullData));

        App.GetService<NotificationService>().NotifyWithoutContent(
            ResourceUtils.GetValue("Notifications", "_ExportLog"),
            icon: "\ue74e");
    }

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
        {
            if (item is GameLoggerOutput GameLoggerOutput && enums.Contains(GameLoggerOutput.Level))
            {
                var loggerItem = new LoggerItem(GameLoggerOutput);
                App.DispatcherQueue.TryEnqueue(() => FilterLoggerItems.Add(loggerItem));
            }
        }

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
        View.ScrollViewer.ScrollToVerticalOffset(View.ScrollViewer.ScrollableHeight);
    }
}
