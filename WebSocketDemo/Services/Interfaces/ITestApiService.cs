using System.Threading.Tasks;

namespace WebSocketDemo.Services
{
    public interface ITestApiService : IMessageReceiverService
    {
        Task SendHeartbeat(long requestId);
    }
}
