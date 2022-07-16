using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using WebSocketDemo.Models;
using WebSocketDemo.Services.Interfaces;

namespace WebSocketDemo.Services;

public class TestApiService : ITestApiService
{
    private readonly ILogger<TestApiService> logger;
    private readonly WebSocketClient<TestApi> client;

    public TestApiService(
        ILogger<TestApiService> logger, WebSocketClient<TestApi> client)
    {
        this.logger = logger;

        this.client = client;
    }

    public async Task ReceiveMessage(CancellationToken cancellationToken)
    {
        var message = await client.Receive(cancellationToken);

        if (message == string.Empty)
        {
            return;
        }

        try
        {
            logger.LogDebug($"Received message: {message}");
        }
        catch (Exception e)
        {
            logger.LogWarning($"Rejecting message due to unhandled exception: {e.Message} {e.StackTrace}");
        }
    }

    public Task SendHeartbeat(long requestId)
    {
        throw new NotImplementedException();
    }
}
