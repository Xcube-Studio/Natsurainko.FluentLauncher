using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Xaml.Interactivity;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Nrk.FluentCore.Authentication;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Bitmap = System.Drawing.Bitmap;
using Graphics = System.Drawing.Graphics;
using GraphicsUnit = System.Drawing.GraphicsUnit;
using Rectangle = System.Drawing.Rectangle;

namespace Natsurainko.FluentLauncher.XamlHelpers.Behaviors;

internal class AccountAvatarLoadBehavior : Behavior<Border>
{
    public Account Account
    {
        get { return (Account)GetValue(AccountProperty); }
        set { SetValue(AccountProperty, value); }
    }

    public static readonly DependencyProperty AccountProperty =
        DependencyProperty.Register("Account", typeof(Account), typeof(AccountAvatarLoadBehavior), new PropertyMetadata(null, OnAccountChanged));

    public string ForegroundImageName { get; set; } = null!; // set in XAML

    public string BackgroundImageName { get; set; } = null!;

    public string ProgressName { get; set; } = null!;

    private Image ForegroundLayout = null!; // set in Border_Loaded
    private Image BackgroundLayout = null!;
    private ProgressRing ProgressRing = null!;
    private readonly CacheSkinService _cacheSkinService = App.GetService<CacheSkinService>();

    protected override void OnAttached()
    {
        AssociatedObject.Loaded += Border_Loaded;
        AssociatedObject.Unloaded += Border_Unloaded;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.Loaded -= Border_Loaded;
        AssociatedObject.Unloaded -= Border_Unloaded;
    }

    private async Task RenderAvatar()
    {
        try
        {
            if (Account == null) return;
            if (ProgressRing == null || BackgroundLayout == null || ForegroundLayout == null) return;

            ProgressRing.IsActive = true;

            var filePath = _cacheSkinService.GetSkinFilePath(Account);

            if (Account.Type == AccountType.Offline)
            {
                var backgroundSource1 = await StretchImageSizeAsync(
                        new Bitmap(
                            System.Drawing.Image.FromFile(
                                (await StorageFile.GetFileFromApplicationUriAsync(new Uri(filePath))).Path)),
                        (int)AssociatedObject.ActualWidth,
                        (int)AssociatedObject.ActualHeight);

                //BackgroundLayout.Source = backgroundSource1;
                //ForegroundLayout.Source = null;

                ProgressRing.IsActive = false;
                return;
            }

            if (!File.Exists(filePath))
            {
                _ = _cacheSkinService.CacheSkinOfAccount(Account);
                ProgressRing.IsActive = false;
                return;
            }

            var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var originImage = System.Drawing.Image.FromStream(stream);
            stream.Close();
            stream.Dispose();

            using var backgroundBitmap = GetAreaFromImage(originImage, new(8, 8, 8, 8));
            using var foregroundBitmap = GetAreaFromImage(originImage, new(40, 8, 8, 8));

            var backgroundSource = await StretchImageSizeAsync(backgroundBitmap, (int)AssociatedObject.ActualWidth, (int)AssociatedObject.ActualHeight);
            var foregroundSource = await StretchImageSizeAsync(foregroundBitmap, (int)AssociatedObject.ActualWidth, (int)AssociatedObject.ActualHeight);

            //BackgroundLayout.Source = backgroundSource;
            //ForegroundLayout.Source = foregroundSource;

            ProgressRing.IsActive = false;
        }
        catch (Exception)
        {
            //App.GetService<NotificationService>().NotifyException(null, ex);
        }
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

        using var bmp = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, transform, ExifOrientationMode.RespectExifOrientation, ColorManagementMode.ColorManageToSRgb);
        var source = new SoftwareBitmapSource();
        await source.SetBitmapAsync(bmp);

        return source;
    }

    #endregion

    private static void OnAccountChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is AccountAvatarLoadBehavior behavior)
            App.DispatcherQueue.TryEnqueue(() => _ = behavior.RenderAvatar());
    }

    private void Border_Unloaded(object sender, RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Unregister<AccountSkinCacheUpdatedMessage>(this);
    }

    private void Border_Loaded(object sender, RoutedEventArgs e)
    {
        ForegroundLayout = (Image)AssociatedObject.FindName(ForegroundImageName);
        BackgroundLayout = (Image)AssociatedObject.FindName(BackgroundImageName);
        ProgressRing = (ProgressRing)AssociatedObject.FindName(ProgressName);

        ForegroundLayout.Source = null;
        BackgroundLayout.Source = null;
        ProgressRing.IsActive = false;

        WeakReferenceMessenger.Default.Register<AccountSkinCacheUpdatedMessage>(this, (r, m) =>
        {
            if (m.Value.Type == Account.Type && m.Value.Uuid == Account.Uuid)
                App.DispatcherQueue.TryEnqueue(() => _ = RenderAvatar());
        });

        _ = RenderAvatar();
    }
}
