using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[HelpURL("http://www.root-motion.com/finalikdox/html/page11.html")]
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Grounder/Grounder IK")]
	public class GrounderIK : Grounder
	{
		public IK[] legs;

		[Tooltip("The pelvis transform. Common ancestor of all the legs.")]
		public Transform pelvis;

		[Tooltip("The root Transform of the character, with the rigidbody and the collider.")]
		public Transform characterRoot;

		[Tooltip("The weight of rotating the character root to the ground normal (range: 0 - 1).")] [Range(0f, 1f)]
		public float rootRotationWeight;

		[Tooltip("The speed of rotating the character root to the ground normal (range: 0 - inf).")]
		public float rootRotationSpeed = 5f;

		[Tooltip("The maximum angle of root rotation (range: 0 - 90).")]
		public float maxRootRotationAngle = 45f;

		private Transform[] feet = new Transform[0];

		private Quaternion[] footRotations = new Quaternion[0];

		private Vector3 animatedPelvisLocalPosition;

		private Vector3 solvedPelvisLocalPosition;

		private int solvedFeet;

		private bool solved;

		private float lastWeight;

		[ContextMenu("User Manual")]
		protected override void OpenUserManual()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page11.html");
		}

		[ContextMenu("Scrpt Reference")]
		protected override void OpenScriptReference()
		{
			Application.OpenURL(
				"http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_grounder_i_k.html");
		}

		public override void Reset()
		{
			solver.Reset();
		}

		private bool IsReadyToInitiate()
		{
			if (pelvis == null)
			{
				return false;
			}

			if (legs.Length == 0)
			{
				return false;
			}

			IK[] array = legs;
			foreach (IK iK in array)
			{
				if (iK == null)
				{
					return false;
				}

				if (iK is FullBodyBipedIK)
				{
					LogWarning(
						"GrounderIK does not support FullBodyBipedIK, use CCDIK, FABRIK, LimbIK or TrigonometricIK instead. If you want to use FullBodyBipedIK, use the GrounderFBBIK component.");
					return false;
				}

				if (iK is FABRIKRoot)
				{
					LogWarning(
						"GrounderIK does not support FABRIKRoot, use CCDIK, FABRIK, LimbIK or TrigonometricIK instead.");
					return false;
				}

				if (iK is AimIK)
				{
					LogWarning(
						"GrounderIK does not support AimIK, use CCDIK, FABRIK, LimbIK or TrigonometricIK instead.");
					return false;
				}
			}

			return true;
		}

		private void OnDisable()
		{
			if (!initiated)
			{
				return;
			}

			for (int i = 0; i < legs.Length; i++)
			{
				if (legs[i] != null)
				{
					legs[i].GetIKSolver().IKPositionWeight = 0f;
				}
			}
		}

		private void Update()
		{
			weight = Mathf.Clamp(weight, 0f, 1f);
			if (weight <= 0f)
			{
				return;
			}

			solved = false;
			if (initiated)
			{
				rootRotationWeight = Mathf.Clamp(rootRotationWeight, 0f, 1f);
				rootRotationSpeed = Mathf.Clamp(rootRotationSpeed, 0f, rootRotationSpeed);
				if (characterRoot != null && rootRotationSpeed > 0f && rootRotationWeight > 0f)
				{
					Vector3 vector = solver.GetLegsPlaneNormal();
					if (rootRotationWeight < 1f)
					{
						vector = Vector3.Slerp(Vector3.up, vector, rootRotationWeight);
					}

					Quaternion from = Quaternion.FromToRotation(base.transform.up, Vector3.up) * characterRoot.rotation;
					Quaternion b = Quaternion.RotateTowards(from,
						Quaternion.FromToRotation(base.transform.up, vector) * characterRoot.rotation,
						maxRootRotationAngle);
					characterRoot.rotation =
						Quaternion.Lerp(characterRoot.rotation, b, Time.deltaTime * rootRotationSpeed);
				}
			}
			else if (IsReadyToInitiate())
			{
				Initiate();
			}
		}

		private void Initiate()
		{
			feet = new Transform[legs.Length];
			footRotations = new Quaternion[legs.Length];
			for (int i = 0; i < feet.Length; i++)
			{
				footRotations[i] = Quaternion.identity;
			}

			for (int j = 0; j < legs.Length; j++)
			{
				IKSolver.Point[] points = legs[j].GetIKSolver().GetPoints();
				feet[j] = points[points.Length - 1].transform;
				IKSolver iKSolver = legs[j].GetIKSolver();
				iKSolver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(iKSolver.OnPreUpdate,
					new IKSolver.UpdateDelegate(OnSolverUpdate));
				IKSolver iKSolver2 = legs[j].GetIKSolver();
				iKSolver2.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(iKSolver2.OnPostUpdate,
					new IKSolver.UpdateDelegate(OnPostSolverUpdate));
			}

			animatedPelvisLocalPosition = pelvis.localPosition;
			solver.Initiate(base.transform, feet);
			initiated = true;
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
			if (!solved)
			{
				if (OnPreGrounder != null)
				{
					OnPreGrounder();
				}

				if (pelvis.localPosition != solvedPelvisLocalPosition)
				{
					animatedPelvisLocalPosition = pelvis.localPosition;
				}
				else
				{
					pelvis.localPosition = animatedPelvisLocalPosition;
				}

				solver.Update();
				for (int i = 0; i < legs.Length; i++)
				{
					SetLegIK(i);
				}

				pelvis.position += solver.pelvis.IKOffset * weight;
				solved = true;
				solvedFeet = 0;
				if (OnPostGrounder != null)
				{
					OnPostGrounder();
				}
			}
		}

		private void SetLegIK(int index)
		{
			footRotations[index] = feet[index].rotation;
			legs[index].GetIKSolver().IKPosition = solver.legs[index].IKPosition;
			legs[index].GetIKSolver().IKPositionWeight = weight;
		}

		private void OnPostSolverUpdate()
		{
			if (weight <= 0f || !base.enabled)
			{
				return;
			}

			solvedFeet++;
			if (solvedFeet >= feet.Length)
			{
				for (int i = 0; i < feet.Length; i++)
				{
					feet[i].rotation = Quaternion.Slerp(Quaternion.identity, solver.legs[i].rotationOffset, weight) *
					                   footRotations[i];
				}

				solvedPelvisLocalPosition = pelvis.localPosition;
			}
		}

		private void OnDestroy()
		{
			if (!initiated)
			{
				return;
			}

			IK[] array = legs;
			foreach (IK iK in array)
			{
				if (iK != null)
				{
					IKSolver iKSolver = iK.GetIKSolver();
					iKSolver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(iKSolver.OnPreUpdate,
						new IKSolver.UpdateDelegate(OnSolverUpdate));
					IKSolver iKSolver2 = iK.GetIKSolver();
					iKSolver2.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(iKSolver2.OnPostUpdate,
						new IKSolver.UpdateDelegate(OnPostSolverUpdate));
				}
			}
		}
	}
}
