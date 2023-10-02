using UnityEngine;

namespace Framework.Bot.Editor
{
	public class ParallelGraphNode : CompositeGraphNode
	{
		public override string Header => "Parallel";
		public override Texture2D Icon => BehaviourTreePreferences.Instance.parallelcon;
	}
}
