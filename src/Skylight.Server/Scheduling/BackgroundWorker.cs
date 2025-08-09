namespace Skylight.Server.Scheduling;

internal abstract class BackgroundWorker
{
	private CancellationTokenSource? cancellationTokenSource;
	private Task? executeTask;

	private protected abstract Task ExecuteAsync(CancellationToken cancellationToken);

	private protected abstract void Complete();

	internal Task StartAsync(CancellationToken cancellationToken)
	{
		this.cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		this.executeTask = this.ExecuteAsync(this.cancellationTokenSource.Token);

		return this.executeTask.IsCompleted
			? this.executeTask
			: Task.CompletedTask;
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
