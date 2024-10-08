﻿using Skylight.API.DependencyInjection;

namespace Skylight.Server.DependencyInjection;

internal sealed class LoadableServiceManager : ILoadableServiceManager
{
	private readonly Dictionary<ILoadableService, HashSet<ILoadableService>> services;
	private readonly Dictionary<Type, ILoadableService> servicesBySnapshotType;

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
		this.services = new Dictionary<ILoadableService, HashSet<ILoadableService>>();
		this.servicesBySnapshotType = new Dictionary<Type, ILoadableService>();

		this.initialLoad = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

		foreach (ILoadableService service in services)
		{
			this.services[service] = [];

			Type? genericServiceType = service.GetType().GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ILoadableService<>));
			if (genericServiceType is not null)
			{
				Type snapshotType = genericServiceType.GetGenericArguments()[0];

				this.servicesBySnapshotType.Add(snapshotType, service);
			}
		}
	}

	private async Task CreateNewContext(Action<LoadableServiceContext> action, bool useTransaction = true, CancellationToken cancellationToken = default)
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

	public Task LoadAsync(bool useTransaction = true, CancellationToken cancellationToken = default)
	{
		return this.CreateNewContext(context =>
		{
			foreach (ILoadableService service in this.services.Keys)
			{
				_ = context.LoadAsync(service, cancellationToken);
			}
		}, useTransaction, cancellationToken);
	}

	public Task LoadAsync(Type serviceType, bool useTransaction = true, CancellationToken cancellationToken = default)
	{
		if (!this.initialLoad.Task.IsCompleted && this.pendingLoad is null)
		{
			return this.LoadAsync(useTransaction, cancellationToken);
		}

		return this.CreateNewContext(context =>
		{
			_ = context.LoadAsync(this.GetService(serviceType), cancellationToken);
		}, useTransaction, cancellationToken);
	}

	public Task LoadAsync(Type[] serviceTypes, bool useTransaction = true, CancellationToken cancellationToken = default)
	{
		if (!this.initialLoad.Task.IsCompleted && this.pendingLoad is null)
		{
			return this.LoadAsync(useTransaction, cancellationToken);
		}

		return this.CreateNewContext(context =>
		{
			foreach (Type type in serviceTypes)
			{
				_ = context.LoadAsync(this.GetService(type), cancellationToken);
			}
		}, useTransaction, cancellationToken);
	}

	public Task LoadAsync<T>(bool useTransaction = true, CancellationToken cancellationToken = default) => this.LoadAsync(typeof(T), useTransaction, cancellationToken);

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
		foreach (ILoadableService service in this.services.Keys)
		{
			if (type.IsInstanceOfType(service))
			{
				return service;
			}
		}

		throw new NotSupportedException(type.ToString());
	}

	internal ILoadableService<T> GetService<T>()
		where T : IServiceSnapshot => (ILoadableService<T>)this.servicesBySnapshotType[typeof(T)] ?? throw new NotSupportedException(typeof(T).ToString());
}
