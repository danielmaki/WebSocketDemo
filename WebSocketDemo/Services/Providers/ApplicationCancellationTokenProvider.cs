using System.Threading;

using Microsoft.Extensions.Hosting;

namespace WebSocketDemo.Services.Providers;

public class ApplicationCancellationTokenProvider
{
    private readonly IHostApplicationLifetime lifetime;

    public ApplicationCancellationTokenProvider(IHostApplicationLifetime lifetime)
    {
        this.lifetime = lifetime;
    }

    public virtual CancellationToken Token => lifetime.ApplicationStopping;
}
