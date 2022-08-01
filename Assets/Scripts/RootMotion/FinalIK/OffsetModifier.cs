using System;
using System.Collections;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public abstract class OffsetModifier : MonoBehaviour
	{
		[Serializable]
		public class OffsetLimits
		{
			[Tooltip("The effector type (this is just an enum)")]
			public FullBodyBipedEffector effector;

			[Tooltip("Spring force, if zero then this is a hard limit, if not, offset can exceed the limit.")]
			public float spring;

			[Tooltip("Which axes to limit the offset on?")]
			public bool x;

			[Tooltip("Which axes to limit the offset on?")]
			public bool y;

			[Tooltip("Which axes to limit the offset on?")]
			public bool z;

			[Tooltip("The limits")]
			public float minX;

			[Tooltip("The limits")]
			public float maxX;

			[Tooltip("The limits")]
			public float minY;

			[Tooltip("The limits")]
			public float maxY;

			[Tooltip("The limits")]
			public float minZ;

			[Tooltip("The limits")]
			public float maxZ;

			public void Apply(IKEffector e, Quaternion rootRotation)
			{
				Vector3 vector = Quaternion.Inverse(rootRotation) * e.positionOffset;
				if (spring <= 0f)
				{
					if (x)
					{
						vector.x = Mathf.Clamp(vector.x, minX, maxX);
					}
					if (y)
					{
						vector.y = Mathf.Clamp(vector.y, minY, maxY);
					}
					if (z)
					{
						vector.z = Mathf.Clamp(vector.z, minZ, maxZ);
					}
				}
				else
				{
					if (x)
					{
						vector.x = SpringAxis(vector.x, minX, maxX);
					}
					if (y)
					{
						vector.y = SpringAxis(vector.y, minY, maxY);
					}
					if (z)
					{
						vector.z = SpringAxis(vector.z, minZ, maxZ);
					}
				}
				e.positionOffset = rootRotation * vector;
			}

			private float SpringAxis(float value, float min, float max)
			{
				if (value > min && value < max)
				{
					return value;
				}
				if (value < min)
				{
					return Spring(value, min, true);
				}
				return Spring(value, max, false);
			}

			private float Spring(float value, float limit, bool negative)
			{
				float num = value - limit;
				float num2 = num * spring;
				if (negative)
				{
					return value + Mathf.Clamp(0f - num2, 0f, 0f - num);
				}
				return value - Mathf.Clamp(num2, 0f, num);
			}
		}

		[Tooltip("The master weight")]
		public float weight = 1f;

		[Tooltip("Reference to the FBBIK component")]
		[SerializeField]
		protected FullBodyBipedIK ik;

		private float lastTime;

		protected float deltaTime
		{
			get
			{
				return Time.time - lastTime;
			}
		}

		protected abstract void OnModifyOffset();

		protected virtual void Start()
		{
			StartCoroutine(Initiate());
		}

		private IEnumerator Initiate()
		{
			while (ik == null)
			{
				yield return null;
			}
			IKSolverFullBodyBiped solver = ik.solver;
			solver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPreUpdate, new IKSolver.UpdateDelegate(ModifyOffset));
			lastTime = Time.time;
		}

		private void ModifyOffset()
		{
			if (base.enabled && !(weight <= 0f) && !(deltaTime <= 0f) && !(ik == null))
			{
				weight = Mathf.Clamp(weight, 0f, 1f);
				OnModifyOffset();
				lastTime = Time.time;
			}
		}

		protected void ApplyLimits(OffsetLimits[] limits)
		{
			foreach (OffsetLimits offsetLimits in limits)
			{
				offsetLimits.Apply(ik.solver.GetEffector(offsetLimits.effector), base.transform.rotation);
			}
		}

		protected virtual void OnDestroy()
		{
			if (ik != null)
			{
				IKSolverFullBodyBiped solver = ik.solver;
				solver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPreUpdate, new IKSolver.UpdateDelegate(ModifyOffset));
			}
		}
	}
}
