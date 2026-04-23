namespace Skylight.Server.Scheduling;

internal abstract class BackgroundWorker
{
	private CancellationTokenSource? cancellationTokenSource;
	private Task? executeTask;

	private protected abstract Task<Task> ExecuteAsync(CancellationToken cancellationToken);

	private protected abstract void Complete();

	internal Task StartAsync(CancellationToken cancellationToken)
	{
		this.cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

		Task<Task> startTask = this.ExecuteAsync(this.cancellationTokenSource.Token);

		this.executeTask = startTask.Unwrap();

		return startTask;
	}

	internal async Task StopAsync(CancellationToken cancellationToken)
	{
		if (this.executeTask is null)
		{
			return;
		}

		cancellationToken.UnsafeRegister(s => ((CancellationTokenSource)s!).Cancel(), this.cancellationTokenSource);

		this.Complete();

		await this.executeTask.WaitAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
	}
}
