using System;
using System.Threading.Tasks;

using Polly;
using WebSocketDemo.Logic.Policies.Interfaces;

namespace WebSocketDemo.Logic.Policies;

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
