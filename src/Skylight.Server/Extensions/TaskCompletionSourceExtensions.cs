using System.Runtime.CompilerServices;

namespace Skylight.Server.Extensions;

internal static class TaskCompletionSourceExtensions
{
	internal static Task SetFromTask(this TaskCompletionSource taskCompletionSource, Task task)
	{
		return task.ContinueWith(static (task, state) =>
		{
			if (task.IsCompletedSuccessfully)
			{
				Unsafe.As<TaskCompletionSource>(state!).SetResult();
			}
			else if (task.IsFaulted)
			{
				Unsafe.As<TaskCompletionSource>(state!).SetException(task.Exception);
			}
			else
			{
				Unsafe.As<TaskCompletionSource>(state!).SetCanceled();
			}
		}, taskCompletionSource, TaskContinuationOptions.ExecuteSynchronously);
	}

	internal static Task SetFromTask<T>(this TaskCompletionSource<T> taskCompletionSource, Task<T> task)
	{
		return task.ContinueWith(static (task, state) =>
		{
			if (task.IsCompletedSuccessfully)
			{
				Unsafe.As<TaskCompletionSource<T>>(state!).SetResult(task.Result);
			}
			else if (task.IsFaulted)
			{
				Unsafe.As<TaskCompletionSource<T>>(state!).SetException(task.Exception);
			}
			else
			{
				Unsafe.As<TaskCompletionSource<T>>(state!).SetCanceled();
			}
		}, taskCompletionSource, TaskContinuationOptions.ExecuteSynchronously);
	}
}
