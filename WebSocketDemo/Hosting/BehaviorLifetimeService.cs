using Microsoft.Extensions.Logging;
using WebSocketDemo.Domain.Behaviors.Interfaces;

namespace WebSocketDemo.Hosting;

public class BehaviorLifetimeService
{
    private readonly ILogger<BehaviorLifetimeService> logger;
    private readonly IBehavior[] behaviors;

    public BehaviorLifetimeService(ILogger<BehaviorLifetimeService> logger, IBehavior[] behaviors)
    {
        this.logger = logger;
        this.behaviors = behaviors;
    }

    public Task EnableBehaviors()
    {
        logger.LogInformation("Enabling behaviors");

        // TODO: Implement feature flags for behaviors
        foreach (var behavior in behaviors ?? Enumerable.Empty<IBehavior>())
        {
            behavior.EnableBehavior();
        }

        return Task.CompletedTask;
    }
}
