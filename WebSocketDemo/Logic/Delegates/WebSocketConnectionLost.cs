﻿using System.Threading.Tasks;

using WebSocketDemo.Models;

namespace WebSocketDemo.Logic.Delegates
{
    /// <summary>
    /// Delegate that is invoked when the websocket connection is lost.
    /// </summary>
    /// <returns>Asynchronous operation that is run when the connection is lost.</returns>
    public delegate Task WebSocketConnectionLost<T>() where T : IApi;
}