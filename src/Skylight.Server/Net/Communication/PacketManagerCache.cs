using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.Loader;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Net.Communication.Attributes;
using Net.Communication.Manager;
using Skylight.Protocol.Attributes;
using Skylight.Protocol.Packets.Manager;
using Skylight.Server.Net.Communication;

[assembly: MetadataUpdateHandler(typeof(PacketManagerCache.MetadataUpdateHandler))]

namespace Skylight.Server.Net.Communication;

internal sealed partial class PacketManagerCache
{
	private readonly IServiceProvider serviceProvider;

	private readonly ILogger<PacketManagerCache> logger;

	private readonly NetworkSettings networkSettings;

	private readonly Dictionary<string, ProtocolData> protocols;

	private PacketManagerData<uint> packetManagerData;

	public PacketManagerCache(IServiceProvider serviceProvider, ILogger<PacketManagerCache> logger, IOptions<NetworkSettings> networkOptions)
	{
		this.serviceProvider = serviceProvider;

		this.protocols = [];

		this.logger = logger;

		this.networkSettings = networkOptions.Value;

		this.packetManagerData = PacketManagerCache.GetPacketManagerData();

		MetadataUpdateHandler.PacketManagerCache = this;
	}

	[Conditional("DEBUG")]
	internal void ScanAppDomain()
	{
		foreach (string protocolPath in this.networkSettings.AdditionalProtocols)
		{
			if (!File.Exists(protocolPath))
			{
				this.logger.LogWarning($"Additional protocol not found at path {protocolPath}");

				continue;
			}

			using FileStream fileStream = File.OpenRead(protocolPath);

			AssemblyLoadContext assemblyLoadContext = new("Unloadable Packet Manager", isCollectible: true);

			if (this.TryLoadProtocolAssembly(assemblyLoadContext.LoadFromStream(fileStream), out ProtocolData? protocolData))
			{
				protocolData.PhysicalFileProvider = new PhysicalFileProvider(Path.GetDirectoryName(protocolPath)!);

				RegisterCallback();

				void RegisterCallback()
				{
					protocolData.PhysicalFileProvider.Watch("*").RegisterChangeCallback(_ =>
					{
						this.logger.LogInformation($"HotSwapping protocol data for {protocolData.Revision}");

						using FileStream fileStream = File.OpenRead(protocolPath);

						AssemblyLoadContext assemblyLoadContext = new("Unloadable Packet Manager", isCollectible: true);

						protocolData.Update(this.serviceProvider, assemblyLoadContext.LoadFromStream(fileStream), this.packetManagerData);

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
			protocolData = new ProtocolData(this.serviceProvider, this.packetManagerData, attribute.Revision, assembly);

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

		this.packetManagerData = PacketManagerCache.GetPacketManagerData();

		foreach (ProtocolData protocol in this.protocols.Values)
		{
			protocol.Update(this.serviceProvider, this.packetManagerData);
		}
	}

	internal bool TryCreatePacketManager(string version, [NotNullWhen(true)] out Func<IGamePacketManager>? packetManagerGetter)
	{
		if (this.protocols.TryGetValue(version, out ProtocolData? protocol))
		{
			packetManagerGetter = protocol.PacketManagerGetter;

			return true;
		}

		packetManagerGetter = null;

		return false;
	}

	[PacketManagerGenerator(typeof(IGamePacketManager))]
	private static partial PacketManagerData<uint> GetPacketManagerData();

	private sealed class ProtocolData
	{
		internal string Revision { get; }

		private Assembly assembly;
		private Lazy<IGamePacketManager> packetManager;

		internal Func<IGamePacketManager> PacketManagerGetter { get; }

		internal PhysicalFileProvider? PhysicalFileProvider { get; set; }

		internal ProtocolData(IServiceProvider serviceProvider, PacketManagerData<uint> packetManagerData, string revision, Assembly assembly)
		{
			this.Revision = revision;

			this.Update(serviceProvider, assembly, packetManagerData);

			this.PacketManagerGetter = () => this.packetManager.Value;
		}

		[MemberNotNull(nameof(ProtocolData.packetManager))]
		internal void Update(IServiceProvider serviceProvider, PacketManagerData<uint> packetManagerData)
		{
			this.Update(serviceProvider, this.assembly, packetManagerData);
		}

		[MemberNotNull(nameof(ProtocolData.packetManager), nameof(ProtocolData.assembly))]
		internal void Update(IServiceProvider serviceProvider, Assembly assembly, PacketManagerData<uint> packetManagerData)
		{
			this.assembly = assembly;
			this.packetManager = new Lazy<IGamePacketManager>(() => assembly.GetCustomAttribute<GameProtocolManagerAttribute>()!.CreatePacketManager(serviceProvider, packetManagerData));
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
