using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	public class Turret : MonoBehaviour
	{
		[Serializable]
		public class Part
		{
			public Transform transform;

			private RotationLimit rotationLimit;

			public void AimAt(Transform target)
			{
				transform.LookAt(target.position, transform.up);
				if (rotationLimit == null)
				{
					rotationLimit = transform.GetComponent<RotationLimit>();
					rotationLimit.Disable();
				}

				rotationLimit.Apply();
			}
		}

		public Transform target;

		public Part[] parts;

		private void Update()
		{
			Part[] array = parts;
			foreach (Part part in array)
			{
				part.AimAt(target);
			}
		}
	}
}
