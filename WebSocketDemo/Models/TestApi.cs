using System;
using WebSocketDemo.Models.Interfaces;

namespace WebSocketDemo.Models;

public class TestApi : IApi
{
    public string Name { get; }

    public Uri Endpoint { get; }

    public TestApi()
    {
        Name = "TEST API";
        Endpoint = new Uri("wss://stream.crypto.com/v2/market");
    }
}
