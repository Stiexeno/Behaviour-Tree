using System.Collections.Generic;
using System.Linq;
using Framework.GraphView.Editor;
using UnityEditor;
using UnityEngine;

namespace Framework.Bot.Editor
{
	public class BehaviourTreeInspector : IGUIView
	{
		// Private fields
		
		private bool initialized;

		private Rect rect;
		private GraphWindow window;

		private List<string> savedGraphs;

		private GUIContent loadIcon = new GUIContent("", BehaviourTreePreferences.Instance.loadIcon, "Load");
		private GUIContent saveIcon = new GUIContent("", BehaviourTreePreferences.Instance.saveIcon, "Save CTRL+S");
		private GUIContent createIcon = new GUIContent("", BehaviourTreePreferences.Instance.createIcon, "Create");
		private GUIContent formatIcon = new GUIContent("", BehaviourTreePreferences.Instance.formatIcon, "Format CTRL+G");
		private GUIContent settingsIcon = new GUIContent("", BehaviourTreePreferences.Instance.settingsIcon, "Settings CTRL+ALT+S");

		public BehaviourTreeInspector(GraphWindow graphWindow)
		{
			this.window = graphWindow;
			savedGraphs = BTLocalPreferences.Instance.GetSavedGraphs();
		}

		public void OnEnable()
		{
		}

		public void OnGUI(EditorWindow window, Rect rect)
		{
			this.rect = rect;
			if (initialized == false)
			{
				OnEnable();
				initialized = true;
			}
            
			DrawTabs();
			DrawNavigation();
			DrawZoom();
		}

		private void DrawTabs()
		{
			var toolbarRect = rect.SetHeight(30f).AddY(31f);
			GraphStyle.DrawHorizontalLine(toolbarRect.AddY(-1), 1, new Color(0.48f, 0.48f, 0.48f));
			EditorGUI.DrawRect(toolbarRect, new Color(0.11f, 0.11f, 0.11f));

			GUILayout.BeginArea(toolbarRect);
			GUILayout.BeginHorizontal();

			for (int i = 0; i < savedGraphs.Count; i++)
			{
				var graphNames = savedGraphs[i].Split("/");
				var graphName = $"{graphNames[^1].Replace(".asset", "")}";

				if (DrawTabButton(graphName, savedGraphs[i], window.Tree != null && graphName == window.Tree.name))
				{
					var graph = AssetDatabase.LoadAssetAtPath<BehaviourTree>(savedGraphs[i]);
					if (graph != null)
					{
						window.SetTree(graph);
					}
				}
			}

			GUILayout.FlexibleSpace();

			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}

		private void DrawNavigation()
		{
			var toolbarRect = rect.SetHeight(30f);

			EditorGUI.DrawRect(toolbarRect, new Color(0.11f, 0.11f, 0.11f));

			GUILayout.BeginArea(toolbarRect.AddY(2.5f).AddX(5).AddWidth(-10));

			GUILayout.BeginHorizontal();

			GUI.color = new Color(0.78f, 0.79f, 0.82f);

			if (DrawNavigationButton(createIcon))
			{
				window.CreateNew<BehaviourTree>();
			}
			
			if (DrawNavigationButton(loadIcon))
			{
				window.Load();
			}

			if (DrawNavigationButton(saveIcon))
			{
				window.QuickSave();
			}

			GUILayout.Space(15f);
			
			if (DrawNavigationButton(formatIcon))
			{
				window.FormatTree();
			}

			GUILayout.FlexibleSpace();

			if (DrawNavigationButton(settingsIcon))
			{
				window.Editor.OpenSettings();
			}

			GUI.color = Color.white;

			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}

		private void DrawZoom()
		{
			var zoomRect = rect;
			zoomRect = zoomRect.SetWidth(100f).SetHeight(20f).AddX(rect.width - 80f).SetY(rect.height - 30f);

			var zoom = 100 - (window.Viewer.zoom.x - GraphPreferences.Instance.minZoom) /
				(GraphPreferences.Instance.maxZoom - GraphPreferences.Instance.minZoom) * 100;

			EditorGUI.LabelField(zoomRect, $"Zoom: {Mathf.Round(zoom)}%");
		}

		private bool DrawNavigationButton(GUIContent content)
		{
			var controlRect = EditorGUILayout.GetControlRect(false, GUILayout.Width(25), GUILayout.Height(25));

			if (controlRect.Contains(Event.current.mousePosition))
			{
				GUI.DrawTexture(
					controlRect, GraphPreferences.Instance.defaultNodeBackground,
					ScaleMode.StretchToFill,
					true,
					0,
					new Color(0.38f, 0.38f, 0.38f),
					0,
					5f);
			}

			if (GUI.Button(controlRect, content, GraphStyle.IconCenter))
			{
				return true;
			}

			return false;
		}

		private bool DrawTabButton(string name, string savedGraph, bool active)
		{
			var controlRect = EditorGUILayout.GetControlRect(false, GUILayout.Height(30));

			if (controlRect.Contains(Event.current.mousePosition))
			{
				EditorGUI.DrawRect(controlRect, new Color(0.38f, 0.38f, 0.38f));
			}

			if (GUI.Button(controlRect, name, GraphStyle.ToolbarTab))
			{
				if ((Event.current.type == EventType.Used ||
				     Event.current.type == EventType.MouseDown) &&
				    Event.current.button == 1)
				{
					BTLocalPreferences.Instance.RemoveSavedGraph(savedGraph);

					if (savedGraphs.Count <= 0)
					{
						window.SetTree(null);
					}
					else
					{
						var lastSavedGraph = BTLocalPreferences.Instance.GetSavedGraphs().LastOrDefault();
						var graph = AssetDatabase.LoadAssetAtPath<BehaviourTree>(lastSavedGraph);
						window.SetTree(graph);
					}

					return false;
				}

				return true;
			}

			if (active)
			{
				EditorGUI.DrawRect(controlRect.SetHeight(2).AddY(controlRect.height - 2), new Color(0.98f, 0.78f, 0.05f));
			}

			return false;
		}
	}
}