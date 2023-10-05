using Entitas;
using Entitas.VisualDebugging.Unity.Editor;
using Framework.Bot;

namespace Plugins.Behaviour_Tree.Runtime.EntitasSupport
{
	public abstract class ECSLeaf : BTLeaf, IECSNode
	{
		// Private fields
		protected IEntity nodeEntity;
		protected IEntity ownerEntity;
		protected IContext gameContext;

		protected ECSParams ecsParams;

		// Properties

		//ECSLeaf

		public void Initialize(ECSParams ecsParams)
		{
			this.ecsParams = ecsParams;
			
			ownerEntity = ecsParams.entity;
			gameContext = ecsParams.contexts.game;

			nodeEntity = gameContext.CreateEntity();

			nodeEntity.AddOwnerId(ownerEntity.Id);
			
			OnInit();
		}

		protected virtual void OnInit() { }


		/// <summary>
		/// Called by Framework. Don't call it manually.
		/// </summary>
		protected sealed override void OnEnter()
		{
			nodeEntity.ReplaceNodeStatus(BTStatus.Running);
			
			Enter();
		}

		/// <summary>
		/// Called by Framework. Don't call it manually.
		/// </summary>
		/// <returns>Node status</returns>
		protected sealed override BTStatus OnUpdate()
		{
			GameEntity entity = nodeEntity;

			if (!entity.hasNodeStatus)
				return BTStatus.Running;

			BTStatus updateResultStatus = Update();
			
			if (updateResultStatus == BTStatus.Success)
				nodeEntity.ReplaceNodeStatus(BTStatus.Inactive);
			
			return updateResultStatus;
		}


		protected virtual void Enter() { }
		protected abstract BTStatus Update();
	}
}