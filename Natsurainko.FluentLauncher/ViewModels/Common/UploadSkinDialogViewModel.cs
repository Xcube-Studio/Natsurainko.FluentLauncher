using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Dialogs;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Win32;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.Utils;
using System;
using System.IO;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Common;

public partial class UploadSkinDialogViewModel : ObservableObject, IDialogParameterAware
{
    private Account _account = null!;
    private ContentDialog _dialog;

    private readonly NotificationService _notificationService = App.GetService<NotificationService>();

    public UploadSkinDialogViewModel() { }

    void IDialogParameterAware.HandleParameter(object param)
    {
        _account = (Account)param;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UploadCommand))]
    public partial string FilePath { get; set; }

    [ObservableProperty]
    public partial bool IsSlimModel { get; set; }

    [RelayCommand]
    public void LoadEvent(object args)
    {
        var grid = args.As<Grid, object>().sender;
        _dialog = grid.FindName("Dialog") as ContentDialog;
    }

    [RelayCommand(CanExecute = nameof(CanUpload))]
    public async Task Upload()
    {
        try
        {
            if (_account is MicrosoftAccount microsoftAccount)
                await SkinHelper.UploadSkinAsync(microsoftAccount, IsSlimModel, FilePath);
            else if (_account is YggdrasilAccount yggdrasilAccount)
                await SkinHelper.UploadSkinAsync(yggdrasilAccount, IsSlimModel, FilePath);

            await App.GetService<CacheSkinService>().CacheSkinOfAccount(_account);
        }
        catch (Exception ex)
        {
            _notificationService.NotifyException(LocalizedStrings.Notifications__SkinUploadException, ex);
        }

        App.DispatcherQueue.TryEnqueue(_dialog.Hide);
    }

    public void BrowserFile()
    {
        var openFileDialog = new OpenFileDialog();
        openFileDialog.Multiselect = false;
        openFileDialog.Filter = "Png Image File|*.png";

        if (openFileDialog.ShowDialog().GetValueOrDefault(false))
            FilePath = openFileDialog.FileName;
    }

    [RelayCommand]
    public void Cancel() => _dialog.Hide();

    private bool CanUpload => File.Exists(FilePath);
}
