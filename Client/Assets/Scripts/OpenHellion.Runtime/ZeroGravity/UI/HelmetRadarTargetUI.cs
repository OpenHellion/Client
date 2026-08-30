using UnityEngine;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class HelmetRadarTargetUI : MonoBehaviour
	{
		public ArtificialBody TargetAB;

		public float drawDistance = 100f;

		private void Start()
		{
		}

		private void Update()
		{
		}

		private void GetPositionOfTargetUI()
		{
			Vector3 forward = TargetAB.transform.position -
			                  MyPlayer.Instance.FpsController.MainCamera.transform.position;
			float num = forward.magnitude - (float)TargetAB.Radius;
			float num2 = 1f;
			Vector3 vector;
			if (num > drawDistance)
			{
				vector = forward.normalized * drawDistance;
			}
			else
			{
				vector = forward.normalized * (forward.magnitude - (float)TargetAB.Radius);
				num2 = Mathf.Clamp01(num / drawDistance);
			}

			transform.localScale = new Vector3(num2, num2, num2);
			transform.position = MyPlayer.Instance.FpsController.MainCamera.transform.position + vector;
			transform.rotation =
				Quaternion.LookRotation(forward, MyPlayer.Instance.FpsController.MainCamera.transform.up);
		}
	}
}
