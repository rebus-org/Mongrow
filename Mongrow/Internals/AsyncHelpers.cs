using System;
using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable AsyncVoidLambda

namespace Mongrow.Internals
{
    static class AsyncHelpers
    {
        /// <summary>
        /// Executes a task synchronously on the calling thread by installing a temporary synchronization context that queues continuations
        ///  </summary>
        public static void RunSync(Func<Task> task)
        {
            var currentContext = SynchronizationContext.Current;
            var customContext = new CustomSynchronizationContext(task);

            try
            {
                SynchronizationContext.SetSynchronizationContext(customContext);

                customContext.Run();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(currentContext);
            }
        }

        /// <summary>
        /// Synchronization context that can be "pumped" in order to have it execute continuations posted back to it
        /// </summary>
        class CustomSynchronizationContext(Func<Task> task) : SynchronizationContext
        {
            readonly ConcurrentQueue<Tuple<SendOrPostCallback, object>> _items = new();
            readonly AutoResetEvent _workItemsWaiting = new(initialState: false);

            ExceptionDispatchInfo _caughtException;

            bool _done;

            public override void Post(SendOrPostCallback function, object state)
            {
                _items.Enqueue(Tuple.Create(function, state));
                _workItemsWaiting.Set();
            }

            /// <summary>
            /// Enqueues the function to be executed and executes all resulting continuations until it is completely done
            /// </summary>
            public void Run()
            {
                Post(async _ =>
                {
                    try
                    {
                        await task();
                    }
                    catch (Exception exception)
                    {
                        _caughtException = ExceptionDispatchInfo.Capture(exception);
                        throw;
                    }
                    finally
                    {
                        Post(_ => _done = true, null);
                    }
                }, null);

                while (!_done)
                {
                    if (_items.TryDequeue(out var item))
                    {
                        item.Item1(item.Item2);

                        if (_caughtException == null) continue;

                        _caughtException.Throw();
                    }
                    else
                    {
                        _workItemsWaiting.WaitOne();
                    }
                }
            }

            public override void Send(SendOrPostCallback d, object state) => throw new NotSupportedException("Cannot send to same thread");

            public override SynchronizationContext CreateCopy() => this;
        }
    }
}