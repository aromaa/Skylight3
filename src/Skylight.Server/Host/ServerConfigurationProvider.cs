using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Skylight.Server.Host;

internal sealed class ServerConfigurationProvider : ConfigurationProvider
{
	private readonly string? connectionString;

	internal ServerConfigurationProvider(string? connectionString)
	{
		this.connectionString = connectionString;
	}

	public override void Load()
	{
		NpgsqlSlimDataSourceBuilder dataSourceBuilder = new(this.connectionString)
		{
			ConnectionStringBuilder =
			{
				Pooling = false,
				Enlist = false
			}
		};

		using NpgsqlDataSource dataSource = dataSourceBuilder.Build();
		using NpgsqlCommand command = dataSource.CreateCommand("SELECT id, value FROM settings");
		using NpgsqlDataReader reader = command.ExecuteReader();

		Dictionary<string, string?> data = new(StringComparer.OrdinalIgnoreCase);
		while (reader.Read())
		{
			data.Add(reader.GetString(0), reader.GetString(1));
		}

		this.Data = data;
	}
}
