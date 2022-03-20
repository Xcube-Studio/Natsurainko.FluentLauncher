using FluentLauncher.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace FluentLauncher.Classes
{
    public static class ExtensionMethods
    {
        public static void AddWithUpdate(this List<MinecraftFolder> list, MinecraftFolder add)
        {
            list.Add(add);
            ShareResource.MinecraftFolders = list;
        }

        public static void AddWithUpdate(this List<JavaRuntimeEnvironment> list, JavaRuntimeEnvironment add)
        {
            list.Add(add);
            ShareResource.JavaRuntimeEnvironments = list;
        }

        public static void AddWithUpdate(this List<MinecraftAccount> list, MinecraftAccount add)
        {
            list.Add(add);
            ShareResource.MinecraftAccounts = list;
        }

        public static void SetItemsSource(this ItemsControl control, object source)
        {
            control.ItemsSource = null;
            control.ItemsSource = source;
        }

        public static void SetSelectedItem<T>(this Selector selector, T source)
            => selector.SelectedIndex = GetIndex((List<T>)selector.ItemsSource, source);

        public static int GetIndex<T>(List<T> items, T item)
        {
            if (item == null || items == null)
                return -1;

            for (int i = 0; i < items.Count; i++)
                if (item.Equals(items[i]))
                    return i;

            return -1;
        }

        public static async Task<int> GetJavaVersionAsync(this JavaRuntimeEnvironment java)
        {
            var info = await JavaHelper.GetInfo(java);

            if (info.JAVA_VERSION.StartsWith("1.8."))
                return 8;
            if (info.JAVA_VERSION.StartsWith("11."))
                return 11;
            if (info.JAVA_VERSION.StartsWith("16."))
                return 16;
            if (info.JAVA_VERSION.StartsWith("17."))
                return 17;

            return Convert.ToInt32(info.JAVA_VERSION.Split('.')[0]);
        }
    }
}
