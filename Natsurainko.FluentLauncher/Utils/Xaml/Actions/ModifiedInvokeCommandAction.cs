// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.Xaml.Interactivity;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace Natsurainko.FluentLauncher.Utils.Xaml.Actions;

/// <summary>
/// Executes a specified <see cref="ICommand"/> when invoked. 
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
            return (ICommand)GetValue(CommandProperty);
        }
        set
        {
            SetValue(CommandProperty, value);
        }
    }

    /// <summary>
    /// Gets or sets the parameter that is passed to <see cref="ICommand.Execute(object)"/>.
    /// If this is not set, the parameter from the <seealso cref="Execute(object, object)"/> method will be used.
    /// This is an optional dependency property.
    /// </summary>
    public object CommandParameter
    {
        get
        {
            return GetValue(CommandParameterProperty);
        }
        set
        {
            SetValue(CommandParameterProperty, value);
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
            return (IValueConverter)GetValue(InputConverterProperty);
        }
        set
        {
            SetValue(InputConverterProperty, value);
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
            return GetValue(InputConverterParameterProperty);
        }
        set
        {
            SetValue(InputConverterParameterProperty, value);
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
            return (string)GetValue(InputConverterLanguageProperty);
        }
        set
        {
            SetValue(InputConverterLanguageProperty, value);
        }
    }

    /// <summary>
    /// Executes the action.
    /// </summary>
    /// <param name="sender">The <see cref="System.Object"/> that is passed to the action by the behavior. Generally this is <seealso cref="IBehavior.AssociatedObject"/> or a target object.</param>
    /// <param name="parameter">The value of this parameter is determined by the caller.</param>
    /// <returns>True if the command is successfully executed; else false.</returns>
    public object Execute(object sender, object parameter)
    {
        if (Command == null)
        {
            return false;
        }

        object resolvedParameter;
        if (ReadLocalValue(CommandParameterProperty) != DependencyProperty.UnsetValue)
        {
            resolvedParameter = CommandParameter;
        }
        else if (InputConverter != null)
        {
            resolvedParameter = InputConverter.Convert(
                (sender, parameter),
                typeof(object),
                InputConverterParameter,
                InputConverterLanguage);
        }
        else
        {
            resolvedParameter = (sender, parameter);
        }

        if (!Command.CanExecute(resolvedParameter))
        {
            return false;
        }

        Command.Execute(resolvedParameter);
        return true;
    }
}