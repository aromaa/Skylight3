using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.FileProviders;
using Skylight.Protocol.Attributes;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Net.Communication;

internal sealed class PacketManagerCache
{
	private readonly IServiceProvider serviceProvider;

	private readonly Dictionary<string, Assembly> protocolAssemblies;

	public PacketManagerCache(IServiceProvider serviceProvider)
	{
		this.serviceProvider = serviceProvider;

		this.protocolAssemblies = new Dictionary<string, Assembly>();
	}

	[Conditional("DEBUG")]
	internal void ScanAppDomain()
	{
		string protocolLibraryName = typeof(GameProtocolAttribute).Assembly.GetName().Name!;

		foreach (RuntimeLibrary library in DependencyContext.Default!.RuntimeLibraries)
		{
			if (library.Dependencies.All(d => d.Name != protocolLibraryName))
			{
				continue;
			}

			foreach (AssemblyName assemblyName in library.GetDefaultAssemblyNames(DependencyContext.Default))
			{
				if (!assemblyName.FullName.StartsWith(protocolLibraryName))
				{
					continue;
				}

				AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName);
			}
		}

		foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			GameProtocolAttribute? attribute = assembly.GetCustomAttribute<GameProtocolAttribute>();
			if (attribute is not null)
			{
				this.protocolAssemblies.Add(attribute.Revision, assembly);
			}
		}
	}

	internal void Load(IDirectoryContents protocolDirectory)
	{
		foreach (IFileInfo file in protocolDirectory)
		{
			if (Path.GetExtension(file.Name) != ".dll")
			{
				continue;
			}

			Assembly assembly;
			if (file.PhysicalPath is not null)
			{
				assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(file.PhysicalPath);
			}
			else
			{
				string symbolsFileName = $"{Path.GetFileNameWithoutExtension(file.Name)}.pdb";

				IFileInfo? symbolsFile = protocolDirectory.FirstOrDefault(symbolsFile => symbolsFile.Name == symbolsFileName);
				if (symbolsFile is not null)
				{
					using Stream stream = file.CreateReadStream();
					using Stream symbolsStream = symbolsFile.CreateReadStream();

					assembly = AssemblyLoadContext.Default.LoadFromStream(stream, symbolsStream);
				}
				else
				{
					using Stream stream = file.CreateReadStream();

					assembly = AssemblyLoadContext.Default.LoadFromStream(stream);
				}
			}

			GameProtocolAttribute? attribute = assembly.GetCustomAttribute<GameProtocolAttribute>();
			if (attribute is not null)
			{
				this.protocolAssemblies.Add(attribute.Revision, assembly);
			}
		}
	}

	internal bool TryCreatePacketManager(string version, [NotNullWhen(true)] out AbstractGamePacketManager? packetManager)
	{
		if (this.protocolAssemblies.TryGetValue(version, out Assembly? assembly))
		{
			GameProtocolManagerAttribute? attribute = assembly.GetCustomAttribute<GameProtocolManagerAttribute>();

			packetManager = attribute!.CreatePacketManager(this.serviceProvider);
			return true;
		}

		packetManager = null;
		return false;
	}
}
