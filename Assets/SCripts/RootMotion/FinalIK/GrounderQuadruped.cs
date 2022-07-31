using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[HelpURL("http://www.root-motion.com/finalikdox/html/page11.html")]
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Grounder/Grounder Quadruped")]
	public class GrounderQuadruped : Grounder
	{
		public struct Foot
		{
			public IKSolver solver;

			public Transform transform;

			public Quaternion rotation;

			public Grounding.Leg leg;

			public Foot(IKSolver solver, Transform transform)
			{
				this.solver = solver;
				this.transform = transform;
				leg = null;
				rotation = transform.rotation;
			}
		}

		[Tooltip("The Grounding solver for the forelegs.")]
		public Grounding forelegSolver = new Grounding();

		[Tooltip("The weight of rotating the character root to the ground angle (range: 0 - 1).")]
		[Range(0f, 1f)]
		public float rootRotationWeight = 0.5f;

		[Tooltip("The maximum angle of rotating the quadruped downwards (going downhill, range: -90 - 0).")]
		[Range(-90f, 0f)]
		public float minRootRotation = -25f;

		[Tooltip("The maximum angle of rotating the quadruped upwards (going uphill, range: 0 - 90).")]
		[Range(0f, 90f)]
		public float maxRootRotation = 45f;

		[Tooltip("The speed of interpolating the character root rotation (range: 0 - inf).")]
		public float rootRotationSpeed = 5f;

		[Tooltip("The maximum IK offset for the legs (range: 0 - inf).")]
		public float maxLegOffset = 0.5f;

		[Tooltip("The maximum IK offset for the forelegs (range: 0 - inf).")]
		public float maxForeLegOffset = 0.5f;

		[Tooltip("The weight of maintaining the head's rotation as it was before solving the Grounding (range: 0 - 1).")]
		[Range(0f, 1f)]
		public float maintainHeadRotationWeight = 0.5f;

		[Tooltip("The root Transform of the character, with the rigidbody and the collider.")]
		public Transform characterRoot;

		[Tooltip("The pelvis transform. Common ancestor of both legs and the spine.")]
		public Transform pelvis;

		[Tooltip("The last bone in the spine that is the common parent for both forelegs.")]
		public Transform lastSpineBone;

		[Tooltip("The head (optional, if you intend to maintain it's rotation).")]
		public Transform head;

		public IK[] legs;

		public IK[] forelegs;

		private Foot[] feet = new Foot[0];

		private Vector3 animatedPelvisLocalPosition;

		private Quaternion animatedPelvisLocalRotation;

		private Quaternion animatedHeadLocalRotation;

		private Vector3 solvedPelvisLocalPosition;

		private Quaternion solvedPelvisLocalRotation;

		private Quaternion solvedHeadLocalRotation;

		private int solvedFeet;

		private bool solved;

		private float angle;

		private Transform forefeetRoot;

		private Quaternion headRotation;

		private float lastWeight;

		[ContextMenu("User Manual")]
		protected override void OpenUserManual()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page11.html");
		}

		[ContextMenu("Scrpt Reference")]
		protected override void OpenScriptReference()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_grounder_quadruped.html");
		}

		public override void Reset()
		{
			solver.Reset();
			forelegSolver.Reset();
		}

		private bool IsReadyToInitiate()
		{
			if (pelvis == null)
			{
				return false;
			}
			if (lastSpineBone == null)
			{
				return false;
			}
			if (legs.Length == 0)
			{
				return false;
			}
			if (forelegs.Length == 0)
			{
				return false;
			}
			if (characterRoot == null)
			{
				return false;
			}
			if (!IsReadyToInitiateLegs(legs))
			{
				return false;
			}
			if (!IsReadyToInitiateLegs(forelegs))
			{
				return false;
			}
			return true;
		}

		private bool IsReadyToInitiateLegs(IK[] ikComponents)
		{
			foreach (IK iK in ikComponents)
			{
				if (iK == null)
				{
					return false;
				}
				if (iK is FullBodyBipedIK)
				{
					LogWarning("GrounderIK does not support FullBodyBipedIK, use CCDIK, FABRIK, LimbIK or TrigonometricIK instead. If you want to use FullBodyBipedIK, use the GrounderFBBIK component.");
					return false;
				}
				if (iK is FABRIKRoot)
				{
					LogWarning("GrounderIK does not support FABRIKRoot, use CCDIK, FABRIK, LimbIK or TrigonometricIK instead.");
					return false;
				}
				if (iK is AimIK)
				{
					LogWarning("GrounderIK does not support AimIK, use CCDIK, FABRIK, LimbIK or TrigonometricIK instead.");
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
			for (int i = 0; i < feet.Length; i++)
			{
				if (feet[i].solver != null)
				{
					feet[i].solver.IKPositionWeight = 0f;
				}
			}
		}

		private void Update()
		{
			weight = Mathf.Clamp(weight, 0f, 1f);
			if (!(weight <= 0f))
			{
				solved = false;
				if (!initiated && IsReadyToInitiate())
				{
					Initiate();
				}
			}
		}

		private void Initiate()
		{
			feet = new Foot[legs.Length + forelegs.Length];
			Transform[] array = InitiateFeet(legs, ref feet, 0);
			Transform[] array2 = InitiateFeet(forelegs, ref feet, legs.Length);
			animatedPelvisLocalPosition = pelvis.localPosition;
			animatedPelvisLocalRotation = pelvis.localRotation;
			if (head != null)
			{
				animatedHeadLocalRotation = head.localRotation;
			}
			forefeetRoot = new GameObject().transform;
			forefeetRoot.parent = base.transform;
			forefeetRoot.name = "Forefeet Root";
			solver.Initiate(base.transform, array);
			forelegSolver.Initiate(forefeetRoot, array2);
			for (int i = 0; i < array.Length; i++)
			{
				feet[i].leg = solver.legs[i];
			}
			for (int j = 0; j < array2.Length; j++)
			{
				feet[j + legs.Length].leg = forelegSolver.legs[j];
			}
			initiated = true;
		}

		private Transform[] InitiateFeet(IK[] ikComponents, ref Foot[] f, int indexOffset)
		{
			Transform[] array = new Transform[ikComponents.Length];
			for (int i = 0; i < ikComponents.Length; i++)
			{
				IKSolver.Point[] points = ikComponents[i].GetIKSolver().GetPoints();
				f[i + indexOffset] = new Foot(ikComponents[i].GetIKSolver(), points[points.Length - 1].transform);
				array[i] = f[i + indexOffset].transform;
				IKSolver iKSolver = f[i + indexOffset].solver;
				iKSolver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(iKSolver.OnPreUpdate, new IKSolver.UpdateDelegate(OnSolverUpdate));
				IKSolver iKSolver2 = f[i + indexOffset].solver;
				iKSolver2.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(iKSolver2.OnPostUpdate, new IKSolver.UpdateDelegate(OnPostSolverUpdate));
			}
			return array;
		}

		private void LateUpdate()
		{
			if (!(weight <= 0f))
			{
				rootRotationWeight = Mathf.Clamp(rootRotationWeight, 0f, 1f);
				minRootRotation = Mathf.Clamp(minRootRotation, -90f, maxRootRotation);
				maxRootRotation = Mathf.Clamp(maxRootRotation, minRootRotation, 90f);
				rootRotationSpeed = Mathf.Clamp(rootRotationSpeed, 0f, rootRotationSpeed);
				maxLegOffset = Mathf.Clamp(maxLegOffset, 0f, maxLegOffset);
				maxForeLegOffset = Mathf.Clamp(maxForeLegOffset, 0f, maxForeLegOffset);
				maintainHeadRotationWeight = Mathf.Clamp(maintainHeadRotationWeight, 0f, 1f);
				RootRotation();
			}
		}

		private void RootRotation()
		{
			if (!(rootRotationWeight <= 0f) && !(rootRotationSpeed <= 0f))
			{
				solver.rotateSolver = true;
				forelegSolver.rotateSolver = true;
				Vector3 forward = characterRoot.forward;
				forward.y = 0f;
				Quaternion quaternion = Quaternion.LookRotation(forward);
				Vector3 vector = forelegSolver.rootHit.point - solver.rootHit.point;
				Vector3 vector2 = Quaternion.Inverse(quaternion) * vector;
				float num = Mathf.Atan2(vector2.y, vector2.z) * 57.29578f;
				num = Mathf.Clamp(num * rootRotationWeight, minRootRotation, maxRootRotation);
				angle = Mathf.Lerp(angle, num, Time.deltaTime * rootRotationSpeed);
				characterRoot.rotation = Quaternion.Slerp(characterRoot.rotation, Quaternion.AngleAxis(0f - angle, characterRoot.right) * quaternion, weight);
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
			if (solved)
			{
				return;
			}
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
			if (pelvis.localRotation != solvedPelvisLocalRotation)
			{
				animatedPelvisLocalRotation = pelvis.localRotation;
			}
			else
			{
				pelvis.localRotation = animatedPelvisLocalRotation;
			}
			if (head != null)
			{
				if (head.localRotation != solvedHeadLocalRotation)
				{
					animatedHeadLocalRotation = head.localRotation;
				}
				else
				{
					head.localRotation = animatedHeadLocalRotation;
				}
			}
			for (int i = 0; i < feet.Length; i++)
			{
				feet[i].rotation = feet[i].transform.rotation;
			}
			if (head != null)
			{
				headRotation = head.rotation;
			}
			UpdateForefeetRoot();
			solver.Update();
			forelegSolver.Update();
			pelvis.position += solver.pelvis.IKOffset * weight;
			Vector3 fromDirection = lastSpineBone.position - pelvis.position;
			Vector3 vector = lastSpineBone.position + forelegSolver.root.up * Mathf.Clamp(forelegSolver.pelvis.heightOffset, float.NegativeInfinity, 0f) - solver.root.up * solver.pelvis.heightOffset;
			Vector3 toDirection = vector - pelvis.position;
			Quaternion b = Quaternion.FromToRotation(fromDirection, toDirection);
			pelvis.rotation = Quaternion.Slerp(Quaternion.identity, b, weight) * pelvis.rotation;
			for (int j = 0; j < feet.Length; j++)
			{
				SetFootIK(feet[j], (j >= 2) ? maxForeLegOffset : maxLegOffset);
			}
			solved = true;
			solvedFeet = 0;
			if (OnPostGrounder != null)
			{
				OnPostGrounder();
			}
		}

		private void UpdateForefeetRoot()
		{
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < forelegSolver.legs.Length; i++)
			{
				zero += forelegSolver.legs[i].transform.position;
			}
			zero /= (float)forelegs.Length;
			Vector3 vector = zero - base.transform.position;
			Vector3 normal = base.transform.up;
			Vector3 tangent = vector;
			Vector3.OrthoNormalize(ref normal, ref tangent);
			forefeetRoot.position = base.transform.position + tangent.normalized * vector.magnitude;
		}

		private void SetFootIK(Foot foot, float maxOffset)
		{
			Vector3 vector = foot.leg.IKPosition - foot.transform.position;
			foot.solver.IKPosition = foot.transform.position + Vector3.ClampMagnitude(vector, maxOffset);
			foot.solver.IKPositionWeight = weight;
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
					feet[i].transform.rotation = Quaternion.Slerp(Quaternion.identity, feet[i].leg.rotationOffset, weight) * feet[i].rotation;
				}
				if (head != null)
				{
					head.rotation = Quaternion.Lerp(head.rotation, headRotation, maintainHeadRotationWeight * weight);
				}
				solvedPelvisLocalPosition = pelvis.localPosition;
				solvedPelvisLocalRotation = pelvis.localRotation;
				if (head != null)
				{
					solvedHeadLocalRotation = head.localRotation;
				}
			}
		}

		private void OnDestroy()
		{
			if (initiated)
			{
				DestroyLegs(legs);
				DestroyLegs(forelegs);
			}
		}

		private void DestroyLegs(IK[] ikComponents)
		{
			foreach (IK iK in ikComponents)
			{
				if (iK != null)
				{
					IKSolver iKSolver = iK.GetIKSolver();
					iKSolver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(iKSolver.OnPreUpdate, new IKSolver.UpdateDelegate(OnSolverUpdate));
					IKSolver iKSolver2 = iK.GetIKSolver();
					iKSolver2.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(iKSolver2.OnPostUpdate, new IKSolver.UpdateDelegate(OnPostSolverUpdate));
				}
			}
		}
	}
}
