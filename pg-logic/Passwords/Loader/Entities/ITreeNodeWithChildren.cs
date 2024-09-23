namespace PG.Logic.Passwords.Loader.Entities
{
	internal interface ITreeNodeWithChildren<T>
	{
		public Dictionary<char, TreeNode<T>> Children { get; set; }
	}
}
