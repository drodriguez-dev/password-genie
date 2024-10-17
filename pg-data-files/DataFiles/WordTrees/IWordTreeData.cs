using PG.Entities.WordTrees;

namespace PG.Data.Files.DataFiles.WordTrees
{
	public interface IWordTreeData
	{
		WordDictionaryTree FetchTree(Stream fileStream);

		void SaveTree(Stream fileStream, WordDictionaryTree tree);
	}
}
