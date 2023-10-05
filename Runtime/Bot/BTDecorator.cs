using Framework.GraphView;

namespace Framework.Bot
{
	public abstract class BTDecorator : BTNode
	{
		public BTNode child;

		public bool invert;
		
		public override NodeType NodeType => NodeType.Decorator;
		public override int MaxChildCount => 1;

		protected override BTStatus OnUpdate()
		{
			var runResult = DryRun();
			runResult = invert ? !runResult : runResult;
			
			if (runResult)
			{
				if (child == null)
					return BTStatus.Success;

				var childStatus = child.RunUpdate();
				return childStatus;
			}

			return BTStatus.Failure;
		}

		protected new abstract bool DryRun();

		public override void OnReset()
		{
			base.OnReset();

			OnExit();

			if (child != null)
			{
				child.OnReset();
			}
		}

		public void SetChild(BTNode btNode)
		{
			child = btNode;
			if (child != null)
			{
				child.Parent = this;
				child.indexOrder = 0;
			}
		}

		public override GraphBehaviour GetChildAt(int index)
		{
			return child;
		}

		public override int ChildCount()
		{
			return child != null ? 1 : 0;
		}
	}
}