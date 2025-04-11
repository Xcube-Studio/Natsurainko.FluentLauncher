using System;
using System.Collections.Generic;

namespace FluentLauncher.Infra.WinUI.ExtensionHost.Utils;

internal partial class DisposableCollection : HashSet<IDisposable>, IDisposable
{
    private bool IsDisposed;

    public DisposableCollection(IEnumerable<IDisposable> collection) : base(collection) { }

    public DisposableCollection(params IDisposable[] items) : base(items) { }

    public bool AddRange(params IDisposable[] items) => AddRange((IEnumerable<IDisposable>)items);

    public bool AddRange(IEnumerable<IDisposable> items)
    {
        bool result = false;

        foreach (IDisposable item in items)
            result |= Add(item);

        return result;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
                Reset();
            }

            IsDisposed = true;
        }
    }

    public void Reset()
    {
        foreach (var item in this)
        {
            item.Dispose();
        }
        Clear();
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
