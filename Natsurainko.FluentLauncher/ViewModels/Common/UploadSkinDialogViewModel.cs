using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Win32;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.Utils;
using System;
using System.IO;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Common;

public partial class UploadSkinDialogViewModel : ObservableObject
{
    private readonly Account _account;
    private ContentDialog _dialog;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UploadCommand))]
    private string filePath;

    [ObservableProperty]
    private bool isSlimModel;

    public UploadSkinDialogViewModel(Account account) 
    {
        _account = account;
    }

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

        }

        App.DispatcherQueue.TryEnqueue(_dialog.Hide);
    }

    [RelayCommand] 
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
