using WebSocketDemo.Models.Interfaces;

namespace WebSocketDemo.Logic.Delegates;

/// <summary>
/// Delegate that is invoked when the websocket connection is established.
/// </summary>
/// <returns>Asynchronous operation that is run when the connection is established.</returns>
public delegate Task WebSocketConnected<T>() where T : IApi;
