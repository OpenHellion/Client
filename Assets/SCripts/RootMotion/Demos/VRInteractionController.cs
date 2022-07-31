using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	[RequireComponent(typeof(InteractionSystem))]
	public class VRInteractionController : MonoBehaviour
	{
		[Tooltip("How long do we need to stare at triggers?")]
		[Range(0f, 10f)]
		public float triggerTime = 1f;

		private float timer;

		public InteractionSystem interactionSystem { get; private set; }

		public float triggerProgress
		{
			get
			{
				if (triggerTime <= 0f)
				{
					return 0f;
				}
				return timer / triggerTime;
			}
		}

		public InteractionTrigger currentTrigger { get; private set; }

		private void Start()
		{
			interactionSystem = GetComponent<InteractionSystem>();
		}

		private void LateUpdate()
		{
			int closestTriggerIndex = interactionSystem.GetClosestTriggerIndex();
			if (CanTrigger(closestTriggerIndex))
			{
				timer += Time.deltaTime;
				currentTrigger = interactionSystem.triggersInRange[closestTriggerIndex];
				if (timer >= triggerTime)
				{
					interactionSystem.TriggerInteraction(closestTriggerIndex, false);
					timer = 0f;
				}
			}
			else
			{
				timer = 0f;
				currentTrigger = null;
			}
		}

		private bool CanTrigger(int index)
		{
			if (index == -1)
			{
				return false;
			}
			if (!interactionSystem.TriggerEffectorsReady(index))
			{
				return false;
			}
			return true;
		}
	}
}
