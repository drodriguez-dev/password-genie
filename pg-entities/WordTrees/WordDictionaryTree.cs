namespace PG.Entities.WordTrees
{
	public class WordDictionaryTree
	{
		public TreeRoot<char> Root { get; set; } = new TreeRoot<char>();

		public override string ToString() => $"{Root.Children.Count} nodes";
	}
}
