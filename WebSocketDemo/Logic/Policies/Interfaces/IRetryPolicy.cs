using System;
using System.Threading.Tasks;

namespace WebSocketDemo.Logic.Policies
{
    /// <summary>
    /// Handles execution of asynchronous actions.
    /// </summary>
    public interface IRetryPolicy
    {
        /// <summary>
        /// Runs code inside policy.
        /// </summary>
        /// <param name="action">Asynchronous action to be executed.</param>
        /// <returns>Asynchronous operation that completes when the action is executed successfully.</returns>
        Task Run(Func<Task> action);
    }
}
