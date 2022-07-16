using WebSocketDemo.Models.Interfaces;

namespace WebSocketDemo.Models;

public class BinanceApi : IApi
{
    public string Name { get; }

    public Uri Endpoint { get; }

    public BinanceApi()
    {
        Name = "Binance API";
        Endpoint = new Uri("wss://stream.binance.com:9443/ws");
    }
}
