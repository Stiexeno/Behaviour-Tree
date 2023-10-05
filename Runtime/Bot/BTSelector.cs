namespace Framework.Bot
{
	public class BTSelector : BTComposite
	{
		protected override BTStatus OnUpdate()
		{
			BTStatus currentStatus;

			if (GetCurrentChild() < children.Length)
			{
				var child = children[GetCurrentChild()];
				currentStatus = child.RunUpdate();

				if (currentStatus == BTStatus.Success)
					return BTStatus.Success;

				if (currentStatus == BTStatus.Failure)
				{
					SetCurrentChild(GetCurrentChild() + 1);
					return BTStatus.Running;
				}
			}
			else
			{
				return BTStatus.Failure;
			}

			return currentStatus;
		}
	}
}