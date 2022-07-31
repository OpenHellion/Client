using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class IKSolverFullBodyBiped : IKSolverFullBody
	{
		public Transform rootNode;

		[Range(0f, 1f)]
		public float spineStiffness = 0.5f;

		[Range(-1f, 1f)]
		public float pullBodyVertical = 0.5f;

		[Range(-1f, 1f)]
		public float pullBodyHorizontal;

		private Vector3 offset;

		public IKEffector bodyEffector
		{
			get
			{
				return GetEffector(FullBodyBipedEffector.Body);
			}
		}

		public IKEffector leftShoulderEffector
		{
			get
			{
				return GetEffector(FullBodyBipedEffector.LeftShoulder);
			}
		}

		public IKEffector rightShoulderEffector
		{
			get
			{
				return GetEffector(FullBodyBipedEffector.RightShoulder);
			}
		}

		public IKEffector leftThighEffector
		{
			get
			{
				return GetEffector(FullBodyBipedEffector.LeftThigh);
			}
		}

		public IKEffector rightThighEffector
		{
			get
			{
				return GetEffector(FullBodyBipedEffector.RightThigh);
			}
		}

		public IKEffector leftHandEffector
		{
			get
			{
				return GetEffector(FullBodyBipedEffector.LeftHand);
			}
		}

		public IKEffector rightHandEffector
		{
			get
			{
				return GetEffector(FullBodyBipedEffector.RightHand);
			}
		}

		public IKEffector leftFootEffector
		{
			get
			{
				return GetEffector(FullBodyBipedEffector.LeftFoot);
			}
		}

		public IKEffector rightFootEffector
		{
			get
			{
				return GetEffector(FullBodyBipedEffector.RightFoot);
			}
		}

		public FBIKChain leftArmChain
		{
			get
			{
				return chain[1];
			}
		}

		public FBIKChain rightArmChain
		{
			get
			{
				return chain[2];
			}
		}

		public FBIKChain leftLegChain
		{
			get
			{
				return chain[3];
			}
		}

		public FBIKChain rightLegChain
		{
			get
			{
				return chain[4];
			}
		}

		public IKMappingLimb leftArmMapping
		{
			get
			{
				return limbMappings[0];
			}
		}

		public IKMappingLimb rightArmMapping
		{
			get
			{
				return limbMappings[1];
			}
		}

		public IKMappingLimb leftLegMapping
		{
			get
			{
				return limbMappings[2];
			}
		}

		public IKMappingLimb rightLegMapping
		{
			get
			{
				return limbMappings[3];
			}
		}

		public IKMappingBone headMapping
		{
			get
			{
				return boneMappings[0];
			}
		}

		public void SetChainWeights(FullBodyBipedChain c, float pull, float reach = 0f)
		{
			GetChain(c).pull = pull;
			GetChain(c).reach = reach;
		}

		public void SetEffectorWeights(FullBodyBipedEffector effector, float positionWeight, float rotationWeight)
		{
			GetEffector(effector).positionWeight = Mathf.Clamp(positionWeight, 0f, 1f);
			GetEffector(effector).rotationWeight = Mathf.Clamp(rotationWeight, 0f, 1f);
		}

		public FBIKChain GetChain(FullBodyBipedChain c)
		{
			switch (c)
			{
			case FullBodyBipedChain.LeftArm:
				return chain[1];
			case FullBodyBipedChain.RightArm:
				return chain[2];
			case FullBodyBipedChain.LeftLeg:
				return chain[3];
			case FullBodyBipedChain.RightLeg:
				return chain[4];
			default:
				return null;
			}
		}

		public FBIKChain GetChain(FullBodyBipedEffector effector)
		{
			switch (effector)
			{
			case FullBodyBipedEffector.Body:
				return chain[0];
			case FullBodyBipedEffector.LeftShoulder:
				return chain[1];
			case FullBodyBipedEffector.RightShoulder:
				return chain[2];
			case FullBodyBipedEffector.LeftThigh:
				return chain[3];
			case FullBodyBipedEffector.RightThigh:
				return chain[4];
			case FullBodyBipedEffector.LeftHand:
				return chain[1];
			case FullBodyBipedEffector.RightHand:
				return chain[2];
			case FullBodyBipedEffector.LeftFoot:
				return chain[3];
			case FullBodyBipedEffector.RightFoot:
				return chain[4];
			default:
				return null;
			}
		}

		public IKEffector GetEffector(FullBodyBipedEffector effector)
		{
			switch (effector)
			{
			case FullBodyBipedEffector.Body:
				return effectors[0];
			case FullBodyBipedEffector.LeftShoulder:
				return effectors[1];
			case FullBodyBipedEffector.RightShoulder:
				return effectors[2];
			case FullBodyBipedEffector.LeftThigh:
				return effectors[3];
			case FullBodyBipedEffector.RightThigh:
				return effectors[4];
			case FullBodyBipedEffector.LeftHand:
				return effectors[5];
			case FullBodyBipedEffector.RightHand:
				return effectors[6];
			case FullBodyBipedEffector.LeftFoot:
				return effectors[7];
			case FullBodyBipedEffector.RightFoot:
				return effectors[8];
			default:
				return null;
			}
		}

		public IKEffector GetEndEffector(FullBodyBipedChain c)
		{
			switch (c)
			{
			case FullBodyBipedChain.LeftArm:
				return effectors[5];
			case FullBodyBipedChain.RightArm:
				return effectors[6];
			case FullBodyBipedChain.LeftLeg:
				return effectors[7];
			case FullBodyBipedChain.RightLeg:
				return effectors[8];
			default:
				return null;
			}
		}

		public IKMappingLimb GetLimbMapping(FullBodyBipedChain chain)
		{
			switch (chain)
			{
			case FullBodyBipedChain.LeftArm:
				return limbMappings[0];
			case FullBodyBipedChain.RightArm:
				return limbMappings[1];
			case FullBodyBipedChain.LeftLeg:
				return limbMappings[2];
			case FullBodyBipedChain.RightLeg:
				return limbMappings[3];
			default:
				return null;
			}
		}

		public IKMappingLimb GetLimbMapping(FullBodyBipedEffector effector)
		{
			switch (effector)
			{
			case FullBodyBipedEffector.LeftShoulder:
				return limbMappings[0];
			case FullBodyBipedEffector.RightShoulder:
				return limbMappings[1];
			case FullBodyBipedEffector.LeftThigh:
				return limbMappings[2];
			case FullBodyBipedEffector.RightThigh:
				return limbMappings[3];
			case FullBodyBipedEffector.LeftHand:
				return limbMappings[0];
			case FullBodyBipedEffector.RightHand:
				return limbMappings[1];
			case FullBodyBipedEffector.LeftFoot:
				return limbMappings[2];
			case FullBodyBipedEffector.RightFoot:
				return limbMappings[3];
			default:
				return null;
			}
		}

		public IKMappingSpine GetSpineMapping()
		{
			return spineMapping;
		}

		public IKMappingBone GetHeadMapping()
		{
			return boneMappings[0];
		}

		public IKConstraintBend GetBendConstraint(FullBodyBipedChain limb)
		{
			switch (limb)
			{
			case FullBodyBipedChain.LeftArm:
				return chain[1].bendConstraint;
			case FullBodyBipedChain.RightArm:
				return chain[2].bendConstraint;
			case FullBodyBipedChain.LeftLeg:
				return chain[3].bendConstraint;
			case FullBodyBipedChain.RightLeg:
				return chain[4].bendConstraint;
			default:
				return null;
			}
		}

		public override bool IsValid(ref string message)
		{
			if (!base.IsValid(ref message))
			{
				return false;
			}
			if (rootNode == null)
			{
				message = "Root Node bone is null. FBBIK will not initiate.";
				return false;
			}
			if (chain.Length != 5 || chain[0].nodes.Length != 1 || chain[1].nodes.Length != 3 || chain[2].nodes.Length != 3 || chain[3].nodes.Length != 3 || chain[4].nodes.Length != 3 || effectors.Length != 9 || limbMappings.Length != 4)
			{
				message = "Invalid FBBIK setup. Please right-click on the component header and select 'Reinitiate'.";
				return false;
			}
			return true;
		}

		public void SetToReferences(BipedReferences references, Transform rootNode = null)
		{
			root = references.root;
			if (rootNode == null)
			{
				rootNode = DetectRootNodeBone(references);
			}
			this.rootNode = rootNode;
			if (chain == null || chain.Length != 5)
			{
				chain = new FBIKChain[5];
			}
			for (int i = 0; i < chain.Length; i++)
			{
				if (chain[i] == null)
				{
					chain[i] = new FBIKChain();
				}
			}
			chain[0].pin = 0f;
			chain[0].SetNodes(rootNode);
			chain[0].children = new int[4] { 1, 2, 3, 4 };
			chain[1].SetNodes(references.leftUpperArm, references.leftForearm, references.leftHand);
			chain[2].SetNodes(references.rightUpperArm, references.rightForearm, references.rightHand);
			chain[3].SetNodes(references.leftThigh, references.leftCalf, references.leftFoot);
			chain[4].SetNodes(references.rightThigh, references.rightCalf, references.rightFoot);
			if (effectors.Length != 9)
			{
				effectors = new IKEffector[9]
				{
					new IKEffector(),
					new IKEffector(),
					new IKEffector(),
					new IKEffector(),
					new IKEffector(),
					new IKEffector(),
					new IKEffector(),
					new IKEffector(),
					new IKEffector()
				};
			}
			effectors[0].bone = rootNode;
			effectors[0].childBones = new Transform[2] { references.leftThigh, references.rightThigh };
			effectors[1].bone = references.leftUpperArm;
			effectors[2].bone = references.rightUpperArm;
			effectors[3].bone = references.leftThigh;
			effectors[4].bone = references.rightThigh;
			effectors[5].bone = references.leftHand;
			effectors[6].bone = references.rightHand;
			effectors[7].bone = references.leftFoot;
			effectors[8].bone = references.rightFoot;
			effectors[5].planeBone1 = references.leftUpperArm;
			effectors[5].planeBone2 = references.rightUpperArm;
			effectors[5].planeBone3 = rootNode;
			effectors[6].planeBone1 = references.rightUpperArm;
			effectors[6].planeBone2 = references.leftUpperArm;
			effectors[6].planeBone3 = rootNode;
			effectors[7].planeBone1 = references.leftThigh;
			effectors[7].planeBone2 = references.rightThigh;
			effectors[7].planeBone3 = rootNode;
			effectors[8].planeBone1 = references.rightThigh;
			effectors[8].planeBone2 = references.leftThigh;
			effectors[8].planeBone3 = rootNode;
			chain[0].childConstraints = new FBIKChain.ChildConstraint[4]
			{
				new FBIKChain.ChildConstraint(references.leftUpperArm, references.rightThigh, 0f, 1f),
				new FBIKChain.ChildConstraint(references.rightUpperArm, references.leftThigh, 0f, 1f),
				new FBIKChain.ChildConstraint(references.leftUpperArm, references.rightUpperArm),
				new FBIKChain.ChildConstraint(references.leftThigh, references.rightThigh)
			};
			Transform[] array = new Transform[references.spine.Length + 1];
			array[0] = references.pelvis;
			for (int j = 0; j < references.spine.Length; j++)
			{
				array[j + 1] = references.spine[j];
			}
			if (spineMapping == null)
			{
				spineMapping = new IKMappingSpine();
				spineMapping.iterations = 3;
			}
			spineMapping.SetBones(array, references.leftUpperArm, references.rightUpperArm, references.leftThigh, references.rightThigh);
			int num = ((references.head != null) ? 1 : 0);
			if (boneMappings.Length != num)
			{
				boneMappings = new IKMappingBone[num];
				for (int k = 0; k < boneMappings.Length; k++)
				{
					boneMappings[k] = new IKMappingBone();
				}
				if (num == 1)
				{
					boneMappings[0].maintainRotationWeight = 0f;
				}
			}
			if (boneMappings.Length > 0)
			{
				boneMappings[0].bone = references.head;
			}
			if (limbMappings.Length != 4)
			{
				limbMappings = new IKMappingLimb[4]
				{
					new IKMappingLimb(),
					new IKMappingLimb(),
					new IKMappingLimb(),
					new IKMappingLimb()
				};
				limbMappings[2].maintainRotationWeight = 1f;
				limbMappings[3].maintainRotationWeight = 1f;
			}
			limbMappings[0].SetBones(references.leftUpperArm, references.leftForearm, references.leftHand, GetLeftClavicle(references));
			limbMappings[1].SetBones(references.rightUpperArm, references.rightForearm, references.rightHand, GetRightClavicle(references));
			limbMappings[2].SetBones(references.leftThigh, references.leftCalf, references.leftFoot);
			limbMappings[3].SetBones(references.rightThigh, references.rightCalf, references.rightFoot);
			if (Application.isPlaying)
			{
				Initiate(references.root);
			}
		}

		public static Transform DetectRootNodeBone(BipedReferences references)
		{
			if (!references.isFilled)
			{
				return null;
			}
			if (references.spine.Length < 1)
			{
				return null;
			}
			int num = references.spine.Length;
			if (num == 1)
			{
				return references.spine[0];
			}
			Vector3 vector = Vector3.Lerp(references.leftThigh.position, references.rightThigh.position, 0.5f);
			Vector3 vector2 = Vector3.Lerp(references.leftUpperArm.position, references.rightUpperArm.position, 0.5f);
			Vector3 onNormal = vector2 - vector;
			float magnitude = onNormal.magnitude;
			if (references.spine.Length < 2)
			{
				return references.spine[0];
			}
			int num2 = 0;
			for (int i = 1; i < num; i++)
			{
				Vector3 vector3 = references.spine[i].position - vector;
				Vector3 vector4 = Vector3.Project(vector3, onNormal);
				float num3 = Vector3.Dot(vector4.normalized, onNormal.normalized);
				if (num3 > 0f)
				{
					float num4 = vector4.magnitude / magnitude;
					if (num4 < 0.5f)
					{
						num2 = i;
					}
				}
			}
			return references.spine[num2];
		}

		public void SetLimbOrientations(BipedLimbOrientations o)
		{
			SetLimbOrientation(FullBodyBipedChain.LeftArm, o.leftArm);
			SetLimbOrientation(FullBodyBipedChain.RightArm, o.rightArm);
			SetLimbOrientation(FullBodyBipedChain.LeftLeg, o.leftLeg);
			SetLimbOrientation(FullBodyBipedChain.RightLeg, o.rightLeg);
		}

		private void SetLimbOrientation(FullBodyBipedChain chain, BipedLimbOrientations.LimbOrientation limbOrientation)
		{
			if (chain == FullBodyBipedChain.LeftArm || chain == FullBodyBipedChain.RightArm)
			{
				GetBendConstraint(chain).SetLimbOrientation(-limbOrientation.upperBoneForwardAxis, -limbOrientation.lowerBoneForwardAxis, -limbOrientation.lastBoneLeftAxis);
				GetLimbMapping(chain).SetLimbOrientation(-limbOrientation.upperBoneForwardAxis, -limbOrientation.lowerBoneForwardAxis);
			}
			else
			{
				GetBendConstraint(chain).SetLimbOrientation(limbOrientation.upperBoneForwardAxis, limbOrientation.lowerBoneForwardAxis, limbOrientation.lastBoneLeftAxis);
				GetLimbMapping(chain).SetLimbOrientation(limbOrientation.upperBoneForwardAxis, limbOrientation.lowerBoneForwardAxis);
			}
		}

		private static Transform GetLeftClavicle(BipedReferences references)
		{
			if (references.leftUpperArm == null)
			{
				return null;
			}
			if (!Contains(references.spine, references.leftUpperArm.parent))
			{
				return references.leftUpperArm.parent;
			}
			return null;
		}

		private static Transform GetRightClavicle(BipedReferences references)
		{
			if (references.rightUpperArm == null)
			{
				return null;
			}
			if (!Contains(references.spine, references.rightUpperArm.parent))
			{
				return references.rightUpperArm.parent;
			}
			return null;
		}

		private static bool Contains(Transform[] array, Transform transform)
		{
			foreach (Transform transform2 in array)
			{
				if (transform2 == transform)
				{
					return true;
				}
			}
			return false;
		}

		protected override void ReadPose()
		{
			for (int i = 0; i < effectors.Length; i++)
			{
				effectors[i].SetToTarget();
			}
			PullBody();
			float pushElasticity = Mathf.Clamp(1f - spineStiffness, 0f, 1f);
			chain[0].childConstraints[0].pushElasticity = pushElasticity;
			chain[0].childConstraints[1].pushElasticity = pushElasticity;
			base.ReadPose();
		}

		private void PullBody()
		{
			if (iterations >= 1 && (pullBodyVertical != 0f || pullBodyHorizontal != 0f))
			{
				Vector3 bodyOffset = GetBodyOffset();
				bodyEffector.positionOffset += V3Tools.ExtractVertical(bodyOffset, root.up, pullBodyVertical) + V3Tools.ExtractHorizontal(bodyOffset, root.up, pullBodyHorizontal);
			}
		}

		private Vector3 GetBodyOffset()
		{
			Vector3 vector = Vector3.zero + GetHandBodyPull(leftHandEffector, leftArmChain, Vector3.zero) * Mathf.Clamp(leftHandEffector.positionWeight, 0f, 1f);
			return vector + GetHandBodyPull(rightHandEffector, rightArmChain, vector) * Mathf.Clamp(rightHandEffector.positionWeight, 0f, 1f);
		}

		private Vector3 GetHandBodyPull(IKEffector effector, FBIKChain arm, Vector3 offset)
		{
			Vector3 vector = effector.position - (arm.nodes[0].transform.position + offset);
			float num = arm.nodes[0].length + arm.nodes[1].length;
			float magnitude = vector.magnitude;
			if (magnitude < num)
			{
				return Vector3.zero;
			}
			float num2 = magnitude - num;
			return vector / magnitude * num2;
		}

		protected override void ApplyBendConstraints()
		{
			if (iterations > 0)
			{
				chain[1].bendConstraint.rotationOffset = leftHandEffector.planeRotationOffset;
				chain[2].bendConstraint.rotationOffset = rightHandEffector.planeRotationOffset;
				chain[3].bendConstraint.rotationOffset = leftFootEffector.planeRotationOffset;
				chain[4].bendConstraint.rotationOffset = rightFootEffector.planeRotationOffset;
			}
			else
			{
				offset = Vector3.Lerp(effectors[0].positionOffset, effectors[0].position - (effectors[0].bone.position + effectors[0].positionOffset), effectors[0].positionWeight);
				for (int i = 0; i < 5; i++)
				{
					effectors[i].GetNode(this).solverPosition += offset;
				}
			}
			base.ApplyBendConstraints();
		}

		protected override void WritePose()
		{
			if (iterations == 0)
			{
				spineMapping.spineBones[0].position += offset;
			}
			base.WritePose();
		}
	}
}
