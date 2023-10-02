using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework.GraphView.Editor
{
	public static class GraphNodeCreation
	{
		public static GraphNode DuplicateSingle(GraphCanvas canvas, GraphNode original)
		{
			GraphNode duplicate = canvas.CreateNode(original.Behaviour.GetType());
            
			duplicate.Position = original.Position + new Vector2(-original.DynamicSize.x / 2f - 15, original.DynamicSize.y + 15);

			return duplicate;
		}
		
		public static List<GraphNode> DuplicateMultiple(GraphCanvas canvas, IEnumerable<GraphNode> originals)
		{
			var duplicateMap = originals.ToDictionary(og => og, og => DuplicateSingle(canvas, og));

			// Reconstruct connection in clone nodes.
			foreach (GraphNode original in originals)
			{
				for (int i = 0; i < original.ChildCount(); i++)
				{
					// Only consider children if they were also cloned.
					if (duplicateMap.TryGetValue(original.GetChildAt(i), out GraphNode cloneChild))
					{
						GraphNode cloneParent = duplicateMap[original];
						cloneChild.SetParent(cloneParent);
					}
				}
			}

			return duplicateMap.Values.ToList();
		}
	}
}