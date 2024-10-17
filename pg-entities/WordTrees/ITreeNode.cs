namespace PG.Entities.WordTrees
{
	public interface ITreeNode<T>
	{
		public Dictionary<string, TreeNode<T>> Children { get; set; }
	}
}
