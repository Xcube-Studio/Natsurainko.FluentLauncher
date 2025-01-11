using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Win32;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Nrk.FluentCore.Launch;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Pages;

internal partial class LoggerViewModel : ObservableObject
{
    private readonly ObservableCollection<GameLoggerOutput> _gameLoggerOutputs;

    public LoggerViewModel(LaunchTaskViewModel launchTaskViewModel, Views.LoggerPage view)
    {
        _gameLoggerOutputs = launchTaskViewModel.Logger;
        _gameLoggerOutputs.CollectionChanged += LoggerItems_CollectionChanged;
        EnabledLevel.CollectionChanged += EnabledLevel_CollectionChanged;

        View = view;
        
        if (launchTaskViewModel.McProcess.State != MinecraftProcessState.Exited)
            View.Unloaded += (_, e) => _gameLoggerOutputs.CollectionChanged -= LoggerItems_CollectionChanged;
        
        OnPropertyChanged();
    }

    [ObservableProperty]
    public partial bool Info { get; set; } = true;

    [ObservableProperty]
    public partial bool Warn { get; set; } = true;

    [ObservableProperty]
    public partial bool Error { get; set; } = true;

    [ObservableProperty]
    public partial bool Fatal { get; set; } = true;

    [ObservableProperty]
    public partial bool Debug { get; set; } = true;

    [ObservableProperty]
    public partial bool EnableAutoScroll { get; set; } = true;
    public ObservableCollection<LoggerItem> FilterLoggerItems { get; } = [];

    private ObservableCollection<GameLoggerOutputLevel> EnabledLevel { get; } = [];

    public Views.LoggerPage View { get; set; }

    public string Title { get; set; }

    [RelayCommand]
    void ExportLog()
    {
        var saveFileDialog = new SaveFileDialog { FileName = "latest.log" };

        if (saveFileDialog.ShowDialog().GetValueOrDefault())
        {
            File.WriteAllLines(saveFileDialog.FileName, _gameLoggerOutputs.Select(x => x.FullData));

            App.GetService<NotificationService>().NotifyWithoutContent(
                ResourceUtils.GetValue("Notifications", "_ExportLog"),
                icon: "\ue74e");
        }
    }

    private void EnabledLevel_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        FilterLoggerItems.Clear();

        foreach (var item in _gameLoggerOutputs)
            if (EnabledLevel.Contains(item.Level))
                FilterLoggerItems.Add(new LoggerItem(item));
    }

    private void LoggerItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        foreach (var item in e.NewItems)
        {
            if (item is GameLoggerOutput GameLoggerOutput && EnabledLevel.Contains(GameLoggerOutput.Level))
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

        if (Info)
        {
            if (!EnabledLevel.Contains(GameLoggerOutputLevel.Info))
                EnabledLevel.Add(GameLoggerOutputLevel.Info);
        }
        else EnabledLevel.Remove(GameLoggerOutputLevel.Info);

        if (Warn)
        {
            if (!EnabledLevel.Contains(GameLoggerOutputLevel.Warn))
                EnabledLevel.Add(GameLoggerOutputLevel.Warn);
        }
        else EnabledLevel.Remove(GameLoggerOutputLevel.Warn);

        if (Error)
        {
            if (!EnabledLevel.Contains(GameLoggerOutputLevel.Error))
                EnabledLevel.Add(GameLoggerOutputLevel.Error);
        }
        else EnabledLevel.Remove(GameLoggerOutputLevel.Error);

        if (Fatal)
        {
            if (!EnabledLevel.Contains(GameLoggerOutputLevel.Fatal))
                EnabledLevel.Add(GameLoggerOutputLevel.Fatal);
        }
        else EnabledLevel.Remove(GameLoggerOutputLevel.Fatal);

        if (Debug)
        {
            if (!EnabledLevel.Contains(GameLoggerOutputLevel.Debug))
                EnabledLevel.Add(GameLoggerOutputLevel.Debug);
        }
        else EnabledLevel.Remove(GameLoggerOutputLevel.Debug);

        if (EnableAutoScroll)
            ScrollToEnd();
    }

    private void ScrollToEnd() => View.ScrollViewer.ScrollToVerticalOffset(View.ScrollViewer.ScrollableHeight);
}

internal partial class LoggerItem : ObservableObject
{
    public LoggerItem(GameLoggerOutput output)
    {
        App.DispatcherQueue.TryEnqueue(() =>
        {
            var richTextBlock = new RichTextBlock();
            LoggerColorLightLanguage.Formatter.FormatRichTextBlock(output.FullData, new LoggerColorLightLanguage(), richTextBlock);
            RichTextBlock = richTextBlock;
        });

        ErrorVisibility = (output.Level == GameLoggerOutputLevel.Error || output.Level == GameLoggerOutputLevel.Fatal)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    [ObservableProperty]
    public partial RichTextBlock RichTextBlock { get; set; }

    [ObservableProperty]
    public partial Visibility ErrorVisibility { get; set; }
}