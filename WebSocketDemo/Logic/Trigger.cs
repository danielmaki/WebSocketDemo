using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using WebSocketDemo.Logic.Delegates;
using WebSocketDemo.Models.Wrappers;
using WebSocketDemo.Services.Providers;

namespace WebSocketDemo.Logic
{
    /// <summary>
    /// A class that is capable of triggering an event.
    /// </summary>
    /// <typeparam name="T">Type of event delegate to trigger.</typeparam>
    /// <remarks>Delegate can only be triggered here. Use Condition if you need to attach listeners to it.</remarks>
    public class Trigger<T> where T : Delegate
    {
        private readonly ILogger<Trigger<T>> logger;
        private readonly MutableValue<T> value;
        private readonly Func<WaitCallback, bool> backgroundQueue;
        private readonly ApplicationCancellationTokenProvider applicationCancellationToken;

        /// <summary>
        /// Constructs an instance of Trigger.
        /// </summary>
        /// <param name="logger">Logger that logs invocations.</param>
        /// <param name="value">Mutable value that holds the Delegate.</param>
        /// <param name="applicationCancellationToken">Application cancellation token that cancels background executions.</param>
        /// <param name="backgroundQueue">Lambda that queues a task for background execution.</param>
        public Trigger(ILogger<Trigger<T>> logger, MutableValue<T> value, ApplicationCancellationTokenProvider applicationCancellationToken,
            Func<WaitCallback, bool> backgroundQueue = null)
        {
            this.logger = logger;
            this.value = value;
            this.applicationCancellationToken = applicationCancellationToken;
            this.backgroundQueue = backgroundQueue ?? ThreadPool.QueueUserWorkItem;
        }

        /// <summary>
        /// Invokes a trigger and ensures that all Conditions are synchronized.
        /// </summary>
        /// <param name="args">Arguments that are passed to underlying Delegate.</param>
        /// <returns>Asynchronous operation that completes when all Condition listeners are completed.</returns>
        /// <remarks>Invocation happens in parallel so ordering when Conditions are invoked is not guaranteed.</remarks>
        public virtual Task Invoke(string description, params object[] args)
        {
            logger.LogDebug($"Trigger {description}");

            return value?.Value?.InvokeParallelAsync(args) ?? Task.CompletedTask;
        }

        /// <summary>
        /// Invokes a trigger without synchronization so there is no guarantee when Condition listeners are invoked.
        /// </summary>
        /// <param name="args">Arguments that are passed to underlying Delegate.</param>
        /// <remarks>This method never blocks. There is no guarantee when Condition listeners are invoked.</remarks>
        public virtual void InvokeWithoutSynchronization(string description, params object[] args)
        {
            backgroundQueue(s =>
            {
                try
                {
                    logger.LogDebug($"Trigger {description}");

                    Invoke(description, args).Wait(applicationCancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    // Omit background operation canceled events since when application
                    // is shutting down and there are outstanding background events,
                    // we can assume that they will be canceled and there is nothing
                    // that we can do about it.
                }
            });
        }
    }
}
