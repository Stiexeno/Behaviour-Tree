using System;
using System.Collections.Generic;
using System.Linq;
using Framework.Bot;
using Framework.Bot.Editor;
using UnityEditor;
using UnityEngine;

namespace Framework.GraphView.Editor
{
	public static class GraphDrawer
	{
		internal static readonly float duration = 10;
		internal static int selectedConnection = -2;

		internal static readonly List<Action> connectionDrawers = new List<Action>();

		private static Dot[] dots =
		{
			new Dot(duration * 0f, duration),
			new Dot(duration * 0.2f, duration),
			new Dot(duration * 0.4f, duration),
			new Dot(duration * 0.6f, duration),
			new Dot(duration * 0.8f, duration)
		};

		// Grid

		public static void DrawGrid(Rect canvas, Texture texture, float zoom, Vector2 pan)
		{
			const float scale = 1f;

			var size = canvas.size;
			var center = size / 2f;

			var tile = new Vector2(texture.width * scale, texture.height * scale);

			float xOffset = -(center.x * zoom + pan.x) / tile.x;
			float yOffset = ((center.y - size.y) * zoom + pan.y) / tile.y;

			Vector2 tileOffset = new Vector2(xOffset, yOffset);

			float tileAmountX = Mathf.Round(size.x * zoom) / tile.x;
			float tileAmountY = Mathf.Round(size.y * zoom) / tile.y;

			Vector2 tileAmount = new Vector2(tileAmountX, tileAmountY);

			GUI.DrawTextureWithTexCoords(canvas, texture, new Rect(tileOffset, tileAmount));
		}

		public static void DrawStaticGrid(Rect canvas, Texture2D texture)
		{
			var size = canvas.size;
			var center = size / 2f;

			float xOffset = -center.x / texture.width;
			float yOffset = (center.y - size.y) / texture.height;

			// Offset from origin in tile units
			Vector2 tileOffset = new Vector2(xOffset, yOffset);

			float tileAmountX = Mathf.Round(size.x) / texture.width;
			float tileAmountY = Mathf.Round(size.y) / texture.height;

			// Amount of tiles
			Vector2 tileAmount = new Vector2(tileAmountX, tileAmountY);

			// Draw tiled background
			GUI.DrawTextureWithTexCoords(canvas, texture, new Rect(tileOffset, tileAmount));
		}

		// Node

		public static void DrawNode(CanvasTransform t, GraphNode node, Color statusColor)
		{
			Rect screenRect = node.RectPosition;
			screenRect.position = t.CanvasToScreenSpace(screenRect.position);

			Color originalColor = GUI.color;

			//DrawNodeGradient(screenRect.AddHeight(6f).AddWidth(6f).AddX(-3f), new Color(0f, 0f, 0f, 0.28f));

			DrawNodeStatus(screenRect, node);
			

			//DrawOutline(screenRect.Expand(1f), Color.black);
			
			//DrawOutline(screenRect.AddHeight(4f).AddY(-2f).AddWidth(-2).AddX(1), node.Outline);
			
			//GUI.DrawTexture(screenRect, GraphPreferences.Instance.defaultNodeBackground, ScaleMode.StretchToFill, true, 5, statusColor, 5, 0);
			//GUI.color = new Color(0.24f, 0.25f, 0.25f);
			GUI.color = new Color(0.97f, 1f, 1f, 0.26f);
			GUI.Label(screenRect.Expand(1, 2), string.Empty, GraphStyle.Skin(GraphStyle.GraphSkin.NodeShadow));
			GUI.color = Color.white;
			if (screenRect.Contains(Event.current.mousePosition))
			{
				GUI.Label(screenRect, string.Empty, GraphStyle.Skin(GraphStyle.GraphSkin.NodeSelectedBackground));
			}
			else
			{
				GUI.Label(screenRect, string.Empty, GraphStyle.Skin(GraphStyle.GraphSkin.NodeBackground));
			}
			
			
			GUI.color = Color.white;
			if (node.Selected)
			{
				GUI.color = new Color(0f, 1f, 0.98f);
				GUI.Label(screenRect.Expand(3), string.Empty, GraphStyle.Skin(GraphStyle.GraphSkin.NodeSelectedOutline));
               
			}

			//GUI.color = new Color(1f, 1f, 1f, 0.22f);
			//GUI.Label(screenRect, string.Empty, GraphStyle.Skin(GraphStyle.GraphSkin.NodeGradient));

			//GUI.color = new Color(0.09f, 1f, 0.17f);
			
			//GUI.color = Color.white;

			GUI.color =	node.Outline;
			GUI.Label(screenRect.AddWidth(-2).AddX(1f), string.Empty, GraphStyle.Skin(GraphStyle.GraphSkin.NodeOutline));
			GUI.color = Color.white;

			//DrawNodeBackground(screenRect, statusColor);
			DrawPorts(t, node);

			GUI.BeginGroup(screenRect);

			Rect localRect = node.RectPosition;
			localRect.position = Vector2.zero;

			GUILayout.BeginArea(localRect, GUIStyle.none);

			node.OnGUI(node.ContentRect);

			GUILayout.EndArea();

			GUI.EndGroup();
			GUI.color = originalColor;
		}

		private static void DrawNodeBackground(Rect screenRect, Color color)
		{
			GUI.DrawTexture(
				screenRect, GraphPreferences.Instance.defaultNodeBackground,
				ScaleMode.StretchToFill,
				true,
				0,
				color,
				0,
				5f);
		}

		private static void DrawNodeGradient(Rect screenRect, Color color)
		{
			GUI.DrawTexture(
				screenRect, GraphPreferences.Instance.defaultNodeGadient,
				ScaleMode.StretchToFill,
				true,
				0,
				color,
				0,
				5f);
		}
		
		private static void DrawNodeStatus(Rect rect, GraphNode node)
		{
			var btNode = (BTNode)node.Behaviour;
			var status = btNode.EditorStatus;

			if (status == BTNode.BTEditorStatus.Inactive)
				return;

			rect = rect.Expand(2f, 4f);
			if (status == BTNode.BTEditorStatus.Success)
			{
				DrawNodeBackground(rect, Color.green);
			}

			if (status == BTNode.BTEditorStatus.Running)
			{
				DrawNodeBackground(rect, new Color(0.28f, 0.26f, 1f));
			}

			if (status == BTNode.BTEditorStatus.Failure)
			{
				DrawNodeBackground(rect, Color.red);
			}
		}

		// Ports

		private static void DrawPorts(CanvasTransform t, GraphNode node)
		{
			var input = node.InputRect;
			input.position = t.CanvasToScreenSpace(node.InputRect.position);

			if (input.Contains(Event.current.mousePosition) && node.IsParentless() == false)
			{
				GUI.Label(input.SetHeight(20f).AddY(5).AddX(2.5f).AddWidth(-5f), string.Empty, GraphStyle.Skin(GraphStyle.GraphSkin.NodePortBox));
			}

			var output = node.OutputRect;
			output.position = t.CanvasToScreenSpace(node.OutputRect.position);

			if (output.Contains(Event.current.mousePosition) && node.HasOutput)
			{
				GUI.Label(output.SetHeight(20f).AddY(0).AddX(2.5f).AddWidth(-5f), string.Empty, GraphStyle.Skin(GraphStyle.GraphSkin.NodePortBox));
			}
		}

		// Connections

		public static void DrawPendingConnection(Vector2 start, Vector2 end, Color color)
		{
			var originalColor = Handles.color;
			Handles.color = color;

			float halfDist = (start - end).magnitude / 2f;

			Vector2 directionToEnd = (end - start).normalized;
			Vector2 directionToStart = (start - end).normalized;

			Vector2 axisForTipAlignment = Vector3.up;

			Vector2 startTip = Vector3.Project(directionToEnd, axisForTipAlignment) * halfDist + (Vector3)start;
			Vector2 endTip = Vector3.Project(directionToStart, axisForTipAlignment) * halfDist + (Vector3)end;

			if (startTip == endTip)
			{
				Handles.DrawAAPolyLine(GraphPreferences.Instance.defaultConnection, 3, start, endTip);
			}

			else
			{
				Handles.DrawAAPolyLine(GraphPreferences.Instance.defaultConnection, 3, start, startTip, endTip, end, endTip);
			}

			end.y -= 10;
			end.x -= 10;
			GUI.DrawTexture(
				end.ToRect(20, 20), GraphPreferences.Instance.edgeArrow,
				ScaleMode.StretchToFill,
				true,
				0,
				color,
				0,
				0f);

			Handles.color = originalColor;
		}

		public static void DrawNodeConnections(CanvasTransform t, GraphNode node)
		{
			if (node.ChildCount() == 0)
				return;

			Color connectionColor = BTLocalPreferences.Instance.connectionColor;

			if (Application.isPlaying)
			{
				connectionColor = new Color(0.3f, 0.3f, 0.31f);
			}

			// Start the Y anchor coord at the tip of the Output port.
			float yoffset = node.RectPosition.yMax;

			// Calculate the anchor position.
			float anchorX = node.RectPosition.center.x;
			float anchorY = (yoffset + node.GetNearestInputY()) / 2f;

			// Anchor line, between the first and last child.

			// Find the min and max X coords between the children and the parent.
			node.GetBoundsX(out float anchorLineStartX, out float anchorLineEndX);

			// Get start and end positions of the anchor line (The common line where the parent and children connect).
			var anchorLineStart = new Vector2(anchorLineStartX, anchorY);
			var anchorLineEnd = new Vector2(anchorLineEndX, anchorY);

			// The tip where the parent starts its line to connect to the anchor line.
			var parentAnchorTip = new Vector2(anchorX, yoffset);

			// The point where the parent connects to the anchor line.
			var parentAnchorLineConnection = new Vector2(anchorX, anchorY);

			var p1 = parentAnchorTip;
			var p2 = parentAnchorLineConnection;

			var targetNodes = node.Children;
			
			if (Application.isPlaying)
			{
				targetNodes = node.Children.OrderBy(n =>
					n.Behaviour is BTNode btNode && btNode.status == (BTStatus)BTNode.BTEditorStatus.Running).ToList();
			}
			foreach (GraphNode child in targetNodes)
			{
				// Get the positions to draw a line between the node and the anchor line.
				Vector2 center = child.InputRect.center;

				var anchorLineConnection = new Vector2(center.x, anchorY);

				var p3 = anchorLineConnection;
				var p4 = center;

				if (child.Behaviour is BTNode btNode && btNode.EditorStatus != BTNode.BTEditorStatus.Inactive)
				{
					DrawStatusConnections(btNode, t,
						p1, p2, p3, p4);
				}
				else
				{
					if ((int)p1.x == (int)p4.x)
					{
						DrawLineScreenSpace(t, connectionColor, 3f, p1, p4);
					}
					else
					{
						DrawLineScreenSpace(t, connectionColor, 3f, p1, p2, p3, p4);
					}
				}

				if (HandleClickConnection(t, child.Behaviour.PreOrderIndex,
					    parentAnchorTip,
					    parentAnchorLineConnection,
					    new Vector2(parentAnchorLineConnection.x, anchorLineStart.y),
					    new Vector2(anchorLineConnection.x, anchorLineEnd.y),
					    anchorLineConnection,
					    center))
				{
				}

				if (selectedConnection == child.Behaviour.PreOrderIndex)
				{
					connectionDrawers.Add(() =>
					{
						if ((int)p1.x == (int)p4.x)
						{
							DrawLineHovered(t, new Color(0.33f, 0.74f, 0.93f), 4f, p1, p4);
						}
						else
						{
							DrawLineHovered(t, new Color(0.33f, 0.74f, 0.93f), 4f, p1, p2, p3, p4);
						}
					});
				}
			}

			connectionDrawers.ForEach(x => x.Invoke());
		}

		private static void DrawEdgeArrow(CanvasTransform t, Vector2 position, Color color)
		{
			position = t.CanvasToScreenSpace(position);

			position.y -= 6;
			position.x -= 10;
			GUI.DrawTexture(
				position.ToRect(20, 20), GraphPreferences.Instance.edgeArrow,
				ScaleMode.StretchToFill,
				true,
				0,
				color,
				0,
				0f);
		}

		private static void DrawStatusConnections(BTNode node, CanvasTransform t, params Vector3[] points)
		{
			var status = node.EditorStatus;

			var color = Color.white;

			if (status == BTNode.BTEditorStatus.Success)
			{
				color = Color.green;
			}

			if (status == BTNode.BTEditorStatus.Running)
			{
				color = new Color(0.28f, 0.26f, 1f);
			}

			if (status == BTNode.BTEditorStatus.Failure)
			{
				color = Color.red;
			}

			if ((int)points[1].x == (int)points[^1].x)
			{
				if (status == BTNode.BTEditorStatus.Running)
				{
					DrawLineHovered(t, color, 4f, points[0], points[^1]);
				}
				else
				{
					DrawLineScreenSpace(t, color, 3f, points[0], points[^1]);
				}
			}
			else
			{
				if (status == BTNode.BTEditorStatus.Running)
				{
					DrawLineHovered(t, color, 4f, points);
				}
				else
				{
					DrawLineScreenSpace(t, color, 3f, points);
				}
			}
		}

		private static bool HandleClickConnection(CanvasTransform t, int index, params Vector2[] points)
		{
			for (int i = 0; i < points.Length - 1; i++)
			{
				var start = t.CanvasToScreenSpace(points[i]);
				var end = t.CanvasToScreenSpace(points[i + 1]);

				if (HandleUtility.DistanceToLine(start, end) < 5f)
				{
					if (GraphInput.IsUnlickAction(Event.current))
					{
						selectedConnection = index;
					}
				}
			}

			return false;
		}

		// private static void DrawLineCanvasSpace(CanvasTransform t, Vector2 start, Vector2 end, Color color, float width)
		// {
		// 	start = t.CanvasToScreenSpace(start);
		// 	end = t.CanvasToScreenSpace(end);
		//
		// 	if (t.IsScreenAxisLineInView(start, end))
		// 	{
		// 		DrawLineScreenSpace(start, end, color, width);
		// 	}
		// }

		private static void DrawLineHovered(CanvasTransform t, Color color, float width, params Vector3[] points)
		{
			DrawLineScreenSpace(t, color, width, points);

			for (int i = 0; i < dots.Length; i++)
			{
				dots[i].Update();
				var point = MultiLerp(points, dots[i].normalizedTime);
				var pointRect = new Rect(point.x, point.y, 20, 20);

				GUI.DrawTexture(
					pointRect.AddX(-9).AddY(-9), GraphPreferences.Instance.edgeFlow,
					ScaleMode.StretchToFill,
					true,
					0,
					color,
					0,
					0f);
			}
		}

		private static void DrawLineScreenSpace(CanvasTransform t, Color color, float width, params Vector3[] points)
		{
			var edgeArrowPoint = points[^1];
			for (int i = 0; i < points.Length; i++)
			{
				var screenPoint = t.CanvasToScreenSpace(points[i]);
				points[i] = new Vector2((int)screenPoint.x, (int)screenPoint.y);
			}

			var originalColor = Handles.color;
			Handles.color = color;
			Handles.DrawAAPolyLine(GraphPreferences.Instance.defaultConnection, width, points);
            
			DrawEdgeArrow(t, edgeArrowPoint, color);

			Handles.color = originalColor;
		}

		// private static void DrawLineScreenSpace(Vector2 start, Vector2 end, Color color, float width)
		// {
		// 	var originalColor = Handles.color;
		// 	Handles.color = color;
		// 	Handles.DrawAAPolyLine(GraphPreferences.Instance.defaultNodeBackground, width, start, end);
		// 	Handles.color = originalColor;
		// }

		// Utility

		private static Vector3 MultiLerp(Vector3[] points, float normalizedDistance)
		{
			if (points.Length == 1)
				return points[0];
			else if (points.Length == 2)
				return Vector3.Lerp(points[0], points[1], normalizedDistance);

			if (normalizedDistance <= 0)
				return points[0];

			if (normalizedDistance >= 1)
				return points[^1];

			float totalDistance = 0f;
			float[] segmentLengths = new float[points.Length - 1];

			for (int i = 0; i < points.Length - 1; i++)
			{
				float segmentLength = Vector3.Distance(points[i], points[i + 1]);
				segmentLengths[i] = segmentLength;
				totalDistance += segmentLength;
			}

			float targetDistance = normalizedDistance * totalDistance;

			float currentDistance = 0f;
			for (int i = 0; i < points.Length - 1; i++)
			{
				float segmentLength = segmentLengths[i];
				if (currentDistance + segmentLength >= targetDistance)
				{
					float t = (targetDistance - currentDistance) / segmentLength;
					return Vector3.Lerp(points[i], points[i + 1], t);
				}

				currentDistance += segmentLength;
			}

			return Vector3.zero; // Handle the case where something goes wrong
		}
	}

	public struct Dot
	{
		public float normalizedTime;
		private float timelapsed;
		private float duration;

		public Dot(float normalizedTime, float duration)
		{
			this.duration = duration;
			timelapsed = normalizedTime;
			this.normalizedTime = 0;
		}

		public void Update()
		{
			normalizedTime = timelapsed / duration;

			timelapsed += GUIWindow.EditorDeltaTime;
			if (timelapsed > duration)
			{
				timelapsed = 0;
				this.duration = GraphDrawer.duration;
			}
		}
	}
}