using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[HelpURL("https://www.youtube.com/watch?v=9MiZiaJorws&index=6&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6")]
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Grounder/Grounder Full Body Biped")]
	public class GrounderFBBIK : Grounder
	{
		[Serializable]
		public class SpineEffector
		{
			[Tooltip("The type of the effector.")] public FullBodyBipedEffector effectorType;

			[Tooltip("The weight of horizontal bend offset towards the slope.")]
			public float horizontalWeight = 1f;

			[Tooltip("The vertical bend offset weight.")]
			public float verticalWeight;
		}

		[Tooltip("Reference to the FBBIK componet.")]
		public FullBodyBipedIK ik;

		[Tooltip("The amount of spine bending towards upward slopes.")]
		public float spineBend = 2f;

		[Tooltip("The interpolation speed of spine bending.")]
		public float spineSpeed = 3f;

		public SpineEffector[] spine = new SpineEffector[0];

		private Transform[] feet = new Transform[2];

		private Vector3 spineOffset;

		private bool firstSolve;

		[ContextMenu("TUTORIAL VIDEO")]
		private void OpenTutorial()
		{
			Application.OpenURL(
				"https://www.youtube.com/watch?v=9MiZiaJorws&index=6&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6");
		}

		[ContextMenu("User Manual")]
		protected override void OpenUserManual()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page11.html");
		}

		[ContextMenu("Scrpt Reference")]
		protected override void OpenScriptReference()
		{
			Application.OpenURL(
				"http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_grounder_f_b_b_i_k.html");
		}

		public override void Reset()
		{
			solver.Reset();
			spineOffset = Vector3.zero;
		}

		private bool IsReadyToInitiate()
		{
			if (ik == null)
			{
				return false;
			}

			if (!ik.solver.initiated)
			{
				return false;
			}

			return true;
		}

		private void Update()
		{
			firstSolve = true;
			weight = Mathf.Clamp(weight, 0f, 1f);
			if (!(weight <= 0f) && !initiated && IsReadyToInitiate())
			{
				Initiate();
			}
		}

		private void FixedUpdate()
		{
			firstSolve = true;
		}

		private void LateUpdate()
		{
			firstSolve = true;
		}

		private void Initiate()
		{
			ik.solver.leftLegMapping.maintainRotationWeight = 1f;
			ik.solver.rightLegMapping.maintainRotationWeight = 1f;
			feet = new Transform[2];
			feet[0] = ik.solver.leftFootEffector.bone;
			feet[1] = ik.solver.rightFootEffector.bone;
			IKSolverFullBodyBiped iKSolverFullBodyBiped = ik.solver;
			iKSolverFullBodyBiped.OnPreUpdate =
				(IKSolver.UpdateDelegate)Delegate.Combine(iKSolverFullBodyBiped.OnPreUpdate,
					new IKSolver.UpdateDelegate(OnSolverUpdate));
			solver.Initiate(ik.references.root, feet);
			initiated = true;
		}

		private void OnSolverUpdate()
		{
			if (!firstSolve)
			{
				return;
			}

			firstSolve = false;
			if (!base.enabled || weight <= 0f)
			{
				return;
			}

			if (OnPreGrounder != null)
			{
				OnPreGrounder();
			}

			solver.Update();
			ik.references.pelvis.position += solver.pelvis.IKOffset * weight;
			SetLegIK(ik.solver.leftFootEffector, solver.legs[0]);
			SetLegIK(ik.solver.rightFootEffector, solver.legs[1]);
			if (spineBend != 0f)
			{
				spineSpeed = Mathf.Clamp(spineSpeed, 0f, spineSpeed);
				Vector3 vector = GetSpineOffsetTarget() * weight;
				spineOffset = Vector3.Lerp(spineOffset, vector * spineBend, Time.deltaTime * spineSpeed);
				Vector3 vector2 = ik.references.root.up * spineOffset.magnitude;
				for (int i = 0; i < spine.Length; i++)
				{
					ik.solver.GetEffector(spine[i].effectorType).positionOffset +=
						spineOffset * spine[i].horizontalWeight + vector2 * spine[i].verticalWeight;
				}
			}

			if (OnPostGrounder != null)
			{
				OnPostGrounder();
			}
		}

		private void SetLegIK(IKEffector effector, Grounding.Leg leg)
		{
			effector.positionOffset += (leg.IKPosition - effector.bone.position) * weight;
			effector.bone.rotation = Quaternion.Slerp(Quaternion.identity, leg.rotationOffset, weight) *
			                         effector.bone.rotation;
		}

		private void OnDrawGizmosSelected()
		{
			if (ik == null)
			{
				ik = GetComponent<FullBodyBipedIK>();
			}

			if (ik == null)
			{
				ik = GetComponentInParent<FullBodyBipedIK>();
			}

			if (ik == null)
			{
				ik = GetComponentInChildren<FullBodyBipedIK>();
			}
		}

		private void OnDestroy()
		{
			if (initiated && ik != null)
			{
				IKSolverFullBodyBiped iKSolverFullBodyBiped = ik.solver;
				iKSolverFullBodyBiped.OnPreUpdate =
					(IKSolver.UpdateDelegate)Delegate.Remove(iKSolverFullBodyBiped.OnPreUpdate,
						new IKSolver.UpdateDelegate(OnSolverUpdate));
			}
		}
	}
}
