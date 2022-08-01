using UnityEngine;

namespace RootMotion.Demos
{
	public class PickUpSphere : PickUp2Handed
	{
		protected override void RotatePivot()
		{
			Vector3 vector = Vector3.Lerp(interactionSystem.ik.solver.leftHandEffector.bone.position, interactionSystem.ik.solver.rightHandEffector.bone.position, 0.5f);
			Vector3 forward = obj.transform.position - vector;
			pivot.rotation = Quaternion.LookRotation(forward);
		}
	}
}
