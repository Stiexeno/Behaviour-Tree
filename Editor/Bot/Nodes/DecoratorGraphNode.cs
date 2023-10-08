using Framework.GraphView.Editor;
using UnityEditor;
using UnityEngine;

namespace Framework.Bot.Editor
{
	public class DecoratorGraphNode : BTGraphNode
	{
		public override Vector2 Size => new Vector2(175, 60);
		public override Color Outline => BTLocalPreferences.Instance.decoratorColor;

		public override void OnGUI(Rect rect)
		{
			DynamicSize = Size;
			EditorGUI.LabelField(rect.SetHeight(20f), GetFormattedName().AddSpacesBetweenCapital(), GraphStyle.Header0Middle);
			GraphStyle.DrawHorizontalLine(rect.AddY(30));

			var decorator = Behaviour as BTDecorator;
			var invertText = decorator.invert ? "Invert" : "Direct";
			
			GUI.color = decorator.invert ? new Color(0.97f, 0.02f, 0f) : new Color(0f, 0.97f, 0.02f);
			EditorGUI.LabelField(rect.SetWidth(20).SetHeight(20).AddX(3).AddY(35), new GUIContent("", BehaviourTreePreferences.Instance.dotIcon));
			GUI.color = Color.white;
			
			EditorGUI.LabelField(rect.AddX(18f).AddY(35).SetHeight(20).AddX(2), invertText);
            
			if (GUI.Button(rect.AddY(35).SetHeight(20).SetWidth(60), "", GUIStyle.none))
			{
				decorator.invert = !decorator.invert;
				EditorUtility.SetDirty(decorator);
			}
		}
	}
}
