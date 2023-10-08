using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	public class FBBIKSettings : MonoBehaviour
	{
		[Serializable]
		public class Limb
		{
			public FBIKChain.Smoothing reachSmoothing;

			public float maintainRelativePositionWeight;

			public float mappingWeight = 1f;

			public void Apply(FullBodyBipedChain chain, IKSolverFullBodyBiped solver)
			{
				solver.GetChain(chain).reachSmoothing = reachSmoothing;
				solver.GetEndEffector(chain).maintainRelativePositionWeight = maintainRelativePositionWeight;
				solver.GetLimbMapping(chain).weight = mappingWeight;
			}
		}

		public FullBodyBipedIK ik;

		public bool disableAfterStart;

		public Limb leftArm;

		public Limb rightArm;

		public Limb leftLeg;

		public Limb rightLeg;

		public float rootPin;

		public bool bodyEffectChildNodes = true;

		public void UpdateSettings()
		{
			if (!(ik == null))
			{
				leftArm.Apply(FullBodyBipedChain.LeftArm, ik.solver);
				rightArm.Apply(FullBodyBipedChain.RightArm, ik.solver);
				leftLeg.Apply(FullBodyBipedChain.LeftLeg, ik.solver);
				rightLeg.Apply(FullBodyBipedChain.RightLeg, ik.solver);
				ik.solver.chain[0].pin = rootPin;
				ik.solver.bodyEffector.effectChildNodes = bodyEffectChildNodes;
			}
		}

		private void Start()
		{
			Debug.Log(
				"FBBIKSettings is deprecated, you can now edit all the settings from the custom inspector of the FullBodyBipedIK component.");
			UpdateSettings();
			if (disableAfterStart)
			{
				base.enabled = false;
			}
		}

		private void Update()
		{
			UpdateSettings();
		}
	}
}
