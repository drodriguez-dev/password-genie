namespace PG.Entities.WordTrees
{
	public interface ITreeNode<T>
	{
		public Dictionary<char, TreeNode<T>> Children { get; set; }
	}
}
