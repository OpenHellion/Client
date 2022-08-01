using UnityEngine;

namespace RootMotion.Demos
{
	public class PickUpBox : PickUp2Handed
	{
		protected override void RotatePivot()
		{
			Vector3 normalized = (pivot.position - interactionSystem.transform.position).normalized;
			normalized.y = 0f;
			Vector3 v = obj.transform.InverseTransformDirection(normalized);
			Vector3 axis = QuaTools.GetAxis(v);
			Vector3 axis2 = QuaTools.GetAxis(obj.transform.InverseTransformDirection(interactionSystem.transform.up));
			pivot.localRotation = Quaternion.LookRotation(axis, axis2);
		}
	}
}
