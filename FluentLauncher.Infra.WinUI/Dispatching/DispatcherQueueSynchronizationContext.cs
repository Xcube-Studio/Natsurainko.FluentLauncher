using Microsoft.UI.Dispatching;
using System;
using System.Threading;
using WinRT;

namespace FluentLauncher.Infra.WinUI.Dispatching;

// fix async exception handing
// https://github.com/microsoft/microsoft-ui-xaml/issues/10447#issuecomment-2800107924
public class DispatcherQueueSynchronizationContext : SynchronizationContext
{
    private readonly DispatcherQueue _dispatcherQueue;

    public DispatcherQueueSynchronizationContext(DispatcherQueue dispatcherQueue)
    {
        ArgumentNullException.ThrowIfNull(dispatcherQueue);
        _dispatcherQueue = dispatcherQueue;
    }

    public override void Post(SendOrPostCallback callback, object? state)
    {
        ArgumentNullException.ThrowIfNull(callback);

        _dispatcherQueue.TryEnqueue(delegate
        {
            try
            {
                callback(state);
            }
            catch (Exception ex)
            {
                ExceptionHelpers.ReportUnhandledError(ex);
            }
        });
    }

    public override void Send(SendOrPostCallback d, object? state) => throw new NotSupportedException("The send method is not supported, use Post instead.");

    public override SynchronizationContext CreateCopy() => new DispatcherQueueSynchronizationContext(_dispatcherQueue);
}
