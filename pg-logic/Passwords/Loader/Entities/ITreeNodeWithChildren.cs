namespace PG.Logic.Passwords.Loader.Entities
{
	public interface ITreeNodeWithChildren<T>
	{
		public Dictionary<char, TreeNode<T>> Children { get; set; }
	}
}
