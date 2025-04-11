using System;

namespace FluentLauncher.Infra.WinUI.ExtensionHost.Utils;

internal partial class RunOnDispose(Action callback) : IDisposable
{
    private bool IsDisposed;

    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
                callback();
            }
            IsDisposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
