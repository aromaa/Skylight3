using Microsoft.Extensions.Hosting;

namespace Skylight.Server.Scheduling;

internal sealed class BackgroundWorkerService(IEnumerable<BackgroundWorker> backgroundWorkers) : IHostedLifecycleService
{
	private readonly IEnumerable<BackgroundWorker> backgroundWorkers = backgroundWorkers;

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		List<Exception> exceptions = [];
		foreach (BackgroundWorker backgroundWorker in this.backgroundWorkers)
		{
			try
			{
				await backgroundWorker.StartAsync(cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				exceptions.Add(ex);
			}
		}

		if (exceptions.Count > 0)
		{
			throw new AggregateException("One or more background workers failed to start.", exceptions);
		}
	}

	// We want to accept worker items right before the teardown.
	public async Task StoppedAsync(CancellationToken cancellationToken)
	{
		List<Exception> exceptions = [];
		foreach (BackgroundWorker backgroundWorker in this.backgroundWorkers)
		{
			try
			{
				await backgroundWorker.StopAsync(cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				exceptions.Add(ex);
			}
		}

		if (exceptions.Count > 0)
		{
			throw new AggregateException("One or more background workers failed to stop.", exceptions);
		}
	}

	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
	public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
	public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
	public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
