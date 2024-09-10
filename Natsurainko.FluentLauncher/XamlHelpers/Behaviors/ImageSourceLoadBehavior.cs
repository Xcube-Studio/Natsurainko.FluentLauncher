using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Xaml.Interactivity;
using Natsurainko.FluentLauncher.Services.Network;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

#nullable disable
namespace Natsurainko.FluentLauncher.XamlHelpers.Behaviors;

public class ImageSourceLoadBehavior : Behavior<FrameworkElement>
{
    #region Properties

    public bool LoadFromInternet
    {
        get { return (bool)GetValue(LoadFromInternetProperty); }
        set { SetValue(LoadFromInternetProperty, value); }
    }

    public static readonly DependencyProperty LoadFromInternetProperty =
        DependencyProperty.Register("LoadFromInternet", typeof(bool), typeof(ImageSourceLoadBehavior), new PropertyMetadata(false));

    public string SourcePropertyName
    {
        get { return (string)GetValue(SourcePropertyNameProperty); }
        set { SetValue(SourcePropertyNameProperty, value); }
    }

    public static readonly DependencyProperty SourcePropertyNameProperty =
        DependencyProperty.Register("SourcePropertyName", typeof(string), typeof(ImageSourceLoadBehavior), new PropertyMetadata(null, OnSourcePropertyNameChanged));

    public string ImageSourceFilePath
    {
        get { return (string)GetValue(ImageSourceFilePathProperty); }
        set { SetValue(ImageSourceFilePathProperty, value); }
    }

    public static readonly DependencyProperty ImageSourceFilePathProperty =
        DependencyProperty.Register("ImageSourceFilePath", typeof(string), typeof(ImageSourceLoadBehavior), new PropertyMetadata(null, OnImageSourceFilePathChanged));

    public string ImageSourceUrl
    {
        get { return (string)GetValue(ImageSourceUrlProperty); }
        set { SetValue(ImageSourceUrlProperty, value); }
    }

    public static readonly DependencyProperty ImageSourceUrlProperty =
        DependencyProperty.Register("ImageSourceUrl", typeof(string), typeof(ImageSourceLoadBehavior), new PropertyMetadata(null, OnImageSourceUrlChanged));

    #endregion

    private bool isLoading = false;
    private readonly CacheInterfaceService _cacheInterfaceService = App.GetService<CacheInterfaceService>();

    public DependencyProperty SourceProperty { get; private set; }

    protected override void OnAttached()
    {
        base.OnAttached();

        SourceProperty = GetDependencyProperty(AssociatedObject.GetType(), SourcePropertyName);
        App.DispatcherQueue.TryEnqueue(LoadImage);
    }

    private static DependencyProperty GetDependencyProperty(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type,
        string propertyName)
    {
        var field = type.GetTypeInfo().GetDeclaredField($"{propertyName}Property");

        if (field != null)
            return (DependencyProperty)field.GetValue(null);

        var property = type.GetTypeInfo().GetDeclaredProperty($"{propertyName}Property");

        if (property != null)
            return (DependencyProperty)property.GetValue(null);

        var baseType = type.GetTypeInfo().BaseType;

        if (baseType == typeof(object))
            return null;

        return GetDependencyProperty(baseType, propertyName);
    }

    private static void OnSourcePropertyNameChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is not ImageSourceLoadBehavior behavior || behavior.AssociatedObject == null)
            return;

        behavior.SourceProperty = GetDependencyProperty(behavior.AssociatedObject.GetType(), e.NewValue.ToString());
    }

    private static void OnImageSourceFilePathChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is not ImageSourceLoadBehavior behavior || behavior.AssociatedObject == null)
            return;

        App.DispatcherQueue.TryEnqueue(behavior.LoadImage);
    }

    private static void OnImageSourceUrlChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is not ImageSourceLoadBehavior behavior || behavior.AssociatedObject == null)
            return;

        App.DispatcherQueue.TryEnqueue(behavior.LoadImage);
    }

    public async void LoadImage()
    {
        if (isLoading
            || AssociatedObject == null
            || SourceProperty == null
            || (!LoadFromInternet && !File.Exists(ImageSourceFilePath))
            || (LoadFromInternet && string.IsNullOrEmpty(ImageSourceUrl)))
            return;

        isLoading = true;

        try
        {
            using Stream imageDataStream = LoadFromInternet
                ? await _cacheInterfaceService.RequestStreamAsync(ImageSourceUrl, Services.Network.Data.InterfaceRequestMethod.Static)
                : File.OpenRead(ImageSourceFilePath);

            using var randomAccessStream = imageDataStream.AsRandomAccessStream();

            var bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(randomAccessStream);

            AssociatedObject?.SetValue(SourceProperty, bitmapImage);
        }
        catch (Exception) 
        {
            AssociatedObject?.SetValue(SourceProperty, null);
        }
        finally
        {
            isLoading = false;
        }
    }
}
