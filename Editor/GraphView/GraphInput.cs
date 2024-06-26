using System;
using System.Collections.Generic;
using System.Timers;
using UnityEditor;
using UnityEngine;

namespace Framework.GraphView.Editor
{
	public enum NodeContext { FormatTree, Delete, DuplicateSelection, Duplicate, OpenSource}

	public class GraphInput
	{
		// Generic events

		public event EventHandler<GraphInputEvent> Click;
		public event EventHandler<GraphInputEvent> MouseDown;
		public event EventHandler<GraphInputEvent> MouseUp;
        
		public static event EventHandler<GraphInputEvent> DoubleClick;

		public event EventHandler<GraphNode> NodeContextClick;
		public event EventHandler CanvasContextClick;
		public event EventHandler<NodeContext> NodeActionRequest;

		public event EventHandler SaveRequest;
		public event EventHandler OnKeySpace;
		public event EventHandler OnFormatTree;
		public event EventHandler OnOpenSettings;
		public event EventHandler OnSearchOpen;
		public event EventHandler OnKeyDelete;
		public event EventHandler CanvasLostFocus;

		private readonly Timer doubleClickTimer = new Timer(400);
		private readonly Timer clickTimer = new Timer(120);

		private int quickClicksCount = 0;
		private Vector2 cachedMouseClickPosition;

		public IGraphSelection selection;
		public readonly GenericMenu nodeTypeSelectionMenu = new GenericMenu();

		public void HandleMouseEvents(
			Event e,
			CanvasTransform transform,
			IReadOnlyList<GraphNode> nodes,
			Rect inputRect)
		{
			// Mouse must be inside the editor canvas.
			if (!inputRect.Contains(e.mousePosition))
			{
				CanvasLostFocus?.Invoke(this, EventArgs.Empty);
				return;
			}

			HandleClickActions(transform, nodes, e);

			HandleEditorShortcuts(e);

			if (e.type == EventType.ContextClick)
			{
				HandleContextInput(transform, nodes);
				e.Use();
			}
		}

		private void HandleClickActions(CanvasTransform canvasTransform, IReadOnlyList<GraphNode> nodes, Event e)
		{
			if (IsClickAction(e))
			{
				if (quickClicksCount == 0)
				{
					doubleClickTimer.Start();
					
					doubleClickTimer.Elapsed += handleDoubleClick;
				}

				clickTimer.Start();
				MouseDown?.Invoke(this, CreateInputEvent(canvasTransform, nodes));
			}

			else if (IsUnlickAction(e))
			{
				GraphInputEvent inputEvent = CreateInputEvent(canvasTransform, nodes);

				// A node click is registered if below a time threshold.
				if (clickTimer.Enabled)
				{
					Click?.Invoke(this, inputEvent);
				}

				
				// Collect quick, consecutive clicks.
				if (doubleClickTimer.Enabled)
				{
					quickClicksCount++;
					cachedMouseClickPosition = inputEvent.canvasMousePostion;
				}
				
				// Double click event occured.
				if (quickClicksCount >= 2 && cachedMouseClickPosition == inputEvent.canvasMousePostion)
				{
					DoubleClick?.Invoke(this, inputEvent);
					handleDoubleClick(this, null);
				}

				clickTimer.Stop();
				MouseUp?.Invoke(this, inputEvent);
			}

			void handleDoubleClick(object sender, ElapsedEventArgs elapsedEventArgs)
			{
				doubleClickTimer.Elapsed -= handleDoubleClick;
				doubleClickTimer.Stop();
				doubleClickTimer.Enabled = false;
				quickClicksCount = 0;
			}
		}

		private void HandleEditorShortcuts(Event e)
		{
			if (e.type == EventType.KeyDown && (e.control || e.command) && e.alt && e.keyCode == KeyCode.S)
			{
				e.Use();
				OnOpenSettings?.Invoke(this, EventArgs.Empty);
			}
            
			if (e.type == EventType.KeyUp && (e.control || e.command) && e.keyCode == KeyCode.S)
			{
				e.Use();
				SaveRequest?.Invoke(this, EventArgs.Empty);
			}
			
			if (e.type == EventType.KeyUp && e.keyCode == KeyCode.Space)
			{
				e.Use();
				OnKeySpace?.Invoke(this, EventArgs.Empty);
			}
			
			if (e.type == EventType.KeyUp && e.keyCode == KeyCode.Delete)
			{
				e.Use();
				OnNodeAction(NodeContext.Delete);
			}
			
			if (e.type == EventType.KeyDown && (e.control || e.command) && e.keyCode == KeyCode.G)
			{
				e.Use();
				OnFormatTree?.Invoke(this, EventArgs.Empty);
			}

			if (e.type == EventType.ContextClick && !e.control && !e.alt)
			{
				e.Use();
				OnSearchOpen?.Invoke(this, EventArgs.Empty);
			}
		}

		private void HandleContextInput(CanvasTransform t, IReadOnlyList<GraphNode> nodes)
		{
			if (selection.IsMultiSelection)
			{
				HandleMultiContext();
			}
			else
			{
				HandleSingleContext(t, nodes);
			}
		}

		private void HandleSingleContext(CanvasTransform t, IReadOnlyList<GraphNode> nodes)
		{
			GraphNode node = NodeUnderMouse(t, nodes);

			if (node != null)
			{
				NodeContextClick?.Invoke(this, node);
				CreateSingleSelectionContextMenu(node).ShowAsContext();
			}

			else
			{
				CanvasContextClick?.Invoke(this, EventArgs.Empty);
				//ShowCreateNodeMenu();
			}
		}

		private void ShowCreateNodeMenu()
		{
			if (nodeTypeSelectionMenu != null)
			{
				nodeTypeSelectionMenu.ShowAsContext();
			}
		}

		private GenericMenu CreateSingleSelectionContextMenu(GraphNode node)
		{
			var menu = new GenericMenu();
			menu.AddItem(new GUIContent("Delete"), false, OnNodeAction, NodeContext.Delete);
			menu.AddItem(new GUIContent("Duplicate"), false, OnNodeAction, NodeContext.Duplicate);
			menu.AddItem(new GUIContent("Format Subtree"), false, OnNodeAction, NodeContext.FormatTree);
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Open Source"), false, OnNodeAction, NodeContext.OpenSource);
			return menu;
		}

		private void HandleMultiContext()
		{
			CreateMultiSelectionContextMenu().ShowAsContext();
		}

		private void OnNodeAction(object o)
		{
			NodeActionRequest?.Invoke(this, (NodeContext)o);
		}
		
		private GenericMenu CreateMultiSelectionContextMenu()
		{
			var menu = new GenericMenu();
			menu.AddItem(new GUIContent("Delete"), false, OnNodeAction, NodeContext.Delete);
			menu.AddItem(new GUIContent("Duplicate"), false, OnNodeAction, NodeContext.DuplicateSelection);
			return menu;
		}

		// Static

		public static bool IsContextAction(Event e)
		{
			return e.type == EventType.MouseDown && e.button == 1;
		}

		public static bool IsPanAction(Event e)
		{
			return e.type == EventType.MouseDrag && e.button == 2 || e.type == EventType.MouseDrag && e.alt && e.button == 1;
		}

		public static bool IsZoomAction(Event e)
		{
			return e.type == EventType.ScrollWheel;
		}

		public static bool IsClickAction(Event e)
		{
			return e.type == EventType.MouseDown && e.button == 0;
		}
		
		public static bool IsExitAction(Event e)
		{
			return e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape;
		}
		
		public static bool IsEnterAction(Event e)
		{
			return e.type == EventType.KeyDown && e.keyCode == KeyCode.Return;
		}

		private static bool IsUnlickAction(Event e)
		{
			return e.type == EventType.MouseUp && e.button == 0;
		}

		public static Vector2 MousePosition(CanvasTransform canvasTransform)
		{
			return canvasTransform.ScreenToCanvasSpace(Event.current.mousePosition);
		}

		public static bool IsUnderMouse(CanvasTransform transform, Rect r)
		{
			return r.Contains(MousePosition(transform));
		}

		public static bool IsHovered(CanvasTransform transform, Rect r)
		{
			var cachedRect = r;
			cachedRect.position = transform.CanvasToScreenSpace(r.position);
			return cachedRect.Contains(Event.current.mousePosition);
		}

		private static GraphNode NodeUnderMouse(CanvasTransform transform, IReadOnlyList<GraphNode> nodes)
		{
			// Iterate in reverse so the last drawn node (top) receives input first.
			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				GraphNode node = nodes[i];
				if (IsUnderMouse(transform, node.RectPosition))
				{
					return node;
				}
				
				var isInputFocused = IsUnderMouse(transform, node.InputRect);
				var isOutputFocused = IsUnderMouse(transform, node.OutputRect);

				if (isInputFocused || isOutputFocused)
				{
					return node;
				}
			}

			return null;
		}

		private static GraphInputEvent CreateInputEvent(CanvasTransform transform, IReadOnlyList<GraphNode> nodes)
		{
			
			bool isInputFocused = false;
			bool isOutputFocused = false;
			GraphNode node = NodeUnderMouse(transform, nodes);
			
			if (node != null)
			{
				isInputFocused = IsUnderMouse(transform, node.InputRect);
				isOutputFocused = IsUnderMouse(transform, node.OutputRect);
			}

			return new GraphInputEvent
			{
				transform = transform,
				canvasMousePostion = MousePosition(transform),
				node = node,
				isInputFocused = isInputFocused,
				isOutputFocused = isOutputFocused
			};
		}
	}
}