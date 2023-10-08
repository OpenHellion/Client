using System.Linq;
using OpenHellion;
using UnityEngine;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public static class SceneTriggerHelper
	{
		public static void InteractWithOverlappingTriggers(GameObject triggerGo, BaseSceneTrigger trigger,
			MyPlayer player)
		{
			BaseSceneTrigger[] components = triggerGo.GetComponents<BaseSceneTrigger>();
			foreach (BaseSceneTrigger baseSceneTrigger in components)
			{
				if (baseSceneTrigger != trigger)
				{
					baseSceneTrigger.Interact(player, interactWithOverlappingTriggers: false);
				}
			}
		}

		public static VesselObjectID GetID(this BaseSceneTrigger trigger)
		{
			string text = trigger.GetType().Name + "|";
			Transform transform = trigger.transform;
			SpaceObjectVessel componentInParent = transform.GetComponentInParent<SpaceObjectVessel>();
			if (componentInParent == null)
			{
				return null;
			}

			while (true)
			{
				transform = transform.transform.parent;
				if (transform != null)
				{
					StructureScene component = transform.GetComponent<StructureScene>();
					if (component != null)
					{
						text += component.GameName;
						break;
					}

					AsteroidScene component2 = transform.GetComponent<AsteroidScene>();
					if (component2 != null)
					{
						text += component2.GameName;
						break;
					}

					text = text + transform.gameObject.name + "|";
					continue;
				}

				text += "root";
				break;
			}

			return new VesselObjectID(componentInParent.GUID, Mathf.Abs(text.GetHashCode()));
		}

		public static bool OtherPlayerLockedToTrigger(this BaseSceneTrigger trigger, World state)
		{
			return state.Players.Values.FirstOrDefault((OtherPlayer m) => m.LockedToTrigger == trigger) != null;
		}
	}
}
