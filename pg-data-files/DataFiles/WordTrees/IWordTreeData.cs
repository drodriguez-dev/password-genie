using PG.Entities.WordTrees;

namespace PG.Data.Files.DataFiles.WordTrees
{
    public interface IWordTreeData
    {
        WordDictionaryTree FetchTree();

        void SaveTree(WordDictionaryTree tree);
    }
}
