using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZeroGravity;
using ZeroGravity.Data;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.UI;

namespace ZeroGravity.UI
{
	public class SpriteManager : MonoBehaviour
	{
		[Serializable]
		public class MachineryParts
		{
			public MachineryPartType Key;

			public Sprite Sprite;
		}

		[Serializable]
		public class GenericItems
		{
			public GenericItemSubType Key;

			public Sprite Sprite;
		}

		[Serializable]
		public class SlotTypes
		{
			public InventorySlot.Group Key;

			public Sprite Sprite;
		}

		[Serializable]
		public class AttachPoints
		{
			public Localization.StandardInteractionTip Key;

			public Sprite Sprite;
		}

		[Serializable]
		public class AllItems
		{
			public ItemType Key;

			public Sprite Sprite;
		}

		[Serializable]
		public class SpaceObjects
		{
			public GameScenes.SceneID Key;

			public Sprite Sprite;
		}

		[Serializable]
		public class RadarObjects
		{
			public SpaceObjectType Key;

			public Sprite Sprite;
		}

		[Serializable]
		public class ResourceObject
		{
			public ResourceType Key;

			public Sprite Sprite;
		}

		[Serializable]
		public class SceneObject
		{
			public SpawnSetupType Key;

			public Texture2D Texture;
		}

		[Serializable]
		public class NotificationObject
		{
			public CanvasUI.NotificationType Key;

			public Sprite Sprite;
		}

		[Serializable]
		public class WeaponModObject
		{
			public WeaponMod.FireMode Key;

			public Sprite Sprite;
		}

		public Sprite DefaultItemSlot;

		public Sprite DefaultMachineryPart;

		[SerializeField]
		private List<MachineryParts> _MachineryPartsSprites;

		[HideInInspector]
		public Dictionary<MachineryPartType, Sprite> MachineryPartsSprites;

		[Space(30f)]
		public Sprite DefaultGenericItemSprite;

		[SerializeField]
		private List<GenericItems> _GenericItemsSprites;

		[HideInInspector]
		public Dictionary<GenericItemSubType, Sprite> GenericItemsSprites;

		[Space(30f)]
		public Sprite DefaultInventorySlotSprite;

		[SerializeField]
		private List<SlotTypes> _InventorySlotSprites;

		[HideInInspector]
		public Dictionary<InventorySlot.Group, Sprite> InventorySlotSprites;

		[Space(30f)]
		public Sprite DefaultAttachPointSprite;

		[SerializeField]
		private List<AttachPoints> _AttachPointSprites;

		[HideInInspector]
		public Dictionary<Localization.StandardInteractionTip, Sprite> AttachPointSprites;

		[Space(30f)]
		public Sprite DefaultItemSprite;

		[SerializeField]
		private List<AllItems> _AllItemsSprites;

		[HideInInspector]
		public Dictionary<ItemType, Sprite> AllItemsSprites;

		[Space(30f)]
		public Sprite DefaultSpaceObject;

		public Sprite MainStationSprite;

		public Sprite MainOutpostSprite;

		public Sprite CustomOrbitSprite;

		[SerializeField]
		private List<SpaceObjects> _SpaceObjectSprites;

		[HideInInspector]
		public Dictionary<GameScenes.SceneID, Sprite> SpaceObjectSprites;

		[Space(30f)]
		public Sprite DefaultRadarObject;

		[SerializeField]
		private List<RadarObjects> _RadarObjectsSprites;

		[HideInInspector]
		public Dictionary<SpaceObjectType, Sprite> RadarObjectsSprites;

		[Space(30f)]
		public Sprite DefaultResourceObject;

		[SerializeField]
		private List<ResourceObject> _ResourceObjectsSprites;

		[HideInInspector]
		public Dictionary<ResourceType, Sprite> ResourceObjectsSprites;

		[Space(30f)]
		public Texture2D DefaultSceneTexture;

		public Texture2D InviteTexture;

		public Texture2D NewGameTexture;

		[SerializeField]
		private List<SceneObject> _SceneObjectTextures;

		[HideInInspector]
		public Dictionary<SpawnSetupType, Texture2D> SceneObjectTextures;

		[Space(30f)]
		public Sprite DefaultNotificationSprite;

		[SerializeField]
		private List<NotificationObject> _NotificationObjectsSprites;

		[HideInInspector]
		public Dictionary<CanvasUI.NotificationType, Sprite> NotificationObjectsSprites;

		[Space(30f)]
		public Sprite DefaultWeaponMod;

		[SerializeField]
		private List<WeaponModObject> _WeaponModSprites;

		[HideInInspector]
		public Dictionary<WeaponMod.FireMode, Sprite> WeaponModSprites;

		private void Start()
		{
			SpaceObjectSprites = _SpaceObjectSprites.ToDictionary((SpaceObjects x) => x.Key, (SpaceObjects y) => y.Sprite);
			MachineryPartsSprites = _MachineryPartsSprites.ToDictionary((MachineryParts x) => x.Key, (MachineryParts y) => y.Sprite);
			GenericItemsSprites = _GenericItemsSprites.ToDictionary((GenericItems x) => x.Key, (GenericItems y) => y.Sprite);
			AllItemsSprites = _AllItemsSprites.ToDictionary((AllItems x) => x.Key, (AllItems y) => y.Sprite);
			RadarObjectsSprites = _RadarObjectsSprites.ToDictionary((RadarObjects x) => x.Key, (RadarObjects y) => y.Sprite);
			ResourceObjectsSprites = _ResourceObjectsSprites.ToDictionary((ResourceObject x) => x.Key, (ResourceObject y) => y.Sprite);
			SceneObjectTextures = _SceneObjectTextures.ToDictionary((SceneObject x) => x.Key, (SceneObject y) => y.Texture);
			InventorySlotSprites = _InventorySlotSprites.ToDictionary((SlotTypes x) => x.Key, (SlotTypes y) => y.Sprite);
			NotificationObjectsSprites = _NotificationObjectsSprites.ToDictionary((NotificationObject x) => x.Key, (NotificationObject y) => y.Sprite);
			AttachPointSprites = _AttachPointSprites.ToDictionary((AttachPoints x) => x.Key, (AttachPoints y) => y.Sprite);
			WeaponModSprites = _WeaponModSprites.ToDictionary((WeaponModObject x) => x.Key, (WeaponModObject y) => y.Sprite);
		}

		public Sprite GetSprite(Item item)
		{
			if (item.Type == ItemType.GenericItem)
			{
				return GetSprite((item as GenericItem).SubType);
			}
			if (item.Type == ItemType.MachineryPart)
			{
				return GetSprite((item as MachineryPart).PartType);
			}
			return GetSprite(item.Type);
		}

		public Sprite GetSprite(ItemCompoundType item)
		{
			if (item.Type == ItemType.GenericItem)
			{
				return GetSprite(item.SubType);
			}
			if (item.Type == ItemType.MachineryPart)
			{
				return GetSprite(item.PartType);
			}
			return GetSprite(item.Type);
		}

		public Sprite GetSprite(SpaceObjectVessel vessel, bool checkDocked = false)
		{
			SpaceObjectVessel mainVessel = vessel.MainVessel;
			if (!checkDocked || !mainVessel.IsOutpostOrStation)
			{
				if (SpaceObjectSprites.TryGetValue(vessel.SceneID, out var value))
				{
					return value;
				}
				return DefaultSpaceObject;
			}
			if (mainVessel.IsStation)
			{
				return MainStationSprite;
			}
			return MainOutpostSprite;
		}

		public Sprite GetSprite(GameScenes.SceneID module)
		{
			if (SpaceObjectSprites.TryGetValue(module, out var value))
			{
				return value;
			}
			return DefaultSpaceObject;
		}

		public Sprite GetSprite(MachineryPartType part)
		{
			if (MachineryPartsSprites.TryGetValue(part, out var value))
			{
				return value;
			}
			return DefaultMachineryPart;
		}

		public Sprite GetSprite(GenericItemSubType generic)
		{
			if (GenericItemsSprites.TryGetValue(generic, out var value))
			{
				return value;
			}
			return DefaultGenericItemSprite;
		}

		public Sprite GetSprite(InventorySlot.Group slot)
		{
			if (InventorySlotSprites.TryGetValue(slot, out var value))
			{
				return value;
			}
			return DefaultInventorySlotSprite;
		}

		public Sprite GetSprite(ItemType item)
		{
			if (AllItemsSprites.TryGetValue(item, out var value))
			{
				return value;
			}
			return DefaultItemSprite;
		}

		public Sprite GetSprite(Localization.StandardInteractionTip tip)
		{
			if (AttachPointSprites.TryGetValue(tip, out var value))
			{
				return value;
			}
			return DefaultAttachPointSprite;
		}

		public Sprite GetSprite(SpaceObjectType spaceObject)
		{
			if (RadarObjectsSprites.TryGetValue(spaceObject, out var value))
			{
				return value;
			}
			return DefaultRadarObject;
		}

		public Sprite GetSprite(ResourceType resourceType)
		{
			if (ResourceObjectsSprites.TryGetValue(resourceType, out var value))
			{
				return value;
			}
			return DefaultResourceObject;
		}

		public Texture2D GetTexture(SpawnSetupType tip)
		{
			if (SceneObjectTextures.TryGetValue(tip, out var value))
			{
				return value;
			}
			return DefaultSceneTexture;
		}

		public Sprite GetSprite(CanvasUI.NotificationType notificationType)
		{
			if (NotificationObjectsSprites.TryGetValue(notificationType, out var value))
			{
				return value;
			}
			return DefaultResourceObject;
		}

		public Sprite GetSprite(WeaponMod.FireMode fireMod)
		{
			if (WeaponModSprites.TryGetValue(fireMod, out var value))
			{
				return value;
			}
			return DefaultWeaponMod;
		}
	}
}
