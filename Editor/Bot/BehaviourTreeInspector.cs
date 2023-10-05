using System.Collections.Generic;
using System.Linq;
using Framework.GraphView.Editor;
using Framework.Bot;
using UnityEditor;
using UnityEngine;

namespace Framework.Bot.Editor
{
	public class BehaviourTreeInspector : IGUIView
	{
		// Private fields

		private BTNode node;

		private float inspectorWidth = 200f;
		private bool isResizing;

		private bool initialized;

		private Rect inspectorRect;
		private Vector2 mouseDragStartPosition;

		private Rect rect;
		private GraphWindow window;

		private List<string> savedGraphs;

		private GUIContent loadIcon = new GUIContent("", BehaviourTreePreferences.Instance.loadIcon, "Load");
		private GUIContent saveIcon = new GUIContent("", BehaviourTreePreferences.Instance.saveIcon, "Save");
		private GUIContent createIcon = new GUIContent("", BehaviourTreePreferences.Instance.createIcon, "Create");
		private GUIContent formatIcon = new GUIContent("", BehaviourTreePreferences.Instance.formatIcon, "Format");
		private GUIContent settingsIcon = new GUIContent("", BehaviourTreePreferences.Instance.settingsIcon, "Settings");

		public BehaviourTreeInspector(GraphWindow graphWindow)
		{
			this.window = graphWindow;
			savedGraphs = BTLocalPreferences.Instance.GetSavedGraphs();
		}

		public void OnEnable()
		{
			Selection.selectionChanged -= SelectionChanged;
			Selection.selectionChanged += SelectionChanged;
		}

		private void SelectionChanged()
		{
			if (Selection.activeObject != null && Selection.activeObject is BTNode btNode)
			{
				node = btNode;
			}
		}

		public void OnGUI(EditorWindow window, Rect rect)
		{
			this.rect = rect;
			if (initialized == false)
			{
				OnEnable();
				initialized = true;
			}

			inspectorRect = rect;
			//inspectorRect.width = inspectorWidth;
			//inspectorRect.y += GUIWindow.ToolbarHeight;
			//inspectorRect.height -= GUIWindow.ToolbarHeight;
//
			//EditorGUI.DrawRect(inspectorRect, BackgroundColor);

			//UpdateDragging(window, rect);
			//DrawContent(inspectorRect);
			DrawTabs();
			DrawNavigation();
			//DrawLayers();
			DrawZoom();
		}

		private void DrawContent(Rect rect)
		{
			var header = new Rect(rect.x, rect.y, rect.width - 1f, 30);

			EditorGUI.DrawRect(header, new Color(0.24f, 0.24f, 0.24f));
			//EditorGUI.LabelField(header, $"{tree.name}");

			GraphStyle.DrawHorizontalLine(header.SetY(30), 1);

			//if (node != null)
			//{
			//	var contentRect = rect;
			//	GUILayout.BeginArea(contentRect.AddX(15).AddWidth(-20).AddY(40));
			//	var serialziedObjhect = new SerializedObject(node);
			//	serialziedObjhect.DrawInspectorExcept("m_Script");
			//	GUILayout.EndArea();
			//}
		}

		private void DrawTabs()
		{
			var toolbarRect = rect.SetHeight(30f).AddY(32f);
			GraphStyle.DrawHorizontalLine(toolbarRect.AddY(-1), 1, new Color(0.48f, 0.48f, 0.48f));
			EditorGUI.DrawRect(toolbarRect, new Color(0.11f, 0.11f, 0.11f));

			GUILayout.BeginArea(toolbarRect);
			GUILayout.BeginHorizontal();

			for (int i = 0; i < savedGraphs.Count; i++)
			{
				var graphNames = savedGraphs[i].Split("/");
				var graphName = $"{graphNames[^1].Replace(".asset", "")}";
				var style = window.Tree != null && graphName == window.Tree.name ? GraphStyle.ToolbarTabActive : GraphStyle.ToolbarTab;
				//if (GUILayout.Button(graphName, style, GUILayout.Height(23f)))
				//{
				//	
				//}

				if (DrawTabButton(graphName, savedGraphs[i], window.Tree != null && graphName == window.Tree.name))
				{
					var graph = AssetDatabase.LoadAssetAtPath<BehaviourTree>(savedGraphs[i]);
					if (graph != null)
					{
						window.SetTree(graph);
					}
				}

				// GUILayout.Space(2);
				
				// if (GUILayout.Button(new GUIContent("", BehaviourTreePreferences.Instance.closeIcon), GraphStyle.IconCenter, GUILayout.Width(15),
				// 	    GUILayout.Height(23)))
				// {
				// 	var result= EditorUtility.DisplayDialog("Close tab",
				// 		$"Do you want to close {graphName} tab?", "Close", "Cancel");
				//
				// 	if (result)
				// 	{
				// 		BTLocalPreferences.Instance.RemoveSavedGraph(savedGraphs[i]);
				//
				// 		if (savedGraphs.Count <= 0)
				// 		{
				// 			window.SetTree(null);
				// 		}
				// 		else
				// 		{
				// 			var lastSavedGraph = BTLocalPreferences.Instance.GetSavedGraphs().LastOrDefault();
				// 			var graph = AssetDatabase.LoadAssetAtPath<BehaviourTree>(lastSavedGraph);
				// 			window.SetTree(graph);
				// 		}
				// 	}
				// }
			}

			GUILayout.FlexibleSpace();

			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}

		private void DrawNavigation()
		{
			var toolbarRect = rect.SetHeight(30f);

			toolbarRect = toolbarRect.AddY(1f);
			EditorGUI.DrawRect(toolbarRect, new Color(0.11f, 0.11f, 0.11f));

			GUILayout.BeginArea(toolbarRect.AddY(2.5f).AddX(5).AddWidth(-10));

			GUILayout.BeginHorizontal();

			GUI.color = new Color(0.78f, 0.79f, 0.82f);
            
			if (DrawNavigationButton(loadIcon))
			{
				window.Load();
			}
			
			if (DrawNavigationButton(saveIcon))
			{
				window.QuickSave();
			}
			
			if (DrawNavigationButton(createIcon))
			{
				window.CreateNew<BehaviourTree>();
			}
			
			if (DrawNavigationButton(formatIcon))
			{
				window.FormatTree();
			}
            
			GUILayout.FlexibleSpace();
			
			if (DrawNavigationButton(settingsIcon))
			{
				EditorGUIUtility.PingObject(BehaviourTreePreferences.Instance);
				Selection.activeObject = BTLocalPreferences.Instance;
			}

			GUI.color = Color.white;
			
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}

		private void DrawLayers()
		{
			var toolbarRect = rect.SetHeight(20f).AddY(58f);
			GraphStyle.DrawHorizontalLine(toolbarRect, 1, new Color(0.25f, 0.25f, 0.25f));

			toolbarRect = toolbarRect.AddY(1f);
			EditorGUI.DrawRect(toolbarRect, new Color(0.1f, 0.1f, 0.1f));
		}

		private void DrawZoom()
		{
			var zoomRect = rect;
			zoomRect = zoomRect.SetWidth(100f).SetHeight(20f).AddX(rect.width - 80f).SetY(rect.height - 30f);

			var zoom = 100 - (window.Viewer.zoom.x - GraphPreferences.Instance.minZoom) /
				(GraphPreferences.Instance.maxZoom - GraphPreferences.Instance.minZoom) * 100;

			EditorGUI.LabelField(zoomRect, $"Zoom: {Mathf.Round(zoom)}%");
		}

		private void UpdateDragging(EditorWindow window, Rect rect)
		{
			var lineRect = inspectorRect;
			lineRect.width = 1f;
			lineRect.x += inspectorRect.width - 1f;
			EditorGUI.DrawRect(lineRect, new Color(0.14f, 0.14f, 0.14f));

			var handleRect = inspectorRect;
			handleRect.width = 5f;
			handleRect.x += inspectorRect.width - 5f;

			EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.ResizeHorizontal);

			if (Event.current.type == EventType.MouseDown && handleRect.Contains(Event.current.mousePosition))
			{
				isResizing = true;
				mouseDragStartPosition = Event.current.mousePosition;
			}
			else if (Event.current.type == EventType.MouseUp || Event.current.type == EventType.Ignore)
			{
				isResizing = false;
			}

			if (isResizing)
			{
				float deltaX = Event.current.mousePosition.x - mouseDragStartPosition.x;
				inspectorWidth += deltaX;
				inspectorWidth = Mathf.Clamp(inspectorWidth, 100f, rect.width);

				mouseDragStartPosition = Event.current.mousePosition;

				window.Repaint();
			}
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