using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skylight.Infrastructure;

namespace Skylight.Server.Scheduling;

internal sealed class DatabaseBackgroundWorker(ILogger<DatabaseBackgroundWorker> logger, IDbContextFactory<SkylightContext> dbContextFactory) : BackgroundWorker
{
	private readonly ILogger<DatabaseBackgroundWorker> logger = logger;

	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	private readonly Channel<Func<IDbContextFactory<SkylightContext>, CancellationToken, Task>> queue = Channel.CreateUnbounded<Func<IDbContextFactory<SkylightContext>, CancellationToken, Task>>(new UnboundedChannelOptions
	{
		AllowSynchronousContinuations = false,
		SingleReader = true
	});

	private protected override async Task ExecuteAsync(CancellationToken cancellationToken)
	{
		await foreach (Func<IDbContextFactory<SkylightContext>, CancellationToken, Task> func in this.queue.Reader.ReadAllAsync(cancellationToken))
		{
			try
			{
				await func(this.dbContextFactory, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				this.logger.LogError(e, "Database task failed");
			}
		}
	}

	private protected override void Complete() => this.queue.Writer.Complete();

	internal void Submit(Func<IDbContextFactory<SkylightContext>, CancellationToken, Task> func) => this.queue.Writer.TryWrite(func);
}
