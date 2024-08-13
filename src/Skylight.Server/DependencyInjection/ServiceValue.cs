using System.Runtime.CompilerServices;
using Skylight.API.DependencyInjection;

namespace Skylight.Server.DependencyInjection;

internal abstract class ServiceValue
{
	internal abstract void Commit();
}

internal sealed class ServiceValue<T>(T value) : ServiceValue, IServiceValue<T>
	where T : class
{
	private object value = value;

	public T Value
	{
		get
		{
			object value = this.value;

			return value.GetType() == typeof(Transaction)
				? Unsafe.As<Transaction>(value).Value
				: Unsafe.As<T>(value);
		}
	}

	internal void StartTransaction(VersionedLoadableServiceBase instance, int oldVersion, T node)
	{
		this.value = new Transaction(instance, oldVersion, (T)this.value, node);
	}

	internal override void Commit()
	{
		if (this.value is Transaction transaction)
		{
			this.value = transaction.NewValue;
		}
	}

	private sealed class Transaction(VersionedLoadableServiceBase instance, int oldVersion, T oldValue, T newValue)
	{
		private readonly VersionedLoadableServiceBase instance = instance;

		private readonly int oldVersion = oldVersion;
		private readonly T oldValue = oldValue;

		internal T NewValue { get; } = newValue;

		internal T Value => this.instance.Current.Version > this.oldVersion
			? this.NewValue
			: this.oldValue;
	}
}
