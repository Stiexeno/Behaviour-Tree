using System;
using System.Linq;
using Framework.GraphView;
using Framework.GraphView.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.VersionControl;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.Bot.Editor
{
	public class BehaviourTreeWindow : GraphWindow
	{
		// Private fields

		private static BehaviourTreeWindow window;

		//BehaviourTreeWindow

		internal override GraphTree Tree { get; set; }
		protected override IGraphNodeRules Rules { get; } = new BehaviourTreeGraphNodeRules();

		[MenuItem("Framework/Bot/Open Editor")]
		private static void Init()
		{
			window = GetWindow<BehaviourTreeWindow>();
			window.titleContent = new GUIContent("Bot Editor");
			window.Show();
		}

		protected override void OnEnable()
		{
			if (Tree != null)
			{
				Validate(Tree as BehaviourTree);
			}

			base.OnEnable();

			graphElements.Add(new BehaviourTreeInspector(this));
			PopulateSearch();
		}

		protected override void Initialize(GraphTree behaviour)
		{
			var behaviourTree = behaviour as BehaviourTree;

			if (behaviourTree == null)
				return;

			if (behaviourTree.root == null)
			{
				var node = Editor.Canvas.CreateNode(typeof(BTRoot));
				behaviourTree.root = Editor.Canvas.Nodes[0].Behaviour as BTNode;

				Editor.Canvas.SetRoot(node);
			}

			if (Application.isPlaying == false)
			{
				BTLocalPreferences.Instance.SaveGraph(AssetDatabase.GetAssetPath(behaviourTree));
			}
		}

		private void PopulateSearch()
		{
			var menu = Editor.Search.Menu;
			menu.AddHeader("Behaviour Tree");
			menu.AddItem("Sequencer", () => RequestCreateNode(typeof(BTSequence)));
			menu.AddItem("Selector", () => RequestCreateNode(typeof(BTSelector)));
			menu.AddItem("Parallel", () => RequestCreateNode(typeof(BTParallel)));
			menu.AddItem("Wait", () => RequestCreateNode(typeof(BTWait)));

			menu.AddHeader("Decorators");

			foreach (var behaviour in GraphEditor.Behaviours)
			{
				var nodeType = behaviour.Key;

				if (behaviour.Key == typeof(BTRoot))
					continue;

				if (nodeType.IsSubclassOf(typeof(BTDecorator)))
				{
					menu.AddItem($"{behaviour.Key.Name}", () => RequestCreateNode(nodeType));
				}
			}

			menu.AddHeader("Leaf");

			foreach (var behaviour in GraphEditor.Behaviours)
			{
				var nodeType = behaviour.Key;

				if (nodeType.IsSubclassOf(typeof(BTLeaf)))
				{
					if (behaviour.Key == typeof(BTWait) ||
					    behaviour.Key == typeof(BTLog))
						continue;

					menu.AddItem($"{behaviour.Key.Name}", () => RequestCreateNode(nodeType));
				}
			}
		}

		private void RequestCreateNode(Type nodeType)
		{
			Editor.CreateNodeFromType(nodeType);
			Editor.Search.Close();
		}

		private static void Validate(BehaviourTree tree)
		{
			for (int i = tree.nodes.Count - 1; i >= 0; i--)
			{
				if (tree.nodes[i] == null)
				{
					tree.nodes.RemoveAt(i);
					continue;
				}

				for (int j = tree.nodes[i].ChildCount() - 1; j >= 0; j--)
				{
					var node = tree.nodes[i];

					if (node is BTComposite composite)
					{
						if (composite.children == null)
							continue;

						if (composite.children[j] == null)
						{
							var children = composite.children.ToList();
							children.RemoveAt(j);
							composite.SetChildren(children.ToArray());
						}

						continue;
					}

					if (node is BTDecorator decorator)
					{
						if (decorator.child == null)
						{
							decorator.SetChild(null);
						}
					}
				}
			}
		}

		[OnOpenAsset]
		public static bool OpenAsset(int instanceId, int line)
		{
			var behaviourTree = EditorUtility.InstanceIDToObject(instanceId) as BehaviourTree;

			if (behaviourTree == null)
				return false;

			Validate(behaviourTree);

			BehaviourTreeWindow behaviourWindow = Open<BehaviourTreeWindow>(behaviourTree);
			behaviourWindow.titleContent = new GUIContent("Bot Editor");

			if (behaviourWindow != null)
			{
				behaviourWindow.SwitchToRuntimeMode();
				return true;
			}

			return false;
		}
	}
}