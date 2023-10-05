using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Framework.Bot.Editor
{
	public class BTLocalPreferences : ScriptableObject
	{
		public Color rootColor = Color.black;
		public Color compositeColor = Color.green;
		public Color decoratorColor = new Color(1f, 0.56f, 0f);
		public Color waitColor = Color.black;
		public Color leafColor = Color.black;
        
		[HideInInspector] public List<string> savedGraphs = new List<string>();
		
		// Private fields
		
		private static BTLocalPreferences instance;

		public const string PATH = "Assets/Resources/Bot/BTLocalPreferences.asset";
		
		// Properties
		
		public static BTLocalPreferences Instance
		{
			get
			{
				if (instance == null)
				{
					instance = LoadDefaultPreferences();
				}

				return instance;
			}
		}

		public void SaveGraph(string path)
		{
			if (savedGraphs.Contains(path))
			{
				return;
			}

			savedGraphs.Add(path);
			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();
		}

		public List<string> GetSavedGraphs()
		{
			for (int i = savedGraphs.Count - 1; i >= 0; i--)
			{
				if (AssetDatabase.LoadAssetAtPath(savedGraphs[i], typeof(BehaviourTree)) == null)
				{
					savedGraphs.RemoveAt(i);
				}
			}

			return savedGraphs;
		}

		public void RemoveSavedGraph(string path)
		{
			savedGraphs.Remove(path);
		}

		private static BTLocalPreferences LoadDefaultPreferences()
		{
			var prefs = AssetDatabase.LoadAssetAtPath<BTLocalPreferences>("Assets/Resources/Bot/BTLocalPreferences.asset");

			if (prefs == null)
			{
				prefs = CreateInstance<BTLocalPreferences>();
				
				var path = "Assets/Resources/Bot/BTLocalPreferences.asset";

				if (AssetDatabase.IsValidFolder("Assets/Resources/Bot") == false)
				{
					AssetDatabase.CreateFolder("Assets/Resources", "Bot");
				}
				
				AssetDatabase.CreateAsset(prefs, path);
			}

			return prefs;
		}
	}
}