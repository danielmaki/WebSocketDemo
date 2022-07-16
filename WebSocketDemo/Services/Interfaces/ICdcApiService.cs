namespace WebSocketDemo.Services.Interfaces;

public interface ICdcApiService : IMessageReceiverService
{
    Task SendHeartbeat(long requestId);
}
