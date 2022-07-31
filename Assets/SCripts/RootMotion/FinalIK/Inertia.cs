using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public class Inertia : OffsetModifier
	{
		[Serializable]
		public class Body
		{
			[Serializable]
			public class EffectorLink
			{
				[Tooltip("Type of the FBBIK effector to use")]
				public FullBodyBipedEffector effector;

				[Tooltip("Weight of using this effector")]
				public float weight;
			}

			[Tooltip("The Transform to follow, can be any bone of the character")]
			public Transform transform;

			[Tooltip("Linking the body to effectors. One Body can be used to offset more than one effector")]
			public EffectorLink[] effectorLinks;

			[Tooltip("The speed to follow the Transform")]
			public float speed = 10f;

			[Tooltip("The acceleration, smaller values means lazyer following")]
			public float acceleration = 3f;

			[Tooltip("Matching target velocity")]
			[Range(0f, 1f)]
			public float matchVelocity;

			[Tooltip("gravity applied to the Body")]
			public float gravity;

			private Vector3 delta;

			private Vector3 lazyPoint;

			private Vector3 direction;

			private Vector3 lastPosition;

			private bool firstUpdate = true;

			public void Reset()
			{
				if (!(transform == null))
				{
					lazyPoint = transform.position;
					lastPosition = transform.position;
					direction = Vector3.zero;
				}
			}

			public void Update(IKSolverFullBodyBiped solver, float weight, float deltaTime)
			{
				if (!(transform == null))
				{
					if (firstUpdate)
					{
						Reset();
						firstUpdate = false;
					}
					direction = Vector3.Lerp(direction, (transform.position - lazyPoint) / deltaTime * 0.01f, deltaTime * acceleration);
					lazyPoint += direction * deltaTime * speed;
					delta = transform.position - lastPosition;
					lazyPoint += delta * matchVelocity;
					lazyPoint.y += gravity * deltaTime;
					EffectorLink[] array = effectorLinks;
					foreach (EffectorLink effectorLink in array)
					{
						solver.GetEffector(effectorLink.effector).positionOffset += (lazyPoint - transform.position) * effectorLink.weight * weight;
					}
					lastPosition = transform.position;
				}
			}
		}

		[Tooltip("The array of Bodies")]
		public Body[] bodies;

		[Tooltip("The array of OffsetLimits")]
		public OffsetLimits[] limits;

		public void ResetBodies()
		{
			Body[] array = bodies;
			foreach (Body body in array)
			{
				body.Reset();
			}
		}

		protected override void OnModifyOffset()
		{
			Body[] array = bodies;
			foreach (Body body in array)
			{
				body.Update(ik.solver, weight, base.deltaTime);
			}
			ApplyLimits(limits);
		}
	}
}
