using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Shared.Class.Model;
using ReactiveUI.Fody.Helpers;

namespace Natsurainko.FluentLauncher.Class.ViewData;

public class LocalModInformationViewData : ViewDataBase<LocalModInformation>
{
    public LocalModInformationViewData(LocalModInformation data) : base(data)
    {
        var template = ConfigurationManager.AppSettings.CurrentLanguage.GetString("PMP_Converter_ModDescription").Split('|')[0];
        string symbol = ConfigurationManager.AppSettings.CurrentLanguage.GetString("PMP_Converter_ModDescription").Split('|')[1];

        Description = template
            .Replace("{name}", data.Name)
            .Replace("{version}", data.Version)
            .Replace("{authors}", string.Join(symbol, data.Authors));
    }

    [Reactive]
    public string Description { get; set; }
}
