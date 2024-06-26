namespace Framework.GraphView.Editor
{
	public static class GraphConnection
	{
		public static GraphNode StartConnection(GraphNode child)
		{
			GraphNode parent = child.Parent;
			child.SetParent(null);

			return parent;
		}

		public static void FinishConnection(GraphCanvas canvas, GraphNode parent, GraphNode child)
		{
			canvas.AddChild(parent, child);
		}
	}
}