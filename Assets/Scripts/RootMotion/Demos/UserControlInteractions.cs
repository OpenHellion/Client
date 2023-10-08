using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	public class UserControlInteractions : UserControlThirdPerson
	{
		[SerializeField] private CharacterThirdPerson character;

		[SerializeField] private InteractionSystem interactionSystem;

		[SerializeField] private bool disableInputInInteraction = true;

		public float enableInputAtProgress = 0.8f;

		protected override void Update()
		{
			if (disableInputInInteraction && interactionSystem != null &&
			    (interactionSystem.inInteraction || interactionSystem.IsPaused()))
			{
				float minActiveProgress = interactionSystem.GetMinActiveProgress();
				if (minActiveProgress > 0f && minActiveProgress < enableInputAtProgress)
				{
					state.move = Vector3.zero;
					state.jump = false;
					return;
				}
			}

			base.Update();
		}

		private void OnGUI()
		{
			if (!character.onGround)
			{
				return;
			}

			if (interactionSystem.IsPaused() && interactionSystem.IsInSync())
			{
				GUILayout.Label("Press E to resume interaction");
				if (Input.GetKey(KeyCode.E))
				{
					interactionSystem.ResumeAll();
				}

				return;
			}

			int closestTriggerIndex = interactionSystem.GetClosestTriggerIndex();
			if (closestTriggerIndex != -1 && interactionSystem.TriggerEffectorsReady(closestTriggerIndex))
			{
				GUILayout.Label("Press E to start interaction");
				if (Input.GetKey(KeyCode.E))
				{
					interactionSystem.TriggerInteraction(closestTriggerIndex, false);
				}
			}
		}
	}
}
