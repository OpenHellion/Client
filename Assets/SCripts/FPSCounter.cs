using UnityEngine;

public class FPSCounter : MonoBehaviour
{
	private float deltaTime;

	private void Update()
	{
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
	}

	private void OnGUI()
	{
		int width = Screen.width;
		int height = Screen.height;
		GUIStyle gUIStyle = new GUIStyle();
		Rect position = new Rect(0f, 0f, width, height * 2 / 100);
		gUIStyle.alignment = TextAnchor.UpperLeft;
		gUIStyle.fontSize = height * 2 / 100;
		gUIStyle.normal.textColor = new Color(0f, 0f, 0.5f, 1f);
		float num = deltaTime * 1000f;
		float num2 = 1f / deltaTime;
		string text = string.Format("{0:0.0} ms ({1:0.} fps)", num, num2);
		GUI.Label(position, text, gUIStyle);
	}
}
