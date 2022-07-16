using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebSocketDemo.Domain.Delegates;
using WebSocketDemo.Wrappers;

namespace WebSocketDemo.Hosting;

public class AppLifetimeService : IHostedService
{
    private readonly ILogger<AppLifetimeService> logger;
    private readonly IHostApplicationLifetime applicationLifetime;
    private readonly BehaviorLifetimeService behaviorLifetime;
    private readonly Trigger<ApplicationStart> applicationStart;
    private readonly Trigger<ApplicationStop> applicationStop;

    public AppLifetimeService(ILogger<AppLifetimeService> logger,
        IHostApplicationLifetime applicationLifetime, BehaviorLifetimeService behaviorLifetime,
        Trigger<ApplicationStart> applicationStart, Trigger<ApplicationStop> applicationStop)
    {
        this.logger = logger;
        this.applicationLifetime = applicationLifetime;
        this.behaviorLifetime = behaviorLifetime;
        this.applicationStart = applicationStart;
        this.applicationStop = applicationStop;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("{serivce} has started", nameof(AppLifetimeService));

        applicationLifetime.ApplicationStarted.Register(() => applicationStart.InvokeWithoutSynchronization("application start", cancellationToken));
        applicationLifetime.ApplicationStopping.Register(() => applicationStop.Invoke("application stop", cancellationToken));

        await behaviorLifetime.EnableBehaviors();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("{serivce} is stopping", nameof(AppLifetimeService));

        return Task.CompletedTask;
    }
}
