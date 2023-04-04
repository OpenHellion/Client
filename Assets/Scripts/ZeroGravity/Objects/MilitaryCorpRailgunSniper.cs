using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using ZeroGravity.Math;

namespace ZeroGravity.Objects
{
	public class MilitaryCorpRailgunSniper : Weapon
	{
		[Title("UI")]
		public GameObject zoomCamera;

		public GameObject rTexture;

		private int currentZoomStep;

		public int[] ZoomSteps;

		private float zoomingSpeed = 3f;

		private new void Start()
		{
			base.Start();
			UpdateUI();
		}

		public override void UpdateUI()
		{
			base.UpdateUI();
		}

		public override void ToggleZoomCamera(bool status)
		{
			zoomCamera.SetActive(status);
			rTexture.SetActive(status);
		}

		private void Update()
		{
			if (!base.IsSpecialStance || !CanZoom || !(MyPlayer.Instance.CurrentActiveItem == this))
			{
				return;
			}
			float axis = Mouse.current.scroll.y.ReadValue();
			if (Mathf.Abs(axis) > float.Epsilon)
			{
				int num = currentZoomStep + MathHelper.Sign(axis);
				if (num <= ZoomSteps.Length - 1 && num >= 0)
				{
					CanZoom = false;
					currentZoomStep = num;
					StartCoroutine(ZoomDelay(0.3f));
				}
			}
		}

		private IEnumerator ChangeZoomCameraFov(float fovVal)
		{
			Camera cam = zoomCamera.GetComponent<Camera>();
			float fromFov = cam.fieldOfView;
			float zoomLerpHelper = 0f;
			while (zoomLerpHelper <= 1f)
			{
				float newFovVal = (cam.fieldOfView = Mathf.Lerp(fromFov, fovVal, zoomLerpHelper));
				zoomLerpHelper += Time.deltaTime * zoomingSpeed;
				yield return new WaitForEndOfFrame();
			}
			yield return null;
		}

		private IEnumerator ZoomDelay(float vreme)
		{
			if (zoomCamera == null)
			{
				MyPlayer.Instance.ChangeCamerasFov(ZoomSteps[currentZoomStep]);
			}
			else
			{
				StartCoroutine(ChangeZoomCameraFov(ZoomSteps[currentZoomStep]));
			}
			yield return new WaitForSeconds(vreme);
			CanZoom = true;
		}
	}
}
