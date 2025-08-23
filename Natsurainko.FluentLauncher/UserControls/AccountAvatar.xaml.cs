using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.Authentication;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Bitmap = System.Drawing.Bitmap;
using Graphics = System.Drawing.Graphics;
using GraphicsUnit = System.Drawing.GraphicsUnit;
using Rectangle = System.Drawing.Rectangle;

namespace Natsurainko.FluentLauncher.UserControls;

public sealed partial class AccountAvatar : UserControl, IRecipient<SkinTextureUpdatedMessage>
{
    private readonly CacheInterfaceService _cacheInterfaceService = App.GetService<CacheInterfaceService>();

    public AccountAvatar()
    {
        this.InitializeComponent();
        WeakReferenceMessenger.Default.Register(this);
    }

    public Account? Account
    {
        get { return (Account)GetValue(AccountProperty); }
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
            Back.Source = null;
            Fore.Source = null;

            var textureProfile = await _cacheInterfaceService.RequestTextureProfileAsync(Account);
            string skinTexturePath = textureProfile.GetSkinTexturePath(out bool isDefaultSkin);

            if (!File.Exists(skinTexturePath))
            {
                _cacheInterfaceService.CacheTexturesAsync(Account).Forget();
                return;
            }

            var stream = File.Open(skinTexturePath, FileMode.Open, FileAccess.Read, FileShare.Read);
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
            // TODO: Log the exception
            ProgressRing.IsActive = false;
        }
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        Fore.Source = null;
        Back.Source = null;
        ProgressRing.IsActive = false;

        DispatcherQueue.TryEnqueue(() => RenderAvatar().Forget());
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e) => WeakReferenceMessenger.Default.UnregisterAll(this);

    void IRecipient<SkinTextureUpdatedMessage>.Receive(SkinTextureUpdatedMessage message)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            if (message.Value.ProfileEquals(Account))
                RenderAvatar().Forget();
        });
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
