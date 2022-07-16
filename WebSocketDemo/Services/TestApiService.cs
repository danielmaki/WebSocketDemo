using Microsoft.Extensions.Logging;
using WebSocketDemo.Models;
using WebSocketDemo.Services.Interfaces;

namespace WebSocketDemo.Services;

public class TestApiService : ITestApiService
{
    private readonly ILogger<TestApiService> logger;
    private readonly WebSocketClient<TestApi> client;

    public TestApiService(ILogger<TestApiService> logger, WebSocketClient<TestApi> client)
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
