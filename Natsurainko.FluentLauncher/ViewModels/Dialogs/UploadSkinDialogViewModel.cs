using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using FluentLauncher.Infra.UI.Notification;
using Microsoft.Win32;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Views.Dialogs;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.Utils;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Dialogs;

internal partial class UploadSkinDialogViewModel(
    INotificationService notificationService,
    CacheInterfaceService cacheInterfaceService) : DialogVM<UploadSkinDialog>
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
        if (_account is not MicrosoftAccount microsoftAccount)
            throw new InvalidOperationException("Only Microsoft accounts can upload skins.");

        try
        {
            await PlayerTextureHelper.UploadSkinTextureAsync(microsoftAccount, FilePath, IsSlimModel ? "slim" : "classic");
            await cacheInterfaceService.CacheTexturesAsync(_account);
        }
        catch (Exception ex)
        {
            string reason = ex switch
            {
                HttpRequestException { StatusCode: HttpStatusCode.Unauthorized } => LocalizedStrings.Exceptions__HttpRequestException_Unauthorized,
                _ => string.Empty
            };

            notificationService.SkinUploadFailed(ex, reason);
        }

        await Dispatcher.EnqueueAsync(this.Dialog.Hide);
    }

    [RelayCommand]
    void Cancel() => this.Dialog.Hide();
}

internal static partial class UploadSkinDialogViewModelNotifications
{
    [ExceptionNotification(Title = "Notifications__SkinUploadFailed", Message = "{reason}")]
    public static partial void SkinUploadFailed(this INotificationService notificationService, Exception exception, string reason);
}