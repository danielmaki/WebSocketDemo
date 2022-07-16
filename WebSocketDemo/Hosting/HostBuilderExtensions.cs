using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebSocketDemo.Domain.Behaviors.Interfaces;

namespace WebSocketDemo.Hosting;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseAppLifetime(this IHostBuilder self)
    {
        self.ConfigureServices(services =>
        {
            services.AddHostedService<AppLifetimeService>();
            services.AddSingleton<ApplicationCancellationTokenProvider>();
            services.AddTransient<BehaviorLifetimeService>();
            services.AddTransient(x => x.GetServices<IBehavior>().ToArray());
        });

        return self;
    }

    public static IHostBuilder UseAppServices(this IHostBuilder self)
    {
        self.ConfigureServices((context, services) =>
        {
            services.AddFactories();
            services.AddServices();
            services.AddDomain();
        });

        return self;
    }
}
