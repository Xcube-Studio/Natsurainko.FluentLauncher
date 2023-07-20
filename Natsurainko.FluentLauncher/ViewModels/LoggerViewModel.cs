using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Components.Launch;
using Natsurainko.FluentLauncher.Models;
using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Classes.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Natsurainko.FluentLauncher.ViewModels.Pages;

partial class LoggerViewModel : ObservableObject
{
    public LoggerViewModel(List<GameLoggerOutput> processOutputs, LaunchProcess launchProcess, Views.LoggerPage view)
    {
        View = view;

        LoggerItems = new(processOutputs);
        LoggerItems.CollectionChanged += LoggerItems_CollectionChanged;

        if (!launchProcess.McProcess.HasExited)
        {
            View.Loaded += (_, e) =>
            {
                launchProcess.McProcess.OutputDataReceived += McProcess_OutputDataReceived;
                launchProcess.McProcess.ErrorDataReceived += McProcess_ErrorDataReceived;
            };
            View.Unloaded += (_, e) =>
            {
                launchProcess.McProcess.OutputDataReceived -= McProcess_OutputDataReceived;
                launchProcess.McProcess.ErrorDataReceived -= McProcess_ErrorDataReceived;
            };
        }

        this.OnPropertyChanged(nameof(LoggerItems));
    }

    private void McProcess_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        => App.MainWindow.DispatcherQueue.TryEnqueue(() => LoggerItems.Add(GameLoggerOutput.Parse(e.Data, true)));

    private void McProcess_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        => App.MainWindow.DispatcherQueue.TryEnqueue(() => LoggerItems.Add(GameLoggerOutput.Parse(e.Data)));

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

    public ObservableCollection<GameLoggerOutput> LoggerItems;

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
                FilterLoggerItems.Add(new LoggerItem(GameLoggerOutput));

        if (EnableAutoScroll)
            ScrollToEnd();
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
