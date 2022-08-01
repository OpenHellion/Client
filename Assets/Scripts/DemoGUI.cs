using UnityEngine;

public class DemoGUI : MonoBehaviour
{
	public GameObject[] Prefabs;

	private int currentNomber;

	private GameObject currentInstance;

	private GUIStyle guiStyleHeader = new GUIStyle();

	private void Start()
	{
		guiStyleHeader.fontSize = 15;
		guiStyleHeader.normal.textColor = new Color(0.15f, 0.15f, 0.15f);
		currentInstance = Object.Instantiate(Prefabs[currentNomber], base.transform.position, default(Quaternion));
		DemoReactivator demoReactivator = currentInstance.AddComponent<DemoReactivator>();
		demoReactivator.TimeDelayToReactivate = 1.5f;
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(10f, 15f, 105f, 30f), "Previous Effect"))
		{
			ChangeCurrent(-1);
		}
		if (GUI.Button(new Rect(130f, 15f, 105f, 30f), "Next Effect"))
		{
			ChangeCurrent(1);
		}
		GUI.Label(new Rect(300f, 15f, 100f, 20f), "Prefab name is \"" + Prefabs[currentNomber].name + "\"  \r\nHold any mouse button that would move the camera", guiStyleHeader);
	}

	private void ChangeCurrent(int delta)
	{
		currentNomber += delta;
		if (currentNomber > Prefabs.Length - 1)
		{
			currentNomber = 0;
		}
		else if (currentNomber < 0)
		{
			currentNomber = Prefabs.Length - 1;
		}
		if (currentInstance != null)
		{
			Object.Destroy(currentInstance);
		}
		currentInstance = Object.Instantiate(Prefabs[currentNomber], base.transform.position, default(Quaternion));
		DemoReactivator demoReactivator = currentInstance.AddComponent<DemoReactivator>();
		demoReactivator.TimeDelayToReactivate = 1.5f;
	}
}
