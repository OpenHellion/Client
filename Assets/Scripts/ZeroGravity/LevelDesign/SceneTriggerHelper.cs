using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public static class SceneTriggerHelper
	{
		[CompilerGenerated]
		private sealed class _003COtherPlayerLockedToTrigger_003Ec__AnonStorey0
		{
			internal BaseSceneTrigger trigger;

			internal bool _003C_003Em__0(OtherPlayer m)
			{
				return m.LockedToTrigger == trigger;
			}
		}

		public static void InteractWithOverlappingTriggers(GameObject triggerGo, BaseSceneTrigger trigger, MyPlayer player)
		{
			BaseSceneTrigger[] components = triggerGo.GetComponents<BaseSceneTrigger>();
			foreach (BaseSceneTrigger baseSceneTrigger in components)
			{
				if (baseSceneTrigger != trigger)
				{
					baseSceneTrigger.Interact(player, false);
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
			return new VesselObjectID(componentInParent.GUID, System.Math.Abs(text.GetHashCode()));
		}

		public static bool OtherPlayerLockedToTrigger(this BaseSceneTrigger trigger)
		{
			_003COtherPlayerLockedToTrigger_003Ec__AnonStorey0 _003COtherPlayerLockedToTrigger_003Ec__AnonStorey = new _003COtherPlayerLockedToTrigger_003Ec__AnonStorey0();
			_003COtherPlayerLockedToTrigger_003Ec__AnonStorey.trigger = trigger;
			return Client.Instance.Players.Values.FirstOrDefault(_003COtherPlayerLockedToTrigger_003Ec__AnonStorey._003C_003Em__0) != null;
		}
	}
}
