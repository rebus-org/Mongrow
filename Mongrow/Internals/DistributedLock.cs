using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongolianBarbecue.Exceptions;
using MongolianBarbecue.Model;
// ReSharper disable ArgumentsStyleAnonymousFunction
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable ArgumentsStyleOther

namespace Mongrow.Internals
{
    class MongoDbDistributedLock
    {
        readonly string _lockId;
        readonly string _description;
        readonly MongolianBarbecue.Config _config;

        AsyncTask _renewTask;
        bool _lockHeld;

        public MongoDbDistributedLock(IMongoDatabase database, string lockId, string description, string collectionName)
        {
            _lockId = lockId;
            _description = description;
            _config = new MongolianBarbecue.Config(
                database,
                collectionName,
                defaultMessageLeaseSeconds: 60,
                maxDeliveryAttempts: int.MaxValue
            );
        }

        public async Task<bool> TryAcquire(CancellationToken cancellationToken = default(CancellationToken), int timeoutSeconds = 15)
        {
            try
            {
                await Acquire(cancellationToken, timeoutSeconds);
                return true;
            }
            catch (LockAcquisitionTimeoutException)
            {
                return false;
            }
        }

        class LockAcquisitionTimeoutException : Exception
        {
            public LockAcquisitionTimeoutException(string message) : base(message)
            {
            }
        }

        public async Task Acquire(CancellationToken cancellationToken = default(CancellationToken), int timeoutSeconds = 15)
        {
            if (_lockHeld) return;

            await EnsureLockExists(_lockId);

            await AcquireLock(_lockId, cancellationToken, timeout: TimeSpan.FromSeconds(timeoutSeconds));

            StartPeriodicRenewalTask(_lockId);
        }

        public async Task Release()
        {
            if (!_lockHeld) return;

            StopPeriodicRenewalTask();

            try
            {
                await ReleaseLock(_lockId);
            }
            finally
            {
                _lockHeld = false;
            }
        }

        async Task ReleaseLock(string lockName)
        {
            var consumer = _config.CreateConsumer(lockName);

            // ignore exceptions here, because the lock's lease will eventually expire anyway
            try
            {
                await consumer.Nack(_lockId);
            }
            catch (Exception exception)
            {
                //Logger.Information(exception, "Lock {lockId} could NOT be released. Lock will automatically expire in about 1 minute", _lockId);
            }
        }

        void StopPeriodicRenewalTask()
        {
            //Logger.WriteVerbose("Stopping lock renewal for {lockId}", _lockId);
            _renewTask?.Dispose();
        }

        async Task AcquireLock(string lockId, CancellationToken cancellationToken, TimeSpan timeout)
        {
            var consumer = _config.CreateConsumer(_lockId);
            var stopwatch = Stopwatch.StartNew();
            var attempt = 0;

            while (stopwatch.Elapsed < timeout)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var message = await consumer.GetNextAsync();

                // if lock was acquired, return
                if (message != null)
                {
                    AssertMessageIdMatchesLockName(message, lockId);
                    _lockHeld = true;
                    //Logger.Write(_logLevel, "Lock {lockId} acquired", lockId);
                    return;
                }

                attempt++;

                //Logger.WriteVerbose("Could not acquire lock {lockId} after {count} attempts over {timeWaited} waiting time",
                //    _lockId, attempt, stopwatch.Elapsed);

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }

            throw new LockAcquisitionTimeoutException($@"Could not acquire lock '{lockId}' within {timeout} timeout");
        }

        static void AssertMessageIdMatchesLockName(ReceivedMessage message, string lockId)
        {
            if (message.MessageId != lockId)
            {
                throw new ApplicationException(
                    $@"The received lock named '{message.MessageId}' did not match the expected name '{lockId}'.

This is an indication that something is very wrong, because the locks are implemented by having
a single message in each queue, where the message's ID and the queue's name are the same.");
            }
        }

        void StartPeriodicRenewalTask(string lockId)
        {
            //Logger.WriteVerbose("Starting periodic lock renewal for {lockId}", lockId);

            var consumer = _config.CreateConsumer(lockId);

            _renewTask = new AsyncTask(
                delayBetweenExecutions: TimeSpan.FromSeconds(20),
                execute: async token =>
                {
                    try
                    {
                        await consumer.Renew(lockId);

                        //Logger.WriteVerbose("Lock {lockId} lease successfully renewed", lockId);
                    }
                    catch (Exception exception)
                    {
                        //Logger.WriteVerbose(exception, "Could not renew lock {lockId}", lockId);
                    }
                }
            );
        }

        public void Dispose()
        {
            Task.Run(Release);
        }

        async Task EnsureLockExists(string lockName)
        {
            var producer = _config.CreateProducer();

            var headers = new Dictionary<string, string>
            {
                {"id", lockName}
            };

            try
            {
                await producer.SendAsync(lockName, new Message(headers, Encoding.ASCII.GetBytes(_description)));

                //Logger.WriteVerbose("The lock {lockId} was created", lockName);
            }
            catch (UniqueMessageIdViolationException)
            {
            }
        }
    }
}