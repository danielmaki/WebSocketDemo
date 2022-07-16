namespace WebSocketDemo.Domain.Delegates;

/// <summary>
/// Delegate that is invoked when application is started.
/// </summary>
/// <returns>Asynchronous operation that is run when application is started.</returns>
public delegate Task ApplicationStart(CancellationToken cancellationToken);
