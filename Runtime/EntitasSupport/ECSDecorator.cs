using Framework.Bot;

namespace Plugins.Behaviour_Tree.Runtime.EntitasSupport
{
	public abstract class ECSDecorator : BTDecorator, IECSNode
	{
		// Private fields

		private ECSParams ecsParams;
		protected GameEntity ownerEntity;
		protected GameContext gameContext;

		// Properties

		//ECSDecorator

		public void Initialize(ECSParams ecsParams)
		{
			this.ecsParams = ecsParams;
			
			ownerEntity = ecsParams.entity;
			gameContext = ecsParams.contexts.game;
		}
	}
}