using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity;
using System;
using System.Reflection;

namespace Natsurainko.FluentLauncher.Components.Mvvm;

public class AncestorBindingBehavior : DependencyObject, IBehavior
{
    public DependencyObject AssociatedObject { get; set; }

    public Binding Binding { get; set; }

    public string AncestorType { get; set; }

    public string TargetPropertyName { get; set; }

    public void Attach(DependencyObject associatedObject)
    {
        this.AssociatedObject = associatedObject;

        if (this.Binding == null || string.IsNullOrWhiteSpace(this.TargetPropertyName) || string.IsNullOrWhiteSpace(this.AncestorType))
            return;

        ((FrameworkElement)this.AssociatedObject).Loaded += this.AncestorBindingBehavior_Loaded;
    }

    private void AncestorBindingBehavior_Loaded(object sender, RoutedEventArgs e)
    {
        ((FrameworkElement)this.AssociatedObject).Loaded -= this.AncestorBindingBehavior_Loaded;

        var source = FindAncestorType(this.AssociatedObject, this.AncestorType);
        var targetProperty = GetDependencyProperty(this.AssociatedObject.GetType(), this.TargetPropertyName);

        if (source == null || targetProperty == null) { return; }
        this.Binding.Source = source;

        ((FrameworkElement)this.AssociatedObject).SetBinding(targetProperty, this.Binding);
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