using StackExchange.Redis;

namespace Skylight.Server.Redis;

public sealed class RedisConnector(string connectionString)
{
	private readonly string connectionString = connectionString;

	private readonly SemaphoreSlim initializationLock = new(initialCount: 1);
	private ConnectionMultiplexer? connection;
	private IDatabase? database;

	public ValueTask<IDatabase> GetDatabaseAsync()
	{
		return this.database is not null
			? ValueTask.FromResult(this.database)
			: GetSlowAsync();

		async ValueTask<IDatabase> GetSlowAsync()
		{
			await this.initializationLock.WaitAsync().ConfigureAwait(false);

			try
			{
				if (this.connection is null)
				{
					this.connection = await ConnectionMultiplexer.ConnectAsync(this.connectionString).ConfigureAwait(false);

					this.database = this.connection.GetDatabase();
				}

				return this.database!;
			}
			finally
			{
				this.initializationLock.Release();
			}
		}
	}
}
