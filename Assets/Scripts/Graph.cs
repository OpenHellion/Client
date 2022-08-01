using UnityEngine;

public class Graph : MonoBehaviour
{
	public GameObject ImagePixPref;

	private void Start()
	{
		DrawLine(0f, 0f);
	}

	private void Update()
	{
	}

	public void DrawLine(float position, float value)
	{
		GameObject gameObject = Object.Instantiate(ImagePixPref, new Vector3(0f, 0f, 0f), Quaternion.identity);
		gameObject.transform.SetParent(base.transform);
	}
}
