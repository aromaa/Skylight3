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
		}
		else
		{
			parent.Right = node;
		}

		this.RebalanceAfterInsert(node);
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
			this.Fixup(node);

			this.count--;
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
		}
		else if (node.Right is null)
		{
			this.ShiftNodes(node, node.Left);
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
			successor.Balance = node.Balance;
		}

		this.RebalanceAfterDeletion(node);
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
		}
		else
		{
			node.Parent.Right = replacement;
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
		for (Node? parent = node.Parent; parent is not null; parent = node.Parent)
		{
			Node? parentOfParent;
			Node newNode;
			if (parent.Right == node)
			{
				if (parent.Balance > 0)
				{
					parentOfParent = parent.Parent;

					newNode = node.Balance < 0
						? this.RotateRightLeft(parent, node)
						: this.RotateLeft(parent, node);
				}
				else
				{
					if (parent.Balance < 0)
					{
						parent.Balance = 0;
						break;
					}

					parent.Balance = 1;
					node = parent;
					continue;
				}
			}
			else
			{
				if (parent.Balance < 0)
				{
					parentOfParent = parent.Parent;

					newNode = node.Balance > 0
						? this.RotateLeftRight(parent, node)
						: this.RotateRight(parent, node);
				}
				else
				{
					if (parent.Balance > 0)
					{
						parent.Balance = 0;
						break;
					}

					parent.Balance = -1;
					node = parent;
					continue;
				}
			}

			newNode.Parent = parentOfParent;

			if (parentOfParent is not null)
			{
				if (parentOfParent.Left == parent)
				{
					parentOfParent.Left = newNode;
				}
				else
				{
					parentOfParent.Right = newNode;
				}
			}
			else
			{
				this.root = newNode;
			}

			break;
		}
	}

	private void RebalanceAfterDeletion(Node? node)
	{
		Node? parentOfParent;
		for (Node? parent = node?.Parent; parent is not null; parent = parentOfParent)
		{
			parentOfParent = parent.Parent;

			int balance;
			if (parent.Left == node)
			{
				if (parent.Balance > 0)
				{
					Node right = parent.Right!;
					balance = right.Balance;

					node = balance < 0
						? this.RotateRightLeft(parent, right)
						: this.RotateLeft(parent, right);
				}
				else
				{
					if (parent.Balance == 0)
					{
						parent.Balance = 1;
						break;
					}

					node = parent;
					node.Balance = 0;
					continue;
				}
			}
			else if (parent.Right == node)
			{
				if (parent.Balance < 0)
				{
					Node left = parent.Left!;
					balance = left.Balance;

					node = balance > 0
						? this.RotateLeftRight(parent, left)
						: this.RotateRight(parent, left);
				}
				else
				{
					if (parent.Balance == 0)
					{
						parent.Balance = -1;
						break;
					}

					node = parent;
					node.Balance = 0;
					continue;
				}
			}
			else
			{
				break;
			}

			node.Parent = parentOfParent;

			if (parentOfParent is not null)
			{
				if (parent == parentOfParent.Left)
				{
					parentOfParent.Left = node;
				}
				else
				{
					parentOfParent.Right = node;
				}
			}
			else
			{
				this.root = node;
			}

			if (balance == 0)
			{
				break;
			}
		}
	}

	private Node RotateLeft(Node node, Node right)
	{
		node.Right = right.Left;
		if (node.Right is not null)
		{
			node.Right.Parent = node;
		}

		right.Left = node;
		node.Parent = right;

		(node.Balance, right.Balance) = right.Balance switch
		{
			0 => (1, -1),
			_ => (0, 0)
		};

		return right;
	}

	private Node RotateRight(Node node, Node left)
	{
		node.Left = left.Right;
		if (node.Left is not null)
		{
			node.Left.Parent = node;
		}

		left.Right = node;
		node.Parent = left;

		(node.Balance, left.Balance) = left.Balance switch
		{
			0 => (1, -1),
			_ => (0, 0)
		};

		return left;
	}

	private Node RotateRightLeft(Node node, Node right)
	{
		Node rightLeft = right.Left!;

		right.Left = rightLeft.Right;
		if (right.Left is not null)
		{
			right.Left.Parent = right;
		}

		rightLeft.Right = right;
		right.Parent = rightLeft;

		node.Right = rightLeft.Left;
		if (node.Right is not null)
		{
			node.Right.Parent = node;
		}

		rightLeft.Left = node;
		node.Parent = rightLeft;

		(node.Balance, right.Balance, rightLeft.Balance) = rightLeft.Balance switch
		{
			0 => (0, 0, 0),
			> 0 => (-1, 0, 0),
			_ => (0, 1, 0)
		};

		return rightLeft;
	}

	private Node RotateLeftRight(Node node, Node left)
	{
		Node leftRight = left.Right!;

		left.Right = leftRight.Left;
		if (left.Right is not null)
		{
			left.Right.Parent = left;
		}

		leftRight.Left = left;
		left.Parent = leftRight;

		node.Left = leftRight.Right;
		if (node.Left is not null)
		{
			node.Left.Parent = node;
		}

		leftRight.Right = node;
		node.Parent = leftRight;

		(node.Balance, left.Balance, leftRight.Balance) = leftRight.Balance switch
		{
			0 => (0, 0, 0),
			< 0 => (1, 0, 0),
			_ => (0, -1, 0)
		};

		return leftRight;
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
