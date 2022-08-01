using UnityEngine;

namespace RootMotion
{
	public class DemoGUIMessage : MonoBehaviour
	{
		public string text;

		public Color color = Color.white;

		private void OnGUI()
		{
			GUI.color = color;
			GUILayout.Label(text);
			GUI.color = Color.white;
		}
	}
}
