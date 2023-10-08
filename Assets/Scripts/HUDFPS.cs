using UnityEngine;

[AddComponentMenu("Utilities/HUDFPS")]
public class HUDFPS : MonoBehaviour
{
	public Rect startRect = new Rect(10f, 10f, 75f, 50f);

	public bool updateColor = true;

	public bool allowDrag = true;

	public float frequency = 0.5f;

	public int nbDecimal = 1;

	private float accum;

	private int frames;

	private Color color = Color.white;

	private string sFPS = string.Empty;

	private GUIStyle style;

	private int frameCountere;

	private int frameInterval = 10;

	private float suma;

	private float fps;

	private void Start()
	{
	}

	private void Update()
	{
		if (frameCountere < frameInterval)
		{
			suma += Time.deltaTime;
			frameCountere++;
		}
		else
		{
			frameCountere = 0;
			fps = 1f / (suma / (float)frameInterval);
			suma = 0f;
		}
	}

	private void OnGUI()
	{
		if (style == null)
		{
			style = new GUIStyle(GUI.skin.label);
			style.normal.textColor = Color.white;
			style.alignment = TextAnchor.MiddleCenter;
		}

		GUI.color = ((!updateColor) ? Color.white : color);
		startRect = GUI.Window(0, startRect, DoMyWindow, string.Empty);
	}

	private void DoMyWindow(int windowID)
	{
		GUI.Label(new Rect(0f, 0f, startRect.width, startRect.height), fps + " FPS", style);
		if (allowDrag)
		{
			GUI.DragWindow(new Rect(0f, 0f, Screen.width, Screen.height));
		}
	}
}
