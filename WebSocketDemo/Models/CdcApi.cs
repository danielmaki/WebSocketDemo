using WebSocketDemo.Models.Interfaces;

namespace WebSocketDemo.Models;

public class CdcApi : IApi
{
    public string Name { get; }

    public Uri Endpoint { get; }

    public CdcApi()
    {
        Name = "Crypto.com API";
        Endpoint = new Uri("wss://stream.crypto.com/v2/market"); // TODO: Refactor hardcoded values into appsettings
    }
}
