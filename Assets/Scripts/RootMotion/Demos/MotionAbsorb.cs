using System;
using System.Collections;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	public class MotionAbsorb : MonoBehaviour
	{
		[Serializable]
		public class Absorber
		{
			[Tooltip("The type of effector (hand, foot, shoulder...) - this is just an enum")]
			public FullBodyBipedEffector effector;

			[Tooltip("How much should motion be absorbed on this effector")]
			public float weight = 1f;

			public void SetToBone(IKSolverFullBodyBiped solver)
			{
				solver.GetEffector(effector).position = solver.GetEffector(effector).bone.position;
				solver.GetEffector(effector).rotation = solver.GetEffector(effector).bone.rotation;
			}

			public void SetEffectorWeights(IKSolverFullBodyBiped solver, float w)
			{
				solver.GetEffector(effector).positionWeight = w * weight;
				solver.GetEffector(effector).rotationWeight = w * weight;
			}
		}

		[Tooltip("Reference to the FBBIK component")]
		public FullBodyBipedIK ik;

		[Tooltip("Array containing the absorbers")]
		public Absorber[] absorbers;

		[Tooltip("The master weight")]
		public float weight = 1f;

		[Tooltip("Weight falloff curve (how fast will the effect reduce after impact)")]
		public AnimationCurve falloff;

		[Tooltip("How fast will the impact fade away. (if 1, effect lasts for 1 second)")]
		public float falloffSpeed = 1f;

		private float timer;

		private void OnCollisionEnter()
		{
			if (!(timer > 0f))
			{
				StartCoroutine(AbsorbMotion());
			}
		}

		private IEnumerator AbsorbMotion()
		{
			timer = 1f;
			for (int i = 0; i < absorbers.Length; i++)
			{
				absorbers[i].SetToBone(ik.solver);
			}
			while (timer > 0f)
			{
				timer -= Time.deltaTime * falloffSpeed;
				float w = falloff.Evaluate(timer);
				for (int j = 0; j < absorbers.Length; j++)
				{
					absorbers[j].SetEffectorWeights(ik.solver, w * weight);
				}
				yield return null;
			}
			yield return null;
		}
	}
}
