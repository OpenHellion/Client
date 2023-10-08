using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	[RequireComponent(typeof(AimIK))]
	[RequireComponent(typeof(FullBodyBipedIK))]
	public class AnimatorController3rdPersonIK : AnimatorController3rdPerson
	{
		[SerializeField] private bool useIK = true;

		[Range(0f, 1f)] public float headLookWeight = 1f;

		public Vector3 gunHoldOffset;

		public Vector3 leftHandOffset;

		private AimIK aim;

		private FullBodyBipedIK ik;

		private Vector3 headLookAxis;

		private Vector3 leftHandPosRelToRightHand;

		private Quaternion leftHandRotRelToRightHand;

		private Vector3 aimTarget;

		private void OnGUI()
		{
			GUILayout.Label("Press F to switch Final IK on/off");
		}

		protected override void Start()
		{
			base.Start();
			aim = GetComponent<AimIK>();
			ik = GetComponent<FullBodyBipedIK>();
			aim.enabled = false;
			ik.enabled = false;
			headLookAxis = ik.references.head.InverseTransformVector(ik.references.root.forward);
			animator.SetLayerWeight(1, 1f);
		}

		public override void Move(Vector3 moveInput, bool isMoving, Vector3 faceDirection, Vector3 aimTarget)
		{
			base.Move(moveInput, isMoving, faceDirection, aimTarget);
			this.aimTarget = aimTarget;
			if (Input.GetKeyDown(KeyCode.F))
			{
				useIK = !useIK;
			}

			if (useIK)
			{
				Read();
				AimIK();
				FBBIK();
				HeadLookAt(aimTarget);
			}
		}

		private void Read()
		{
			leftHandPosRelToRightHand = ik.references.rightHand.InverseTransformPoint(ik.references.leftHand.position);
			leftHandRotRelToRightHand =
				Quaternion.Inverse(ik.references.rightHand.rotation) * ik.references.leftHand.rotation;
		}

		private void AimIK()
		{
			aim.solver.IKPosition = aimTarget;
			aim.solver.Update();
		}

		private void FBBIK()
		{
			Quaternion rotation = ik.references.rightHand.rotation;
			Vector3 vector = ik.references.rightHand.TransformPoint(leftHandPosRelToRightHand);
			ik.solver.leftHandEffector.positionOffset += vector - ik.references.leftHand.position;
			Vector3 vector2 = ik.references.rightHand.rotation * gunHoldOffset;
			ik.solver.rightHandEffector.positionOffset += vector2;
			ik.solver.leftHandEffector.positionOffset += vector2 + ik.references.rightHand.rotation * leftHandOffset;
			ik.solver.Update();
			ik.references.rightHand.rotation = rotation;
			ik.references.leftHand.rotation = rotation * leftHandRotRelToRightHand;
		}

		private void HeadLookAt(Vector3 lookAtTarget)
		{
			Quaternion b = Quaternion.FromToRotation(ik.references.head.rotation * headLookAxis,
				lookAtTarget - ik.references.head.position);
			ik.references.head.rotation =
				Quaternion.Lerp(Quaternion.identity, b, headLookWeight) * ik.references.head.rotation;
		}
	}
}
