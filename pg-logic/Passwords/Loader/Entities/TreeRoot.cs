namespace PG.Logic.Passwords.Loader.Entities
{
    internal class TreeRoot<T> : ITreeNodeWithChildren<T>
    {
        public Dictionary<char, TreeNode<T>> Children { get; set; } = [];
    }
}
