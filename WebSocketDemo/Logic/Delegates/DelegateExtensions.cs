using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebSocketDemo.Logic.Delegates;

public static class DelegateExtensions
{
    /// <summary>
    /// Executes a delegate in parallel and completes when all delegates in an invocation list are completed.
    /// </summary>
    /// <param name="self">The delegate to execute.</param>
    /// <param name="args">Argument list that is passed to delegates when invoked.</param>
    /// <returns>Asynchronous task that completes when all delegates in an invocation list are completed.</returns>
    public static Task InvokeParallelAsync(this Delegate self, params object[] args)
    {
        if (self == null)
        {
            throw new ArgumentNullException(nameof(self));
        }

        return Task.WhenAll(self.GetInvocationList().Select(x =>
        {
            var result = x.DynamicInvoke(args);

            // Return asynchronous task if Delegate is asynchronous.
            if (result is Task task)
            {
                return task;
            }

            // Otherwise, Delegate has already completed synchronously.
            return Task.CompletedTask;
        }));
    }

    /// <summary>
    /// Executes a delegate in parallel and completes when all delegates in an invocation list are completed.
    /// </summary>
    /// <typeparam name="TResult">Result type that is returned by all delegates.</typeparam>
    /// <param name="self">The delegate to execute.</param>
    /// <param name="args">Argument list that is passed to delegates when invoked.</param>
    /// <returns>Asynchronous task that completes when all delegates in an invocation list are completed.</returns>
    public static Task<TResult[]> InvokeParallelAsync<TResult>(this Delegate self, params object[] args)
    {
        if (self == null)
        {
            throw new ArgumentNullException(nameof(self));
        }

        return Task.WhenAll(self.GetInvocationList().Select(x =>
        {
            var result = x.DynamicInvoke(args);

            // Return asynchronous task if Delegate is asynchronous.
            if (result is Task<TResult> task)
            {
                return task;
            }

            // Otherwise, Delegate has already completed synchronously.
            return Task.FromResult((TResult)result);
        }));
    }
}
