using System;
using System.Collections.Generic;

namespace Framework.GraphView.Editor
{
	public interface IGraphNodeRules
	{
		Dictionary<Type, NodeProperties> FetchGraphBehaviours();
	}
}
