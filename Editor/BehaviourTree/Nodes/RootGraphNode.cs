﻿using Framework.GraphView.Editor;
using Framework.Bot;
using UnityEditor;
using UnityEngine;

namespace Framework.Bot.Editor
{
	public class RootGraphNode : BTGraphNode
	{
		private BTRoot btRoot;
	
		public override Vector2 Size => new Vector2(100, 40);
		public override Color Outline => new Color(0.73f, 0.73f, 0.75f);

		public override void OnGUI(Rect rect)
		{
			DynamicSize = Size;
			EditorGUI.LabelField(rect.SetHeight(20f), "Root", GraphStyle.Header0Middle);
		
			//base.OnGUI(rect);
		}
	}
}
