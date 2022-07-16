using Microsoft.Extensions.Hosting;

namespace WebSocketDemo.Hosting;

public class ApplicationCancellationTokenProvider
{
    private readonly IHostApplicationLifetime lifetime;

    public ApplicationCancellationTokenProvider(IHostApplicationLifetime lifetime)
    {
        this.lifetime = lifetime;
    }

    public virtual CancellationToken Token => lifetime.ApplicationStopping;
}
