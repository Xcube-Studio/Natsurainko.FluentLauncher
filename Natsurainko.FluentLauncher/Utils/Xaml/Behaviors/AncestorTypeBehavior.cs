using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity;
using System;
using System.Reflection;

namespace Natsurainko.FluentLauncher.Utils.Xaml.Behaviors;

public class AncestorBindingBehavior : DependencyObject, IBehavior
{
    public DependencyObject AssociatedObject { get; set; }

    public Binding Binding { get; set; }

    public string AncestorType { get; set; }

    public string TargetPropertyName { get; set; }

    public void Attach(DependencyObject associatedObject)
    {
        AssociatedObject = associatedObject;

        if (Binding == null || string.IsNullOrWhiteSpace(TargetPropertyName) || string.IsNullOrWhiteSpace(AncestorType))
            return;

        ((FrameworkElement)AssociatedObject).Loaded += AncestorBindingBehavior_Loaded;
    }

    private void AncestorBindingBehavior_Loaded(object sender, RoutedEventArgs e)
    {
        ((FrameworkElement)AssociatedObject).Loaded -= AncestorBindingBehavior_Loaded;

        var source = FindAncestorType(AssociatedObject, AncestorType);
        var targetProperty = GetDependencyProperty(AssociatedObject.GetType(), TargetPropertyName);

        if (source == null || targetProperty == null) { return; }
        Binding.Source = source;

        ((FrameworkElement)AssociatedObject).SetBinding(targetProperty, Binding);
    }

    public void Detach()
    {
    }

    private static DependencyObject FindAncestorType(DependencyObject element, string type)
    {
        if (element.GetType().Name == type)
            return element;

        var parent = VisualTreeHelper.GetParent(element);

        if (parent == null)
            return null;

        return FindAncestorType(parent, type);
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
}