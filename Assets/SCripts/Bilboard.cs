using UnityEngine;
using ZeroGravity;

public class Bilboard : MonoBehaviour
{
	private void Update()
	{
		if (Client.IsGameBuild)
		{
			base.transform.LookAt(Client.Instance.SunCameraTransform.transform.position, Client.Instance.SunCameraTransform.transform.up);
		}
	}
}
