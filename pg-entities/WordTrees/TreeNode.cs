
namespace PG.Entities.WordTrees
{
	/// <summary>
	/// Represents a node in a tree structure of type <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">Type of the node</typeparam>
	/// <param name="value">The value of type <typeparamref name="T"/> to store.</param>
	public class TreeNode<T>(T value) : ITreeNode<T>, IEquatable<TreeNode<T>?>
	{
		/// <summary>
		/// The value of the node.
		/// </summary>
		public T Value { get; set; } = value;

		/// <summary>
		/// The children of the node.
		/// </summary>
		public Dictionary<string, TreeNode<T>> Children { get; set; } = [];

		#region IEquatable implementation
		public override bool Equals(object? obj)
		{
			return Equals(obj as TreeNode<T>);
		}

		public virtual bool Equals(TreeNode<T>? other)
		{
			return other is not null
				&& EqualityComparer<T>.Default.Equals(Value, other.Value)
				&& Children.SequenceEqual(other.Children);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Value, Children);
		}

		public static bool operator ==(TreeNode<T>? left, TreeNode<T>? right)
		{
			return EqualityComparer<TreeNode<T>>.Default.Equals(left, right);
		}

		public static bool operator !=(TreeNode<T>? left, TreeNode<T>? right)
		{
			return !(left == right);
		}
		#endregion

		public override string ToString() => $"{Value} (+{Children.Count})";
	}
}
