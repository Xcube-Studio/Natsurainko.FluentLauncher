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
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Bitmap = System.Drawing.Bitmap;
using Graphics = System.Drawing.Graphics;
using GraphicsUnit = System.Drawing.GraphicsUnit;
using Rectangle = System.Drawing.Rectangle;

#nullable disable
namespace Natsurainko.FluentLauncher.XamlHelpers.Behaviors;

internal class AccountAvatarLoadBehavior : DependencyObject, IBehavior
{
    public Account Account
    {
        get { return (Account)GetValue(AccountProperty); }
        set { SetValue(AccountProperty, value); }
    } 

    public static readonly DependencyProperty AccountProperty =
        DependencyProperty.Register("Account", typeof(Account), typeof(AccountAvatarLoadBehavior), new PropertyMetadata(null, OnAccountChanged));

    public string ForegroundImageName { get; set; }

    public string BackgroundImageName { get; set; }

    public string ProgressName { get; set; }

    public DependencyObject AssociatedObject { get; set; }

    private Image ForegroundLayout;
    private Image BackgroundLayout;
    private ProgressRing ProgressRing;
    private readonly CacheSkinService _cacheSkinService = App.GetService<CacheSkinService>();

    public void Attach(DependencyObject associatedObject)
    {
        if (associatedObject is not Border) 
            return;

        AssociatedObject = associatedObject;
        Border border = (Border)associatedObject;

        border.Loaded += Border_Loaded;
        border.Unloaded += Border_Unloaded;
    }

    public void Detach() {  }

    private async void RenderAvatar()
    {
        if (Account == null) return;
        if (ProgressRing == null || BackgroundLayout == null || ForegroundLayout == null) return;

        ProgressRing.IsActive = true;
        Border border = (Border)AssociatedObject;

        var filePath = _cacheSkinService.GetSkinFilePath(Account);

        if (Account.Type == AccountType.Offline)
        {
            BackgroundLayout.Source = await StretchImageSizeAsync(
                new Bitmap(
                    System.Drawing.Image.FromFile(
                        (await StorageFile.GetFileFromApplicationUriAsync(new Uri(filePath))).Path)), 
                (int)border.ActualWidth, 
                (int)border.ActualHeight);
            ForegroundLayout.Source = null;

            ProgressRing.IsActive = false;
            return;
        }

        if (!File.Exists(filePath))
        {
            _cacheSkinService.CacheSkinOfAccount(Account).ContinueWith(task => 
            { 

            });
            ProgressRing.IsActive = false;
            return;
        }

        var stream = File.OpenRead(filePath);
        using var originImage = System.Drawing.Image.FromStream(stream);
        stream.Close();
        stream.Dispose();

        using var backgroundBitmap = GetAreaFromImage(originImage, new(8, 8, 8, 8));
        using var foregroundBitmap = GetAreaFromImage(originImage, new(40, 8, 8, 8));

        BackgroundLayout.Source = await StretchImageSizeAsync(backgroundBitmap, (int)border.ActualWidth, (int)border.ActualHeight);
        ForegroundLayout.Source = await StretchImageSizeAsync(foregroundBitmap, (int)border.ActualWidth, (int)border.ActualHeight);

        ProgressRing.IsActive = false;
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
            App.DispatcherQueue.TryEnqueue(behavior.RenderAvatar);
    }

    private void Border_Unloaded(object sender, RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Unregister<AccountSkinCacheUpdatedMessage>(this);
    }

    private void Border_Loaded(object sender, RoutedEventArgs e)
    {
        Border border = (Border)AssociatedObject;

        ForegroundLayout = border.FindName(ForegroundImageName) as Image;
        BackgroundLayout = border.FindName(BackgroundImageName) as Image;
        ProgressRing = border.FindName(ProgressName) as ProgressRing;

        ForegroundLayout.Source = null;
        BackgroundLayout.Source = null;
        ProgressRing.IsActive = false;

        RenderAvatar();

        WeakReferenceMessenger.Default.Register<AccountSkinCacheUpdatedMessage>(this, (r, m) =>
        {
            if (m.Value.Type == Account.Type && m.Value.Uuid == Account.Uuid)
                App.DispatcherQueue.TryEnqueue(RenderAvatar);
        });
    }
}
