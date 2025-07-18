using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using System.IO;
using System.Runtime.InteropServices;
using Windows.ApplicationModel;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;
using WinRT.Interop;
using WinUIEx;

namespace Natsurainko.FluentLauncher.Utils.Extensions;

internal static class WindowExtensions
{
    public static void ConfigureTitleBarTheme(this WindowEx window)
    {
        FrameworkElement frameworkElement = ((FrameworkElement)window.Content);
        ElementTheme actualTheme = frameworkElement.ActualTheme;
        AppWindow appWindow = window.AppWindow;

        var hoverColor = actualTheme == ElementTheme.Light ? Colors.Black : Colors.White;
        hoverColor.A = 35;

        appWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets/AppIcon.ico"));
        appWindow.TitleBar.ButtonBackgroundColor = appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        appWindow.TitleBar.ButtonForegroundColor = actualTheme == ElementTheme.Light ? Colors.Black : Colors.White;
        appWindow.TitleBar.ButtonHoverForegroundColor = actualTheme == ElementTheme.Light ? Colors.Black : Colors.White;
        appWindow.TitleBar.ButtonHoverBackgroundColor = hoverColor;
        appWindow.TitleBar.ExtendsContentIntoTitleBar = true;

        frameworkElement.ActualThemeChanged += (frameworkElement, _) =>
        {
            ElementTheme actualTheme = frameworkElement.ActualTheme;

            appWindow.TitleBar.ButtonBackgroundColor = appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            appWindow.TitleBar.ButtonForegroundColor = actualTheme == ElementTheme.Light ? Colors.Black : Colors.White;
            appWindow.TitleBar.ButtonHoverForegroundColor = actualTheme == ElementTheme.Light ? Colors.Black : Colors.White;

            var hoverColor = actualTheme == ElementTheme.Light ? Colors.Black : Colors.White;
            hoverColor.A = 35;

            appWindow.TitleBar.ButtonHoverBackgroundColor = hoverColor;
        };
    }

    public static void ConfigureElementTheme(this Window window)
    {
        if (window.Content is not FrameworkElement frameworkElement)
            return;

        ElementTheme theme = (ElementTheme)App.GetService<SettingsService>().DisplayTheme;
        frameworkElement.RequestedTheme = theme;

        WeakReferenceMessenger.Default.Register<SettingsRequestThemeChangedMessage>(window, (sender, m) => frameworkElement.RequestedTheme = m.Value);
    }

    //public static WINDOWPLACEMENT AdjustWindowPlacement(Window window, WINDOWPLACEMENT wINDOWPLACEMENT)
    //{
    //    var hMONITOR = PInvoke.MonitorFromWindow(
    //        new HWND(WindowNative.GetWindowHandle(window)), 
    //        MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);

    //    MONITORINFO mONITORINFO = new()
    //    {
    //        cbSize = (uint)Marshal.SizeOf<MONITORINFO>()
    //    };

    //    PInvoke.GetMonitorInfo(hMONITOR, ref mONITORINFO);

    //    return wINDOWPLACEMENT;
    //}

    public static WINDOWPLACEMENT GetCurrentWindowPlacement(this Window window)
    {
        HWND hWND = new(WindowNative.GetWindowHandle(window));

        WINDOWPLACEMENT placement = new()
        {
            length = (uint)Marshal.SizeOf<WINDOWPLACEMENT>(),
        };
        PInvoke.GetWindowPlacement(hWND, ref placement);

        HMONITOR hMONITOR = PInvoke.MonitorFromWindow(hWND, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
        PInvoke.GetDpiForMonitor(hMONITOR, Windows.Win32.UI.HiDpi.MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out uint dpiX, out uint dpiY);

        placement.rcNormalPosition.right -= (placement.rcNormalPosition.Width - (int)(placement.rcNormalPosition.Width * (96 / (double)dpiX)));
        placement.rcNormalPosition.bottom -= (placement.rcNormalPosition.Height - (int)(placement.rcNormalPosition.Height * (96 / (double)dpiY)));

        return placement;
    }

    public static MONITORINFO GetCurrentMonitorInfoForWindow(HWND hwnd)
    {
        var hMONITOR = PInvoke.MonitorFromWindow(hwnd, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);

        MONITORINFO mONITORINFO = new()
        {
            cbSize = (uint)Marshal.SizeOf<MONITORINFO>()
        };

        PInvoke.GetMonitorInfo(hMONITOR, ref mONITORINFO);
        return mONITORINFO;
    }

    public static MONITORINFO GetCurrentMonitorInfo(this Window window) => GetCurrentMonitorInfoForWindow(new HWND(WindowNative.GetWindowHandle(window)));
}
