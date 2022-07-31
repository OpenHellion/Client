using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[HelpURL("http://www.root-motion.com/finalikdox/html/page11.html")]
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Grounder/Grounder Biped")]
	public class GrounderBipedIK : Grounder
	{
		[Tooltip("The BipedIK componet.")]
		public BipedIK ik;

		[Tooltip("The amount of spine bending towards upward slopes.")]
		public float spineBend = 7f;

		[Tooltip("The interpolation speed of spine bending.")]
		public float spineSpeed = 3f;

		private Transform[] feet = new Transform[2];

		private Quaternion[] footRotations = new Quaternion[2];

		private Vector3 animatedPelvisLocalPosition;

		private Vector3 solvedPelvisLocalPosition;

		private Vector3 spineOffset;

		private float lastWeight;

		[ContextMenu("User Manual")]
		protected override void OpenUserManual()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page11.html");
		}

		[ContextMenu("Scrpt Reference")]
		protected override void OpenScriptReference()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_grounder_biped_i_k.html");
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
			if (!ik.solvers.leftFoot.initiated)
			{
				return false;
			}
			if (!ik.solvers.rightFoot.initiated)
			{
				return false;
			}
			return true;
		}

		private void Update()
		{
			weight = Mathf.Clamp(weight, 0f, 1f);
			if (!(weight <= 0f) && !initiated && IsReadyToInitiate())
			{
				Initiate();
			}
		}

		private void Initiate()
		{
			feet = new Transform[2];
			footRotations = new Quaternion[2];
			feet[0] = ik.references.leftFoot;
			feet[1] = ik.references.rightFoot;
			footRotations[0] = Quaternion.identity;
			footRotations[1] = Quaternion.identity;
			IKSolverFABRIK spine = ik.solvers.spine;
			spine.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(spine.OnPreUpdate, new IKSolver.UpdateDelegate(OnSolverUpdate));
			IKSolverLimb rightFoot = ik.solvers.rightFoot;
			rightFoot.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(rightFoot.OnPostUpdate, new IKSolver.UpdateDelegate(OnPostSolverUpdate));
			animatedPelvisLocalPosition = ik.references.pelvis.localPosition;
			solver.Initiate(ik.references.root, feet);
			initiated = true;
		}

		private void OnDisable()
		{
			if (initiated)
			{
				ik.solvers.leftFoot.IKPositionWeight = 0f;
				ik.solvers.rightFoot.IKPositionWeight = 0f;
			}
		}

		private void OnSolverUpdate()
		{
			if (!base.enabled)
			{
				return;
			}
			if (weight <= 0f)
			{
				if (lastWeight <= 0f)
				{
					return;
				}
				OnDisable();
			}
			lastWeight = weight;
			if (OnPreGrounder != null)
			{
				OnPreGrounder();
			}
			if (ik.references.pelvis.localPosition != solvedPelvisLocalPosition)
			{
				animatedPelvisLocalPosition = ik.references.pelvis.localPosition;
			}
			else
			{
				ik.references.pelvis.localPosition = animatedPelvisLocalPosition;
			}
			solver.Update();
			ik.references.pelvis.position += solver.pelvis.IKOffset * weight;
			SetLegIK(ik.solvers.leftFoot, 0);
			SetLegIK(ik.solvers.rightFoot, 1);
			if (spineBend != 0f && ik.references.spine.Length > 0)
			{
				spineSpeed = Mathf.Clamp(spineSpeed, 0f, spineSpeed);
				Vector3 vector = GetSpineOffsetTarget() * weight;
				spineOffset = Vector3.Lerp(spineOffset, vector * spineBend, Time.deltaTime * spineSpeed);
				Quaternion rotation = ik.references.leftUpperArm.rotation;
				Quaternion rotation2 = ik.references.rightUpperArm.rotation;
				Vector3 up = solver.up;
				Quaternion quaternion = Quaternion.FromToRotation(up, up + spineOffset);
				ik.references.spine[0].rotation = quaternion * ik.references.spine[0].rotation;
				ik.references.leftUpperArm.rotation = rotation;
				ik.references.rightUpperArm.rotation = rotation2;
			}
			if (OnPostGrounder != null)
			{
				OnPostGrounder();
			}
		}

		private void SetLegIK(IKSolverLimb limb, int index)
		{
			footRotations[index] = feet[index].rotation;
			limb.IKPosition = solver.legs[index].IKPosition;
			limb.IKPositionWeight = weight;
		}

		private void OnPostSolverUpdate()
		{
			if (!(weight <= 0f) && base.enabled)
			{
				for (int i = 0; i < feet.Length; i++)
				{
					feet[i].rotation = Quaternion.Slerp(Quaternion.identity, solver.legs[i].rotationOffset, weight) * footRotations[i];
				}
				solvedPelvisLocalPosition = ik.references.pelvis.localPosition;
			}
		}

		private void OnDestroy()
		{
			if (initiated && ik != null)
			{
				IKSolverFABRIK spine = ik.solvers.spine;
				spine.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(spine.OnPreUpdate, new IKSolver.UpdateDelegate(OnSolverUpdate));
				IKSolverLimb rightFoot = ik.solvers.rightFoot;
				rightFoot.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(rightFoot.OnPostUpdate, new IKSolver.UpdateDelegate(OnPostSolverUpdate));
			}
		}
	}
}
