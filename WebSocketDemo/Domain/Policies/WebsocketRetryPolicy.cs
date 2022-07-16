
using Polly;
using WebSocketDemo.Domain.Policies.Interfaces;

namespace WebSocketDemo.Domain.Policies;

public class WebsocketRetryPolicy : IRetryPolicy
{
    public IAsyncPolicy Policy { get; }

    public WebsocketRetryPolicy(IAsyncPolicy policy)
    {
        Policy = policy;
    }

    public Task Run(Func<Task> action)
    {
        return Policy.ExecuteAsync(action);
    }
}
