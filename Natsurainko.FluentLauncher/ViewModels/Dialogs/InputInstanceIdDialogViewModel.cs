using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Services.Launch;
using System.Linq;

namespace Natsurainko.FluentLauncher.ViewModels.Dialogs;

internal partial class InputInstanceIdDialogViewModel(GameService gameService) : DialogVM
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InstanceIdValidity))]
    public partial string InstanceId { get; set; }

    public bool InstanceIdValidity => !string.IsNullOrEmpty(InstanceId) && !gameService.Games.Any(x => x.InstanceId.Equals(InstanceId));

    public override void HandleParameter(object param)
    {
        if (param is string id)
            InstanceId = id;
    }
}
