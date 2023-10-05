using Entitas;
using Framework.Bot;

namespace Plugins.Behaviour_Tree.Runtime.EntitasSupport
{
    public class EcsAgent : BTAgent
    {
        private ECSParams _ecsParams;
        private IContexts _contexts;

        public void Setup(IEntity entity, Contexts contexts)
        {
            _ecsParams.entity = entity;
            _ecsParams.contexts = contexts;
		
            foreach (var graphBehaviour in treeInstance.nodes)
            {
                if (graphBehaviour is IECSNode ecsNode)
                {
                    ecsNode.Initialize(_ecsParams);
                }
            }
        }
    }
}
