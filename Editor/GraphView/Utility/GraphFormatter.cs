using System.Text;
using Framework;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace Framework.GraphView.Editor
{
	public static class GraphFormatter
	{
		/// <summary>
		/// Formats the tree to look nicely.
		/// </summary>
		public static void Format(GraphNode root, Vector2 anchor)
		{
			// Sort parent-child connections so formatter uses latest changes.
			foreach (GraphNode node in TreeTraversal.PreOrder(root))
			{
				node.SortChildren();
			}

			var positioning = new FormatPositioning();

			foreach (GraphNode node in TreeTraversal.PostOrder(root))
			{
				PositionHorizontal(node, positioning);
			}

			foreach (GraphNode node in TreeTraversal.PreOrder(root))
			{
				PositionVertical(node);
			}

			// Move the entire subtree to the anchor.
			Vector2 offset = GraphSingleDrag.StartDrag(root, root.Center);
			GraphSingleDrag.SetSubtreePosition(root, anchor, offset);
		}

		private static void PositionHorizontal(GraphNode node, FormatPositioning positioning)
		{
			float xCoord;

			int childCount = node.ChildCount();

			// If it is a parent of 2 or more children then center in between the children.
			if (childCount > 1)
			{
				// Get the x-midpoint between the first and last children.
				Vector2 firstChildPos = node.GetChildAt(0).Center;
				Vector2 lastChildPos = node.GetChildAt(childCount - 1).Center;
				float xMid = (firstChildPos.x + lastChildPos.x) / 2f;

				xCoord = xMid;
				positioning.xIntermediate = xMid;
			}

			// A node with 1 child, place directly above child.
			else if (childCount == 1)
			{
				xCoord = positioning.xIntermediate;
			}

			// A leaf node
			else
			{
				float branchWidth = MaxWidthForBranchList(node);
				positioning.xLeaf += 0.5f * (positioning.lastLeafWidth + branchWidth) + FormatPositioning.xLeafSeparation;

				xCoord = positioning.xLeaf;
				positioning.xIntermediate = positioning.xLeaf;
				positioning.lastLeafWidth = branchWidth;
			}

			// Set to 0 on the y-axis for this pass.
			node.Center = new Vector2(xCoord, 0f);
		}

		private static void PositionVertical(GraphNode node)
		{
			GraphNode parent = node.Parent;
			if (parent != null)
			{
				float ySeperation = parent.ChildCount() == 1
					? FormatPositioning.yLevelSeparation / 2f
					: FormatPositioning.yLevelSeparation;

				float x = node.Position.x;
				float y = parent.Position.y + parent.Size.y + ySeperation;
				node.Position = new Vector2(x, y);
			}
		}

		// A "branch list" is a tree branch where nodes only have a single child.
		// e.g. Decorator -> Decorator -> Decorator -> Task
		private static float MaxWidthForBranchList(GraphNode leaf)
		{
			float maxWidth = leaf.Size.x;
			var parent = leaf.Parent;

			while (parent != null && parent.ChildCount() == 1)
			{
				maxWidth = Mathf.Max(maxWidth, parent.Size.x);
				parent = parent.Parent;
			}

			return maxWidth;
		}
		
		public static string AddSpacesBetweenCapital(this string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return string.Empty;

			var newText = new StringBuilder(text.Length * 2);
			newText.Append(text[0]);

			for (int i = 1; i < text.Length; i++)
			{
				if (char.IsUpper(text[i]))
					if (text[i - 1] != ' ' && !char.IsUpper(text[i - 1]) ||
					    char.IsUpper(text[i - 1]) &&
					    i < text.Length - 1 && !char.IsUpper(text[i + 1]))
						newText.Append(' ');
				newText.Append(text[i]);
			}

			return newText.ToString();
		}

		/// <summary>
		/// A helper class to accumulate positioning data when formatting the tree.
		/// </summary>
		private class FormatPositioning
		{
			public float xLeaf = 0f;
			public float xIntermediate = 0f;
			public float lastLeafWidth = 0f;

			/// <summary>
			/// Horizontal separation between leaf nodes.
			/// </summary>
			public const float xLeafSeparation = 30f;

			/// <summary>
			/// Vertical separation between nodes.
			/// </summary>
			public const float yLevelSeparation = 120f;
		}
	}
}