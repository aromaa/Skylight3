using System.Diagnostics.CodeAnalysis;

namespace Skylight.API.Registry;

public interface IRegistryHolder
{
	public IRegistry<T> Registry<T>(RegistryType<T> type)
		where T : notnull;

	public bool TryGetRegistry<T>(RegistryType<T> type, [NotNullWhen(true)] out IRegistry<T>? registry)
		where T : notnull;
}
