using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using SF = UnityEngine.SerializeField;

namespace Framework.GraphView.Editor
{
	public abstract class GUIWindow : EditorWindow
	{
		protected readonly HashSet<IGUIElement> graphElements = new HashSet<IGUIElement>();
		
		public Rect CanvasInputRect
		{
			get
			{
				var rect = new Rect(Vector2.zero, position.size);
				rect.y += 0;
				rect.height -= 0;
				return rect;
			}
		}

		public static float ToolbarHeight { get; set; } = 0;
		
		public static float EditorDeltaTime { get; private set; }
		double lastTimeSinceStartup = 0f;

		protected virtual void OnGUI()
		{
			for (int i = 0; i < graphElements.Count; i++)
			{
				if (graphElements.ElementAt(i) is IGUIView guiView)
				{
					guiView.OnGUI(this, CanvasInputRect);
				}
			}
			
			SetEditorDeltaTime();
		}

		protected virtual void OnEnable()
		{
		}
		
		private void SetEditorDeltaTime()
		{
			if (lastTimeSinceStartup == 0f)
			{
				lastTimeSinceStartup = Time.realtimeSinceStartup;
			}
			
			EditorDeltaTime = (float)(Time.realtimeSinceStartup- lastTimeSinceStartup);
			lastTimeSinceStartup = Time.realtimeSinceStartup;
		}
	}
}