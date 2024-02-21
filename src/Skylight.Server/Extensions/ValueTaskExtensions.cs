using System.Runtime.CompilerServices;

namespace Skylight.Server.Extensions;

internal static class ValueTaskExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void Wait(this ValueTask task)
	{
		if (!task.IsCompletedSuccessfully)
		{
			task.AsTask().GetAwaiter().GetResult();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool TryGetOrSuppressThrowing<T>(this ValueTask<T> task, out T? value, out Awaiter<T> awaiter)
	{
		if (task.IsCompletedSuccessfully)
		{
			value = task.Result;
			awaiter = default;

			return true;
		}
		else
		{
			value = default;
			awaiter = new Awaiter<T>(task.AsTask());

			return false;
		}
	}

	internal readonly struct Awaiter<T> : ICriticalNotifyCompletion
	{
		private readonly ConfiguredTaskAwaitable.ConfiguredTaskAwaiter awaitable;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Awaiter(Task task)
		{
			this.awaitable = task.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing).GetAwaiter();
		}

		public bool IsCompleted
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => this.awaitable.IsCompleted;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T? GetResult()
		{
			this.awaitable.GetResult();

			return default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void OnCompleted(Action continuation) => this.awaitable.OnCompleted(continuation);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UnsafeOnCompleted(Action continuation) => this.awaitable.UnsafeOnCompleted(continuation);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Awaiter<T> GetAwaiter() => this;
	}
}
