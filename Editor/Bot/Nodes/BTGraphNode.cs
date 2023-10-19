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
			
			GUI.Label(rect.SetHeight(2).AddY(position.y).AddX(1).AddWidth(-2f), string.Empty, GraphStyle.Skin(GraphStyle.GraphSkin.DefaultSeparator));
			//GraphStyle.DrawHorizontalLine(rect.AddY(position.y));
			position.y += 20;
			DynamicSize = position;
		}
	}
}