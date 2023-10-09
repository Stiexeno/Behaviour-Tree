using System;
using System.Linq;
using Framework.Bot.Editor;
using Framework.Bot;
using UnityEditor;
using UnityEngine;

namespace Framework.GraphView.Editor
{
	public abstract class GraphWindow : GUIWindow
	{
		// Private fields

		// Properties

		public GraphEditor Editor { get; private set; }
		private GraphSaver Saver { get; set; }
		internal GraphSearch Search { get; set; }
		internal GraphViewer Viewer { get; private set; }

		internal abstract GraphTree Tree { get; set; }
		protected abstract IGraphNodeRules Rules { get; }
		public virtual GraphSearchMenu Menu { get; } = new GraphSearchMenu();

		private CanvasTransform CanvasTransform =>
			new CanvasTransform
			{
				pan = Viewer.panOffset,
				zoom = Viewer.ZoomScale,
				size = position.size
			};

		private GraphSaver.TreeMetaData TreeMetaData =>
			new()
			{
				zoom = Viewer.zoom,
				pan = Viewer.panOffset
			};

		// GraphEditor

		/// <summary>
		/// Open file browser to load a graph.
		/// </summary>
		internal void Load()
		{
			QuickSave();

			var tree = Saver.LoadGraphTree();

			if (tree)
			{
				SetTree(tree);
			}
		}

		/// <summary>
		/// Create a new GraphTree of given type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		internal void CreateNew<T>() where T : GraphTree
		{
			QuickSave();
			SetTree(Saver.CreateNewGraphTree<T>());
			ShowNotification(new GUIContent("New Tree Created"));
		}

		/// <summary>
		/// Set the GraphTree to view.
		/// </summary>
		/// <param name="graphTree"></param>
		internal void SetTree(GraphTree graphTree)
		{
			QuickSave();

			Tree = graphTree;
			BuildCanvas();
			Initialize(graphTree);
		}

		/// <summary>
		/// Format the tree to be nicely positioned.
		/// </summary>
		internal void FormatTree()
		{
			if (Tree && Editor.Canvas != null)
			{
				if (Editor.Canvas.Root == null)
				{
					ShowNotification(new GUIContent("Set a root to nicely format the tree!"));
				}
				else
				{
					GraphFormatter.Format(Editor.Canvas.Root, Vector2.zero);
				}
			}
		}

		/// <summary>
		/// Save the current GraphTree.
		/// </summary>
		internal void QuickSave()
		{
			if (Saver.CanSaveTree(Tree))
			{
				Saver.SaveCanvas(Editor.Canvas, TreeMetaData);
			}
		}

		/// <summary>
		/// Initialize overriden window with given GraphTree.
		/// </summary>
		/// <param name="root"></param>
		protected virtual void Initialize(GraphTree root)
		{
		}

		/// <summary>
		/// Open the search window.
		/// </summary>
		public virtual void OpenSearch(GraphSearchMenu menu = null, Action onClose = null, Vector2 overrideSize = default)
		{
		}

		protected static T Open<T>(GraphTree behaviour) where T : GraphWindow
		{
			if (behaviour == null)
				return null;
            
			var windows = Resources.FindObjectsOfTypeAll<T>();

			T window = windows.FirstOrDefault();

			if (window == null)
			{
				window = CreateInstance<T>();
				window.Show();
			}

			window.SetTree(behaviour);
			return window;
		}
		
		protected override void OnEnable()
		{
			GraphEditor.FetchGraphBehaviours(Rules);

			Saver = new GraphSaver();
			Editor = new GraphEditor();
			Viewer = new GraphViewer();
			Search = new GraphSearch(Editor);

			Editor.Viewer = Viewer;
			Editor.Search = Search;
			Editor.Window = this;
			Editor.CanvasTransform = CanvasTransform;

			Editor.OnCanvasChanged += Repaint;
			Editor.Input.SaveRequest += Save;
			Saver.SaveMessage += (sender, message) => ShowNotification(new GUIContent(message), 0.5f);
			Editor.Input.OnKeySpace += OpenSearch;
			Editor.Input.OnFormatTree += FormatTree;
			Editor.Input.OnSearchOpen += OpenSearch;

			EditorApplication.playModeStateChanged += PlayModeStateChanged;
			AssemblyReloadEvents.beforeAssemblyReload += BeforeAssemblyReload;
			Selection.selectionChanged += SelectionChanged;

			SwitchToRuntimeMode();
		}

		private void OnDestroy()
		{
			OnExit();
		}

		protected virtual void OnDisable()
		{
			EditorApplication.playModeStateChanged -= PlayModeStateChanged;
			AssemblyReloadEvents.beforeAssemblyReload -= BeforeAssemblyReload;
			Selection.selectionChanged -= SelectionChanged;
			Editor.Input.OnKeySpace -= OpenSearch;
		}

		private void OnExit()
		{
			Editor.NodeSelection.ClearSelection();
			QuickSave();
		}

		protected override void OnGUI()
		{
			if (Tree == null)
			{
				Viewer.DrawStaticGrid(position.size);
			}
			else
			{
				if (Editor.Canvas == null)
				{
					BuildCanvas();
				}

				Editor.PollInput(Event.current, CanvasTransform, CanvasInputRect);
				Editor.UpdateView();
				Viewer.Draw(CanvasTransform);
			}

			base.OnGUI();
			Repaint();
			
			Search.Draw();
		}

		protected void SwitchToRuntimeMode()
		{
			if (EditorApplication.isPlaying == false || Selection.activeGameObject == null)
				return;

			var btAgent = Selection.activeGameObject.GetComponent<BTAgent>();
			var btTree = btAgent ? btAgent.Tree : null;

			if (btTree && Tree != btTree)
			{
				var windows = Resources.FindObjectsOfTypeAll<BehaviourTreeWindow>();

				bool alreadyInView = windows.Any(w => w.Tree == btTree);

				if (alreadyInView)
				{
					return;
				}

				BehaviourTreeWindow window = windows.FirstOrDefault(w => !w.Tree);

				// Have the window without a set tree to view the tree selected.
				if (window)
				{
					window.SetTree(btTree);
				}
				else
				{
					// View tree in this window.
					SetTree(btTree);
				}
			}
		}

		private void OpenSearch(object sender, EventArgs e)
		{
			//Viewer.CustomOverlayDraw += DrawSearch;
			OpenSearch();
		}

		private void BuildCanvas()
		{
			if (Tree)
			{
				Editor.SetGraphTree(Tree);
				Repaint();
			}
		}

		private void Save(object sender, EventArgs eventArgs)
		{
			if (Editor.Canvas != null)
			{
				Saver.SaveCanvas(Editor.Canvas, TreeMetaData);
			}
		}

		private void SelectionChanged()
		{
			SwitchToRuntimeMode();
		}

		private void BeforeAssemblyReload()
		{
			if (!EditorApplication.isPlayingOrWillChangePlaymode)
			{
				OnExit();
			}
		}

		private void PlayModeStateChanged(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.ExitingEditMode)
			{
				QuickSave();
			}

			if (state == PlayModeStateChange.EnteredPlayMode)
			{
				SwitchToRuntimeMode();
			}
		}

		private void FormatTree(object sender, EventArgs e) => FormatTree();
	}
}