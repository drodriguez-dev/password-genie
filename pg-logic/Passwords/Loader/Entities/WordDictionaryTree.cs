namespace PG.Logic.Passwords.Loader.Entities
{
	internal class WordDictionaryTree
	{
		public TreeRoot<char> Root { get; set; } = new TreeRoot<char>();

		public override string ToString() => $"{Root.Children.Count} nodes";
	}
}
