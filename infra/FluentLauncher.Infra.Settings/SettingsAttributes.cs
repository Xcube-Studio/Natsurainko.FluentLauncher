using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentLauncher.Infra.Settings
{
    /// <summary>
    /// Must be applied to properties of a class which extends SettingsManagerBase
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SettingItemAttribute : Attribute
    {
        /// <summary>
        /// An optional converter to convert the value stored in the storage to the type of the property.<br/>If not specified, type casting will be used.
        /// </summary>
        public Type? Converter { get; init; }
        /// <summary>
        /// Default value of the setting item.<br/>null means default value is not provided.
        /// </summary>
        public object? Default { get; init; }

        /// <summary>
        /// Initialize a new instance of the SettingItemAttribute class
        /// </summary>
        /// <param name="key">Key of the setting item</param>
        /// <param name="type">Data type of the value stored</param>
        public SettingItemAttribute()
        {
        }

    }

    /// <summary>
    /// Indicate that the property is used as a settings container.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = true)]
    public class SettingsContainerAttribute : Attribute
    {
        public string ContainerName { get; }
        public Type ContainerType { get; }

        public SettingsContainerAttribute(Type type, string name)
        {
            ContainerType = type;
            ContainerName = name;
        }
    }

    /// <summary>
    /// Defines a collection of settings of the same type
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = true)]
    public class SettingsCollectionAttribute : Attribute
    {
        public string CollectionName { get; }
        public Type ElementType { get; }
        public Type? Converter { get; init; }

        public SettingsCollectionAttribute(Type type, string name)
        {
            CollectionName = name;
            ElementType = type;
        }
    }
}
