using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Components;

public static class DispatcherQueueExtension
{
    public static void SynchronousTryEnqueue(this DispatcherQueue dispatcher, DispatcherQueueHandler callback)
        => SynchronousTryEnqueue(dispatcher, DispatcherQueuePriority.Normal, callback);

    public static void SynchronousTryEnqueue(this DispatcherQueue dispatcher, DispatcherQueuePriority priority, DispatcherQueueHandler callback)
    {
        bool taskDone = false;

        dispatcher.TryEnqueue(priority, () =>
        {
            callback.Invoke();
            taskDone = true;
        });

        while (!taskDone)
            Task.Delay(75).Wait();
    }
}
