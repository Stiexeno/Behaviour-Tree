namespace Framework.Bot
{
	public class BTSequence : BTComposite
	{
		protected override BTStatus OnUpdate()
		{
			var currentStatus = BTStatus.Success;

			if (GetCurrentChild() < children.Length)
			{
				var child = children[GetCurrentChild()];
				currentStatus = child.RunUpdate();

				if (currentStatus == BTStatus.Failure)
					return BTStatus.Failure;

				if (currentStatus == BTStatus.Success)
				{
					SetCurrentChild(GetCurrentChild() + 1);
					return BTStatus.Running;
				}
			}

			return currentStatus;
		}
	}
}