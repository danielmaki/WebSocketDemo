namespace WebSocketDemo.Domain.Behaviors.Interfaces;

// TODO: Add additional methods for disabling and retrieve behavior state (enabled/disabled)

/// <summary>
/// Interface for any generic domain behavior.
/// </summary>
public interface IBehavior
{
    /// <summary>
    /// Method that is called when the behavior is created by the application lifetime.
    /// </summary>
    void EnableBehavior();
}
