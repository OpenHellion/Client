using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	public class FPSAiming : MonoBehaviour
	{
		[Range(0f, 1f)] public float aimWeight = 1f;

		[Range(0f, 1f)] public float sightWeight = 1f;

		[Range(0f, 180f)] public float maxAngle = 80f;

		[SerializeField] private bool animatePhysics;

		[SerializeField] private Transform gun;

		[SerializeField] private Transform gunTarget;

		[SerializeField] private FullBodyBipedIK ik;

		[SerializeField] private AimIK gunAim;

		[SerializeField] private CameraControllerFPS cam;

		private Vector3 gunTargetDefaultLocalPosition;

		private Quaternion gunTargetDefaultLocalRotation;

		private Vector3 camDefaultLocalPosition;

		private Vector3 camRelativeToGunTarget;

		private bool updateFrame;

		private void Start()
		{
			gunTargetDefaultLocalPosition = gunTarget.localPosition;
			gunTargetDefaultLocalRotation = gunTarget.localRotation;
			camDefaultLocalPosition = cam.transform.localPosition;
			cam.enabled = false;
			gunAim.enabled = false;
			ik.enabled = false;
		}

		private void FixedUpdate()
		{
			updateFrame = true;
		}

		private void LateUpdate()
		{
			if (!animatePhysics)
			{
				updateFrame = true;
			}

			if (updateFrame)
			{
				updateFrame = false;
				cam.transform.localPosition = camDefaultLocalPosition;
				camRelativeToGunTarget = gunTarget.InverseTransformPoint(cam.transform.position);
				cam.LateUpdate();
				RotateCharacter();
				ik.solver.leftHandEffector.positionWeight =
					((!(aimWeight > 0f) || !(sightWeight > 0f)) ? 0f : (aimWeight * sightWeight));
				ik.solver.rightHandEffector.positionWeight = ik.solver.leftHandEffector.positionWeight;
				Aiming();
				LookDownTheSight();
			}
		}

		private void Aiming()
		{
			if (!(aimWeight <= 0f))
			{
				Quaternion rotation = cam.transform.rotation;
				gunAim.solver.IKPosition = cam.transform.position + cam.transform.forward * 10f;
				gunAim.solver.IKPositionWeight = aimWeight;
				gunAim.solver.Update();
				cam.transform.rotation = rotation;
			}
		}

		private void LookDownTheSight()
		{
			float num = aimWeight * sightWeight;
			if (!(num <= 0f))
			{
				gunTarget.position = Vector3.Lerp(gun.position,
					gunTarget.parent.TransformPoint(gunTargetDefaultLocalPosition), num);
				gunTarget.rotation = Quaternion.Lerp(gun.rotation,
					gunTarget.parent.rotation * gunTargetDefaultLocalRotation, num);
				Vector3 position = gun.InverseTransformPoint(ik.solver.leftHandEffector.bone.position);
				Vector3 position2 = gun.InverseTransformPoint(ik.solver.rightHandEffector.bone.position);
				Quaternion quaternion = Quaternion.Inverse(gun.rotation) * ik.solver.leftHandEffector.bone.rotation;
				Quaternion quaternion2 = Quaternion.Inverse(gun.rotation) * ik.solver.rightHandEffector.bone.rotation;
				ik.solver.leftHandEffector.position = gunTarget.TransformPoint(position);
				ik.solver.rightHandEffector.position = gunTarget.TransformPoint(position2);
				ik.solver.headMapping.maintainRotationWeight = 1f;
				ik.solver.Update();
				ik.references.leftHand.rotation = gunTarget.rotation * quaternion;
				ik.references.rightHand.rotation = gunTarget.rotation * quaternion2;
				cam.transform.position = Vector3.Lerp(cam.transform.position,
					gun.transform.TransformPoint(camRelativeToGunTarget), num);
			}
		}

		private void RotateCharacter()
		{
			if (maxAngle >= 180f)
			{
				return;
			}

			if (maxAngle <= 0f)
			{
				base.transform.rotation =
					Quaternion.LookRotation(new Vector3(cam.transform.forward.x, 0f, cam.transform.forward.z));
				return;
			}

			Vector3 vector = base.transform.InverseTransformDirection(cam.transform.forward);
			float num = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
			if (Mathf.Abs(num) > Mathf.Abs(maxAngle))
			{
				float angle = num - maxAngle;
				if (num < 0f)
				{
					angle = num + maxAngle;
				}

				base.transform.rotation = Quaternion.AngleAxis(angle, base.transform.up) * base.transform.rotation;
			}
		}
	}
}
