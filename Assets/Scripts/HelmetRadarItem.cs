using UnityEngine;
using ZeroGravity.Math;
using ZeroGravity.Objects;

public class HelmetRadarItem : MonoBehaviour
{
	public HelmetRadarIndicator Indicator_Visible;

	public HelmetRadarIndicator Indicator_Up;

	public HelmetRadarIndicator Indicator_Down;

	public HelmetRadarIndicator currIndicator;

	public Transform RadarTarget;

	private Transform playerCamera;

	private void Start()
	{
		playerCamera = MyPlayer.Instance.FpsController.MainCamera.transform;
	}

	private void Update()
	{
		if (RadarTarget != null)
		{
			Vector3 vec = Vector3.ProjectOnPlane(RadarTarget.position - playerCamera.position, playerCamera.up);
			float basedOnCurrent = MathHelper.AngleSigned(playerCamera.forward, vec, playerCamera.up);
			Vector3 vec2 = Vector3.ProjectOnPlane(RadarTarget.position - playerCamera.position, playerCamera.right);
			float num = MathHelper.AngleSigned(playerCamera.forward, vec2, playerCamera.right);
			if ((num < -8f && num > -172f) || (num > 8f && num < 172f))
			{
				if (MathHelper.Sign(num) < 1f)
				{
					currIndicator = Indicator_Up;
					Indicator_Visible.SetActive(false);
					Indicator_Up.SetActive(true);
					Indicator_Down.SetActive(false);
				}
				else
				{
					currIndicator = Indicator_Down;
					Indicator_Visible.SetActive(false);
					Indicator_Up.SetActive(false);
					Indicator_Down.SetActive(true);
				}
			}
			else
			{
				currIndicator = Indicator_Visible;
				Indicator_Visible.SetActive(true);
				Indicator_Up.SetActive(false);
				Indicator_Down.SetActive(false);
			}

			base.transform.localPosition = new Vector3(
				Mathf.Clamp(MathHelper.ProportionalValue(basedOnCurrent, -180f, 180f, -242f, 242f), -242f, 242f),
				Mathf.Clamp(Mathf.Abs(MathHelper.ProportionalValue(basedOnCurrent, -180f, 180f, -16.73f, 16.73f)) * -1f,
					-16.73f, 16.73f), base.transform.localPosition.z);
			base.transform.localRotation = Quaternion.Euler(new Vector3(base.transform.localRotation.x,
				base.transform.localRotation.y,
				-1f * Mathf.Clamp(MathHelper.ProportionalValue(basedOnCurrent, -180f, 180f, -6.842f, 6.842f), -6.842f,
					6.842f)));
		}
		else
		{
			Indicator_Visible.SetActive(false);
			Indicator_Up.SetActive(false);
			Indicator_Down.SetActive(false);
		}
	}
}
