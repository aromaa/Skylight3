namespace Skylight.API.Events;

public sealed class ValueChangedEventArgs<T>(T oldValue, T newValue) : EventArgs
{
	public T OldValue { get; } = oldValue;
	public T NewValue { get; } = newValue;
}
