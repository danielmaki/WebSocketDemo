using WebSocketDemo.Models.Wrappers;

namespace WebSocketDemo.Logic;

/// <summary>
/// A class for Conditions that connect Delegates and Filters together.
/// </summary>
/// <typeparam name="TWhen">Type of delegate.</typeparam>
/// <typeparam name="TFilter">Type of Filter that is used when TWhen is triggered.</typeparam>
public class Condition<TWhen, TFilter> where TFilter : Filter<TWhen>
{
    private readonly TFilter filter;
    private readonly MutableValue<TWhen> when;

    public TWhen When
    {
        get => when.Value;
        set
        {
            lock (when)
            {
                when.Value = filter.Use(value, when.Value);
            }
        }
    }

    public Condition(MutableValue<TWhen> when, TFilter filter)
    {
        this.when = when;
        this.filter = filter;
    }
}

/// <inheritdoc />
/// <summary>
/// Specialization of Condition with identity filter.
/// </summary>
/// <typeparam name="TWhen">Type of delegate.</typeparam>
public class Condition<TWhen> : Condition<TWhen, Filter<TWhen>>
{
    public Condition(MutableValue<TWhen> when, Filter<TWhen> filter) : base(when, filter)
    {
    }
}
