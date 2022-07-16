
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebSocketDemo.Domain.Policies.Interfaces;
using WebSocketDemo.Services.Interfaces;

namespace WebSocketDemo.Services;

public class MessageReceiverHostedService<T> : BackgroundService where T : IMessageReceiverService
{
    private readonly IServiceProvider services;

    public MessageReceiverHostedService(IServiceProvider services)
    {
        this.services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Create dependency injection scope and resolve requested service.
        using var scope = services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<T>();
        var policy = scope.ServiceProvider.GetRequiredService<IRetryPolicy>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MessageReceiverHostedService<T>>>();

        logger.LogInformation("Starting to receive messages.");

        // Receive messages until cancellation is requested.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await policy.Run(() => service.ReceiveMessage(stoppingToken));
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }

        logger.LogInformation("Stopping message receiver.");
    }
}
