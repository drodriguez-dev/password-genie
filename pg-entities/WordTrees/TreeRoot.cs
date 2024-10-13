namespace PG.Entities.WordTrees
{
	public class TreeRoot<T> : ITreeNodeWithChildren<T>
	{
		public Dictionary<char, TreeNode<T>> Children { get; set; } = [];

		public override string ToString() => $"{Children.Count} children";
	}
}
