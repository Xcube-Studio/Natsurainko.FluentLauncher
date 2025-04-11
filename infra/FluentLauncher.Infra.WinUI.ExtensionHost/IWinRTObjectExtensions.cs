using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Runtime.CompilerServices;
using WinRT;

namespace FluentLauncher.Infra.WinUI.ExtensionHost;

public static class IWinRTObjectExtensions
{
    public static void LoadComponent<T>(this T component, ref bool contentLoaded, [CallerFilePath] string callerFilePath = "") where T : IWinRTObject
    {
        if (contentLoaded) return;

        contentLoaded = true;

        Uri resourceLocator = ApplicationExtensionHost.Current.LocateResource(component, callerFilePath);
        Application.LoadComponent(component, resourceLocator, ComponentResourceLocation.Nested);
    }
}
