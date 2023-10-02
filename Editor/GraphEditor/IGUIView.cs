using UnityEditor;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace Framework.GraphView.Editor
{
    public interface IGUIView : IGUIElement
    {
        public void OnGUI(EditorWindow window, Rect rect);
    }
}