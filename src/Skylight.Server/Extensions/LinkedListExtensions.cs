namespace Skylight.Server.Extensions;

internal static class LinkedListExtensions
{
	internal static void Shuffle<T>(this LinkedList<T> linkedList)
	{
		LinkedListNode<T>? node = linkedList.First;
		if (node?.Next is null)
		{
			return;
		}

		for (LinkedListNode<T> current = node; current.Next is not null; current = current.Next)
		{
			if (Random.Shared.NextDouble() < 0.5)
			{
				(current.Value, current.Next.Value) = (current.Next.Value, current.Value);
			}
		}
	}
}
