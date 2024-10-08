﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Natsurainko.FluentLauncher.XamlHelpers.Behaviors;

public class AncestorBindingBehavior : Behavior<FrameworkElement>
{
    public Binding Binding { get; set; } = null!;

    public string AncestorType { get; set; } = null!;

    public string TargetPropertyName { get; set; } = null!;

    protected override void OnAttached()
    {
        if (Binding == null || string.IsNullOrWhiteSpace(TargetPropertyName) || string.IsNullOrWhiteSpace(AncestorType))
            return;

        AssociatedObject.Loaded += AncestorBindingBehavior_Loaded;
    }

    private void AncestorBindingBehavior_Loaded(object sender, RoutedEventArgs e)
    {
        AssociatedObject.Loaded -= AncestorBindingBehavior_Loaded;

        var source = FindAncestorType(AssociatedObject, AncestorType);
        var targetProperty = GetDependencyProperty(AssociatedObject.GetType(), TargetPropertyName);

        if (source == null || targetProperty == null) { return; }

        try
        {
            Binding.Source = source;

            AssociatedObject.SetBinding(targetProperty, Binding);
        }
        catch
        {
            // TODO: fix the unhandled exception when set Binding.Source
            // System.Runtime.InteropServices.COMException:“Error HRESULT E_FAIL has been returned from a call to a COM component.”
        }
    }

    private static DependencyObject? FindAncestorType(DependencyObject element, string type)
    {
        if (element.GetType().Name == type)
            return element;

        var parent = VisualTreeHelper.GetParent(element);

        if (parent == null)
            return null;

        return FindAncestorType(parent, type);
    }

    private static DependencyProperty? GetDependencyProperty(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type,
        string propertyName)
    {
        var field = type.GetTypeInfo().GetDeclaredField($"{propertyName}Property");

        if (field != null)
            return (DependencyProperty?)field.GetValue(null);

        var property = type.GetTypeInfo().GetDeclaredProperty($"{propertyName}Property");

        if (property != null)
            return (DependencyProperty?)property.GetValue(null);

        var baseType = type.GetTypeInfo().BaseType;

        if (baseType == typeof(object) || baseType == null)
            return null;

        return GetDependencyProperty(baseType, propertyName);
    }
}