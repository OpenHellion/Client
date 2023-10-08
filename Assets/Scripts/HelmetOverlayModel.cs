using UnityEngine;
using UnityEngine.UI;

public class HelmetOverlayModel : MonoBehaviour
{
	public Animator Animator;

	public float xAxis;

	public float yAxis;

	public float zAxis;

	public float forwardAxis;

	public float rightAxis;

	public float LerpSpeed = 1f;

	public Image HelmetImage;

	public Image VisorImage;

	public HelmetOverlayObject CurrentHelmetOverlay;

	public void SetAxis(float x, float y, float z)
	{
		if (base.gameObject.activeInHierarchy)
		{
			xAxis = Mathf.Lerp(xAxis, x, LerpSpeed * Time.deltaTime);
			yAxis = Mathf.Lerp(yAxis, y, LerpSpeed * Time.deltaTime);
			zAxis = Mathf.Lerp(zAxis, z, LerpSpeed * Time.deltaTime);
			Animator.SetFloat("XAxis", Mathf.Clamp(xAxis, -10f, 10f));
			Animator.SetFloat("YAxis", Mathf.Clamp(0f - yAxis, -10f, 10f));
			Animator.SetFloat("ZAxis", Mathf.Clamp(zAxis, -10f, 10f));
		}
	}

	public void SetMovement(float forwardVelocity, float rightVelocity)
	{
		if (base.gameObject.activeInHierarchy)
		{
			forwardAxis = Mathf.Lerp(forwardAxis, forwardVelocity, LerpSpeed * Time.deltaTime);
			rightAxis = Mathf.Lerp(rightAxis, rightVelocity, LerpSpeed * Time.deltaTime);
			Animator.SetFloat("ForwardVelocity", forwardAxis);
			Animator.SetFloat("RightVelocity", rightAxis);
		}
	}

	public void UpdateHelmetOverlay(HelmetOverlayObject helmetOverlay)
	{
		if (helmetOverlay != CurrentHelmetOverlay)
		{
			CurrentHelmetOverlay = helmetOverlay;
			if (helmetOverlay.HelmetOverlay != null)
			{
				HelmetImage.enabled = true;
				HelmetImage.sprite = helmetOverlay.HelmetOverlay;
			}
			else
			{
				HelmetImage.enabled = false;
			}

			if (helmetOverlay.HelmetVisor != null)
			{
				VisorImage.enabled = true;
				VisorImage.sprite = helmetOverlay.HelmetVisor;
			}
			else
			{
				VisorImage.enabled = false;
			}
		}
	}
}
