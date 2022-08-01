using System.Collections.Generic;
using UnityEngine;

namespace ZeroGravity.LevelDesign
{
	public class SceneDamagePoint : MonoBehaviour
	{
		[Tooltip("Max. visibility distance[m] from player")]
		public float VisibilityThreshold = 300f;

		public bool UseOcclusion = true;

		public List<GameObject> Effects;

		private void OnDrawGizmos()
		{
			Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.5f);
			Gizmos.DrawSphere(base.transform.position + base.transform.forward * 0.05f, 0.05f);
			Gizmos.DrawSphere(base.transform.position + base.transform.forward * 0.3f, 0.1f);
			Gizmos.DrawSphere(base.transform.position + base.transform.forward * 0.75f, 0.25f);
		}
	}
}
