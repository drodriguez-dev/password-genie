namespace PG.Entities.WordTrees
{
	public interface ITreeNodeWithChildren<T>
	{
		public Dictionary<char, TreeNode<T>> Children { get; set; }
	}
}
