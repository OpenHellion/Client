using UnityEngine;

namespace RootMotion.Demos
{
	public abstract class CharacterAnimationBase : MonoBehaviour
	{
		public bool smoothFollow = true;

		public float smoothFollowSpeed = 20f;

		private Vector3 lastPosition;

		private Vector3 localPosition;

		private Quaternion localRotation;

		private Quaternion lastRotation;

		public virtual bool animationGrounded
		{
			get
			{
				return true;
			}
		}

		public virtual Vector3 GetPivotPoint()
		{
			return base.transform.position;
		}

		public float GetAngleFromForward(Vector3 worldDirection)
		{
			Vector3 vector = base.transform.InverseTransformDirection(worldDirection);
			return Mathf.Atan2(vector.x, vector.z) * 57.29578f;
		}

		protected virtual void Start()
		{
			if (base.transform.parent.GetComponent<CharacterBase>() == null)
			{
				Debug.LogWarning("Animation controllers should be parented to character controllers!", base.transform);
			}
			lastPosition = base.transform.position;
			localPosition = base.transform.parent.InverseTransformPoint(base.transform.position);
			lastRotation = base.transform.rotation;
			localRotation = Quaternion.Inverse(base.transform.parent.rotation) * base.transform.rotation;
		}

		protected virtual void LateUpdate()
		{
			if (smoothFollow)
			{
				base.transform.position = Vector3.Lerp(lastPosition, base.transform.parent.TransformPoint(localPosition), Time.deltaTime * smoothFollowSpeed);
				base.transform.rotation = Quaternion.Lerp(lastRotation, base.transform.parent.rotation * localRotation, Time.deltaTime * smoothFollowSpeed);
			}
			lastPosition = base.transform.position;
			lastRotation = base.transform.rotation;
		}
	}
}
