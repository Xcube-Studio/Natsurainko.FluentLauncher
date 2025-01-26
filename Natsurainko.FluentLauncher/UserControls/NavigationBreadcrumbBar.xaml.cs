using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Natsurainko.FluentLauncher.UserControls;

public sealed partial class NavigationBreadcrumbBar : UserControl
{
    private Stack<string[]> _backStack = new();

    public string LocalizationBasePath { get; set; } = "";

    public ObservableCollection<string> Items { get; } = new();

    public bool CanGoBack => _backStack.Count > 0;

    public event EventHandler<string[]>? ItemClicked;

    public NavigationBreadcrumbBar()
    {
        InitializeComponent();
    }

    public void AddItem(string page)
    {
        _backStack.Push(Items.ToArray());
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
}
