using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;
using SF = UnityEngine.SerializeField;

namespace Framework.GraphView.Editor
{
	public class GraphSearch
	{
		private Rect rect;
		private Vector2 position;

		private string searchQuerry = "";

		private GraphEditor editor;
		private Action onClose;
		private GraphSearchMenu menu;
		private Vector2 size;

		// Properties

		public bool IsActive { get; set; }

		//public GraphSearchMenu Menu { get; } = new GraphSearchMenu();

		public GraphSearch(GraphEditor editor)
		{
			this.editor = editor;
			editor.Input.MouseDown += OnMouseDown;
		}

		private void OnMouseDown(object sender, GraphInputEvent e)
		{
			if (IsActive && rect.Contains(Event.current.mousePosition) == false)
			{
				Close();
			}
		}
		
		public void Open(GraphSearchMenu menu, Vector2 mousePosition, Rect rect, Vector2 size, Action onClose = null)
		{
			this.size = size;
			if (size == default)
			{
				this.size = new Vector2(400, 420);
			}
			
			this.menu = menu;
			this.onClose = onClose;
			var clampedRect = new Rect(mousePosition, this.size);
			clampedRect = clampedRect.ClampToRect(rect, 5);
			this.position = new Vector2(clampedRect.x, clampedRect.y);

			IsActive = true;
			searchQuerry = "";
		}

		public void Close()
		{
			IsActive = false;
			editor.ClearActions();
			onClose?.Invoke();
		}

		public void Draw()
		{
			if (IsActive == false)
				return;

			PollInput();

			rect = new Rect(position, size);
			EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f));
			GraphStyle.DrawBorderRect(rect, new Color(0.5f, 0.5f, 0.5f), 1f);

			var fieldRect = new Rect(rect.x + 5, rect.y + 5, rect.width - 10, 25);

			GUI.SetNextControlName("SearchField");
			searchQuerry = EditorGUI.TextField(fieldRect, searchQuerry);
			EditorGUI.FocusTextInControl("SearchField");

			DrawContent(rect);
		}

		private void PollInput()
		{
			if (GraphInput.IsExitAction(Event.current))
				Close();

			if (rect.Contains(Event.current.mousePosition))
				return;

			if (GraphInput.IsClickAction(Event.current))
				Close();
			
			if (GraphInput.IsContextAction(Event.current))
				Close();
		}

		private void DrawContent(Rect rect)
		{
			var buttonRect = new Rect(rect.x + 5, rect.y + 35, rect.width - 10, 15);

			for (var i = 0; i < menu.menuItems.Count; i++)
			{
				var menuItem = menu.menuItems[i];

				if (menuItem.isHeader)
				{
					EditorGUI.LabelField(buttonRect, menuItem.name, GraphStyle.SearchHeader);
				}
				else
				{
					if (GUI.Button(buttonRect, menuItem.name, GraphStyle.SearchItem))
					{
						menuItem.action?.Invoke();
					}
				}

				buttonRect.y += 15;
			}
		}
	}

	public class GraphSearchMenu
	{
		public readonly List<GraphMenuItem> menuItems = new List<GraphMenuItem>();

		public void AddItem(string name, Action action)
		{
			menuItems.Add(new GraphMenuItem
			{
				name = name,
				action = action,
				isHeader = false,
				isSeparator = false
			});
		}

		public void AddSeparator(string name)
		{
			menuItems.Add(new GraphMenuItem
			{
				name = name,
				action = null,
				isHeader = false,
				isSeparator = true
			});
		}

		public void AddHeader(string name)
		{
			menuItems.Add(new GraphMenuItem
			{
				name = name,
				action = null,
				isHeader = true,
				isSeparator = false
			});
		}

		public void Clear()
		{
			menuItems.Clear();
		}
		
		public class GraphMenuItem
		{
			public string name;
			public Action action;
			public bool isHeader;
			public bool isSeparator;
		}
	}
}