using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	public class InteractionDemo : MonoBehaviour
	{
		public InteractionSystem interactionSystem;

		public bool interrupt;

		public InteractionObject ball;

		public InteractionObject benchMain;

		public InteractionObject benchHands;

		public InteractionObject button;

		public InteractionObject cigarette;

		public InteractionObject door;

		private bool isSitting;

		private void OnGUI()
		{
			interrupt = GUILayout.Toggle(interrupt, "Interrupt");
			if (isSitting)
			{
				if (!interactionSystem.inInteraction && GUILayout.Button("Stand Up"))
				{
					interactionSystem.ResumeAll();
					isSitting = false;
				}

				return;
			}

			if (GUILayout.Button("Pick Up Ball"))
			{
				interactionSystem.StartInteraction(FullBodyBipedEffector.RightHand, ball, interrupt);
			}

			if (GUILayout.Button("Button Left Hand"))
			{
				interactionSystem.StartInteraction(FullBodyBipedEffector.LeftHand, button, interrupt);
			}

			if (GUILayout.Button("Button Right Hand"))
			{
				interactionSystem.StartInteraction(FullBodyBipedEffector.RightHand, button, interrupt);
			}

			if (GUILayout.Button("Put Out Cigarette"))
			{
				interactionSystem.StartInteraction(FullBodyBipedEffector.RightFoot, cigarette, interrupt);
			}

			if (GUILayout.Button("Open Door"))
			{
				interactionSystem.StartInteraction(FullBodyBipedEffector.LeftHand, door, interrupt);
			}

			if (!interactionSystem.inInteraction && GUILayout.Button("Sit Down"))
			{
				interactionSystem.StartInteraction(FullBodyBipedEffector.Body, benchMain, interrupt);
				interactionSystem.StartInteraction(FullBodyBipedEffector.LeftThigh, benchMain, interrupt);
				interactionSystem.StartInteraction(FullBodyBipedEffector.RightThigh, benchMain, interrupt);
				interactionSystem.StartInteraction(FullBodyBipedEffector.LeftFoot, benchMain, interrupt);
				interactionSystem.StartInteraction(FullBodyBipedEffector.LeftHand, benchHands, interrupt);
				interactionSystem.StartInteraction(FullBodyBipedEffector.RightHand, benchHands, interrupt);
				isSitting = true;
			}
		}
	}
}
