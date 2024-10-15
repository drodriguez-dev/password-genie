using PG.Entities.WordTrees;

namespace PG.Logic.Passwords.Loader
{
	public interface IDictionaryLoader
	{
		internal WordDictionaryTree WordTree { get; set; }

		void Load();

		bool IsLeafNodeReached(string word);

		bool TrySearchLastPossibleLeafNode(string word, int depthLevel, out ITreeNode<string> node);
	}
}
