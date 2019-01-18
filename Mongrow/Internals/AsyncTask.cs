using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mongrow.Internals
{
    class AsyncTask : IDisposable
    {
        readonly ManualResetEvent _stopped = new ManualResetEvent(false);
        readonly CancellationTokenSource _cancellationTokenSource;
        readonly TimeSpan _delayBetweenExecutions;
        readonly Func<CancellationToken, Task> _execute;

        bool _disposed;

        public AsyncTask(TimeSpan delayBetweenExecutions, Func<CancellationToken, Task> execute)
        {
            _delayBetweenExecutions = delayBetweenExecutions;
            _execute = execute;
            _cancellationTokenSource = new CancellationTokenSource();

            Task.Run(Run);
        }

        async Task Run()
        {
            try
            {
                var cancellationToken = _cancellationTokenSource.Token;

                while (true)
                {
                    await Wait(cancellationToken);

                    await Execute(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // we're quitting
            }
            finally
            {
                _stopped.Set();
            }
        }

        Task Wait(CancellationToken cancellationToken)
        {
            return Task.Delay(_delayBetweenExecutions, cancellationToken);
        }

        async Task Execute(CancellationToken cancellationToken)
        {
            try
            {
                await _execute(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // we're quitting
                throw;
            }
            catch (Exception exception)
            {
                //Logger.Error(exception, "Error when executing callback");
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
}