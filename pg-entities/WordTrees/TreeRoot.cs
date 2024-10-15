
namespace PG.Entities.WordTrees
{
	public class TreeRoot<T> : ITreeNode<T>, IEquatable<TreeRoot<T>?>
	{
		public Dictionary<string, TreeNode<T>> Children { get; set; } = [];

		#region IEquatable implementation
		public override bool Equals(object? obj)
		{
			return Equals(obj as TreeRoot<T>);
		}

		public virtual bool Equals(TreeRoot<T>? other)
		{
			return other is not null
				&& Children.SequenceEqual(other.Children);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Children);
		}

		public static bool operator ==(TreeRoot<T>? left, TreeRoot<T>? right)
		{
			return EqualityComparer<TreeRoot<T>>.Default.Equals(left, right);
		}

		public static bool operator !=(TreeRoot<T>? left, TreeRoot<T>? right)
		{
			return !(left == right);
		} 
		#endregion

		public override string ToString() => $"{Children.Count} children";
	}
}
