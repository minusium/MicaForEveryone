using Microsoft.UI.Dispatching;
using System;
using System.Threading.Tasks.Sources;
using WinRT;

namespace MicaForEveryone.App.Dispatching;

public partial class DispatcherQueueValueTaskSource : IValueTaskSource
{
    private DispatcherQueue _queue;

    public DispatcherQueueValueTaskSource(DispatcherQueue queue)
    {
        _queue = queue;
    }

    public void GetResult(short token)
    { }

    public ValueTaskSourceStatus GetStatus(short token)
        => _queue.HasThreadAccess ? ValueTaskSourceStatus.Succeeded : ValueTaskSourceStatus.Pending;

    public unsafe void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
    {
        DispatcherQueueProxyHandler* dispatcherQueueProxyHandler = DispatcherQueueProxyHandler.Create(continuation, state);
        int hResult;

        try
        {
            IDispatcherQueue* dispatcherQueue = (IDispatcherQueue*)((IWinRTObject)_queue).NativeObject.ThisPtr;
            bool success;

            hResult = dispatcherQueue->TryEnqueue(dispatcherQueueProxyHandler, &success);

            GC.KeepAlive(this);
        }
        finally
        {
            dispatcherQueueProxyHandler->Release();
        }

        if (hResult != 0)
        {
            ExceptionHelpers.ThrowExceptionForHR(hResult);
        }
    }
}