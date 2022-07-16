namespace WebSocketDemo.Models.Wrappers;

/// <summary>
/// A value wrapper that allows getting and setting the value of instance.
/// </summary>
/// <typeparam name="T">Type of the value.</typeparam>
public class MutableValue<T>
{
    /// <summary>
    /// Gets and sets a value.
    /// </summary>
    public T Value { get; set; }
}
