using Skylight.API.Registry;
using Skylight.Server.Registry;

namespace Skylight.Server.Tests.Registry;

internal sealed class DummyRegistryHolder(params IEnumerable<IRegistry> registries) : RegistryHolder(registries);
