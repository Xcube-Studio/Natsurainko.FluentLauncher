using Natsurainko.FluentCore.Class.Model.Install;
using Natsurainko.FluentLauncher.Class.Component;
using ReactiveUI.Fody.Helpers;
using System;
using Windows.UI.Xaml.Media.Imaging;

namespace Natsurainko.FluentLauncher.Class.ViewData;

public class ModLoaderInformationViewData : ViewDataBase<ModLoaderInformation>
{
    public ModLoaderInformationViewData(ModLoaderInformation data) : base(data)
    {
        DispatcherHelper.RunAsync(() =>
        {
            Icon = new BitmapImage(new Uri(string.Format("ms-appx:///Assets/Icons/{0}Icon.png", data.LoaderType.ToString()), UriKind.RelativeOrAbsolute));
        });
    }

    [Reactive]
    public BitmapImage Icon { get; private set; }

    public object Build { get; set; }
}
