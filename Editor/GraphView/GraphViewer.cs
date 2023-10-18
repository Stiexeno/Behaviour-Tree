using System;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace Framework.GraphView.Editor
{
	public class GraphViewer
	{
		public Vector2 zoom = Vector2.one;
		public Vector2 panOffset = Vector2.zero;
		
		public GraphCanvas Canvas { get; set; }
        
		public float ZoomScale => zoom.x;

		public static float ZoomDelta => GraphPreferences.Instance.zoomDelta;
		public static float MinZoom => GraphPreferences.Instance.minZoom;
		public static float MaxZoom => GraphPreferences.Instance.maxZoom;
		public static float PanSpeed => GraphPreferences.Instance.panSpeed;
		public static float ZoomSmoothTime => 35f;

		public Action CustomOverlayDraw;
		public Action<CanvasTransform> CustomDraw;

		public void Draw(CanvasTransform canvasTransform)
		{
			if (Event.current.type == EventType.Repaint)
			{
				DrawGrid(canvasTransform);
			}
			
			DrawContents(canvasTransform);
		}

		private void DrawContents(CanvasTransform canvasTransform)
		{
			var canvasRect = new Rect(Vector2.zero, canvasTransform.size);
			ScaleUtility.BeginScale(canvasRect, ZoomScale, 20);

			CustomDraw?.Invoke(canvasTransform);
			DrawNodes(canvasTransform);
			DraweConnections(canvasTransform);
			
			ScaleUtility.EndScale(canvasRect, ZoomScale, 20);
			
			CustomOverlayDraw?.Invoke();
		}

		private void DrawGrid(CanvasTransform canvasTransform)
		{
			var canvasRect = new Rect(Vector2.zero, canvasTransform.size);
			GraphDrawer.DrawGrid(canvasRect, GraphPreferences.Instance.gridTexture, canvasTransform.zoom, canvasTransform.pan);
		}
		
		public void DrawStaticGrid(Vector2 size)
		{
			var canvasRect = new Rect(Vector2.zero, size);
			GraphDrawer.DrawStaticGrid(canvasRect, GraphPreferences.Instance.gridTexture);
		}

		private void DrawNodes(CanvasTransform canvasTransform)
		{
			var nodes = Canvas.Nodes;
			
			for (int i = 0; i < nodes.Count; i++)
			{
				GraphDrawer.DrawNode(canvasTransform, nodes[i], new Color(0.24f, 0.25f, 0.26f));
			}
		}

		private void DraweConnections(CanvasTransform canvasTransform)
		{
			var nodes = Canvas.Nodes;

			if (Canvas.Nodes == null || Canvas.Nodes.Count == 0)
				return;
			
			GraphDrawer.connectionDrawers.Clear();
			for (int i = 0; i < nodes.Count; i++)
			{
				GraphNode node = nodes[i];

				if (true)
				{
					GraphDrawer.DrawNodeConnections(canvasTransform, node);
				}
			}
		}
	}
}