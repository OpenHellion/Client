using System;
using UnityEngine;

namespace ZeroGravity.Effects
{
	[ExecuteInEditMode]
	public class SunFlarePostEffect : MonoBehaviour
	{
		[SerializeField] private Material material;

		[SerializeField] private Vector3 sunPosition;

		[SerializeField] private float lerpStrengthNormal = 10f;

		[SerializeField] private float lerpStrengthPlanet = 0.4f;

		[SerializeField] private float strengthNormal = 1f;

		[SerializeField] private float strengthPlanet = 1f;

		private bool isVisibleNormal;

		private bool isVisiblePlanet;

		[SerializeField] private float fovValue;

		public void SetSunFlare(Vector3 sunScreenPosition, float fovValue, bool isSunVisibleNormal,
			bool isSunVisiblePlanet, bool forceVisibility)
		{
			sunPosition = sunScreenPosition;
			isVisibleNormal = isSunVisibleNormal;
			isVisiblePlanet = isSunVisiblePlanet;
			this.fovValue = fovValue;
			if (forceVisibility)
			{
				strengthNormal = (isSunVisibleNormal ? 1 : 0);
				strengthPlanet = (isSunVisiblePlanet ? 1 : 0);
			}
		}

		private void Update()
		{
			if (isVisibleNormal && strengthNormal < 1f)
			{
				strengthNormal = Mathf.Clamp01(strengthNormal + Time.deltaTime * lerpStrengthNormal);
			}
			else if (!isVisibleNormal && strengthNormal > 0f)
			{
				strengthNormal = Mathf.Clamp01(strengthNormal - Time.deltaTime * lerpStrengthNormal);
			}

			if (isVisiblePlanet && strengthPlanet < 1f)
			{
				strengthPlanet = Mathf.Clamp01(strengthPlanet + Time.deltaTime * lerpStrengthPlanet);
			}
			else if (!isVisiblePlanet && strengthPlanet > 0f)
			{
				strengthPlanet = Mathf.Clamp01(strengthPlanet - Time.deltaTime * lerpStrengthPlanet);
			}
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (!(material == null))
			{
				material.SetFloat("_sunX", sunPosition.x);
				material.SetFloat("_sunY", sunPosition.y);
				material.SetFloat("_sunFlareStrength", System.Math.Min(strengthNormal, strengthPlanet));
				material.SetFloat("_Fov", fovValue);
				Graphics.Blit(source, destination, material);
			}
		}
	}
}
