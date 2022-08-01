using System.Collections.Generic;
using UnityEngine;

namespace ZeroGravity.Objects
{
	public class DamagePointData
	{
		public float VisibilityThreshold;

		public bool UseOcclusion;

		public Transform ParentTransform;

		public Vector3 Position;

		public Quaternion Rotation;

		public Vector3 Scale;

		public List<GameObject> Effects = new List<GameObject>();
	}
}
