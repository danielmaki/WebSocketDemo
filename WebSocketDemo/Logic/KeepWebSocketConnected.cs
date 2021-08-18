using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using WebSocketDemo.Logic.Delegates;
using WebSocketDemo.Logic.Policies;
using WebSocketDemo.Models;
using WebSocketDemo.Services;

namespace WebSocketDemo.Logic
{
    public class KeepWebSocketConnected<T> : IBehavior where T : IApi, new()
    {
        private readonly ILogger<KeepWebSocketConnected<T>> logger;
        private readonly IRetryPolicy policy;

        private readonly WebSocketClient<T> client;

        private readonly Condition<WebSocketConnectionLost<T>> connectionLost;

        private readonly T api = new();

        private uint connectionLostCounter;

        public KeepWebSocketConnected(ILogger<KeepWebSocketConnected<T>> logger, IRetryPolicy policy, WebSocketClient<T> client,
            Condition<ApplicationStart> applicationStart, Condition<ApplicationStop> applicationStop, Condition<WebSocketConnectionLost<T>> connectionLost)
        {
            this.logger = logger;
            this.policy = policy;

            this.client = client;

            this.connectionLost = connectionLost;

            connectionLostCounter = 0;

            if (applicationStart != null)
                applicationStart.When += OnApplicationStart;

            if (applicationStop != null)
                applicationStop.When += OnApplicationStop;

            if (connectionLost != null)
                connectionLost.When += OnConnectionLost;
        }

        public void OnCreated()
        {
            logger.LogInformation($"I try to keep connected to the {api.Name}.");
        }

        public virtual Task Open()
        {
            return policy.Run(() => client.Connect());
        }

        private Task OnApplicationStart()
        {
            logger.LogDebug($"Establishing {api.Name} websocket connection on application startup.");

            return Open();
        }

        private async Task OnApplicationStop()
        {
            logger.LogDebug($"Closing {api.Name} websocket connection.");

            connectionLost.When -= OnConnectionLost;

            await client.Close();

            logger.LogInformation($"{api.Name} websocket is now closed.");
            logger.LogInformation($"Total times connection to {api.Name} was lost: {connectionLostCounter}.");
        }

        private Task OnConnectionLost()
        {
            connectionLostCounter += 1;

            logger.LogInformation($"{api.Name} websocket connection was lost for the {connectionLostCounter}. time, trying to re-establish connection.");

            return Open();
        }
    }
}
