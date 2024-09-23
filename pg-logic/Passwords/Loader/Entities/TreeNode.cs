﻿namespace PG.Logic.Passwords.Loader.Entities
{
    /// <summary>
    /// Represents a node in a tree structure of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type of the node</typeparam>
    /// <param name="value">The value of type <typeparamref name="T"/> to store.</param>
    internal class TreeNode<T>(T value) : ITreeNodeWithChildren<T>
    {
        /// <summary>
        /// The value of the node.
        /// </summary>
        public T Value { get; set; } = value;

        /// <summary>
        /// The children of the node.
        /// </summary>
        public Dictionary<char, TreeNode<T>> Children { get; set; } = [];
    }
}
