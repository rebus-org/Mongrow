using System;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable EmptyGeneralCatchClause

namespace Mongrow.Internals;

class AsyncTask : IDisposable
{
    readonly CancellationTokenSource _cancellationTokenSource = new();
    readonly ManualResetEvent _stopped = new(initialState: false);
    readonly Func<CancellationToken, Task> _execute;
    readonly TimeSpan _delayBetweenExecutions;

    bool _disposed;

    public AsyncTask(TimeSpan delayBetweenExecutions, Func<CancellationToken, Task> execute)
    {
        _delayBetweenExecutions = delayBetweenExecutions;
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));

        Task.Run(Run);
    }

    async Task Run()
    {
        try
        {
            var cancellationToken = _cancellationTokenSource.Token;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_delayBetweenExecutions, cancellationToken);

                    await _execute(cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // we're quitting
                }
                catch (Exception)
                {
                }
            }
        }
        finally
        {
            _stopped.Set();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            _cancellationTokenSource?.Cancel();

            if (!_stopped.WaitOne(TimeSpan.FromSeconds(5)))
            {
                //Logger.Warning("Async task did not stop within 5 s timeout!!!");//
            }

            _cancellationTokenSource?.Dispose();
        }
        finally
        {
            _disposed = true;
        }
    }
}