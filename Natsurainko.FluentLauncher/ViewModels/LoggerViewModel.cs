using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Notification;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Win32;
using Nrk.FluentCore.Launch;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Natsurainko.FluentLauncher.ViewModels.Pages;

internal partial class LoggerViewModel : ObservableRecipient
{
    private readonly ObservableCollection<GameLoggerOutputLevel> enabledLevel = [];

    [ObservableProperty]
    public partial bool Info { get; set; }

    [ObservableProperty]
    public partial bool Warn { get; set; }

    [ObservableProperty]
    public partial bool Error { get; set; }

    [ObservableProperty]
    public partial bool Fatal { get; set; }

    [ObservableProperty]
    public partial bool Debug { get; set; }

    [ObservableProperty]
    public partial bool EnableAutoScroll { get; set; } = true;

    [ObservableProperty]
    public partial string Title { get; set; }

    public ObservableCollection<GameLoggerOutput> FilterLoggerItems { get; } = [];

    public required ObservableCollection<GameLoggerOutput> Outputs { get; init; }

    public required Action ScrollToEnd { get; init; }

    [RelayCommand]
    void ExportLog()
    {
        var saveFileDialog = new SaveFileDialog { FileName = "latest.log" };

        if (saveFileDialog.ShowDialog().GetValueOrDefault())
        {
            File.WriteAllLines(saveFileDialog.FileName, Outputs.Select(x => x.FullData));
            App.GetService<INotificationService>().LogExported(saveFileDialog.FileName);
        }
    }

    private void EnabledLevel_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        FilterLoggerItems.Clear();

        foreach (var item in Outputs)
            if (enabledLevel.Contains(item.Level))
                FilterLoggerItems.Add(item);
    }

    private void LoggerItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is null)
            return;

        foreach (var item in e.NewItems)
        {
            if (item is GameLoggerOutput loggerOutput && enabledLevel.Contains(loggerOutput.Level))
            {
                App.DispatcherQueue.TryEnqueue(() => FilterLoggerItems.Add(loggerOutput));
            }
        }

        if (EnableAutoScroll)
            App.DispatcherQueue.TryEnqueue(() => ScrollToEnd());
    }

    partial void OnInfoChanged(bool value)
    {
        if (value)
            enabledLevel.Add(GameLoggerOutputLevel.Info);
        else enabledLevel.Remove(GameLoggerOutputLevel.Info);
    }

    partial void OnWarnChanged(bool value)
    {
        if (value)
            enabledLevel.Add(GameLoggerOutputLevel.Warn);
        else enabledLevel.Remove(GameLoggerOutputLevel.Warn);
    }

    partial void OnErrorChanged(bool value)
    {
        if (value)
            enabledLevel.Add(GameLoggerOutputLevel.Error);
        else enabledLevel.Remove(GameLoggerOutputLevel.Error);
    }

    partial void OnFatalChanged(bool value)
    {
        if (value)
            enabledLevel.Add(GameLoggerOutputLevel.Fatal);
        else enabledLevel.Remove(GameLoggerOutputLevel.Fatal);
    }

    partial void OnDebugChanged(bool value)
    {
        if (value)
            enabledLevel.Add(GameLoggerOutputLevel.Debug);
        else enabledLevel.Remove(GameLoggerOutputLevel.Debug);
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        Outputs.CollectionChanged += LoggerItems_CollectionChanged;
        enabledLevel.CollectionChanged += EnabledLevel_CollectionChanged;
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        Outputs.CollectionChanged -= LoggerItems_CollectionChanged;
        enabledLevel.CollectionChanged -= EnabledLevel_CollectionChanged;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (EnableAutoScroll)
            App.DispatcherQueue.TryEnqueue(() => ScrollToEnd());
    }
}

internal static partial class LoggerViewModelNotifications
{
    [Notification<InfoBar>(Title = "Notifications__LogExported", Message = "{filePath}")]
    public static partial void LogExported(this INotificationService notificationService, string filePath);
}