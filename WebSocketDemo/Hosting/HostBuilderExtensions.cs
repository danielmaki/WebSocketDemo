using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;

using WebSocketDemo.Logic;
using WebSocketDemo.Services.Providers;

namespace WebSocketDemo.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseLifetime(this IHostBuilder self)
        {
            self.ConfigureServices(services =>
            {
                services.AddTransient<ConsoleLifetime>();
                services.AddTransient(x => x.GetServices<IBehavior>().ToArray());
                services.AddSingleton<IHostLifetime, AppLifetimeService>();
                services.AddSingleton<ApplicationCancellationTokenProvider>();
            });

            return self;
        }

        public static IHostBuilder UseAppServices(this IHostBuilder self)
        {
            self.ConfigureServices((context, services) =>
            {
                services.AddModels();

                services.AddFactories();

                services.AddServices();

                services.AddLogic();
            });

            return self;
        }
    }
}
