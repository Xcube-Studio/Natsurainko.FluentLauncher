using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Natsurainko.FluentLauncher.UserControls;

public sealed partial class NavigationBreadcrumbBar : UserControl, INotifyPropertyChanged
{
    private Stack<string[]> _backStack = new();

    public string LocalizationBasePath { get; set; } = "";

    public ObservableCollection<string> Items
    {
        get => field;
        set
        {
            // Save last path
            string[] lastPath = new string[field.Count];
            field.CopyTo(lastPath, 0);
            _backStack.Push(lastPath);

            // Replace
            field = value;
            OnPropertyChanged(nameof(Items));
        }
    } = new();

    public bool CanGoBack => _backStack.Count > 0;

    public event EventHandler<string[]>? ItemClicked;
    public event PropertyChangedEventHandler? PropertyChanged;

    public NavigationBreadcrumbBar()
    {
        InitializeComponent();
    }

    public void AddItem(string page)
    {
        _backStack.Push([.. Items]);
        Items.Add(page);
    }

    public void GoBack()
    {
        Items.Clear();
        string[] prevPath = _backStack.Pop();
        foreach (string item in prevPath)
        {
            Items.Add(item);
        }
    }

    private void bar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        ItemClicked?.Invoke(sender, Items.Take(args.Index + 1).ToArray());
        for (int i = Items.Count - 1; i > args.Index; i--)
        {
            Items.RemoveAt(i);
        }
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
