using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public class FBBIKHeadEffector : MonoBehaviour
	{
		[Serializable]
		public class BendBone
		{
			[Tooltip("Assign spine and/or neck bones.")]
			public Transform transform;

			[Tooltip("The weight of rotating this bone.")] [Range(0f, 1f)]
			public float weight = 0.5f;

			private Quaternion defaultLocalRotation = Quaternion.identity;

			public BendBone()
			{
			}

			public BendBone(Transform transform, float weight)
			{
				this.transform = transform;
				this.weight = weight;
			}

			public void StoreDefaultLocalState()
			{
				defaultLocalRotation = transform.localRotation;
			}

			public void FixTransforms()
			{
				transform.localRotation = defaultLocalRotation;
			}
		}

		[Tooltip("Reference to the FBBIK component.")]
		public FullBodyBipedIK ik;

		[Header("Position")] [Tooltip("Master weight for positioning the head.")] [Range(0f, 1f)]
		public float positionWeight = 1f;

		[Tooltip("The weight of moving the body along with the head")] [Range(0f, 1f)]
		public float bodyWeight = 0.8f;

		[Tooltip("The weight of moving the thighs along with the head")] [Range(0f, 1f)]
		public float thighWeight = 0.8f;

		[Header("Rotation")] [Tooltip("The weight of rotating the head bone after solving")] [Range(0f, 1f)]
		public float rotationWeight;

		[Tooltip(
			"The master weight of bending/twisting the spine to the rotation of the head effector. This is similar to CCD, but uses the rotation of the head effector not the position.")]
		[Range(0f, 1f)]
		public float bendWeight = 1f;

		[Tooltip("The bones to use for bending.")]
		public BendBone[] bendBones = new BendBone[0];

		[Header("CCD")]
		[Tooltip(
			"Optional. The master weight of the CCD (Cyclic Coordinate Descent) IK effect that bends the spine towards the head effector before FBBIK solves.")]
		[Range(0f, 1f)]
		public float CCDWeight = 1f;

		[Tooltip("The weight of rolling the bones in towards the target")] [Range(0f, 1f)]
		public float roll;

		[Tooltip("Smoothing the CCD effect.")] [Range(0f, 1000f)]
		public float damper = 500f;

		[Tooltip("Bones to use for the CCD pass. Assign spine and/or neck bones.")]
		public Transform[] CCDBones = new Transform[0];

		[Header("Stretching")]
		[Tooltip(
			"Stretching the spine/neck to help reach the target. This is useful for making sure the head stays locked relative to the VR headset. NB! Stretching is done after FBBIK has solved so if you have the hand effectors pinned and spine bones included in the 'Stretch Bones', the hands might become offset from their target positions.")]
		[Range(0f, 1f)]
		public float stretchWeight = 1f;

		[Tooltip("Stretch magnitude limit.")] public float maxStretch = 0.1f;

		[Tooltip("If > 0, dampers the stretching effect.")]
		public float stretchDamper;

		[Tooltip(
			"If true, will fix head position to this Transform no matter what. Good for making sure the head will not budge away from the VR headset")]
		public bool fixHead;

		[Tooltip("Bones to use for stretching. The more bones you add, the less noticable the effect.")]
		public Transform[] stretchBones = new Transform[0];

		private Vector3 offset;

		private Vector3 headToBody;

		private Vector3 shoulderCenterToHead;

		private Vector3 headToLeftThigh;

		private Vector3 headToRightThigh;

		private Vector3 leftShoulderPos;

		private Vector3 rightShoulderPos;

		private float shoulderDist;

		private float leftShoulderDist;

		private float rightShoulderDist;

		private Quaternion chestRotation;

		private Quaternion headRotationRelativeToRoot;

		private Quaternion[] ccdDefaultLocalRotations = new Quaternion[0];

		private Vector3 headLocalPosition;

		private Quaternion headLocalRotation;

		private Vector3[] stretchLocalPositions = new Vector3[0];

		private Quaternion[] stretchLocalRotations = new Quaternion[0];

		private int bendBonesCount;

		private int ccdBonesCount;

		private int stretchBonesCount;

		private void Start()
		{
			IKSolverFullBodyBiped solver = ik.solver;
			solver.OnPreRead =
				(IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPreRead, new IKSolver.UpdateDelegate(OnPreRead));
			IKSolverFullBodyBiped solver2 = ik.solver;
			solver2.OnPreIteration =
				(IKSolver.IterationDelegate)Delegate.Combine(solver2.OnPreIteration,
					new IKSolver.IterationDelegate(Iterate));
			IKSolverFullBodyBiped solver3 = ik.solver;
			solver3.OnPostUpdate =
				(IKSolver.UpdateDelegate)Delegate.Combine(solver3.OnPostUpdate,
					new IKSolver.UpdateDelegate(OnPostUpdate));
			IKSolverFullBodyBiped solver4 = ik.solver;
			solver4.OnStoreDefaultLocalState = (IKSolver.UpdateDelegate)Delegate.Combine(
				solver4.OnStoreDefaultLocalState, new IKSolver.UpdateDelegate(OnStoreDefaultLocalState));
			IKSolverFullBodyBiped solver5 = ik.solver;
			solver5.OnFixTransforms = (IKSolver.UpdateDelegate)Delegate.Combine(solver5.OnFixTransforms,
				new IKSolver.UpdateDelegate(OnFixTransforms));
			headRotationRelativeToRoot = Quaternion.Inverse(ik.references.root.rotation) * ik.references.head.rotation;
		}

		private void OnStoreDefaultLocalState()
		{
			BendBone[] array = bendBones;
			foreach (BendBone bendBone in array)
			{
				if (bendBone != null)
				{
					bendBone.StoreDefaultLocalState();
				}
			}

			ccdDefaultLocalRotations = new Quaternion[CCDBones.Length];
			for (int j = 0; j < CCDBones.Length; j++)
			{
				if (CCDBones[j] != null)
				{
					ccdDefaultLocalRotations[j] = CCDBones[j].localRotation;
				}
			}

			headLocalPosition = ik.references.head.localPosition;
			headLocalRotation = ik.references.head.localRotation;
			stretchLocalPositions = new Vector3[stretchBones.Length];
			stretchLocalRotations = new Quaternion[stretchBones.Length];
			for (int k = 0; k < stretchBones.Length; k++)
			{
				if (stretchBones[k] != null)
				{
					stretchLocalPositions[k] = stretchBones[k].localPosition;
					stretchLocalRotations[k] = stretchBones[k].localRotation;
				}
			}

			bendBonesCount = bendBones.Length;
			ccdBonesCount = CCDBones.Length;
			stretchBonesCount = stretchBones.Length;
		}

		private void OnFixTransforms()
		{
			BendBone[] array = bendBones;
			foreach (BendBone bendBone in array)
			{
				if (bendBone != null)
				{
					bendBone.FixTransforms();
				}
			}

			for (int j = 0; j < CCDBones.Length; j++)
			{
				if (CCDBones[j] != null)
				{
					CCDBones[j].localRotation = ccdDefaultLocalRotations[j];
				}
			}

			ik.references.head.localPosition = headLocalPosition;
			ik.references.head.localRotation = headLocalRotation;
			for (int k = 0; k < stretchBones.Length; k++)
			{
				if (stretchBones[k] != null)
				{
					stretchBones[k].localPosition = stretchLocalPositions[k];
					stretchBones[k].localRotation = stretchLocalRotations[k];
				}
			}
		}

		private void OnPreRead()
		{
			if (base.enabled && base.gameObject.activeInHierarchy && ik.solver.iterations != 0)
			{
				if (bendBonesCount != bendBones.Length || ccdBonesCount != CCDBones.Length ||
				    stretchBonesCount != stretchBones.Length)
				{
					OnStoreDefaultLocalState();
				}

				SpineBend();
				CCDPass();
				offset = base.transform.position - ik.references.head.position;
				shoulderDist = Vector3.Distance(ik.references.leftUpperArm.position,
					ik.references.rightUpperArm.position);
				leftShoulderDist = Vector3.Distance(ik.references.head.position, ik.references.leftUpperArm.position);
				rightShoulderDist = Vector3.Distance(ik.references.head.position, ik.references.rightUpperArm.position);
				headToBody = ik.solver.rootNode.position - ik.references.head.position;
				headToLeftThigh = ik.references.leftThigh.position - ik.references.head.position;
				headToRightThigh = ik.references.rightThigh.position - ik.references.head.position;
				leftShoulderPos = ik.references.leftUpperArm.position + offset * bodyWeight;
				rightShoulderPos = ik.references.rightUpperArm.position + offset * bodyWeight;
				chestRotation =
					Quaternion.LookRotation(ik.references.head.position - ik.references.leftUpperArm.position,
						ik.references.rightUpperArm.position - ik.references.leftUpperArm.position);
			}
		}

		private void SpineBend()
		{
			float num = bendWeight * ik.solver.IKPositionWeight;
			if (num <= 0f || bendBones.Length == 0)
			{
				return;
			}

			Quaternion b = base.transform.rotation *
			               Quaternion.Inverse(ik.references.root.rotation * headRotationRelativeToRoot);
			float num2 = 1f / (float)bendBones.Length;
			for (int i = 0; i < bendBones.Length; i++)
			{
				if (bendBones[i].transform != null)
				{
					bendBones[i].transform.rotation =
						Quaternion.Lerp(Quaternion.identity, b, num2 * bendBones[i].weight * num) *
						bendBones[i].transform.rotation;
				}
			}
		}

		private void CCDPass()
		{
			float num = CCDWeight * ik.solver.IKPositionWeight;
			if (!(num <= 0f))
			{
				for (int num2 = CCDBones.Length - 1; num2 > -1; num2--)
				{
					Quaternion quaternion =
						Quaternion.FromToRotation(ik.references.head.position - CCDBones[num2].position,
							base.transform.position - CCDBones[num2].position) * CCDBones[num2].rotation;
					float num3 = Mathf.Lerp((CCDBones.Length - num2) / CCDBones.Length, 1f, roll);
					float num4 = Quaternion.Angle(Quaternion.identity, quaternion);
					num4 = Mathf.Lerp(0f, num4, (damper - num4) / damper);
					CCDBones[num2].rotation =
						Quaternion.RotateTowards(CCDBones[num2].rotation, quaternion, num4 * num * num3);
				}
			}
		}

		private void Iterate(int iteration)
		{
			if (base.enabled && base.gameObject.activeInHierarchy && ik.solver.iterations != 0)
			{
				leftShoulderPos = base.transform.position +
				                  (leftShoulderPos - base.transform.position).normalized * leftShoulderDist;
				rightShoulderPos = base.transform.position +
				                   (rightShoulderPos - base.transform.position).normalized * rightShoulderDist;
				Solve(ref leftShoulderPos, ref rightShoulderPos, shoulderDist);
				LerpSolverPosition(ik.solver.leftShoulderEffector, leftShoulderPos,
					positionWeight * ik.solver.IKPositionWeight);
				LerpSolverPosition(ik.solver.rightShoulderEffector, rightShoulderPos,
					positionWeight * ik.solver.IKPositionWeight);
				Quaternion to = Quaternion.LookRotation(base.transform.position - leftShoulderPos,
					rightShoulderPos - leftShoulderPos);
				Quaternion quaternion = QuaTools.FromToRotation(chestRotation, to);
				Vector3 vector = quaternion * headToBody;
				LerpSolverPosition(ik.solver.bodyEffector, base.transform.position + vector,
					positionWeight * ik.solver.IKPositionWeight);
				Quaternion quaternion2 = Quaternion.Lerp(Quaternion.identity, quaternion, thighWeight);
				Vector3 vector2 = quaternion2 * headToLeftThigh;
				Vector3 vector3 = quaternion2 * headToRightThigh;
				LerpSolverPosition(ik.solver.leftThighEffector, base.transform.position + vector2,
					positionWeight * ik.solver.IKPositionWeight);
				LerpSolverPosition(ik.solver.rightThighEffector, base.transform.position + vector3,
					positionWeight * ik.solver.IKPositionWeight);
			}
		}

		private void OnPostUpdate()
		{
			if (base.enabled && base.gameObject.activeInHierarchy)
			{
				Stretching();
				ik.references.head.rotation = Quaternion.Lerp(ik.references.head.rotation, base.transform.rotation,
					rotationWeight * ik.solver.IKPositionWeight);
			}
		}

		private void Stretching()
		{
			float num = stretchWeight * ik.solver.IKPositionWeight;
			if (num <= 0f)
			{
				return;
			}

			Vector3 vector = Vector3.ClampMagnitude(base.transform.position - ik.references.head.position, maxStretch);
			vector *= num;
			stretchDamper = Mathf.Max(stretchDamper, 0f);
			if (stretchDamper > 0f)
			{
				vector /= (1f + vector.magnitude) * (1f + stretchDamper);
			}

			for (int i = 0; i < stretchBones.Length; i++)
			{
				if (stretchBones[i] != null)
				{
					stretchBones[i].position += vector / stretchBones.Length;
				}
			}

			if (fixHead && ik.solver.IKPositionWeight > 0f)
			{
				ik.references.head.position = base.transform.position;
			}
		}

		private void LerpSolverPosition(IKEffector effector, Vector3 position, float weight)
		{
			effector.GetNode(ik.solver).solverPosition =
				Vector3.Lerp(effector.GetNode(ik.solver).solverPosition, position, weight);
		}

		private void Solve(ref Vector3 pos1, ref Vector3 pos2, float nominalDistance)
		{
			Vector3 vector = pos2 - pos1;
			float magnitude = vector.magnitude;
			if (magnitude != nominalDistance && magnitude != 0f)
			{
				float num = 1f;
				num *= 1f - nominalDistance / magnitude;
				Vector3 vector2 = vector * num * 0.5f;
				pos1 += vector2;
				pos2 -= vector2;
			}
		}

		private void OnDestroy()
		{
			if (ik != null)
			{
				IKSolverFullBodyBiped solver = ik.solver;
				solver.OnPreRead =
					(IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPreRead, new IKSolver.UpdateDelegate(OnPreRead));
				IKSolverFullBodyBiped solver2 = ik.solver;
				solver2.OnPreIteration = (IKSolver.IterationDelegate)Delegate.Remove(solver2.OnPreIteration,
					new IKSolver.IterationDelegate(Iterate));
				IKSolverFullBodyBiped solver3 = ik.solver;
				solver3.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver3.OnPostUpdate,
					new IKSolver.UpdateDelegate(OnPostUpdate));
				IKSolverFullBodyBiped solver4 = ik.solver;
				solver4.OnStoreDefaultLocalState = (IKSolver.UpdateDelegate)Delegate.Remove(
					solver4.OnStoreDefaultLocalState, new IKSolver.UpdateDelegate(OnStoreDefaultLocalState));
				IKSolverFullBodyBiped solver5 = ik.solver;
				solver5.OnFixTransforms = (IKSolver.UpdateDelegate)Delegate.Remove(solver5.OnFixTransforms,
					new IKSolver.UpdateDelegate(OnFixTransforms));
			}
		}
	}
}
