// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.Xaml.Interactivity;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace Natsurainko.FluentLauncher.Components.Mvvm;

/// <summary>
/// Executes a specified <see cref="global::System.Windows.Input.ICommand"/> when invoked. 
/// </summary>
public sealed class ModifiedInvokeCommandAction : DependencyObject, IAction
{
    /// <summary>
    /// Identifies the <seealso cref="Command"/> dependency property.
    /// </summary>
    [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        "Command",
        typeof(ICommand),
        typeof(ModifiedInvokeCommandAction),
        new PropertyMetadata(null));

    /// <summary>
    /// Identifies the <seealso cref="CommandParameter"/> dependency property.
    /// </summary>
    [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
        "CommandParameter",
        typeof(object),
        typeof(ModifiedInvokeCommandAction),
        new PropertyMetadata(null));

    /// <summary>
    /// Identifies the <seealso cref="InputConverter"/> dependency property.
    /// </summary>
    [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly DependencyProperty InputConverterProperty = DependencyProperty.Register(
        "InputConverter",
        typeof(IValueConverter),
        typeof(ModifiedInvokeCommandAction),
        new PropertyMetadata(null));

    /// <summary>
    /// Identifies the <seealso cref="InputConverterParameter"/> dependency property.
    /// </summary>
    [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly DependencyProperty InputConverterParameterProperty = DependencyProperty.Register(
        "InputConverterParameter",
        typeof(object),
        typeof(ModifiedInvokeCommandAction),
        new PropertyMetadata(null));

    /// <summary>
    /// Identifies the <seealso cref="InputConverterLanguage"/> dependency property.
    /// </summary>
    [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly DependencyProperty InputConverterLanguageProperty = DependencyProperty.Register(
        "InputConverterLanguage",
        typeof(string),
        typeof(ModifiedInvokeCommandAction),
        new PropertyMetadata(string.Empty)); // Empty string means the invariant culture.

    /// <summary>
    /// Gets or sets the command this action should invoke. This is a dependency property.
    /// </summary>
    public ICommand Command
    {
        get
        {
            return (ICommand)this.GetValue(ModifiedInvokeCommandAction.CommandProperty);
        }
        set
        {
            this.SetValue(ModifiedInvokeCommandAction.CommandProperty, value);
        }
    }

    /// <summary>
    /// Gets or sets the parameter that is passed to <see cref="global::System.Windows.Input.ICommand.Execute(object)"/>.
    /// If this is not set, the parameter from the <seealso cref="Execute(object, object)"/> method will be used.
    /// This is an optional dependency property.
    /// </summary>
    public object CommandParameter
    {
        get
        {
            return this.GetValue(ModifiedInvokeCommandAction.CommandParameterProperty);
        }
        set
        {
            this.SetValue(ModifiedInvokeCommandAction.CommandParameterProperty, value);
        }
    }

    /// <summary>
    /// Gets or sets the converter that is run on the parameter from the <seealso cref="Execute(object, object)"/> method.
    /// This is an optional dependency property.
    /// </summary>
    public IValueConverter InputConverter
    {
        get
        {
            return (IValueConverter)this.GetValue(ModifiedInvokeCommandAction.InputConverterProperty);
        }
        set
        {
            this.SetValue(ModifiedInvokeCommandAction.InputConverterProperty, value);
        }
    }

    /// <summary>
    /// Gets or sets the parameter that is passed to the <see cref="IValueConverter.Convert"/>
    /// method of <see cref="InputConverter"/>.
    /// This is an optional dependency property.
    /// </summary>
    public object InputConverterParameter
    {
        get
        {
            return this.GetValue(ModifiedInvokeCommandAction.InputConverterParameterProperty);
        }
        set
        {
            this.SetValue(ModifiedInvokeCommandAction.InputConverterParameterProperty, value);
        }
    }

    /// <summary>
    /// Gets or sets the language that is passed to the <see cref="IValueConverter.Convert"/>
    /// method of <see cref="InputConverter"/>.
    /// This is an optional dependency property.
    /// </summary>
    public string InputConverterLanguage
    {
        get
        {
            return (string)this.GetValue(ModifiedInvokeCommandAction.InputConverterLanguageProperty);
        }
        set
        {
            this.SetValue(ModifiedInvokeCommandAction.InputConverterLanguageProperty, value);
        }
    }

    /// <summary>
    /// Executes the action.
    /// </summary>
    /// <param name="sender">The <see cref="global::System.Object"/> that is passed to the action by the behavior. Generally this is <seealso cref="Microsoft.Xaml.Interactivity.IBehavior.AssociatedObject"/> or a target object.</param>
    /// <param name="parameter">The value of this parameter is determined by the caller.</param>
    /// <returns>True if the command is successfully executed; else false.</returns>
    public object Execute(object sender, object parameter)
    {
        if (this.Command == null)
        {
            return false;
        }

        object resolvedParameter;
        if (this.ReadLocalValue(ModifiedInvokeCommandAction.CommandParameterProperty) != DependencyProperty.UnsetValue)
        {
            resolvedParameter = this.CommandParameter;
        }
        else if (this.InputConverter != null)
        {
            resolvedParameter = this.InputConverter.Convert(
                (sender, parameter),
                typeof(object),
                this.InputConverterParameter,
                this.InputConverterLanguage);
        }
        else
        {
            resolvedParameter = (sender, parameter);
        }

        if (!this.Command.CanExecute(resolvedParameter))
        {
            return false;
        }

        this.Command.Execute(resolvedParameter);
        return true;
    }
}