using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;

using WebSocketDemo.Logic;
using WebSocketDemo.Logic.Delegates;

namespace WebSocketDemo.Hosting;

public class AppLifetimeService : IHostLifetime, IDisposable
{
    private readonly ILogger<AppLifetimeService> logger;

    private readonly IDisposable applicationStartedCallback;
    private readonly ConsoleLifetime consoleLifetime;

    private readonly IHostApplicationLifetime appLifetime;
    private readonly Trigger<ApplicationStart> applicationStart;
    private readonly Trigger<ApplicationStop> applicationStop;

    public AppLifetimeService(ILogger<AppLifetimeService> logger, ConsoleLifetime consoleLifetime,
        IHostApplicationLifetime appLifetime, IBehavior[] behaviors,
        Trigger<ApplicationStart> applicationStart, Trigger<ApplicationStop> applicationStop)
    {
        this.logger = logger;
        this.consoleLifetime = consoleLifetime;

        this.appLifetime = appLifetime;
        this.applicationStart = applicationStart;
        this.applicationStop = applicationStop;

        applicationStartedCallback =
            this.appLifetime?.ApplicationStarted.Register(() =>
                this.applicationStart.InvokeWithoutSynchronization("application start"));

        foreach (var behavior in behaviors ?? Enumerable.Empty<IBehavior>())
        {
            behavior.OnCreated();
        }

        logger.LogInformation("AppLifetimeService started.");
    }

    public virtual Task WaitForStartAsync(CancellationToken cancellationToken)
    {
        return consoleLifetime.WaitForStartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Application is stopping.");
        await (applicationStop?.Invoke("application stop") ?? Task.CompletedTask);
        await consoleLifetime.StopAsync(cancellationToken);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        applicationStartedCallback?.Dispose();
    }
}
