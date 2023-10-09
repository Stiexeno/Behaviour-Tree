using UnityEngine;

namespace Framework.Bot.Editor
{
	public class BehaviourTreePreferences : ScriptableObject
	{
		public Texture2D sequencerIcon;
		public Texture2D selectorIcon;
		public Texture2D waitIcon;
		public Texture2D parallelcon;

		public Texture2D closeIcon;
		public Texture2D dotIcon;

		public Texture2D saveIcon;
		public Texture2D settingsIcon;
		public Texture2D loadIcon;
		public Texture2D formatIcon;
		public Texture2D createIcon;
		
		// Private fields
		
		private static BehaviourTreePreferences instance;

		public static BehaviourTreePreferences Instance
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

		private static BehaviourTreePreferences LoadDefaultPreferences()
		{
			var prefs = Resources.Load<BehaviourTreePreferences>("BehaviourTreePreferences");

			if (prefs == null)
			{
				Debug.LogWarning("Failed to load BehaviourTreePreferences");
				prefs = CreateInstance<BehaviourTreePreferences>();
			}

			return prefs;
		}
	}
}