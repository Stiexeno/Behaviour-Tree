using Framework.GraphView.Editor;
using UnityEngine;

namespace Framework.Bot.Editor
{
	public class BTGraphNode : GraphNode
	{
		public override void OnGUI(Rect rect)
		{
			DynamicSize = Size;
			var position = Size;
			GraphStyle.DrawHorizontalLine(rect.AddY(position.y));
			position.y += 20;
			DynamicSize = position;
		}
	}
}