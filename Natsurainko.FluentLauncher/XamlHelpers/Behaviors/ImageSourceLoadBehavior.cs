using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Xaml.Interactivity;
using System;
using System.IO;
using System.Reflection;

#nullable disable
namespace Natsurainko.FluentLauncher.XamlHelpers.Behaviors;

public class ImageSourceLoadBehavior : Behavior<FrameworkElement>
{
    #region Properties

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

    #endregion

    public DependencyProperty SourceProperty { get; private set; }

    protected override void OnAttached()
    {
        base.OnAttached();

        SourceProperty = GetDependencyProperty(AssociatedObject.GetType(), SourcePropertyName);
        LoadImage();
    }

    private static DependencyProperty GetDependencyProperty(Type type, string propertyName)
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

        behavior.LoadImage();
    }

    public async void LoadImage()
    {
        if (SourceProperty == null || !File.Exists(ImageSourceFilePath))
            return;

        using var fileStream = File.OpenRead(ImageSourceFilePath);

        var bitmapImage = new BitmapImage();
        await bitmapImage.SetSourceAsync(fileStream.AsRandomAccessStream());

        AssociatedObject.SetValue(SourceProperty, bitmapImage);
    }
}
