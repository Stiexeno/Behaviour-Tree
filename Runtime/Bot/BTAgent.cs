using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace Framework.Bot
{
	public class BTAgent : MonoBehaviour
    {
	    // Serialized fields
	    
    	[SF] private BehaviourTree behaviourTree;
    
	    // Private fields
	    
    	protected BehaviourTree treeInstance;
    	
	    // Properties
	    
    	public BehaviourTree Tree => treeInstance;
    
    	private void Awake()
    	{
    		treeInstance = BehaviourTree.Clone(behaviourTree);
    		Initialize();
    	}
    
    	protected virtual void Initialize()
    	{
    		foreach (var graphBehaviour in treeInstance.nodes)
    		{
    			var node = (BTNode)graphBehaviour;
    			node.Init(this, null);
    		}
    	}
    
    	private void Update()
    	{
    		var rootNode = treeInstance.root;
    		UpdateSubtree(rootNode);
    	}
    
    	private void UpdateSubtree(BTNode node)
    	{
    		var result = node.RunUpdate();
    		
    		#if UNITY_EDITOR
    		node.EditorStatus = (BTNode.BTEditorStatus) result;
    		#endif
    		
    		if (result == BTStatus.Success || result == BTStatus.Failure)
    		{
    			node.OnReset();
    		}
    	}
    }
}
