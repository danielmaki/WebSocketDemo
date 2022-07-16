using Microsoft.Extensions.Logging;
using WebSocketDemo.Models;
using WebSocketDemo.Services.Interfaces;

namespace WebSocketDemo.Services;

public class CdcApiService : ICdcApiService
{
    private readonly ILogger<CdcApiService> logger;
    private readonly WebSocketClient<CdcApi> client;

    public CdcApiService(ILogger<CdcApiService> logger, WebSocketClient<CdcApi> client)
    {
        this.logger = logger;
        this.client = client;
    }

    public async Task ReceiveMessage(CancellationToken cancellationToken)
    {
        var message = await client.Receive(cancellationToken);

        logger.LogDebug("Received message: {message}", message);
    }

    public Task SendHeartbeat(long requestId)
    {
        throw new NotImplementedException();
    }
}
