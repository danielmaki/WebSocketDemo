using System.Threading;
using System.Threading.Tasks;

namespace WebSocketDemo.Services
{
    /// <summary>
    /// Service for receiving messages asynchronously.
    /// </summary>
    public interface IMessageReceiverService
    {
        /// <summary>
        /// Method that asynchronously receives one message and then returns.
        /// </summary>
        /// <returns>Asynchronous operation that completes until there is a message available.</returns>
        Task ReceiveMessage(CancellationToken cancellationToken);
    }
}
