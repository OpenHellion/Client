using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class Constraints
	{
		public Transform transform;

		public Vector3 positionOffset;

		public Vector3 position;

		[Range(0f, 1f)] public float positionWeight;

		public Vector3 rotationOffset;

		public Vector3 rotation;

		[Range(0f, 1f)] public float rotationWeight;

		public bool IsValid()
		{
			return transform != null;
		}

		public void Initiate(Transform transform)
		{
			this.transform = transform;
			position = transform.position;
			rotation = transform.eulerAngles;
		}

		public void Update()
		{
			if (IsValid())
			{
				transform.position += positionOffset;
				if (positionWeight > 0f)
				{
					transform.position = Vector3.Lerp(transform.position, position, positionWeight);
				}

				transform.rotation = Quaternion.Euler(rotationOffset) * transform.rotation;
				if (rotationWeight > 0f)
				{
					transform.rotation =
						Quaternion.Slerp(transform.rotation, Quaternion.Euler(rotation), rotationWeight);
				}
			}
		}
	}
}
