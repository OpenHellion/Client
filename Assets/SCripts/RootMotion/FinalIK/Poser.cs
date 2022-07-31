using UnityEngine;

namespace RootMotion.FinalIK
{
	public abstract class Poser : MonoBehaviour
	{
		public Transform poseRoot;

		[Range(0f, 1f)]
		public float weight = 1f;

		[Range(0f, 1f)]
		public float localRotationWeight = 1f;

		[Range(0f, 1f)]
		public float localPositionWeight;

		public bool fixTransforms = true;

		public abstract void AutoMapping();

		public abstract void StoreDefaultState();

		public abstract void FixTransforms();

		protected virtual void Start()
		{
			StoreDefaultState();
		}

		protected virtual void Update()
		{
			if (fixTransforms)
			{
				FixTransforms();
			}
		}
	}
}
