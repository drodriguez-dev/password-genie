namespace PG.Logic.Passwords.Loader.Entities
{
	public class WordDictionaryTree
	{
		public TreeRoot<char> Root { get; set; } = new TreeRoot<char>();

		public override string ToString() => $"{Root.Children.Count} nodes";
	}
}
