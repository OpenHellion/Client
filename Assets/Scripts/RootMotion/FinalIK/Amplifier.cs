using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public class Amplifier : OffsetModifier
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

			[Tooltip("The Transform that's motion we are reading.")]
			public Transform transform;

			[Tooltip("Amplify the 'transform's' position relative to this Transform.")]
			public Transform relativeTo;

			[Tooltip("Linking the body to effectors. One Body can be used to offset more than one effector.")]
			public EffectorLink[] effectorLinks;

			[Tooltip("Amplification magnitude along the up axis of the character.")]
			public float verticalWeight = 1f;

			[Tooltip("Amplification magnitude along the horizontal axes of the character.")]
			public float horizontalWeight = 1f;

			[Tooltip("Speed of the amplifier. 0 means instant.")]
			public float speed = 3f;

			private Vector3 lastRelativePos;

			private Vector3 smoothDelta;

			private bool firstUpdate;

			public void Update(IKSolverFullBodyBiped solver, float w, float deltaTime)
			{
				if (!(transform == null) && !(relativeTo == null))
				{
					Vector3 vector = relativeTo.InverseTransformDirection(transform.position - relativeTo.position);
					if (firstUpdate)
					{
						lastRelativePos = vector;
						firstUpdate = false;
					}

					Vector3 vector2 = (vector - lastRelativePos) / deltaTime;
					smoothDelta = ((!(speed <= 0f)) ? Vector3.Lerp(smoothDelta, vector2, deltaTime * speed) : vector2);
					Vector3 v = relativeTo.TransformDirection(smoothDelta);
					Vector3 vector3 = V3Tools.ExtractVertical(v, solver.GetRoot().up, verticalWeight) +
					                  V3Tools.ExtractHorizontal(v, solver.GetRoot().up, horizontalWeight);
					for (int i = 0; i < effectorLinks.Length; i++)
					{
						solver.GetEffector(effectorLinks[i].effector).positionOffset +=
							vector3 * w * effectorLinks[i].weight;
					}

					lastRelativePos = vector;
				}
			}

			private static Vector3 Multiply(Vector3 v1, Vector3 v2)
			{
				v1.x *= v2.x;
				v1.y *= v2.y;
				v1.z *= v2.z;
				return v1;
			}
		}

		[Tooltip("The amplified bodies.")] public Body[] bodies;

		protected override void OnModifyOffset()
		{
			if (!ik.fixTransforms)
			{
				if (!Warning.logged)
				{
					Warning.Log(
						"Amplifier needs the Fix Transforms option of the FBBIK to be set to true. Otherwise it might amplify to infinity, should the animator of the character stop because of culling.",
						base.transform);
				}

				return;
			}

			Body[] array = bodies;
			foreach (Body body in array)
			{
				body.Update(ik.solver, weight, base.deltaTime);
			}
		}
	}
}
