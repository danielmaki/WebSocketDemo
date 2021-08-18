using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using WebSocketDemo.Factories;
using WebSocketDemo.Models;
using WebSocketDemo.Logic;
using WebSocketDemo.Logic.Delegates;

namespace WebSocketDemo.Services
{
    public class WebSocketClient<T> : IDisposable where T : IApi, new()
    {
        private readonly ILogger<WebSocketClient<T>> logger;

        private readonly ClientWebSocketFactory clientWebSocketFactory;
        private ClientWebSocket client;

        private readonly Trigger<WebSocketConnected<T>> wsConnectedTrigger;
        private readonly Trigger<WebSocketConnectionLost<T>> wsConnectionLostTrigger;

        private WebSocketState connectionStatus;

        private TaskCompletionSource<object> connected;
        public virtual bool IsConnected => connected.Task.IsCompletedSuccessfully;

        private readonly T api = new();

        public WebSocketClient(ILogger<WebSocketClient<T>> logger,
            ClientWebSocketFactory clientWebSocketFactory,
            Trigger<WebSocketConnected<T>> wsConnectedTrigger, Trigger<WebSocketConnectionLost<T>> wsConnectionLostTrigger)
        {
            this.logger = logger;

            this.clientWebSocketFactory = clientWebSocketFactory;

            CreateNewClient();

            connected = new TaskCompletionSource<object>();

            this.wsConnectedTrigger = wsConnectedTrigger;
            this.wsConnectionLostTrigger = wsConnectionLostTrigger;
        }

        private void CreateNewClient()
        {
            DisposeClient();
            client = clientWebSocketFactory.Create();
            connectionStatus = client.State;
        }

        public async Task Connect(CancellationToken cancellationToken = default)
        {
            logger.LogDebug($"Connecting to {api.Name} websocket.");

            if (client.State != WebSocketState.None)
                CreateNewClient();

            try
            {
                await client.ConnectAsync(api.Endpoint, cancellationToken);
            }
            catch (TaskCanceledException e)
            {
                logger.LogError($"Attempt to connect to {api.Endpoint} websocket was canceled: {e}");
                connected.SetCanceled(cancellationToken);
                throw;
            }
            catch (WebSocketException e)
            {
                logger.LogError($"Attempt to connect to {api.Endpoint} websocket failed, retrying...\n{e.Message}");
                throw;
            }
            catch (Exception e)
            {
                logger.LogError($"Error connecting to {api.Endpoint} websocket: {e.Message}");
                connected.SetException(e);
                throw;
            }

            logger.LogInformation($"Established connection to {api.Name} at endpoint {api.Endpoint}.");

            connected.SetResult(null);

            await wsConnectedTrigger.Invoke($"websocket connected to {api.Name}");
        }

        public async Task<string> Receive(CancellationToken cancellationToken = default)
        {
            var buffer = new ArraySegment<byte>(new byte[1024 * 4]);
            var data = string.Empty;

            await WaitForConnection(cancellationToken);

            try
            {
                WebSocketReceiveResult result;
                using var ms = new MemoryStream();
                do
                {
                    result = await client.ReceiveAsync(buffer, cancellationToken);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                } while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Close)
                    logger.LogWarning($"{api.Name} websocket received Close message, connection now closed.");
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    using var reader = new StreamReader(ms, Encoding.UTF8);
                    data = await reader.ReadToEndAsync();
                }
            }
            catch (TaskCanceledException)
            {
                logger.LogWarning($"Receive operation for {api.Name} websocket was canceled.");
            }
            catch (Exception e)
            {
                logger.LogError($"Error while receiving message from {api.Name} websocket: {e.Message}");
            }

            return data;
        }

        public virtual async Task Send(string data, CancellationToken cancellationToken = default)
        {
            await WaitForConnection(cancellationToken);

            try
            {
                await client.SendAsync(Encoding.UTF8.GetBytes(data), WebSocketMessageType.Text, true, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                logger.LogWarning($"Send operation {api.Name} websocket was canceled.");
            }
            catch (Exception e)
            {
                logger.LogError($"Error while trying to send message to {api.Name} websocket: {e.Message}");
            }
        }

        private void OnConnectionStatusChanged()
        {
            logger.LogDebug($"{api.Name} websocket connection status has changed from '{connectionStatus}' to '{client.State}'.");

            connectionStatus = client.State;

            if (connectionStatus == WebSocketState.CloseReceived || connectionStatus == WebSocketState.Closed || connectionStatus == WebSocketState.Aborted)
            {
                // Try cancelling old connection wait and create new one.
                connected.TrySetCanceled();
                connected = new TaskCompletionSource<object>();

                wsConnectionLostTrigger?.InvokeWithoutSynchronization($"websocket connection lost for {api.Name}");
            }
        }

        private async Task WaitForConnection(CancellationToken cancellationToken)
        {
            if (connectionStatus != client.State)
                OnConnectionStatusChanged();

            if (!IsConnected)
            {
                logger.LogTrace($"Waiting for {api.Name} websocket connection.");

                using (cancellationToken.Register(() => connected.TrySetCanceled()))
                {
                    try
                    {
                        await connected.Task;
                    }
                    catch (TaskCanceledException)
                    {
                        logger.LogInformation($"Connection to {api.Name} websocket was canceled.");
                        throw;
                    }
                }
            }
        }

        public async Task Close(CancellationToken cancellationToken = default)
        {
            logger.LogTrace($"Closing {api.Name} websocket connection.");

            connectionStatus = client.State;

            if (connectionStatus == WebSocketState.Connecting || connectionStatus == WebSocketState.Open)
            {
                try
                {
                    await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal Closure", cancellationToken);
                }
                catch (Exception e)
                {
                    logger.LogError($"Error closing {api.Name} websocket connection: {e.Message}");
                }
            }
        }

        public void DisposeClient()
        {
            if (client == null)
                return;

            connectionStatus = client.State;
            if (connectionStatus == WebSocketState.Connecting || connectionStatus == WebSocketState.Open)
            {
                logger.LogDebug($"Aborting {api.Name} websocket connection.");
                client.Abort();
            }

            client.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeClient();
            }
        }
    }
}
