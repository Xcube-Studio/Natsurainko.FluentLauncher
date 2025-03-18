using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.Win32;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Views.Dialogs;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.Utils;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Dialogs;

internal partial class UploadSkinDialogViewModel(
    NotificationService notificationService, 
    CacheSkinService cacheSkinService) : DialogVM<UploadSkinDialog>
{
    private Account _account = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UploadCommand))]
    public partial string FilePath { get; set; }

    [ObservableProperty]
    public partial bool IsSlimModel { get; set; }

    private bool CanUpload => File.Exists(FilePath);

    protected override void OnLoaded()
    {
        this.Dialog.AutoSuggestBox.QuerySubmitted += (_, _) =>
        {
            OpenFileDialog openFileDialog = new()
            {
                Multiselect = false,
                Filter = "Png Image File|*.png"
            };

            if (openFileDialog.ShowDialog().GetValueOrDefault(false))
                FilePath = openFileDialog.FileName;
        };
    }

    public override void HandleParameter(object param) => _account = (Account)param;

    [RelayCommand(CanExecute = nameof(CanUpload))]
    async Task Upload()
    {
        try
        {
            if (_account is MicrosoftAccount microsoftAccount)
                await SkinHelper.UploadSkinAsync(microsoftAccount, IsSlimModel, FilePath);
            else if (_account is YggdrasilAccount yggdrasilAccount)
                await SkinHelper.UploadSkinAsync(yggdrasilAccount, IsSlimModel, FilePath);

            await cacheSkinService.CacheSkinOfAccount(_account);
        }
        catch (Exception ex)
        {
            notificationService.NotifyException(LocalizedStrings.Notifications__SkinUploadException, ex);
        }

        await Dispatcher.EnqueueAsync(this.Dialog.Hide);
    }

    [RelayCommand]
    void Cancel() => this.Dialog.Hide();
}
