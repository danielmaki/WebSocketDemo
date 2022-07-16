using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Logging;
using WebSocketDemo.Domain.Delegates;
using WebSocketDemo.Factories;
using WebSocketDemo.Models.Interfaces;
using WebSocketDemo.Wrappers;

namespace WebSocketDemo.Services;

public class WebSocketClient<T> : IAsyncDisposable where T : IApi, new()
{
    private readonly ILogger<WebSocketClient<T>> logger;
    private readonly ClientWebSocketFactory clientWebSocketFactory;
    private readonly Trigger<WebSocketConnected<T>> wsConnectedTrigger;
    private readonly Trigger<WebSocketConnectionLost<T>> wsConnectionLostTrigger;

    private ClientWebSocket client;
    private WebSocketState connectionStatus;
    private TaskCompletionSource connected;
    private readonly T api = new();

    public virtual bool IsConnected => connected.Task.IsCompletedSuccessfully;

    public WebSocketClient(ILogger<WebSocketClient<T>> logger,
        ClientWebSocketFactory clientWebSocketFactory,
        Trigger<WebSocketConnected<T>> wsConnectedTrigger, Trigger<WebSocketConnectionLost<T>> wsConnectionLostTrigger)
    {
        this.logger = logger;
        this.clientWebSocketFactory = clientWebSocketFactory;

        client = clientWebSocketFactory.Create();
        connectionStatus = client.State;
        connected = new TaskCompletionSource();

        this.wsConnectedTrigger = wsConnectedTrigger;
        this.wsConnectionLostTrigger = wsConnectionLostTrigger;
    }

    public async Task Connect(CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Connecting to websocket {name}.", api.Name);

        if (client.State != WebSocketState.None)
        {
            await DisposeClientAsync();
            client = clientWebSocketFactory.Create();
            connectionStatus = client.State;
        }

        try
        {
            await client.ConnectAsync(api.Endpoint, cancellationToken);
        }
        catch (TaskCanceledException e)
        {
            logger.LogError("Attempt to connect to {endpoint} websocket was canceled: {e}", api.Endpoint, e.Message);
            connected.SetCanceled(cancellationToken);
            throw;
        }
        catch (WebSocketException e)
        {
            logger.LogError("Attempt to connect to {endpoint} websocket failed: {e}", api.Endpoint, e.Message);
            throw;
        }
        catch (Exception e)
        {
            logger.LogError("Error connecting to {endpoint} websocket: {e}", api.Endpoint, e.Message);
            connected.SetException(e);
            throw;
        }

        logger.LogInformation("Established connection to {name} at endpoint {endpoint}.", api.Name, api.Endpoint);

        connected.SetResult();

        await WaitForConnection(cancellationToken);

        await wsConnectedTrigger.Invoke($"Websocket connected to {api.Name}");
    }

    public async Task<string> Receive(CancellationToken cancellationToken = default)
    {
        await WaitForConnection(cancellationToken);

        var buffer = new ArraySegment<byte>(new byte[1024 * 4]);
        WebSocketReceiveResult result;
        using var stream = new MemoryStream();
        do
        {
            result = await client.ReceiveAsync(buffer, cancellationToken);
            if (buffer.Array == null)
            {
                break;
            }
            stream.Write(buffer.Array, buffer.Offset, result.Count);
        }
        while (!result.EndOfMessage);

        if (result.MessageType == WebSocketMessageType.Close)
        {
            logger.LogWarning("Websocket {name} received Close message, connection now closed.", api.Name);

            return "Closed";
        }

        stream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        return await reader.ReadToEndAsync();
    }

    public virtual async Task Send(string data, CancellationToken cancellationToken = default)
    {
        await WaitForConnection(cancellationToken);

        try
        {
            logger.LogDebug("Send message through websocket {name}: {message}", api.Name, data);

            await client.SendAsync(Encoding.UTF8.GetBytes(data), WebSocketMessageType.Text, true, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            logger.LogWarning("Send operation was canceled for websocket {name}.", api.Name);
        }
        catch (Exception e)
        {
            logger.LogError("Error while trying to send message to websocket {name}: {e}", api.Name, e.Message);
        }
    }

    private void OnConnectionStatusChanged(CancellationToken cancellationToken)
    {
        logger.LogDebug("Status of websocket connection to {name} has changed from '{status}' to '{state}'.", api.Name, connectionStatus, client.State);

        connectionStatus = client.State;

        if (connectionStatus == WebSocketState.CloseReceived || connectionStatus == WebSocketState.Closed || connectionStatus == WebSocketState.Aborted)
        {
            // Try cancelling old connection wait and create new one.
            connected.TrySetCanceled(cancellationToken);
            connected = new TaskCompletionSource();

            wsConnectionLostTrigger?.InvokeWithoutSynchronization($"Websocket connection to {api.Name} lost", cancellationToken);
        }
    }

    private async Task WaitForConnection(CancellationToken cancellationToken)
    {
        if (connectionStatus != client.State)
        {
            OnConnectionStatusChanged(cancellationToken);
        }

        if (!IsConnected)
        {
            logger.LogTrace("Waiting for websocket connection to {name}.", api.Name);

            using (cancellationToken.Register(() => connected.TrySetCanceled(cancellationToken)))
            {
                try
                {
                    await connected.Task;
                }
                catch (TaskCanceledException)
                {
                    logger.LogInformation("Websocket connection to {name} was canceled.", api.Name);
                    throw;
                }
            }
        }
    }

    public async Task Close(CancellationToken cancellationToken)
    {
        logger.LogTrace("Closing websocket connection to {name}.", api.Name);

        connectionStatus = client.State;

        if (connectionStatus == WebSocketState.Connecting || connectionStatus == WebSocketState.Open)
        {
            try
            {
                await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Normal Closure", cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError("Error closing websocket connection to {name}: {e}", api.Name, e.Message);
            }
        }
    }

    public Task DisposeClientAsync()
    {
        if (client == null)
        {
            return Task.CompletedTask;
        }

        connectionStatus = client.State;

        if (connectionStatus == WebSocketState.Connecting || connectionStatus == WebSocketState.Open)
        {
            logger.LogDebug("Aborting websocket connection to {name}.", api.Name);
            client.Abort();
        }

        client.Dispose();

        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }

    protected virtual async Task DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            await DisposeClientAsync();
        }
    }
}
