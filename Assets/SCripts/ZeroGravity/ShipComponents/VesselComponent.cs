using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity.ShipComponents
{
	public abstract class VesselComponent : MonoBehaviour, ISceneObject
	{
		[CompilerGenerated]
		private sealed class _003CGetScopeMultiplier_003Ec__AnonStorey0
		{
			internal MachineryPartSlotScope scope;

			internal bool _003C_003Em__0(SceneMachineryPartSlot m)
			{
				return m.Scope == scope;
			}
		}

		[SerializeField]
		private int _inSceneID;

		[Space(5f)]
		public SceneMachineryPartSlot[] MachineryPartSlots = new SceneMachineryPartSlot[0];

		protected SpaceObjectVessel _ParentVessel;

		public int InSceneID
		{
			get
			{
				return _inSceneID;
			}
			set
			{
				_inSceneID = value;
			}
		}

		public abstract VesselComponentType ComponentType { get; }

		public SpaceObjectVessel ParentVessel
		{
			get
			{
				return _ParentVessel;
			}
		}

		public virtual SystemAuxData GetAuxData()
		{
			return null;
		}

		public virtual IAuxDetails GetAuxDetails()
		{
			return null;
		}

		public float GetScopeMultiplier(MachineryPartSlotScope scope)
		{
			int slots;
			int parts;
			int workingParts;
			return GetScopeMultiplier(scope, out slots, out parts, out workingParts);
		}

		public float GetScopeMultiplier(MachineryPartSlotScope scope, out int slots, out int parts, out int workingParts)
		{
			_003CGetScopeMultiplier_003Ec__AnonStorey0 _003CGetScopeMultiplier_003Ec__AnonStorey = new _003CGetScopeMultiplier_003Ec__AnonStorey0();
			_003CGetScopeMultiplier_003Ec__AnonStorey.scope = scope;
			float num = 1f;
			parts = 0;
			workingParts = 0;
			slots = 0;
			foreach (SceneMachineryPartSlot item in MachineryPartSlots.Where(_003CGetScopeMultiplier_003Ec__AnonStorey._003C_003Em__0))
			{
				slots++;
				MachineryPart componentInChildren = item.GetComponentInChildren<MachineryPart>();
				if (componentInChildren != null)
				{
					num *= componentInChildren.TierMultiplier;
					parts++;
					if (componentInChildren.Health > float.Epsilon)
					{
						workingParts++;
					}
				}
			}
			return num;
		}

		protected virtual void Start()
		{
			SceneMachineryPartSlot[] machineryPartSlots = MachineryPartSlots;
			foreach (SceneMachineryPartSlot sceneMachineryPartSlot in machineryPartSlots)
			{
				sceneMachineryPartSlot.ParentVesselComponent = this;
			}
		}

		public virtual void MachineryPartAttached(SceneMachineryPartSlot slot)
		{
		}

		public virtual void MachineryPartDetached(SceneMachineryPartSlot slot)
		{
		}
	}
}
