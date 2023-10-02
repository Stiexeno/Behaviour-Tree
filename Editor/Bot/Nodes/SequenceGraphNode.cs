using UnityEngine;

namespace Framework.Bot.Editor
{
	public class SequenceGraphNode : CompositeGraphNode
	{
		private BTSequence btSequence;

		public override string Header => "Sequencer";
		public override Texture2D Icon => BehaviourTreePreferences.Instance.sequencerIcon;
	}
}
