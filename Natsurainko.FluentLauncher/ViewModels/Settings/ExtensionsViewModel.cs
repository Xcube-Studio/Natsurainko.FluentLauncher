using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.ExtensionHost.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

internal partial class ExtensionsViewModel(List<IExtension> extensions) : PageVM
{
    public string ExtensionsFolder { get; } = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Extensions");

    public IReadOnlyList<IExtension> Extensions { get; } = extensions;

    [RelayCommand]
    async Task OpenExtensionsFolder() => await Launcher.LaunchFolderPathAsync(ExtensionsFolder);
}