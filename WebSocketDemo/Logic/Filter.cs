namespace WebSocketDemo.Logic;

/// <summary>
/// Filter base class that can be used to filter delegates.
/// </summary>
/// <typeparam name="TWhen">Type of delegate.</typeparam>
/// <remarks>This class works as an identity filter, thus passes all triggers through. Derive the class and override Use() method to inject the actual filtering.</remarks>
public class Filter<TWhen>
{
    /// <summary>
    /// Injects TWhen filtering when it is changed.
    /// </summary>
    /// <param name="newWhen">Desired TWhen for change.</param>
    /// <param name="oldWhen">TWhen before the change.</param>
    /// <returns>Injected TWhen after change, by default just newWhen.</returns>
    public virtual TWhen Use(TWhen newWhen, TWhen oldWhen)
    {
        return newWhen;
    }
}
