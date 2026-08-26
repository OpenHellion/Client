using UnityEngine;

namespace RootMotion.FinalIK
{
	public abstract class RotationLimit : MonoBehaviour
	{
		public Vector3 axis = Vector3.forward;

		[HideInInspector] public Quaternion defaultLocalRotation;

		private bool initiated;

		private bool applicationQuit;

		public Vector3 secondaryAxis
		{
			get { return new Vector3(axis.y, axis.z, axis.x); }
		}

		public Vector3 crossAxis
		{
			get { return Vector3.Cross(axis, secondaryAxis); }
		}

		public void SetDefaultLocalRotation()
		{
			defaultLocalRotation = base.transform.localRotation;
		}

		public Quaternion GetLimitedLocalRotation(Quaternion localRotation, out bool changed)
		{
			if (!initiated)
			{
				Awake();
			}

			Quaternion quaternion = Quaternion.Inverse(defaultLocalRotation) * localRotation;
			Quaternion quaternion2 = LimitRotation(quaternion);
			changed = quaternion2 != quaternion;
			if (!changed)
			{
				return localRotation;
			}

			return defaultLocalRotation * quaternion2;
		}

		public bool Apply()
		{
			bool changed = false;
			base.transform.localRotation = GetLimitedLocalRotation(base.transform.localRotation, out changed);
			return changed;
		}

		public void Disable()
		{
			if (initiated)
			{
				base.enabled = false;
				return;
			}

			Awake();
			base.enabled = false;
		}

		protected abstract Quaternion LimitRotation(Quaternion rotation);

		private void Awake()
		{
			SetDefaultLocalRotation();
			if (axis == Vector3.zero)
			{
				Debug.LogError("Axis is Vector3.zero.");
			}

			initiated = true;
		}

		private void LateUpdate()
		{
			Apply();
		}

		public void LogWarning(string message)
		{
			Warning.Log(message, base.transform);
		}

		protected static Quaternion Limit1DOF(Quaternion rotation, Vector3 axis)
		{
			return Quaternion.FromToRotation(rotation * axis, axis) * rotation;
		}

		protected static Quaternion LimitTwist(Quaternion rotation, Vector3 axis, Vector3 orthoAxis, float twistLimit)
		{
			twistLimit = Mathf.Clamp(twistLimit, 0f, 180f);
			if (twistLimit >= 180f)
			{
				return rotation;
			}

			Vector3 normal = rotation * axis;
			Vector3 tangent = orthoAxis;
			Vector3.OrthoNormalize(ref normal, ref tangent);
			Vector3 tangent2 = rotation * orthoAxis;
			Vector3.OrthoNormalize(ref normal, ref tangent2);
			Quaternion quaternion = Quaternion.FromToRotation(tangent2, tangent) * rotation;
			if (twistLimit <= 0f)
			{
				return quaternion;
			}

			return Quaternion.RotateTowards(quaternion, rotation, twistLimit);
		}

		protected static float GetOrthogonalAngle(Vector3 v1, Vector3 v2, Vector3 normal)
		{
			Vector3.OrthoNormalize(ref normal, ref v1);
			Vector3.OrthoNormalize(ref normal, ref v2);
			return Vector3.Angle(v1, v2);
		}
	}
}
