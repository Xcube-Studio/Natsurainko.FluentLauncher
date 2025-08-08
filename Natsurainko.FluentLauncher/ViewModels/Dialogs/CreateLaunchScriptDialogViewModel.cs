using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Nrk.FluentCore.Launch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Natsurainko.FluentLauncher.ViewModels.Dialogs;

internal partial class CreateLaunchScriptDialogViewModel : DialogVM
{
    private MinecraftProcess _minecraftProcess = null!;

    [ObservableProperty]
    public partial bool HideAccessToken { get; set; } = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCreate))]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    public partial string BatchFilePath { get; set; } = string.Empty;

    public bool CanCreate => !string.IsNullOrEmpty(BatchFilePath);

    public override void HandleParameter(object param)
    {
        _minecraftProcess = (MinecraftProcess)param;
    }

    public void SelectFilePath()
    {
        SaveFileDialog saveFileDialog = new() 
        { 
            FileName = "cmd-launch.bat", 
            Filter = "Batch files (*.bat)|*.bat|All files (*.*)|*.*",
        };

        if (saveFileDialog.ShowDialog().GetValueOrDefault())
            BatchFilePath = saveFileDialog.FileName;
    }

    [RelayCommand(CanExecute = nameof(CanCreate))]
    void Create()
    {
        List<string> arguments = [.. _minecraftProcess.ArgumentList];

        if (HideAccessToken)
        {
            arguments[arguments.FindIndex(a => a.StartsWith("--accessToken"))] = $"--accessToken {Guid.Empty}";
        }

        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine("@echo off");
        stringBuilder.AppendLine($"cd /{_minecraftProcess.WorkingDirectory[0]} \"{_minecraftProcess.WorkingDirectory}\"");
        stringBuilder.AppendLine($"\"{_minecraftProcess.JavaPath.Replace("javaw.exe", "java.exe")}\" {string.Join(' ', arguments)}");
        stringBuilder.AppendLine("pause");

        FileInfo fileInfo = new(BatchFilePath);

        if (!fileInfo.Directory!.Exists)
            fileInfo.Directory.Create();

        File.WriteAllText(BatchFilePath, stringBuilder.ToString());

        Dialog.Hide();
    }

    [RelayCommand]
    void Cancel() => Dialog.Hide();
}
