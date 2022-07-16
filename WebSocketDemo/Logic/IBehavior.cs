namespace WebSocketDemo.Logic;

/// <summary>
/// Interface for any generic business behavior.
/// </summary>
public interface IBehavior
{
    /// <summary>
    /// Method that is called when the behavior is created by the application lifetime.
    /// </summary>
    void OnCreated();
}
