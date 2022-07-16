using System.Threading.Tasks;

namespace WebSocketDemo.Services.Interfaces;

public interface ITestApiService : IMessageReceiverService
{
    Task SendHeartbeat(long requestId);
}
