using Microsoft.Extensions.Logging;
using WebSocketDemo.Domain.Behaviors.Interfaces;
using WebSocketDemo.Domain.Delegates;
using WebSocketDemo.Domain.Policies.Interfaces;
using WebSocketDemo.Models.Interfaces;
using WebSocketDemo.Services;
using WebSocketDemo.Wrappers;

namespace WebSocketDemo.Domain;

public class KeepWebSocketConnected<T> : IBehavior where T : IApi, new()
{
    private readonly ILogger<KeepWebSocketConnected<T>> logger;
    private readonly IRetryPolicy policy;
    private readonly WebSocketClient<T> client;
    private readonly Condition<WebSocketConnectionLost<T>> connectionLost;
    private readonly T api = new();
    private uint connectionLostCounter = 0;

    public KeepWebSocketConnected(ILogger<KeepWebSocketConnected<T>> logger, IRetryPolicy policy, WebSocketClient<T> client,
        Condition<ApplicationStart> applicationStart, Condition<ApplicationStop> applicationStop, Condition<WebSocketConnectionLost<T>> connectionLost)
    {
        this.logger = logger;
        this.policy = policy;
        this.client = client;
        this.connectionLost = connectionLost;

        if (applicationStart != null)
        {
            applicationStart.When += OnApplicationStart;
        }
        if (applicationStop != null)
        {
            applicationStop.When += OnApplicationStop;
        }
        if (connectionLost != null)
        {
            connectionLost.When += OnConnectionLost;
        }
    }

    public void EnableBehavior()
    {
        logger.LogInformation("I try to keep connected to the websocket {name}.", api.Name);
    }

    public virtual Task Open()
    {
        return policy.Run(() => client.Connect());
    }

    private Task OnApplicationStart()
    {
        logger.LogDebug("Establishing websocket connection to {name} on application startup.", api.Name);

        return Open();
    }

    private async Task OnApplicationStop()
    {
        logger.LogDebug("Closing websocket connection to {name}.", api.Name);

        connectionLost.When -= OnConnectionLost;
        await client.Close();

        logger.LogInformation("Websocket connection to {name} is now closed.", api.Name);
        logger.LogInformation("Total times connection to {name} was lost: {counter}.", api.Name, connectionLostCounter);
    }

    private Task OnConnectionLost()
    {
        connectionLostCounter += 1;

        logger.LogInformation("Websocket connection to {name} was lost for the {counter}. time, trying to re-establish connection.", api.Name, connectionLostCounter);

        return Open();
    }
}
