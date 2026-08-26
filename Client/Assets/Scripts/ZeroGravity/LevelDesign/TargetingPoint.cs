using UnityEngine;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	[RequireComponent(typeof(SphereCollider))]
	public class TargetingPoint : MonoBehaviour
	{
		public SpaceObject MainObject;

		public int Priority = 1;

		public bool Targetable;

		private void Start()
		{
			Targetable = true;
		}
	}
}
