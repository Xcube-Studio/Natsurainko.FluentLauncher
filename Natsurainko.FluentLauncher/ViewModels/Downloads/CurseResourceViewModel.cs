using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Services.Storage;
using Nrk.FluentCore.Classes.Datas.Download;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads;

internal partial class CurseResourceViewModel : ObservableObject
{
    public CurseResource Resource { get; init; }

    public CurseResourceViewModel(CurseResource resource)
    {
        Resource = resource;
    }
}
