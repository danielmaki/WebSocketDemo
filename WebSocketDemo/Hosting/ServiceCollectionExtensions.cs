using System.Net.WebSockets;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using WebSocketDemo.Domain.Behaviors;
using WebSocketDemo.Domain.Behaviors.Interfaces;
using WebSocketDemo.Domain.Delegates;
using WebSocketDemo.Domain.Policies;
using WebSocketDemo.Domain.Policies.Interfaces;
using WebSocketDemo.Factories;
using WebSocketDemo.Models;
using WebSocketDemo.Services;
using WebSocketDemo.Services.Interfaces;
using WebSocketDemo.Wrappers;

namespace WebSocketDemo.Hosting;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFactories(this IServiceCollection self)
    {
        self.AddTransient<ClientWebSocketFactory>();

        return self;
    }

    public static IServiceCollection AddServices(this IServiceCollection self)
    {
        self.AddSingleton<WebSocketClient<CdcApi>>();
        self.AddSingleton<ICdcApiService, CdcApiService>();
        self.AddSingleton<WebSocketClient<BinanceApi>>();
        self.AddSingleton<IBinanceApiService, BinanceApiService>();
        self.AddHostedService<MessageReceiverHostedService<ICdcApiService>>();

        return self;
    }

    public static IServiceCollection AddDomain(this IServiceCollection self)
    {
        self.AddBehaviors();
        self.AddConditions();
        self.AddTriggers();
        self.AddPolicies();

        return self;
    }

    public static IServiceCollection AddBehaviors(this IServiceCollection self)
    {
        self.AddBehavior<KeepWebSocketConnected<CdcApi>>();
        self.AddBehavior<KeepWebSocketConnected<BinanceApi>>();

        return self;
    }

    public static IServiceCollection AddConditions(this IServiceCollection self)
    {
        self.AddCondition<ApplicationStart>();
        self.AddCondition<ApplicationStop>();

        self.AddCondition<WebSocketConnected<CdcApi>>();
        self.AddCondition<WebSocketConnectionLost<CdcApi>>();
        self.AddCondition<WebSocketConnected<BinanceApi>>();
        self.AddCondition<WebSocketConnectionLost<BinanceApi>>();

        return self;
    }

    public static IServiceCollection AddTriggers(this IServiceCollection self)
    {
        self.AddTrigger<ApplicationStart>();
        self.AddTrigger<ApplicationStop>();

        self.AddTrigger<WebSocketConnected<CdcApi>>();
        self.AddTrigger<WebSocketConnectionLost<CdcApi>>();
        self.AddTrigger<WebSocketConnected<BinanceApi>>();
        self.AddTrigger<WebSocketConnectionLost<BinanceApi>>();

        return self;
    }

    public static IServiceCollection AddPolicies(this IServiceCollection self)
    {
        IAsyncPolicy websocketRetrypolicy = Policy.Handle<WebSocketException>().WaitAndRetryForeverAsync(x => TimeSpan.FromSeconds(0.1));
        self.AddSingleton<IRetryPolicy, WebsocketRetryPolicy>(x => new WebsocketRetryPolicy(websocketRetrypolicy));

        return self;
    }

    public static IServiceCollection AddBehavior<TBehavior>(this IServiceCollection self) where TBehavior : class, IBehavior
    {
        self.AddTransient<IBehavior, TBehavior>();

        return self;
    }

    public static IServiceCollection AddCondition<TDelegate>(this IServiceCollection self) where TDelegate : Delegate
    {
        self.AddTransient<Condition<TDelegate>>();
        self.AddSingleton<Filter<TDelegate>>();

        return self;
    }

    public static IServiceCollection AddTrigger<TDelegate>(this IServiceCollection self) where TDelegate : Delegate
    {
        self.AddTransient<Trigger<TDelegate>>();
        self.AddSingleton<MutableValue<TDelegate>>();

        return self;
    }
}
