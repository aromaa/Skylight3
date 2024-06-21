using System.Numerics;
using System.Runtime.CompilerServices;

namespace Skylight.Server.Collections;

internal sealed class IntervalTree<TKey, TValue>
	where TKey : INumber<TKey>
{
	private Node? root;

	private int count;

	internal int Count => this.count;

	internal bool Add(TKey from, TKey to, TValue value)
	{
		Interval interval = new(from, to);

		Node? current = this.root;
		if (current is null)
		{
			this.root = new Node(interval, value);
			this.count++;

			return true;
		}

		Node parent;
		int compare;
		do
		{
			compare = interval.CompareTo(current.Interval);
			if (compare == 0)
			{
				return current.Add(value);
			}

			parent = current;
			current = (compare < 0) ? current.Left : current.Right;
		}
		while (current is not null);

		Node node = new(parent, interval, value);
		if (compare < 0)
		{
			parent.Left = node;
			parent.Balance--;
		}
		else
		{
			parent.Right = node;
			parent.Balance++;
		}

		this.RebalanceAfterInsert(parent);
		this.Fixup(node);

		this.count++;

		return true;
	}

	internal void Remove(TKey min, TKey max, TValue value)
	{
		Node? node = this.GetNode(new Interval(min, max));
		if (node is not null && node.Remove(value) && node.Items.Count <= 0)
		{
			this.Remove(node);
		}
	}

	internal IEnumerable<TValue> GetItemsBetween(TKey min, TKey max)
	{
		Stack<Node> stack = new(2 * BitOperations.Log2((uint)this.count + 1));

		Node? node = this.root;
		while (node is not null)
		{
			if (node.Interval.Min >= min && node.Interval.Min <= max)
			{
				stack.Push(node);
			}
			else if (node.Left is null || node.Left.Interval.Min < min || node.Left.Interval.Min > max)
			{
				node = node.Right;

				continue;
			}

			node = node.Left;
		}

		while (stack.TryPop(out node))
		{
			foreach (TValue item in node.Items)
			{
				yield return item;
			}

			node = node.Right;
			while (node is not null)
			{
				if (node.Interval.Min >= min && node.Interval.Min <= max)
				{
					stack.Push(node);

					node = node.Left;
				}
				else
				{
					break;
				}
			}
		}
	}

	internal SearchResult FindGabGreedy(TKey from, TKey emptySpace, Predicate<TValue> predicate, out TKey value)
	{
		if (this.root is null)
		{
			Unsafe.SkipInit(out value);

			return SearchResult.Fail;
		}

		Stack<Node> stack = new(2 * BitOperations.Log2((uint)this.count + 1));

		Node? node = this.root;
		while (node is not null)
		{
			if (node.Max <= from || node.Interval.Max <= from)
			{
				stack.Push(node);
			}
			else if (node.Right is null || (node.Right.Max > from && node.Right.Interval.Max > from))
			{
				node = node.Left;

				continue;
			}

			node = node.Right;
		}

		while (stack.TryPop(out node))
		{
			if (node.Interval.Max <= from)
			{
				if (predicate(node.Items.First()))
				{
					Node? compareAgainst = this.Successor(node);
					if (compareAgainst is null || compareAgainst.Interval.Min - node.Interval.Max >= emptySpace)
					{
						if (compareAgainst?.Left is null || compareAgainst.Left.Max <= from)
						{
							value = node.Interval.Max;

							return SearchResult.Success;
						}
					}
				}

				//The position was invalid, jump ahead to the next possible position
				from = node.Interval.Min - emptySpace;
			}

			node = node.Left;
			while (node is not null)
			{
				if (node.Max <= from || node.Interval.Max <= from)
				{
					stack.Push(node);

					node = node.Right;
				}
				else
				{
					break;
				}
			}
		}

		for (node = this.root; node.Left is not null; node = node.Left)
		{
		}

		value = node.Interval.Min - emptySpace;

		return SearchResult.Fallback;
	}

	internal TKey? Max
	{
		get
		{
			if (this.root is null)
			{
				return default;
			}

			Node node = this.root;
			while (node.Right is not null)
			{
				node = node.Right;
			}

			return node.Interval.Max;
		}
	}

	internal TKey? Min
	{
		get
		{
			if (this.root is null)
			{
				return default;
			}

			Node node = this.root;
			while (node.Left is not null)
			{
				node = node.Left;
			}

			return node.Interval.Min;
		}
	}

	internal IEnumerable<TValue> Values
	{
		get
		{
			Stack<Node> stack = new(2 * BitOperations.Log2((uint)this.count + 1));

			Node? node = this.root;
			while (node is not null)
			{
				stack.Push(node);

				node = node.Left;
			}

			while (stack.TryPop(out node))
			{
				foreach (TValue item in node.Items)
				{
					yield return item;
				}

				node = node.Right;
				while (node is not null)
				{
					stack.Push(node);

					node = node.Left;
				}
			}
		}
	}

	private Node? GetNode(Interval interval)
	{
		Node? node = this.root;
		while (node is not null)
		{
			int compare = interval.CompareTo(node.Interval);
			if (compare == 0)
			{
				return node;
			}

			node = (compare < 0) ? node.Left : node.Right;
		}

		return null;
	}

	private void Remove(Node node)
	{
		if (node.Left is null)
		{
			this.ShiftNodes(node, node.Right);

			this.RebalanceAfterDeletion(node.Parent);
		}
		else if (node.Right is null)
		{
			this.ShiftNodes(node, node.Left);

			this.RebalanceAfterDeletion(node.Parent);
		}
		else
		{
			Node successor = this.Successor(node)!;
			if (successor.Parent != node)
			{
				this.ShiftNodes(successor, successor.Right);

				successor.Right = node.Right;
				successor.Right.Parent = successor;
			}

			this.ShiftNodes(node, successor);

			successor.Left = node.Left;
			successor.Left.Parent = successor;

			this.RebalanceAfterDeletion(successor);
		}
	}

	private void ShiftNodes(Node node, Node? replacement)
	{
		if (node.Parent is null)
		{
			this.root = replacement;
		}
		else if (node == node.Parent.Left)
		{
			node.Parent.Left = replacement;
			node.Parent.Balance++;
		}
		else
		{
			node.Parent.Right = replacement;
			node.Parent.Balance--;
		}

		if (replacement is not null)
		{
			replacement.Parent = node.Parent;
		}
	}

	private Node? Successor(Node node)
	{
		if (node.Right is not null)
		{
			Node nextNode = node.Right;
			while (nextNode.Left is not null)
			{
				nextNode = nextNode.Left;
			}

			return nextNode;
		}
		else
		{
			Node? parentNode = node.Parent;
			while (parentNode is not null && parentNode.Right == node)
			{
				node = parentNode;
				parentNode = parentNode.Parent;
			}

			return parentNode;
		}
	}

	private void Fixup(Node node)
	{
		node.Max = node.Interval.Max;
		if (node.Left is not null)
		{
			node.Max = TKey.Max(node.Max, node.Left.Max);
		}

		if (node.Right is not null)
		{
			node.Max = TKey.Max(node.Max, node.Right.Max);
		}

		Node? parent = node.Parent;
		while (parent is not null)
		{
			parent.Max = TKey.Max(node.Max, parent.Max);

			node = parent;
			parent = node.Parent;
		}
	}

	private void RebalanceAfterInsert(Node node)
	{
		while (node.Balance != 0)
		{
			if (node.Balance == 2)
			{
				if (node.Right is null)
				{
					break;
				}

				Node rightNode = node.Right;
				if (rightNode.Balance == 1)
				{
					(rightNode.Balance, node.Balance) = (0, 0);

					this.RotateLeft(node, rightNode);
				}
				else
				{
					Node rightLeftNode = rightNode.Left!;

					(int rightLeftBalance, rightLeftNode.Balance) = (rightLeftNode.Balance, 0);

					(rightNode.Balance, node.Balance) = rightLeftBalance switch
					{
						1 => (0, -1),
						-1 => (1, 0),
						_ => (0, 0)
					};

					this.RotateRight(rightNode, rightLeftNode);
					this.RotateLeft(node, rightNode);
				}

				break;
			}
			else if (node.Balance == -2)
			{
				if (node.Left is null)
				{
					break;
				}

				Node leftNode = node.Left;
				if (leftNode.Balance == -1)
				{
					(leftNode.Balance, node.Balance) = (0, 0);

					this.RotateRight(node, leftNode);
				}
				else
				{
					Node leftRightNode = leftNode.Right!;

					(int leftRightBalance, leftRightNode.Balance) = (leftRightNode.Balance, 0);

					(leftNode.Balance, node.Balance) = leftRightBalance switch
					{
						1 => (-1, 0),
						-1 => (0, 1),
						_ => (0, 0)
					};

					this.RotateLeft(leftNode, leftRightNode);
					this.RotateRight(node, leftNode);
				}

				break;
			}

			if (node.Parent is null)
			{
				break;
			}

			if (node.Parent.Left == node)
			{
				node.Parent.Balance--;
			}
			else
			{
				node.Parent.Balance++;
			}

			node = node.Parent;
		}
	}

	private void RebalanceAfterDeletion(Node? node)
	{
		if (node is null)
		{
			return;
		}

		this.RebalanceAfterInsert(node);
	}

	private void RotateLeft(Node node, Node rightNode)
	{
		Node? rightLeft = rightNode.Left;
		Node? parent = node.Parent;

		rightNode.Parent = parent;
		rightNode.Left = node;
		node.Right = rightLeft;
		node.Parent = rightNode;

		if (rightLeft is not null)
		{
			rightLeft.Parent = node;
		}

		if (this.root == node)
		{
			this.root = rightNode;
		}
		else if (parent!.Right == node)
		{
			parent.Right = rightNode;
		}
		else
		{
			parent.Left = rightNode;
		}

		this.Fixup(node);
		this.Fixup(rightNode);
	}

	private void RotateRight(Node node, Node leftNode)
	{
		Node? leftRight = leftNode.Right;
		Node? parent = node.Parent;

		leftNode.Parent = parent;
		leftNode.Right = node;
		node.Left = leftRight;
		node.Parent = leftNode;

		if (leftRight is not null)
		{
			leftRight.Parent = node;
		}

		if (this.root == node)
		{
			this.root = leftNode;
		}
		else if (parent!.Left == node)
		{
			parent.Left = leftNode;
		}
		else
		{
			parent.Right = leftNode;
		}

		this.Fixup(node);
		this.Fixup(leftNode);
	}

	private sealed class Node
	{
		internal Interval Interval { get; }
		internal TKey Max { get; set; }

		internal HashSet<TValue> Items { get; set; }

		internal Node? Parent { get; set; }
		internal Node? Left { get; set; }
		internal Node? Right { get; set; }

		internal int Balance { get; set; }

		internal Node(Interval interval, TValue item)
		{
			this.Interval = interval;
			this.Max = interval.Max;

			this.Items = [item];
		}

		internal Node(Node parent, Interval interval, TValue item)
			: this(interval, item)
		{
			this.Parent = parent;
		}

		internal bool Add(TValue value) => this.Items.Add(value);
		internal bool Remove(TValue value) => this.Items.Remove(value);
	}

	internal readonly struct Interval(TKey min, TKey max) : IComparable<Interval>
	{
		internal TKey Min { get; } = min;
		internal TKey Max { get; } = max;

		public int CompareTo(Interval other)
		{
			int result = this.Min.CompareTo(other.Min);

			return result != 0 ? result : this.Max.CompareTo(other.Max);
		}

		public override string ToString() => $"{this.Min} - {this.Max}";
	}

	internal enum SearchResult
	{
		Fail,
		Success,
		Fallback
	}
}
