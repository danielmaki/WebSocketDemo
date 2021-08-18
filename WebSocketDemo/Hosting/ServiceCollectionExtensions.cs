using System;
using System.Net.WebSockets;

using Microsoft.Extensions.DependencyInjection;

using Polly;

using WebSocketDemo.Factories;
using WebSocketDemo.Logic;
using WebSocketDemo.Logic.Delegates;
using WebSocketDemo.Logic.Policies;
using WebSocketDemo.Models;
using WebSocketDemo.Services;
using WebSocketDemo.Wrappers;

namespace WebSocketDemo.Hosting
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddModels(this IServiceCollection self)
        {
            self.AddSingleton<IApi, TestApi>();

            return self;
        }

        public static IServiceCollection AddFactories(this IServiceCollection self)
        {
            self.AddTransient<ClientWebSocketFactory>();

            return self;
        }

        public static IServiceCollection AddServices(this IServiceCollection self)
        {
            self.AddSingleton<WebSocketClient<TestApi>>();
            self.AddSingleton<ITestApiService, TestApiService>();
            self.AddHostedService<MessageReceiverHostedService<ITestApiService>>();

            return self;
        }

        public static IServiceCollection AddTrigger<TDelegate>(this IServiceCollection self) where TDelegate : Delegate
        {
            self.AddTransient<Trigger<TDelegate>>();
            self.AddSingleton<MutableValue<TDelegate>>();

            return self;
        }

        public static IServiceCollection AddCondition<TDelegate>(this IServiceCollection self) where TDelegate : Delegate
        {
            self.AddTransient<Condition<TDelegate>>();
            self.AddSingleton<Filter<TDelegate>>();

            return self;
        }

        public static IServiceCollection AddTriggers(this IServiceCollection self)
        {
            self.AddTrigger<ApplicationStart>();
            self.AddTrigger<ApplicationStop>();

            self.AddTrigger<WebSocketConnected<TestApi>>();
            self.AddTrigger<WebSocketConnectionLost<TestApi>>();

            return self;
        }

        public static IServiceCollection AddConditions(this IServiceCollection self)
        {
            self.AddCondition<ApplicationStart>();
            self.AddCondition<ApplicationStop>();

            self.AddCondition<WebSocketConnected<TestApi>>();
            self.AddCondition<WebSocketConnectionLost<TestApi>>();

            return self;
        }

        public static IServiceCollection AddBehavior<TBehavior>(this IServiceCollection self) where TBehavior : class, IBehavior
        {
            self.AddTransient<IBehavior, TBehavior>();

            return self;
        }

        public static IServiceCollection AddBehaviors(this IServiceCollection self)
        {
            self.AddBehavior<KeepWebSocketConnected<TestApi>>();

            return self;
        }

        public static IServiceCollection AddPolicies(this IServiceCollection self)
        {
            IAsyncPolicy websocketRetrypolicy = Policy.Handle<WebSocketException>().WaitAndRetryForeverAsync(x => TimeSpan.FromSeconds(0.1));
            self.AddSingleton<IRetryPolicy, WebsocketRetryPolicy>(x => new WebsocketRetryPolicy(websocketRetrypolicy));

            return self;
        }

        public static IServiceCollection AddLogic(this IServiceCollection self)
        {
            self.AddTriggers();

            self.AddConditions();

            self.AddBehaviors();

            self.AddPolicies();

            return self;
        }
    }
}
