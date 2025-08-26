using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Utils;
using System;
using System.Linq;
using Windows.System;

namespace Natsurainko.FluentLauncher.Views.Dialogs;

// This dialog is not managed by the DI framework because it is called in App.xaml.cs, where a scope is not available
public sealed partial class ExceptionDialog : ContentDialog, IRecipient<ExceptionDialogRepeatedlyRequestMessage>
{
    public ExceptionDialog(Exception exception)
    {
        InitializeComponent();

        XamlRoot = MainWindow.XamlRoot;
        RequestedTheme = (App.MainWindow.Content as FrameworkElement)!.RequestedTheme;

        Segmented.Items.Add(new SegmentedItem
        {
            Content = exception.GetType().Name,
            IsSelected = true,
            Tag = exception
        });

        WeakReferenceMessenger.Default.Register(this);
    }

    private void Dialog_Unloaded(object sender, RoutedEventArgs e) => WeakReferenceMessenger.Default.UnregisterAll(this);

    void IRecipient<ExceptionDialogRepeatedlyRequestMessage>.Receive(ExceptionDialogRepeatedlyRequestMessage message)
    {
        App.DispatcherQueue.TryEnqueue(() =>
        {
            Segmented.Items.Add(new SegmentedItem
            {
                Content = message.Value.GetType().Name,
                Tag = message.Value
            });
        });
    }
    private void Segmented_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        TextBlock.Text = App.GetErrorMessage((Exception)((SegmentedItem)Segmented.SelectedItem).Tag!);
    }

    private async void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        string[] exceptions = [.. Segmented.Items.OfType<SegmentedItem>()
            .Select(s => s.Tag as Exception)
            .Select(e => App.GetErrorMessage(e!))];

        ClipboardHepler.SetText(string.Join("\r\n", exceptions));
        await Launcher.LaunchUriAsync(new Uri("https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/issues/new/choose"));
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => this.Hide();
}