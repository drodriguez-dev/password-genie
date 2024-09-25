namespace PG.Logic.Passwords.Loader.Entities
{
	public class TreeRoot<T> : ITreeNodeWithChildren<T>
	{
		public Dictionary<char, TreeNode<T>> Children { get; set; } = [];

		public override string ToString() => $"{Children.Count} children";
	}
}
