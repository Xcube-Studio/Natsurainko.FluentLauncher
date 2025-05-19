using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Nrk.FluentCore.Authentication;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Bitmap = System.Drawing.Bitmap;
using Graphics = System.Drawing.Graphics;
using GraphicsUnit = System.Drawing.GraphicsUnit;
using Rectangle = System.Drawing.Rectangle;

namespace Natsurainko.FluentLauncher.UserControls;

public sealed partial class AccountAvatar : UserControl
{
    private readonly CacheSkinService _cacheSkinService = App.GetService<CacheSkinService>();

    public AccountAvatar()
    {
        this.InitializeComponent();
    }

    public Account? Account
    {
        get { try { return (Account)GetValue(AccountProperty); } catch { return null; } }
        set { SetValue(AccountProperty, value); }
    }

    public static readonly DependencyProperty AccountProperty =
        DependencyProperty.Register("Account", typeof(Account), typeof(AccountAvatar), new PropertyMetadata(null, OnAccountChanged));

    private static void OnAccountChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is AccountAvatar accountAvatar && accountAvatar.IsLoaded)
            App.DispatcherQueue.TryEnqueue(() => _ = accountAvatar.RenderAvatar());
    }

    private async Task RenderAvatar()
    {
        try
        {
            if (Account == null) return;
            if (ProgressRing == null || Back == null || Fore == null) return;

            ProgressRing.IsActive = true;

            var filePath = _cacheSkinService.GetSkinFilePath(Account);

            if (Account.Type == AccountType.Offline)
            {
                var backgroundSource1 = await StretchImageSizeAsync(new Bitmap
                (
                    System.Drawing.Image.FromFile((await StorageFile.GetFileFromApplicationUriAsync(new Uri(filePath))).Path)),
                    (int)ActualWidth,
                    (int)ActualHeight
                );

                Back.Source = backgroundSource1;
                Fore.Source = null;

                ProgressRing.IsActive = false;
                return;
            }

            if (!File.Exists(filePath))
            {
                _ = _cacheSkinService.CacheSkinOfAccount(Account).ContinueWith(async task 
                    => await DispatcherQueue.EnqueueAsync(() => ProgressRing.IsActive = false));

                Back.Source = null;
                Fore.Source = null;
                return;
            }

            var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var originImage = System.Drawing.Image.FromStream(stream);
            stream.Close();
            stream.Dispose();

            using var backgroundBitmap = GetAreaFromImage(originImage, new(8, 8, 8, 8));
            using var foregroundBitmap = GetAreaFromImage(originImage, new(40, 8, 8, 8));

            var backgroundSource = await StretchImageSizeAsync(backgroundBitmap, (int)ActualWidth, (int)ActualHeight);
            var foregroundSource = await StretchImageSizeAsync(foregroundBitmap, (int)ActualWidth, (int)ActualHeight);

            Back.Source = backgroundSource;
            Fore.Source = foregroundSource;

            ProgressRing.IsActive = false;
        }
        catch (Exception)
        {
            //App.GetService<NotificationService>().NotifyException(null, ex);

            ProgressRing.IsActive = false;
        }
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        Fore.Source = null;
        Back.Source = null;
        ProgressRing.IsActive = false;

        WeakReferenceMessenger.Default.Register<AccountSkinCacheUpdatedMessage>(this, SkinCacheUpdatedMessageReceived);

        App.DispatcherQueue.TryEnqueue(() => _ = RenderAvatar());
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Unregister<AccountSkinCacheUpdatedMessage>(this);
    }

    private void SkinCacheUpdatedMessageReceived(object sender, AccountSkinCacheUpdatedMessage m)
    {
        if (!m.Value.ProfileEquals(Account))
            return;

        DispatcherQueue.TryEnqueue(() => _ = RenderAvatar());
    }

    #region Bitmap Operations

    private static Bitmap GetAreaFromImage(System.Drawing.Image image, Rectangle rectangle)
    {
        var resultBitmap = new Bitmap(8, 8);
        using var graphics = Graphics.FromImage(resultBitmap);

        graphics.DrawImage(image, new Rectangle(0, 0, 8, 8), rectangle, GraphicsUnit.Pixel);

        return resultBitmap;
    }

    private static async Task<SoftwareBitmapSource> StretchImageSizeAsync(Bitmap bitmap, int width, int height)
    {
        using var memoryStream = new MemoryStream();
        bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

        var decoder = await BitmapDecoder.CreateAsync(memoryStream.AsRandomAccessStream());

        var transform = new BitmapTransform
        {
            InterpolationMode = BitmapInterpolationMode.NearestNeighbor,
            ScaledWidth = (uint)width,
            ScaledHeight = (uint)height
        };

        var bmp = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, transform, ExifOrientationMode.RespectExifOrientation, ColorManagementMode.ColorManageToSRgb);
        var source = new SoftwareBitmapSource();
        await source.SetBitmapAsync(bmp);

        return source;
    }

    #endregion
}
