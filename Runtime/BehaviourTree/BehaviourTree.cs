using System.Linq;
using UnityEngine;

namespace Framework.Graph.BT
{
	[CreateAssetMenu(menuName = "Framework/BehaviourTree/BehaviourTree")]
    public class BehaviourTree : GraphTree
    {
    	public BTNode root;

        private static BTNode GetInstanceVersion(BehaviourTree tree, BTNode original)
    	{
    		int index = original.preOrderIndex;
    		return tree.nodes[index] as BTNode;
    	}

        public static BehaviourTree Clone(BehaviourTree blueprint)
    	{
    		var clone = CreateInstance<BehaviourTree>();
    		clone.name = blueprint.name;
            
    		clone.SetNodes(blueprint.nodes.Select(Instantiate));
    
    		// Relink children and parents for the cloned nodes.
    		int maxCloneNodeCount = clone.nodes.Count;
    		for (int i = 0; i < maxCloneNodeCount; ++i)
    		{
    			BTNode nodeSource = blueprint.nodes[i] as BTNode;
    			BTNode copyNode = GetInstanceVersion(clone, nodeSource);
    
    			if (copyNode.NodeType == NodeType.Composite)
    			{
    				var copyComposite = copyNode as BTComposite;
    				copyComposite.SetChildren(
    					Enumerable.Range(0, nodeSource.ChildCount())
    						.Select(childIndex => GetInstanceVersion(clone, nodeSource.GetChildAt(childIndex) as BTNode))
    						.ToArray());
    			}
    
    			else if ((copyNode.NodeType == NodeType.Decorator || copyNode.NodeType == NodeType.Root) && nodeSource.ChildCount() == 1)
    			{
    				var copyDecorator = copyNode as BTDecorator;
    				copyDecorator.SetChild(GetInstanceVersion(clone, nodeSource.GetChildAt(0) as BTNode));
    			}
    		}
    		
    		clone.root = clone.nodes[0] as BTNode;
    
    		return clone;
    	}
    }
}

