using PG.Entities.WordTrees;

namespace PG.Data.Files.WordTrees
{
	public interface IWordTreeData
	{
		WordDictionaryTree FetchTree();

		void SaveTree(WordDictionaryTree tree);
	}
}
