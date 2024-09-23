namespace PG.Logic.Passwords.Loader.Entities
{
    internal class WordDictionary
    {
        public HashSet<string> Words { get; set; } = [];

        public TreeRoot<char> Root { get; set; } = new TreeRoot<char>();
    }
}
