
namespace PG.Entities.WordTrees
{
	public class WordDictionaryTree : IEquatable<WordDictionaryTree?>
	{
		public TreeRoot<string> Root { get; set; } = new TreeRoot<string>();

		#region IEquatable implementation
		public override bool Equals(object? obj)
		{
			return Equals(obj as WordDictionaryTree);
		}

		public virtual bool Equals(WordDictionaryTree? other)
		{
			return other is not null
				&& EqualityComparer<TreeRoot<string>>.Default.Equals(Root, other.Root);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Root);
		}

		public static bool operator ==(WordDictionaryTree? left, WordDictionaryTree? right)
		{
			return EqualityComparer<WordDictionaryTree>.Default.Equals(left, right);
		}

		public static bool operator !=(WordDictionaryTree? left, WordDictionaryTree? right)
		{
			return !(left == right);
		} 
		#endregion

		public override string ToString() => $"{Root.Children.Count} nodes";
	}
}
