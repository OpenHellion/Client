using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class IKSolverTrigonometric : IKSolver
	{
		[Serializable]
		public class TrigonometricBone : Bone
		{
			public float sqrMag;

			private Quaternion targetToLocalSpace;

			private Vector3 defaultLocalBendNormal;

			public void Initiate(Vector3 childPosition, Vector3 bendNormal)
			{
				Quaternion rotation = Quaternion.LookRotation(childPosition - transform.position, bendNormal);
				targetToLocalSpace = QuaTools.RotationToLocalSpace(transform.rotation, rotation);
				defaultLocalBendNormal = Quaternion.Inverse(transform.rotation) * bendNormal;
			}

			public Quaternion GetRotation(Vector3 direction, Vector3 bendNormal)
			{
				return Quaternion.LookRotation(direction, bendNormal) * targetToLocalSpace;
			}

			public Vector3 GetBendNormalFromCurrentRotation()
			{
				return transform.rotation * defaultLocalBendNormal;
			}
		}

		public Transform target;

		[Range(0f, 1f)]
		public float IKRotationWeight = 1f;

		public Quaternion IKRotation;

		public Vector3 bendNormal = Vector3.right;

		public TrigonometricBone bone1 = new TrigonometricBone();

		public TrigonometricBone bone2 = new TrigonometricBone();

		public TrigonometricBone bone3 = new TrigonometricBone();

		protected Vector3 weightIKPosition;

		protected bool directHierarchy = true;

		public void SetBendGoalPosition(Vector3 goalPosition, float weight)
		{
			if (!base.initiated || weight <= 0f)
			{
				return;
			}
			Vector3 vector = Vector3.Cross(goalPosition - bone1.transform.position, IKPosition - bone1.transform.position);
			if (vector != Vector3.zero)
			{
				if (weight >= 1f)
				{
					bendNormal = vector;
				}
				else
				{
					bendNormal = Vector3.Lerp(bendNormal, vector, weight);
				}
			}
		}

		public void SetBendPlaneToCurrent()
		{
			if (base.initiated)
			{
				Vector3 vector = Vector3.Cross(bone2.transform.position - bone1.transform.position, bone3.transform.position - bone2.transform.position);
				if (vector != Vector3.zero)
				{
					bendNormal = vector;
				}
			}
		}

		public void SetIKRotation(Quaternion rotation)
		{
			IKRotation = rotation;
		}

		public void SetIKRotationWeight(float weight)
		{
			IKRotationWeight = Mathf.Clamp(weight, 0f, 1f);
		}

		public Quaternion GetIKRotation()
		{
			return IKRotation;
		}

		public float GetIKRotationWeight()
		{
			return IKRotationWeight;
		}

		public override Point[] GetPoints()
		{
			return new Point[3] { bone1, bone2, bone3 };
		}

		public override Point GetPoint(Transform transform)
		{
			if (bone1.transform == transform)
			{
				return bone1;
			}
			if (bone2.transform == transform)
			{
				return bone2;
			}
			if (bone3.transform == transform)
			{
				return bone3;
			}
			return null;
		}

		public override void StoreDefaultLocalState()
		{
			bone1.StoreDefaultLocalState();
			bone2.StoreDefaultLocalState();
			bone3.StoreDefaultLocalState();
		}

		public override void FixTransforms()
		{
			bone1.FixTransform();
			bone2.FixTransform();
			bone3.FixTransform();
		}

		public override bool IsValid(ref string message)
		{
			if (bone1.transform == null || bone2.transform == null || bone3.transform == null)
			{
				message = "Please assign all Bones to the IK solver.";
				return false;
			}
			Transform transform = (Transform)Hierarchy.ContainsDuplicate(new Transform[3] { bone1.transform, bone2.transform, bone3.transform });
			if (transform != null)
			{
				message = transform.name + " is represented multiple times in the Bones.";
				return false;
			}
			if (bone1.transform.position == bone2.transform.position)
			{
				message = "first bone position is the same as second bone position.";
				return false;
			}
			if (bone2.transform.position == bone3.transform.position)
			{
				message = "second bone position is the same as third bone position.";
				return false;
			}
			return true;
		}

		public bool SetChain(Transform bone1, Transform bone2, Transform bone3, Transform root)
		{
			this.bone1.transform = bone1;
			this.bone2.transform = bone2;
			this.bone3.transform = bone3;
			Initiate(root);
			return base.initiated;
		}

		protected override void OnInitiate()
		{
			if (bendNormal == Vector3.zero)
			{
				bendNormal = Vector3.right;
			}
			OnInitiateVirtual();
			IKPosition = bone3.transform.position;
			IKRotation = bone3.transform.rotation;
			InitiateBones();
			directHierarchy = IsDirectHierarchy();
		}

		private bool IsDirectHierarchy()
		{
			if (bone3.transform.parent != bone2.transform)
			{
				return false;
			}
			if (bone2.transform.parent != bone1.transform)
			{
				return false;
			}
			return true;
		}

		private void InitiateBones()
		{
			bone1.Initiate(bone2.transform.position, bendNormal);
			bone2.Initiate(bone3.transform.position, bendNormal);
			SetBendPlaneToCurrent();
		}

		protected override void OnUpdate()
		{
			IKPositionWeight = Mathf.Clamp(IKPositionWeight, 0f, 1f);
			IKRotationWeight = Mathf.Clamp(IKRotationWeight, 0f, 1f);
			if (target != null)
			{
				IKPosition = target.position;
				IKRotation = target.rotation;
			}
			OnUpdateVirtual();
			if (IKPositionWeight > 0f)
			{
				if (!directHierarchy)
				{
					bone1.Initiate(bone2.transform.position, bendNormal);
					bone2.Initiate(bone3.transform.position, bendNormal);
				}
				bone1.sqrMag = (bone2.transform.position - bone1.transform.position).sqrMagnitude;
				bone2.sqrMag = (bone3.transform.position - bone2.transform.position).sqrMagnitude;
				if (bendNormal == Vector3.zero && !Warning.logged)
				{
					LogWarning("IKSolverTrigonometric Bend Normal is Vector3.zero.");
				}
				weightIKPosition = Vector3.Lerp(bone3.transform.position, IKPosition, IKPositionWeight);
				Vector3 vector = Vector3.Lerp(bone1.GetBendNormalFromCurrentRotation(), bendNormal, IKPositionWeight);
				Vector3 vector2 = Vector3.Lerp(bone2.transform.position - bone1.transform.position, GetBendDirection(weightIKPosition, vector), IKPositionWeight);
				if (vector2 == Vector3.zero)
				{
					vector2 = bone2.transform.position - bone1.transform.position;
				}
				bone1.transform.rotation = bone1.GetRotation(vector2, vector);
				bone2.transform.rotation = bone2.GetRotation(weightIKPosition - bone2.transform.position, bone2.GetBendNormalFromCurrentRotation());
			}
			if (IKRotationWeight > 0f)
			{
				bone3.transform.rotation = Quaternion.Slerp(bone3.transform.rotation, IKRotation, IKRotationWeight);
			}
			OnPostSolveVirtual();
		}

		protected virtual void OnInitiateVirtual()
		{
		}

		protected virtual void OnUpdateVirtual()
		{
		}

		protected virtual void OnPostSolveVirtual()
		{
		}

		protected Vector3 GetBendDirection(Vector3 IKPosition, Vector3 bendNormal)
		{
			Vector3 vector = IKPosition - bone1.transform.position;
			if (vector == Vector3.zero)
			{
				return Vector3.zero;
			}
			float sqrMagnitude = vector.sqrMagnitude;
			float num = (float)Math.Sqrt(sqrMagnitude);
			float num2 = (sqrMagnitude + bone1.sqrMag - bone2.sqrMag) / 2f / num;
			float y = (float)Math.Sqrt(Mathf.Clamp(bone1.sqrMag - num2 * num2, 0f, float.PositiveInfinity));
			Vector3 upwards = Vector3.Cross(vector, bendNormal);
			return Quaternion.LookRotation(vector, upwards) * new Vector3(0f, y, num2);
		}
	}
}
