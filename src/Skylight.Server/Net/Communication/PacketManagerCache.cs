using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.Loader;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Net.Communication.Attributes;
using Skylight.Protocol.Attributes;
using Skylight.Protocol.Packets.Manager;
using Skylight.Server.Attributes;
using Skylight.Server.Net.Communication;

[assembly: MetadataUpdateHandler(typeof(PacketManagerCache.MetadataUpdateHandler))]

namespace Skylight.Server.Net.Communication;

internal sealed class PacketManagerCache
{
	private readonly IServiceProvider serviceProvider;

	private readonly ILogger<PacketManagerCache> logger;

	private readonly Dictionary<string, ProtocolData> protocols;

	public PacketManagerCache(IServiceProvider serviceProvider, ILogger<PacketManagerCache> logger)
	{
		this.serviceProvider = serviceProvider;

		this.protocols = [];

		this.logger = logger;

		MetadataUpdateHandler.PacketManagerCache = this;
	}

	[Conditional("DEBUG")]
	internal void ScanAppDomain()
	{
		foreach (InternalProtocolLibraryPathAttribute attribute in Assembly.GetEntryAssembly()!.GetCustomAttributes<InternalProtocolLibraryPathAttribute>())
		{
			using FileStream fileStream = File.OpenRead(attribute.Path);

			AssemblyLoadContext assemblyLoadContext = new("Unloadable Packet Manager", isCollectible: true);

			if (this.TryLoadProtocolAssembly(assemblyLoadContext.LoadFromStream(fileStream), out ProtocolData? protocolData))
			{
				protocolData.PhysicalFileProvider = new PhysicalFileProvider(Path.GetDirectoryName(attribute.Path)!);

				RegisterCallback();

				void RegisterCallback()
				{
					protocolData.PhysicalFileProvider.Watch("*").RegisterChangeCallback(_ =>
					{
						this.logger.LogInformation($"HotSwapping protocol data for {protocolData.Revision}");

						using FileStream fileStream = File.OpenRead(attribute.Path);

						AssemblyLoadContext assemblyLoadContext = new("Unloadable Packet Manager", isCollectible: true);

						protocolData.Update(this.serviceProvider, assemblyLoadContext.LoadFromStream(fileStream));

						RegisterCallback();
					}, null);
				}
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

			AssemblyLoadContext assemblyLoadContext = AssemblyLoadContext.Default;

			Assembly assembly;
			if (!assemblyLoadContext.IsCollectible && file.PhysicalPath is not null)
			{
				assembly = assemblyLoadContext.LoadFromAssemblyPath(file.PhysicalPath);
			}
			else
			{
				string symbolsFileName = $"{Path.GetFileNameWithoutExtension(file.Name)}.pdb";

				IFileInfo? symbolsFile = protocolDirectory.FirstOrDefault(symbolsFile => symbolsFile.Name == symbolsFileName);
				if (symbolsFile is not null)
				{
					using Stream stream = file.CreateReadStream();
					using Stream symbolsStream = symbolsFile.CreateReadStream();

					assembly = assemblyLoadContext.LoadFromStream(stream, symbolsStream);
				}
				else
				{
					using Stream stream = file.CreateReadStream();

					assembly = assemblyLoadContext.LoadFromStream(stream);
				}
			}

			this.TryLoadProtocolAssembly(assembly, out _);
		}
	}

	private bool TryLoadProtocolAssembly(Assembly assembly, [NotNullWhen(true)] out ProtocolData? protocolData)
	{
		GameProtocolAttribute? attribute = assembly.GetCustomAttribute<GameProtocolAttribute>();
		if (attribute is not null)
		{
			protocolData = new ProtocolData(this.serviceProvider, attribute.Revision, assembly);

			this.protocols.Add(attribute.Revision, protocolData);

			this.logger.LogInformation($"Found protocol data for revision {attribute.Revision}");

			return true;
		}
		else
		{
			this.logger.LogWarning($"Found invalid protocol assembly {assembly.FullName}");
		}

		protocolData = null;

		return false;
	}

	private void Refresh()
	{
		this.logger.LogInformation("Updating protocols");

		foreach (ProtocolData protocol in this.protocols.Values)
		{
			protocol.Update(this.serviceProvider);
		}
	}

	internal bool TryCreatePacketManager(string version, [NotNullWhen(true)] out Func<AbstractGamePacketManager>? packetManagerGetter)
	{
		if (this.protocols.TryGetValue(version, out ProtocolData? protocol))
		{
			packetManagerGetter = protocol.PacketManagerGetter;

			return true;
		}

		packetManagerGetter = null;

		return false;
	}

	private sealed class ProtocolData
	{
		internal string Revision { get; }

		private Assembly assembly;
		private Lazy<AbstractGamePacketManager> packetManager;

		internal Func<AbstractGamePacketManager> PacketManagerGetter { get; }

		internal PhysicalFileProvider? PhysicalFileProvider { get; set; }

		internal ProtocolData(IServiceProvider serviceProvider, string revision, Assembly assembly)
		{
			this.Revision = revision;

			this.Update(serviceProvider, assembly);

			this.PacketManagerGetter = () => this.packetManager.Value;
		}

		[MemberNotNull(nameof(ProtocolData.packetManager))]
		internal void Update(IServiceProvider serviceProvider)
		{
			this.Update(serviceProvider, this.assembly);
		}

		[MemberNotNull(nameof(ProtocolData.packetManager), nameof(ProtocolData.assembly))]
		internal void Update(IServiceProvider serviceProvider, Assembly assembly)
		{
			this.assembly = assembly;
			this.packetManager = new Lazy<AbstractGamePacketManager>(() => assembly.GetCustomAttribute<GameProtocolManagerAttribute>()!.CreatePacketManager(serviceProvider));
		}
	}

	internal static class MetadataUpdateHandler
	{
		internal static PacketManagerCache? PacketManagerCache { get; set; }

#pragma warning disable IDE0051 // Remove unused private members
		private static void ClearCache(Type[]? updatedTypes)
#pragma warning restore IDE0051 // Remove unused private members
		{
		}

#pragma warning disable IDE0051 // Remove unused private members
		private static void UpdateApplication(Type[]? updatedTypes)
#pragma warning restore IDE0051 // Remove unused private members
		{
			if (MetadataUpdateHandler.PacketManagerCache is not { } packetManagerCache)
			{
				return;
			}

			if (updatedTypes is null)
			{
				packetManagerCache.Refresh();
				return;
			}

			foreach (Type updatedType in updatedTypes)
			{
				if (updatedType.GetCustomAttribute<PacketManagerRegisterAttribute>() is not null)
				{
					packetManagerCache.Refresh();
					return;
				}
			}
		}
	}
}
