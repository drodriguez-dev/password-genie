namespace PG.Entities.WordTrees
{
	public static class TreeNodeExtensions
	{
		/// <summary>
		/// Calculates the maximum depth for each node in the tree using post-order traversal.
		/// Leaf nodes will have MaxDepth = 0, their parents = 1, and so on.
		/// </summary>
		public static void CalculateMaxDepth(this ITreeNode<string> node)
		{
			int maxChildDepth = -1;

			foreach (var child in node.Children.Values)
			{
				child.CalculateMaxDepth();
				if (child.MaxDepth > maxChildDepth)
					maxChildDepth = child.MaxDepth;
			}

			if (node is TreeNode<string> treeNode)
				treeNode.MaxDepth = maxChildDepth + 1;
		}
	}
}
