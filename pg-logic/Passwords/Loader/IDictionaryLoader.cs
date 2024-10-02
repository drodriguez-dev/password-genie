using PG.Logic.Passwords.Loader.Entities;

namespace PG.Logic.Passwords.Loader
{
	public interface IDictionaryLoader
	{
		internal WordDictionaryTree WordTree { get; set; }

		void Load();

		bool IsLeafNodeReached(string word);

		bool TrySearchLastPossibleLeafNode(string word, int depthLevel, out ITreeNodeWithChildren<char> node);
	}
}
