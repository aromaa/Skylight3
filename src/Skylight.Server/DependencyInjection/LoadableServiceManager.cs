using System.Collections.Concurrent;
using Skylight.API.DependencyInjection;
using Skylight.Server.Extensions;

namespace Skylight.Server.DependencyInjection;

internal sealed class LoadableServiceManager : ILoadableServiceManager
{
	private readonly ConcurrentDictionary<ILoadableService, HashSet<ILoadableService>> services;
	private readonly ConcurrentDictionary<Type, ILoadableService> servicesByType;

	private readonly TaskCompletionSource initialLoad;

	//Normally there should never be multiple loads at the same time
	//but its possible due to pure coincidence or race conditions.
	//Limit the effects on timings related to these by artificially
	//allowing to only have one pending load at any given time.
	//This also helps to ensure we are not loading too much at once.
	//We could technically try to piggyback these to already pending
	//load but that's overly complex for little to no gain for rare case.
	private Task? pendingLoad;

	public LoadableServiceManager(IEnumerable<ILoadableService> services)
	{
		this.services = new ConcurrentDictionary<ILoadableService, HashSet<ILoadableService>>();
		this.servicesByType = new ConcurrentDictionary<Type, ILoadableService>();

		this.initialLoad = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

		foreach (ILoadableService service in services)
		{
			this.services[service] = new HashSet<ILoadableService>();
		}
	}

	private async Task CreateNewContext(Action<LoadableServiceContext> action, CancellationToken cancellationToken = default, bool useTransaction = true)
	{
		TaskCompletionSource loadTask = new(TaskCreationOptions.RunContinuationsAsynchronously);
		if (Interlocked.Exchange(ref this.pendingLoad, loadTask.Task) is { } pendingLoad)
		{
			await pendingLoad.ConfigureAwait(false);
		}

		try
		{
			LoadableServiceContext context = new(this, useTransaction);
			action(context);

			Task completeTask = context.CompleteAsync(cancellationToken);

			if (!this.initialLoad.Task.IsCompleted)
			{
				_ = completeTask.ContinueWith(static (task, state) =>
				{
					((TaskCompletionSource)state!).SetFromTask(task);
				}, this.initialLoad, cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
			}

			await completeTask.ConfigureAwait(false);
		}
		finally
		{
			loadTask.SetResult();

			_ = Interlocked.CompareExchange(ref this.pendingLoad, null, loadTask.Task);
		}
	}

	public Task LoadAsync(CancellationToken cancellationToken = default, bool useTransaction = true)
	{
		return this.CreateNewContext(context =>
		{
			foreach (ILoadableService service in this.services.Keys)
			{
				_ = context.LoadAsync(service, cancellationToken);
			}
		}, cancellationToken, useTransaction);
	}

	public Task LoadAsync(Type serviceType, CancellationToken cancellationToken = default, bool useTransaction = true)
	{
		if (!this.initialLoad.Task.IsCompleted && this.pendingLoad is null)
		{
			return this.LoadAsync(cancellationToken, useTransaction);
		}

		return this.CreateNewContext(context =>
		{
			_ = context.LoadAsync(this.GetService(serviceType), cancellationToken);
		}, cancellationToken, useTransaction);
	}

	public Task LoadAsync(Type[] serviceTypes, CancellationToken cancellationToken = default, bool useTransaction = true)
	{
		if (!this.initialLoad.Task.IsCompleted && this.pendingLoad is null)
		{
			return this.LoadAsync(cancellationToken, useTransaction);
		}

		return this.CreateNewContext(context =>
		{
			foreach (Type type in serviceTypes)
			{
				_ = context.LoadAsync(this.GetService(type), cancellationToken);
			}
		}, cancellationToken, useTransaction);
	}

	public Task LoadAsync<T>(CancellationToken cancellationToken = default, bool useTransaction = true) => this.LoadAsync(typeof(T), cancellationToken, useTransaction);

	public Task WaitForInitialization(CancellationToken cancellationToken = default) => this.initialLoad.Task.WaitAsync(cancellationToken);

	internal HashSet<ILoadableService> GetDependents(ILoadableService service) => this.services[service];

	internal void AddDependent(ILoadableService dependency, ILoadableService dependent)
	{
		HashSet<ILoadableService> dependents = this.GetDependents(dependency);

		lock (dependents)
		{
			dependents.Add(dependent);
		}
	}

	internal ILoadableService GetService(Type type)
	{
		return this.servicesByType.GetOrAdd(type, static (key, state) =>
		{
			foreach (ILoadableService service in state.services.Keys)
			{
				if (key.IsInstanceOfType(service))
				{
					return service;
				}
			}

			throw new NotSupportedException(key.ToString());
		}, this);
	}
}
