using CommunityToolkit.Mvvm.ComponentModel;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Models.UI;
using Nrk.FluentCore.GameManagement.Installer;
using System.Collections.Generic;
using System.IO;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads.Instances;

internal partial class InstallViewModel : ObservableObject, INavigationAware
{
    [ObservableProperty]
    public partial IEnumerable<InstanceLoaderItem> LoaderItems { get; set; } = [];

    [ObservableProperty]
    public partial VersionManifestItem CurrentInstance { get; set; } = null!;

    void INavigationAware.OnNavigatedTo(object? parameter)
    {
        CurrentInstance = parameter as VersionManifestItem
            ?? throw new InvalidDataException();

        LoaderItems = InstanceLoaderItem.GetInstanceLoaderItems(CurrentInstance);
    }
}
