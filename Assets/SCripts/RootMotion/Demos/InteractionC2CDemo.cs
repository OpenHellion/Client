using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	public class InteractionC2CDemo : MonoBehaviour
	{
		public InteractionSystem character1;

		public InteractionSystem character2;

		public InteractionObject handShake;

		private void OnGUI()
		{
			if (GUILayout.Button("Shake Hands"))
			{
				character1.StartInteraction(FullBodyBipedEffector.RightHand, handShake, true);
				character2.StartInteraction(FullBodyBipedEffector.RightHand, handShake, true);
			}
		}

		private void LateUpdate()
		{
			Vector3 position = Vector3.Lerp(character1.ik.solver.rightHandEffector.bone.position, character2.ik.solver.rightHandEffector.bone.position, 0.5f);
			handShake.transform.position = position;
		}
	}
}
