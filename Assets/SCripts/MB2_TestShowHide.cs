using UnityEngine;

public class MB2_TestShowHide : MonoBehaviour
{
	public MB3_MeshBaker mb;

	public GameObject[] objs;

	private void Update()
	{
		if (Time.frameCount == 100)
		{
			mb.ShowHide(null, objs);
			mb.ApplyShowHide();
			Debug.Log("should have disappeared");
		}
		if (Time.frameCount == 200)
		{
			mb.ShowHide(objs, null);
			mb.ApplyShowHide();
			Debug.Log("should show");
		}
	}
}
