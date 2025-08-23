using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.Experimental.GameManagement.Modpacks;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Natsurainko.FluentLauncher.ViewModels.Dialogs;

internal partial class ImportModpackDialogViewModel(
    GameService gameService,
    DownloadService downloadService) : DialogVM
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ModpackParsed))]
    [NotifyPropertyChangedFor(nameof(ModpackInfoTags))]
    [NotifyPropertyChangedFor(nameof(ShowModpackVersionTag))]
    [NotifyPropertyChangedFor(nameof(ShowModpackDescription))]
    [NotifyPropertyChangedFor(nameof(ModpackVersion))]
    [NotifyPropertyChangedFor(nameof(ModpackDescription))]
    public partial ModpackInfo? ModpackInfo { get; set; }

    [ObservableProperty]
    public partial bool ModpackParseFailed { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InstanceIdValidity))]
    [NotifyPropertyChangedFor(nameof(CanInstall))]
    [NotifyCanExecuteChangedFor(nameof(InstallCommand))]
    public partial string InstanceId { get; set; } = null!;

    private string? ModpackFilePath { get; set; }

    public bool ModpackParsed => ModpackInfo != null;

    public bool InstanceIdValidity => !string.IsNullOrEmpty(InstanceId) && !gameService.Games.Any(x => x.InstanceId.Equals(InstanceId));

    public bool CanInstall => ModpackParsed && InstanceIdValidity;

    public bool ShowModpackVersionTag => !string.IsNullOrEmpty(ModpackInfo?.Version);

    public bool ShowModpackDescription => !string.IsNullOrEmpty(ModpackInfo?.Description);

    public string ModpackVersion => ModpackInfo?.Version ?? string.Empty;

    public string ModpackDescription => ModpackInfo?.Description ?? string.Empty;

    public List<string> ModpackInfoTags
    {
        get
        {
            if (ModpackInfo == null)
                return [];

            List<string> tags = [ ModpackInfo.ModpackType.ToString(), ModpackInfo.McVersion ];

            if (ModpackInfo.ModLoader != null)
                tags.Add($"{ModpackInfo.ModLoader.Value.Type} {ModpackInfo.ModLoader.Value.Version}");

            if (!string.IsNullOrEmpty(ModpackInfo.Author))
                tags.Add(ModpackInfo.Author);

            return tags;
        }
    } 

    [RelayCommand]
    void OpenModpack()
    {
        var openFileDialog = new OpenFileDialog
        {
            Multiselect = false,
            Filter = "All Files|*.*|Zip File|*.zip"
        };

        if (openFileDialog.ShowDialog().GetValueOrDefault(false))
            TryParseModpack(openFileDialog.FileName);
    }

    [RelayCommand(CanExecute = nameof(CanInstall))]
    void Install()
    {
        downloadService.InstallModpackAsync(new()
        {
            InstanceId = InstanceId,
            ModpackFilePath = ModpackFilePath!,
            ModpackInfo = ModpackInfo!
        }).Forget();

        this.Messenger.Send(new DownloadTaskCreatedMessage(1));
    }

    public void TryParseModpack(string value)
    {
        if (string.IsNullOrEmpty(value) || !File.Exists(value)) return;

        if (ModpackInfoParser.TryParseModpack(value, out var modpackInfo))
        {
            ModpackInfo = modpackInfo;
            ModpackFilePath = value;
            InstanceId = modpackInfo.Name;
            ModpackParseFailed = false;
        }
        else ModpackParseFailed = true;
    }
}
