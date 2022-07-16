using Microsoft.Extensions.Logging;
using WebSocketDemo.Models;
using WebSocketDemo.Services.Interfaces;

namespace WebSocketDemo.Services;

public class BinanceApiService : IBinanceApiService
{
    private readonly ILogger<BinanceApiService> logger;
    private readonly WebSocketClient<CdcApi> client;

    public BinanceApiService(ILogger<BinanceApiService> logger, WebSocketClient<CdcApi> client)
    {
        this.logger = logger;
        this.client = client;
    }

    public async Task ReceiveMessage(CancellationToken cancellationToken)
    {
        var message = await client.Receive(cancellationToken);

        logger.LogDebug("Received message: {message}", message);
    }
}
