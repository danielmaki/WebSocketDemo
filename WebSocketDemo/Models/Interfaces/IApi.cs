namespace WebSocketDemo.Models.Interfaces;

public interface IApi
{
    public string Name { get; }

    public Uri Endpoint { get; }
}
