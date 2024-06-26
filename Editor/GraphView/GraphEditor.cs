using System;
using System.Collections.Generic;
using System.Linq;
using Framework.Bot.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Framework.GraphView.Editor
{
	public class GraphEditor
	{
		private GraphNode lastCreatedNode;
		private GraphNode pendingParentConnection;

		private static Dictionary<Type, NodeProperties> nodeProperties;

		public GraphViewer Viewer { get; set; }
		public GraphSearch Search { get; set; }
		public GraphCanvas Canvas { get; private set; }
		public GraphWindow Window { get; set; }
		public GraphInput Input { get; } = new GraphInput();
		public GraphSelection NodeSelection { get; } = new GraphSelection();

		public CanvasTransform CanvasTransform { get; set; }

		public static IEnumerable<KeyValuePair<Type, NodeProperties>> Behaviours
		{
			get { return nodeProperties; }
		}

		public event Action OnCanvasChanged;
		private Action<CanvasTransform> MotionAction;
		private Action<GraphInputEvent> ApplyAction;

		public GraphEditor()
		{
			Input.selection = NodeSelection;
			Input.MouseDown += BeginOnMouseDown;
			Input.Click += Clicked;
			Input.MouseUp += MouseUp;
			Input.NodeActionRequest += SingleNodeAction;
			Input.NodeContextClick += NodeContextClick;
			Input.OnOpenSettings += OpenSettings;
		}

		public void SetGraphTree(GraphTree tree)
		{
			NodeSelection.ClearSelection();
			Canvas = new GraphCanvas(tree);
			Viewer.Canvas = Canvas;
			Viewer.zoom = tree.zoomPosition;
			Viewer.panOffset = tree.panPosition;
		}

		public void UpdateView()
		{
			Canvas.OnGUI();
		}

		public void PollInput(Event e, CanvasTransform canvas, Rect inputRect)
		{
			if (lastCreatedNode != null)
			{
				var creationPosition = GraphInput.MousePosition(canvas);
                
				lastCreatedNode.Center = creationPosition;
				lastCreatedNode = null;
			}

			if (Search.IsActive)
				return;

			if (e.type == EventType.MouseDrag)
			{
				if (MotionAction != null)
				{
					MotionAction(canvas);
					OnCanvasChanged();
				}
			}

			if (GraphInput.IsPanAction(e))
			{
				Pan(e.delta);
				OnCanvasChanged();
			}

			if (GraphInput.IsZoomAction(e))
			{
				Zoom(e.delta.y);
				OnCanvasChanged();
			}

			Input.HandleMouseEvents(e, canvas, Canvas.Nodes, inputRect);
		}

		private void Pan(Vector2 delta)
		{
			Viewer.panOffset += delta * Viewer.ZoomScale * GraphViewer.PanSpeed;

			Viewer.panOffset.x = Mathf.Round(Viewer.panOffset.x);
			Viewer.panOffset.y = Mathf.Round(Viewer.panOffset.y);
		}

		private void Zoom(float zoomDirection)
		{
			if (Search.IsActive)
				return;

			float scale = (zoomDirection < 0f) ? (1f - GraphViewer.ZoomDelta) : (1f + GraphViewer.ZoomDelta);
			Viewer.zoom *= scale;

			float cap = Mathf.Clamp(Viewer.zoom.x, GraphViewer.MinZoom, GraphViewer.MaxZoom);
			Viewer.zoom.Set(cap, cap);
		}

		private void StartDrag(GraphInputEvent e)
		{
			if (NodeSelection.IsSingleSelection)
			{
				StartSingleDrag(e);
			}
			else if (NodeSelection.IsMultiSelection)
			{
				StartMultiDrag(e);
			}
		}

		private void StartSingleDrag(GraphInputEvent e)
		{
			GraphNode node = e.node;
			Vector2 offset = GraphSingleDrag.StartDrag(node, e.canvasMousePostion);
			MotionAction = (canvasTransform) => GraphSingleDrag.Drag(node, GraphInput.MousePosition(canvasTransform), offset);
		}

		private void StartMultiDrag(GraphInputEvent e)
		{
			var nodes = GraphMultiDrag.StartDrag(NodeSelection.SelectedNodes, e.canvasMousePostion);
			MotionAction = canvasTransform => GraphMultiDrag.Drag(GraphInput.MousePosition(canvasTransform), nodes);
		}

		private void BeginOnMouseDown(object sender, GraphInputEvent inputEvent)
		{
			if (MotionAction != null)
				return;

			if (inputEvent.IsPortFocused())
			{
				StartConnection(inputEvent);
			}
			else if (inputEvent.IsNodeFocused())
			{
				if (NodeSelection.IsNodeSelected(inputEvent.node) == false)
				{
					NodeSelection.SetSingleSelection(inputEvent.node);
				}

				StartDrag(inputEvent);
			}
			else
			{
				StartAreaSelection(inputEvent);
			}
		}

		private void Clicked(object sender, GraphInputEvent inputEvent)
		{
			if (Event.current.control && inputEvent.node != null)
			{
				NodeSelection.SetSingleSelection(inputEvent.node);
			}
		}

		private void MouseUp(object sender, GraphInputEvent e)
		{
			ApplyAction?.Invoke(e);
			ClearActions();
		}

		private void NodeContextClick(object sender, GraphNode e)
		{
			NodeSelection.SetSingleSelection(e);
		}

		private void RemoveSelectedNodes()
		{
			Canvas.Remove(node => NodeSelection.IsNodeSelected(node));
			NodeSelection.SetTreeSelection(Canvas.Tree);
		}

		private void SingleNodeAction(object sender, NodeContext actionType)
		{
			switch (actionType)
			{
				case NodeContext.FormatTree:
					GraphNode root = NodeSelection.SingleSelectedNode;
					GraphFormatter.Format(root, root.Center);
					break;

				case NodeContext.Delete:
					RemoveSelectedNodes();
					break;

				case NodeContext.Duplicate:
					NodeSelection.SetSingleSelection(GraphNodeCreation.DuplicateSingle(Canvas, NodeSelection.SingleSelectedNode));
					break;

				case NodeContext.DuplicateSelection:
					NodeSelection.SetMultiSelection(GraphNodeCreation.DuplicateMultiple(Canvas, NodeSelection.SelectedNodes));
					break;

				case NodeContext.OpenSource:
					OpenSource(NodeSelection.SingleSelectedNode.Behaviour);
					break;
			}

			//UpdateAbortableSelection();
		}

		public void ClearActions()
		{
			if (Search.IsActive)
				return;

			ApplyAction = null;
			MotionAction = null;
			Viewer.CustomDraw = null;
			Viewer.CustomOverlayDraw = null;
		}

		private void StartAreaSelection(GraphInputEvent startEvent)
		{
			Vector2 startScreenSpace = Event.current.mousePosition;
			Vector2 start = startEvent.canvasMousePostion;

			ApplyAction = applyEvent =>
			{
				Vector2 end = applyEvent.canvasMousePostion;
				var areaSelection = GraphAreaSelect.NodesUnderArea(Canvas.Nodes, start, end);
				NodeSelection.SetMultiSelection(areaSelection.ToList());
			};

			Viewer.CustomOverlayDraw = () =>
			{
				// Construct and display the rect.
				Vector2 endScreenSpace = Event.current.mousePosition;
				Rect selectionRect = GraphAreaSelect.SelectionArea(startScreenSpace, endScreenSpace);
				Color selectionColor = new Color(0f, 0.69f, 0.89f, 0.1f);
				Handles.DrawSolidRectangleWithOutline(selectionRect, selectionColor, new Color(0f, 0.95f, 1f, 0.6f));
				OnCanvasChanged();
			};
		}

		private void StartConnection(GraphInputEvent startEvent)
		{
			bool isOutputFocused = startEvent.isOutputFocused;

			GraphNode parent = isOutputFocused ? startEvent.node : GraphConnection.StartConnection(startEvent.node);

			if ((isOutputFocused && startEvent.node.HasOutput == false))
				return;

			if (parent != null)
			{
				ApplyAction = (applyEvent) =>
				{
					if (applyEvent.node != null)
					{
						if (parent != applyEvent.node)
						{
							GraphConnection.FinishConnection(Canvas, parent, applyEvent.node);
						}
					}
					else
					{
						if (isOutputFocused)
						{
							pendingParentConnection = parent;
							Window.OpenSearch(onClose: () =>
							{
								pendingParentConnection = null;
							});
						}
					}
				};

				Viewer.CustomDraw = (canvasTransform) =>
				{
					var start = canvasTransform.CanvasToScreenSpace(parent.OutputRect.center);
					start.y -= GraphPreferences.Instance.portHeight;
					var end = Event.current.mousePosition;

					GraphDrawer.DrawRectConnectionScreenSpace(start, end, BTLocalPreferences.Instance.connectionColor);
					OnCanvasChanged();
				};
			}
		}

		public void CreateNodeFromType(Type type)
		{
			var node = Canvas.CreateNode(type);
			NodeSelection.SetSingleSelection(node);

			lastCreatedNode = node;

			if (pendingParentConnection != null)
			{
				GraphConnection.FinishConnection(Canvas, pendingParentConnection, node);
			}
		}

		private void CreateNodeFromType(object sender, Type type)
		{
			CreateNodeFromType(type);
		}

		public static void FetchGraphBehaviours(IGraphNodeRules graphNodeRules)
		{
			nodeProperties = graphNodeRules.FetchGraphBehaviours();
		}

		public static NodeProperties GetNodeProperties(Type type)
		{
			if (nodeProperties.TryGetValue(type, out var properties))
			{
				return properties;
			}

			return null;
		}
		
		private void OpenSource(GraphBehaviour graphBehaviour)
		{
			var monoScript = MonoScript.FromScriptableObject(graphBehaviour);
			var scriptPath = AssetDatabase.GetAssetPath(monoScript);
			InternalEditorUtility.OpenFileAtLineExternal(scriptPath, 0);
		}

		public void OpenSettings()
		{
			OpenSettings(null, null);
		}
		
		private void OpenSettings(object sender, EventArgs e)
		{
			EditorGUIUtility.PingObject(BehaviourTreePreferences.Instance);
			Selection.activeObject = BTLocalPreferences.Instance;
		}
	}
}