using UnityEditor;
using UnityEngine;

namespace Framework.GraphView.Editor
{
	public abstract class GraphToolbar : IGUIView
	{
		public void OnGUI(EditorWindow window, Rect rect)
		{
			var toolbarRect = rect;
			toolbarRect.height = 20f;
			
			GUILayout.BeginArea(toolbarRect, EditorStyles.toolbar);
			OnGUI();
			GUILayout.EndArea();
		}

		protected abstract void OnGUI();
	}
}