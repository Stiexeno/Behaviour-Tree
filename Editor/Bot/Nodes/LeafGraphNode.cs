using Framework.GraphView.Editor;
using UnityEditor;
using UnityEngine;

namespace Framework.Bot.Editor
{
	public class LeafGraphNode : BTGraphNode
	{
		public override Vector2 Size => new Vector2(175, 30);
		public override Color Outline => BTLocalPreferences.Instance.leafColor;

		public override void OnGUI(Rect rect)
		{
			DynamicSize = Size;
			EditorGUI.LabelField(rect.SetHeight(20f), GetFormattedName().AddSpacesBetweenCapital(), GraphStyle.Header0Middle);
			base.OnGUI(rect);
		}
	}
}