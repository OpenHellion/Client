using System;
using System.Collections.Generic;
using System.Linq;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.XR;

namespace RootMotion.Demos
{
	[RequireComponent(typeof(FullBodyBipedIK))]
	public class VRAimingController : MonoBehaviour
	{
		[Serializable]
		public struct Targets
		{
			public Transform leftHand;

			public Transform rightHand;

			public Transform bendGoalLeftArm;

			public Transform bendGoalRightArm;

			public BoneRotationOffset[] boneRotationOffsets;
		}

		[Serializable]
		public enum Handedness
		{
			Right = 0,
			Left = 1
		}

		[Serializable]
		public class BoneRotationOffset
		{
			public Transform transform;

			public Vector3 value;
		}

		[Header("Component References")]
		public VRAnimatorController animatorController;

		[Tooltip("Which weapon is the character holding at this time?")]
		public WeaponBase currentWeapon;

		[Header("Weights")]
		[Tooltip("The master weight of aiming.")]
		[Range(0f, 1f)]
		public float weight = 1f;

		[Tooltip("The weight of twisting the spine to better hold the weapons")]
		[Range(0f, 1f)]
		public float spineTwistWeight = 1f;

		[Header("Hands")]
		[Tooltip("Which hand holds the weapon?")]
		public Handedness handedness;

		[Tooltip("How far left/right to offset the weapons?")]
		public float sideOffset = 0.1f;

		[Tooltip("Various references and settings for left handed weapons.")]
		public Targets leftHandedTargets;

		[Tooltip("Various references and settings for right handed weapons.")]
		public Targets rightHandedTargets;

		[Header("Weapon Positioning")]
		[Tooltip("The Transform that rotates the weapon.")]
		public Transform weaponsPivot;

		[Tooltip("Child of weaponsPivot, parent of all weapons.")]
		public Transform weaponsAnchor;

		[Tooltip("Weapons will inherit motion from that Transform.")]
		public Transform pivotMotionTarget;

		[Tooltip("Speed of various position/rotation interpolations.")]
		public float lerpSpeed = 8f;

		[Tooltip("The smoothing speed of inheriting motion from the pivotMotionTarget.")]
		public float pivotMotionSmoothSpeed = 5f;

		[Tooltip("The weight of inheriting motion from the pivotMotionTarget,")]
		[Range(0f, 1f)]
		public float pivotMotionWeight = 0.5f;

		[Tooltip("The limit of up/down rotation for the weapons.")]
		[Range(0f, 90f)]
		public float aimVerticalLimit = 80f;

		[Tooltip("Local Z position of the weapons anchor when the weapon is locked to the camera (while holding RMB).")]
		public float aimZ = 0.05f;

		private FullBodyBipedIK ik;

		private float lastWeight;

		private Poser poserLeftHand;

		private Poser poserRightHand;

		private Vector3 pivotRelativePosition;

		private Vector3 weaponsPivotLocalPosition;

		private Vector3 defaultWeaponsAnchorLocalPosition;

		private Vector3 aimVel;

		private Vector3 aimRandom;

		private float x;

		private float y;

		private float aimWeight;

		private Vector3 cameraPosition;

		private Vector3 lastCharacterPosition;

		private Transform cam
		{
			get
			{
				return animatorController.cam;
			}
		}

		private Transform characterController
		{
			get
			{
				return animatorController.characterController;
			}
		}

		private Targets targets
		{
			get
			{
				if (handedness == Handedness.Right)
				{
					return rightHandedTargets;
				}
				return leftHandedTargets;
			}
		}

		private void Start()
		{
			ik = GetComponent<FullBodyBipedIK>();
			poserLeftHand = ik.references.leftHand.GetComponent<Poser>();
			poserRightHand = ik.references.rightHand.GetComponent<Poser>();
			IKSolverFullBodyBiped solver = ik.solver;
			solver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPostUpdate, new IKSolver.UpdateDelegate(AfterFBBIK));
			lastWeight = weight;
			SetHandedness(handedness);
			defaultWeaponsAnchorLocalPosition = weaponsAnchor.localPosition;
			weaponsPivotLocalPosition = weaponsPivot.localPosition;
			pivotRelativePosition = pivotMotionTarget.InverseTransformPoint(weaponsPivot.position);
			cameraPosition = TargetsCameraPosition();
			lastCharacterPosition = characterController.position;
		}

		private void LateUpdate()
		{
			cameraPosition += characterController.position - lastCharacterPosition;
			lastCharacterPosition = characterController.position;
			cameraPosition = Vector3.Lerp(cameraPosition, TargetsCameraPosition(), Time.deltaTime * lerpSpeed);
			if (weight <= 0f && lastWeight <= 0f)
			{
				return;
			}
			lastWeight = weight;
			float t = animatorController.velocity.magnitude * pivotMotionWeight;
			weaponsPivot.position = Vector3.Lerp(weaponsPivot.position, Vector3.Lerp(weaponsPivot.parent.TransformPoint(weaponsPivotLocalPosition), pivotMotionTarget.TransformPoint(pivotRelativePosition), t), Time.deltaTime * pivotMotionSmoothSpeed);
			if (Input.GetKeyDown(KeyCode.H))
			{
				SetHandedness((handedness == Handedness.Right) ? Handedness.Left : Handedness.Right);
			}
			defaultWeaponsAnchorLocalPosition.x = ((handedness != 0) ? (0f - sideOffset) : sideOffset);
			weaponsAnchor.localPosition = Vector3.Lerp(weaponsAnchor.localPosition, defaultWeaponsAnchorLocalPosition, Time.deltaTime * lerpSpeed);
			if (currentWeapon != null && Input.GetMouseButtonDown(0))
			{
				currentWeapon.Fire();
				weaponsAnchor.localPosition += currentWeapon.recoilDirection + UnityEngine.Random.insideUnitSphere * currentWeapon.recoilDirection.magnitude * UnityEngine.Random.value * currentWeapon.recoilRandom;
				aimVel.x -= currentWeapon.recoilAngleVertical + currentWeapon.recoilAngleVertical * UnityEngine.Random.value * currentWeapon.recoilRandom;
				float num = currentWeapon.recoilAngleHorizontal * UnityEngine.Random.value;
				if (UnityEngine.Random.value > 0.5f)
				{
					num = 0f - num;
				}
				aimVel.y += num + num * UnityEngine.Random.value * currentWeapon.recoilRandom;
			}
			BoneRotationOffset[] boneRotationOffsets = targets.boneRotationOffsets;
			foreach (BoneRotationOffset boneRotationOffset in boneRotationOffsets)
			{
				boneRotationOffset.transform.localRotation = Quaternion.Euler(boneRotationOffset.value * weight) * boneRotationOffset.transform.localRotation;
			}
			bool mouseButton = Input.GetMouseButton(1);
			float target = ((!mouseButton) ? 0f : 1f);
			aimWeight = Mathf.MoveTowards(aimWeight, target, Time.deltaTime * 3f);
			RotateWeapon(Input.GetAxis("Mouse X") * (1f - aimWeight), Input.GetAxis("Mouse Y") * (1f - aimWeight));
			if (mouseButton)
			{
				weaponsPivot.position = Vector3.Lerp(weaponsPivot.position, cameraPosition, aimWeight);
				weaponsAnchor.localPosition = Vector3.Lerp(weaponsAnchor.localPosition, new Vector3(0f, weaponsAnchor.localPosition.y, aimZ), aimWeight);
				weaponsPivot.rotation = Quaternion.Lerp(weaponsPivot.rotation, Quaternion.LookRotation(cam.forward), Time.deltaTime * lerpSpeed);
			}
			Vector3 vector = Vector3.Project(weaponsAnchor.position - TargetsCameraPosition(), cam.forward);
			if (Vector3.Dot(vector, cam.forward) < 0f)
			{
				weaponsAnchor.position -= vector;
			}
			ik.solver.leftHandEffector.position = targets.leftHand.position;
			ik.solver.rightHandEffector.position = targets.rightHand.position;
			ik.solver.leftHandEffector.positionWeight = weight;
			ik.solver.rightHandEffector.positionWeight = weight;
			ik.solver.leftArmChain.bendConstraint.bendGoal = targets.bendGoalLeftArm;
			ik.solver.rightArmChain.bendConstraint.bendGoal = targets.bendGoalRightArm;
			ik.solver.leftArmChain.bendConstraint.weight = weight;
			ik.solver.rightArmChain.bendConstraint.weight = weight;
			poserLeftHand.weight = weight;
			poserRightHand.weight = weight;
			animatorController.RotateCharacter(weaponsAnchor.forward, animatorController.maxViewAngle, weaponsPivot);
			TwistSpine();
		}

		private void RotateWeapon(float horAdd, float vertAdd)
		{
			Vector3 b = new Vector3(0f - vertAdd, horAdd, 0f);
			aimRandom = Vector3.Lerp(aimRandom, UnityEngine.Random.onUnitSphere, Time.deltaTime);
			b += aimRandom * 0.25f;
			aimVel = Vector3.Lerp(aimVel, b, Time.deltaTime * 20f);
			Vector3 forward = weaponsPivot.forward;
			forward.y = 0f;
			Quaternion quaternion = Quaternion.AngleAxis(aimVel.x, Quaternion.LookRotation(forward) * Vector3.right);
			Quaternion quaternion2 = Quaternion.AngleAxis(aimVel.y, Vector3.up);
			Vector3 vector = Vector3.RotateTowards(forward, quaternion * weaponsPivot.forward, aimVerticalLimit * ((float)Math.PI / 180f), 1f);
			weaponsPivot.rotation = Quaternion.LookRotation(quaternion2 * vector, Vector3.up);
		}

		private Vector3 TargetsCameraPosition()
		{
			// Get nodes.
			List<XRNodeState> nodeStates = new List<XRNodeState>();
			InputTracking.GetNodeStates(nodeStates);

			// Get position.
			nodeStates.FirstOrDefault(node => node.nodeType == XRNode.LeftEye).TryGetPosition(out Vector3 left);
			nodeStates.FirstOrDefault(node => node.nodeType == XRNode.RightEye).TryGetPosition(out Vector3 right);

			// Calculate difference.
			float num = (left - right).magnitude * 0.5f;
			Vector3 vector = Vector3.right * num;
			if (handedness == Handedness.Left)
			{
				vector = -vector;
			}
			return cam.position + cam.rotation * vector;
		}

		private void SetHandedness(Handedness h)
		{
			handedness = h;
			poserLeftHand.poseRoot = targets.leftHand;
			poserRightHand.poseRoot = targets.rightHand;
			poserLeftHand.AutoMapping();
			poserRightHand.AutoMapping();
		}

		private void TwistSpine()
		{
			if (!(spineTwistWeight <= 0f))
			{
				Vector3 forward = weaponsAnchor.forward;
				forward.y = 0f;
				Quaternion b = Quaternion.FromToRotation(base.transform.forward, forward);
				BoneRotationOffset[] boneRotationOffsets = targets.boneRotationOffsets;
				foreach (BoneRotationOffset boneRotationOffset in boneRotationOffsets)
				{
					boneRotationOffset.transform.rotation = Quaternion.Lerp(Quaternion.identity, b, 1f / (float)targets.boneRotationOffsets.Length * spineTwistWeight) * boneRotationOffset.transform.rotation;
				}
			}
		}

		private void AfterFBBIK()
		{
			if (!(weight <= 0f))
			{
				ik.references.leftHand.rotation = Quaternion.Lerp(ik.references.leftHand.rotation, targets.leftHand.rotation, weight);
				ik.references.rightHand.rotation = Quaternion.Lerp(ik.references.rightHand.rotation, targets.rightHand.rotation, weight);
			}
		}

		private void OnDestroy()
		{
			if (ik != null)
			{
				IKSolverFullBodyBiped solver = ik.solver;
				solver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPostUpdate, new IKSolver.UpdateDelegate(AfterFBBIK));
			}
		}

		private float ClampAngle(float angle, float min, float max)
		{
			if (angle < -360f)
			{
				angle += 360f;
			}
			if (angle > 360f)
			{
				angle -= 360f;
			}
			return Mathf.Clamp(angle, min, max);
		}
	}
}
