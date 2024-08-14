using System.Runtime.CompilerServices;
using Skylight.API.DependencyInjection;

namespace Skylight.Server.DependencyInjection;

internal sealed class LoadableServiceContext : ILoadableServiceContext
{
	private static readonly AsyncLocal<ILoadableService> currentCallerData = new();

	private readonly LoadableServiceManager loader;

	private readonly bool useTransaction;

	private readonly Dictionary<ILoadableService, Task> loading;
	private readonly List<Action> transactions;

	internal LoadableServiceContext(LoadableServiceManager loader, bool useTransaction = true)
	{
		this.loader = loader;

		this.useTransaction = useTransaction;

		this.loading = [];
		this.transactions = [];
	}

	private (Action? RunAction, Task Task) Prepare(ILoadableService service, CancellationToken cancellationToken = default)
	{
		lock (this.loading)
		{
			if (this.loading.TryGetValue(service, out Task? task))
			{
				return (null, task);
			}

			TaskCompletionSource<Task> taskCompletionSource = new();

			void RunAction()
			{
				//Flow upwards, do first to avoid race conditions
				foreach (ILoadableService dependent in this.loader.GetDependents(service))
				{
					this.LoadAsync(dependent, cancellationToken);
				}

				LoadableServiceContext.currentCallerData.Value = service;

				service.LoadAsync(this, cancellationToken).ContinueWith(static (task, state) =>
				{
					if (task.IsCompletedSuccessfully)
					{
						Unsafe.As<TaskCompletionSource<Task>>(state!).SetResult(task);
					}
					else if (task.IsFaulted)
					{
						Unsafe.As<TaskCompletionSource<Task>>(state!).SetException(task.Exception);
					}
					else
					{
						Unsafe.As<TaskCompletionSource<Task>>(state!).SetCanceled();
					}
				}, taskCompletionSource, TaskContinuationOptions.ExecuteSynchronously);
			}

			this.loading[service] = taskCompletionSource.Task;

			return (RunAction, taskCompletionSource.Task);
		}
	}

	internal Task LoadAsync(ILoadableService service, CancellationToken cancellationToken = default)
	{
		(Action? runAction, Task task) = this.Prepare(service, cancellationToken);
		if (runAction is not null)
		{
			Task.Run(runAction, cancellationToken);
		}

		return task;
	}

	public async Task<T> RequestDependencyAsync<T>(CancellationToken cancellationToken = default)
		where T : IServiceSnapshot
	{
		ILoadableService<T> service = this.loader.GetService<T>();

		Task? task;
		if (LoadableServiceContext.currentCallerData.Value is { } caller)
		{
			this.loader.AddDependent(service, caller);

			lock (this.loading)
			{
				if (!this.loading.TryGetValue(service, out task))
				{
					return service.Current;
				}
			}
		}
		else
		{
			task = this.LoadAsync(service, cancellationToken);
		}

		Task<Task> wrappedTask = (Task<Task>)task;
		Task original = await wrappedTask.ConfigureAwait(false);
		return ((Task<T>)original).Result;
	}

	public void Commit(Action action)
	{
		if (!this.useTransaction)
		{
			action();
			return;
		}

		lock (this.transactions)
		{
			this.transactions.Add(action);
		}
	}

	public async Task CompleteAsync(CancellationToken cancellationToken = default)
	{
		int taskCount = 0;

		while (true)
		{
			Task task;

			lock (this.loading)
			{
				if (taskCount == this.loading.Count)
				{
					break;
				}

				taskCount = this.loading.Count;
				task = Task.WhenAll(this.loading.Values);
			}

			await task.WaitAsync(cancellationToken).ConfigureAwait(false);
		}

		lock (this.loading)
		{
			this.loading.Clear();
		}

		lock (this.transactions)
		{
			foreach (Action transaction in this.transactions)
			{
				transaction();
			}

			this.transactions.Clear();
		}
	}
}
