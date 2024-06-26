using UnityEditor;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace Framework.GraphView.Editor
{
	public class GraphPreferences : ScriptableObject
	{
		public Texture2D gridTexture;
		public Texture2D defaultNodeBackground;
		public Texture2D defaultNodeGadient;
		public Texture2D edgeArrow;
		
		private static GraphPreferences instance;
		
		public float zoomDelta;
		public float minZoom;
		public float maxZoom;
		public float panSpeed;
		
		public float iconSize;
		public float portHeight;
		public Vector2 nodeSizePadding;
		public float nodeWidthPadding;
		public float snapThreshold;

		public static GraphPreferences Instance
		{
			get
			{
				if (instance == null)
				{
					instance = LoadDefaultPreferences();
				}
				return instance;
			}

			set
			{
				instance = value;
			}
		}

		private static GraphPreferences LoadDefaultPreferences()
		{
			var prefs = Resources.Load<GraphPreferences>("GraphPreferences");

			if (prefs == null)
			{
				Debug.LogWarning("Failed to load GraphPreferences");
				prefs = CreateInstance<GraphPreferences>();
			}

			return prefs;
		}
	}
}