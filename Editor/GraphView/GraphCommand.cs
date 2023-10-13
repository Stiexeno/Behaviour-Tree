using System;
using System.Collections.Generic;
using System.Linq;
using Framework.GraphView.Editor;
using UnityEngine;

internal static class GraphCommand
{
	private static Stack<UndoRecord> undoStack = new Stack<UndoRecord>();
	
	public static void Record(Action action)
	{
		undoStack.Push(new UndoRecord(action));
	}
	
	internal static void AppendLastRecord(Action action)
	{
		var lastUndoRecord = undoStack.First();
		lastUndoRecord.Record(action);
	}
	
	internal static void Undo()
	{
		if (undoStack.Count > 0)
		{
			undoStack.Pop().Undo();
		}
	}
	
	internal static void RecordConnectionWithChild(GraphNode child) => Record(() => child.SetParent(null));

	internal static void RecordConnectionRemoval(GraphNode parent, GraphNode child) => Record(() => child.SetParent(parent));

	internal static void RecordNodeCreation(GraphCanvas canvas, GraphNode node) => Record(() => canvas.Remove(node));

	/// <summary>
	/// Record node removal and create undo nodes.
	/// </summary>
	/// <param name="canvas">Canvas</param>
	/// <param name="nodes">Nodes</param>
	/// <param name="parents">Referenced nodes parents</param>
	internal static void RecordNodeRemoval(GraphCanvas canvas, List<GraphNode> nodes, List<GraphNode> parents)
	{
		Record(() =>
		{
			var childs = new List<GraphNode>();

			for (var index = 0; index < nodes.Count; index++)
			{
				var node = nodes[index];
				var undoNode = canvas.CreateNode(node.Behaviour.GetType());
				undoNode.Position = node.Position;

				childs.Add(undoNode);
				if (node.Parent != null)
				{
					parents.Add(node.Parent);
				}
			}

			for (int i = 0; i < parents.Count; i++)
			{
				childs[i].SetParent(parents[i]);
			}
		});
	}
}

internal class UndoRecord
{
	private readonly List<Action> actions = new List<Action>();

	internal UndoRecord(Action action)
	{
		Record(action);
	}
	
	internal void Record(Action action)
	{
		actions.Add(action);
	}

	internal void Undo()
	{
		foreach (var action in actions)
		{
			action?.Invoke();
		}
	}
}