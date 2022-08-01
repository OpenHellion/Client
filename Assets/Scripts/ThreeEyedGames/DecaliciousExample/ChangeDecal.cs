using System.Collections;
using UnityEngine;

namespace ThreeEyedGames.DecaliciousExample
{
	public class ChangeDecal : IInteract
	{
		public Decalicious Target;

		public float LerpSeconds;

		public override void Interact()
		{
			StartCoroutine(SetDecalColor(GetComponent<MeshRenderer>().material.color));
		}

		private IEnumerator SetDecalColor(Color targetColor)
		{
			Material material = Target.Material;
			Color from = material.color;
			for (float t = 0f; t <= 1f; t += Time.deltaTime / LerpSeconds)
			{
				material.color = Color.Lerp(from, targetColor, t);
				yield return null;
			}
		}
	}
}
