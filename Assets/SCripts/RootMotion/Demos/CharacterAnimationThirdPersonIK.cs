using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	[RequireComponent(typeof(FullBodyBipedIK))]
	public class CharacterAnimationThirdPersonIK : CharacterAnimationThirdPerson
	{
		private FullBodyBipedIK ik;

		protected override void Start()
		{
			base.Start();
			ik = GetComponent<FullBodyBipedIK>();
		}

		protected override void LateUpdate()
		{
			base.LateUpdate();
			if (!(Vector3.Angle(base.transform.up, Vector3.up) <= 0.01f))
			{
				Quaternion rotation = Quaternion.FromToRotation(base.transform.up, Vector3.up);
				RotateEffector(ik.solver.bodyEffector, rotation, 0.1f);
				RotateEffector(ik.solver.leftShoulderEffector, rotation, 0.2f);
				RotateEffector(ik.solver.rightShoulderEffector, rotation, 0.2f);
				RotateEffector(ik.solver.leftHandEffector, rotation, 0.1f);
				RotateEffector(ik.solver.rightHandEffector, rotation, 0.1f);
			}
		}

		private void RotateEffector(IKEffector effector, Quaternion rotation, float mlp)
		{
			Vector3 vector = effector.bone.position - base.transform.position;
			Vector3 vector2 = rotation * vector;
			Vector3 vector3 = vector2 - vector;
			effector.positionOffset += vector3 * mlp;
		}
	}
}
