using Framework.GraphView.Editor;
using UnityEditor;
using UnityEngine;

namespace Framework.Bot.Editor
{
	public abstract class CompositeGraphNode : BTGraphNode
	{
		private BTSequence btSequence;

		public override Vector2 Size => new Vector2(175, 70);
		public override Color Outline => BTLocalPreferences.Instance.compositeColor;

		public virtual string Header { get; } = "Composite";
		public virtual Texture2D Icon { get; } = BehaviourTreePreferences.Instance.sequencerIcon;

		public override void OnGUI(Rect rect)
		{
			EditorGUI.LabelField(rect.SetHeight(15f), Header, GraphStyle.Header0Middle);

			var icon = Icon;
			var iconRect = new Rect(rect.x + rect.width / 2f - 32, rect.y + 25, 64, 32);

			if (icon != null)
			{
				GUI.DrawTexture(iconRect, icon);
			}

			base.OnGUI(rect);
		}
	}
}