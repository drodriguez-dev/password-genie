
namespace PG.Entities.WordTrees
{
	public class WordDictionaryTree : IEquatable<WordDictionaryTree?>
	{
		public TreeRoot<char> Root { get; set; } = new TreeRoot<char>();

		#region IEquatable implementation
		public override bool Equals(object? obj)
		{
			return Equals(obj as WordDictionaryTree);
		}

		public virtual bool Equals(WordDictionaryTree? other)
		{
			return other is not null
				&& EqualityComparer<TreeRoot<char>>.Default.Equals(Root, other.Root);
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
