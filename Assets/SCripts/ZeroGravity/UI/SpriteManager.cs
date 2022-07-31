using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Network;
using ZeroGravity.Objects;

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

		public Dictionary<MachineryPartType, Sprite> MachineryPartsSprites;

		[Space(30f)]
		public Sprite DefaultGenericItemSprite;

		[SerializeField]
		private List<GenericItems> _GenericItemsSprites;

		public Dictionary<GenericItemSubType, Sprite> GenericItemsSprites;

		[Space(30f)]
		public Sprite DefaultInventorySlotSprite;

		[SerializeField]
		private List<SlotTypes> _InventorySlotSprites;

		public Dictionary<InventorySlot.Group, Sprite> InventorySlotSprites;

		[Space(30f)]
		public Sprite DefaultAttachPointSprite;

		[SerializeField]
		private List<AttachPoints> _AttachPointSprites;

		public Dictionary<Localization.StandardInteractionTip, Sprite> AttachPointSprites;

		[Space(30f)]
		public Sprite DefaultItemSprite;

		[SerializeField]
		private List<AllItems> _AllItemsSprites;

		public Dictionary<ItemType, Sprite> AllItemsSprites;

		[Space(30f)]
		public Sprite DefaultSpaceObject;

		public Sprite MainStationSprite;

		public Sprite MainOutpostSprite;

		public Sprite CustomOrbitSprite;

		[SerializeField]
		private List<SpaceObjects> _SpaceObjectSprites;

		public Dictionary<GameScenes.SceneID, Sprite> SpaceObjectSprites;

		[Space(30f)]
		public Sprite DefaultRadarObject;

		[SerializeField]
		private List<RadarObjects> _RadarObjectsSprites;

		public Dictionary<SpaceObjectType, Sprite> RadarObjectsSprites;

		[Space(30f)]
		public Sprite DefaultResourceObject;

		[SerializeField]
		private List<ResourceObject> _ResourceObjectsSprites;

		public Dictionary<ResourceType, Sprite> ResourceObjectsSprites;

		[Space(30f)]
		public Texture2D DefaultSceneTexture;

		public Texture2D InviteTexture;

		public Texture2D NewGameTexture;

		[SerializeField]
		private List<SceneObject> _SceneObjectTextures;

		public Dictionary<SpawnSetupType, Texture2D> SceneObjectTextures;

		[Space(30f)]
		public Sprite DefaultNotificationSprite;

		[SerializeField]
		private List<NotificationObject> _NotificationObjectsSprites;

		public Dictionary<CanvasUI.NotificationType, Sprite> NotificationObjectsSprites;

		[Space(30f)]
		public Sprite DefaultWeaponMod;

		[SerializeField]
		private List<WeaponModObject> _WeaponModSprites;

		public Dictionary<WeaponMod.FireMode, Sprite> WeaponModSprites;

		[CompilerGenerated]
		private static Func<SpaceObjects, GameScenes.SceneID> _003C_003Ef__am_0024cache0;

		[CompilerGenerated]
		private static Func<SpaceObjects, Sprite> _003C_003Ef__am_0024cache1;

		[CompilerGenerated]
		private static Func<MachineryParts, MachineryPartType> _003C_003Ef__am_0024cache2;

		[CompilerGenerated]
		private static Func<MachineryParts, Sprite> _003C_003Ef__am_0024cache3;

		[CompilerGenerated]
		private static Func<GenericItems, GenericItemSubType> _003C_003Ef__am_0024cache4;

		[CompilerGenerated]
		private static Func<GenericItems, Sprite> _003C_003Ef__am_0024cache5;

		[CompilerGenerated]
		private static Func<AllItems, ItemType> _003C_003Ef__am_0024cache6;

		[CompilerGenerated]
		private static Func<AllItems, Sprite> _003C_003Ef__am_0024cache7;

		[CompilerGenerated]
		private static Func<RadarObjects, SpaceObjectType> _003C_003Ef__am_0024cache8;

		[CompilerGenerated]
		private static Func<RadarObjects, Sprite> _003C_003Ef__am_0024cache9;

		[CompilerGenerated]
		private static Func<ResourceObject, ResourceType> _003C_003Ef__am_0024cacheA;

		[CompilerGenerated]
		private static Func<ResourceObject, Sprite> _003C_003Ef__am_0024cacheB;

		[CompilerGenerated]
		private static Func<SceneObject, SpawnSetupType> _003C_003Ef__am_0024cacheC;

		[CompilerGenerated]
		private static Func<SceneObject, Texture2D> _003C_003Ef__am_0024cacheD;

		[CompilerGenerated]
		private static Func<SlotTypes, InventorySlot.Group> _003C_003Ef__am_0024cacheE;

		[CompilerGenerated]
		private static Func<SlotTypes, Sprite> _003C_003Ef__am_0024cacheF;

		[CompilerGenerated]
		private static Func<NotificationObject, CanvasUI.NotificationType> _003C_003Ef__am_0024cache10;

		[CompilerGenerated]
		private static Func<NotificationObject, Sprite> _003C_003Ef__am_0024cache11;

		[CompilerGenerated]
		private static Func<AttachPoints, Localization.StandardInteractionTip> _003C_003Ef__am_0024cache12;

		[CompilerGenerated]
		private static Func<AttachPoints, Sprite> _003C_003Ef__am_0024cache13;

		[CompilerGenerated]
		private static Func<WeaponModObject, WeaponMod.FireMode> _003C_003Ef__am_0024cache14;

		[CompilerGenerated]
		private static Func<WeaponModObject, Sprite> _003C_003Ef__am_0024cache15;

		private void Start()
		{
			List<SpaceObjects> spaceObjectSprites = _SpaceObjectSprites;
			if (_003C_003Ef__am_0024cache0 == null)
			{
				_003C_003Ef__am_0024cache0 = _003CStart_003Em__0;
			}
			Func<SpaceObjects, GameScenes.SceneID> keySelector = _003C_003Ef__am_0024cache0;
			if (_003C_003Ef__am_0024cache1 == null)
			{
				_003C_003Ef__am_0024cache1 = _003CStart_003Em__1;
			}
			SpaceObjectSprites = spaceObjectSprites.ToDictionary(keySelector, _003C_003Ef__am_0024cache1);
			List<MachineryParts> machineryPartsSprites = _MachineryPartsSprites;
			if (_003C_003Ef__am_0024cache2 == null)
			{
				_003C_003Ef__am_0024cache2 = _003CStart_003Em__2;
			}
			Func<MachineryParts, MachineryPartType> keySelector2 = _003C_003Ef__am_0024cache2;
			if (_003C_003Ef__am_0024cache3 == null)
			{
				_003C_003Ef__am_0024cache3 = _003CStart_003Em__3;
			}
			MachineryPartsSprites = machineryPartsSprites.ToDictionary(keySelector2, _003C_003Ef__am_0024cache3);
			List<GenericItems> genericItemsSprites = _GenericItemsSprites;
			if (_003C_003Ef__am_0024cache4 == null)
			{
				_003C_003Ef__am_0024cache4 = _003CStart_003Em__4;
			}
			Func<GenericItems, GenericItemSubType> keySelector3 = _003C_003Ef__am_0024cache4;
			if (_003C_003Ef__am_0024cache5 == null)
			{
				_003C_003Ef__am_0024cache5 = _003CStart_003Em__5;
			}
			GenericItemsSprites = genericItemsSprites.ToDictionary(keySelector3, _003C_003Ef__am_0024cache5);
			List<AllItems> allItemsSprites = _AllItemsSprites;
			if (_003C_003Ef__am_0024cache6 == null)
			{
				_003C_003Ef__am_0024cache6 = _003CStart_003Em__6;
			}
			Func<AllItems, ItemType> keySelector4 = _003C_003Ef__am_0024cache6;
			if (_003C_003Ef__am_0024cache7 == null)
			{
				_003C_003Ef__am_0024cache7 = _003CStart_003Em__7;
			}
			AllItemsSprites = allItemsSprites.ToDictionary(keySelector4, _003C_003Ef__am_0024cache7);
			List<RadarObjects> radarObjectsSprites = _RadarObjectsSprites;
			if (_003C_003Ef__am_0024cache8 == null)
			{
				_003C_003Ef__am_0024cache8 = _003CStart_003Em__8;
			}
			Func<RadarObjects, SpaceObjectType> keySelector5 = _003C_003Ef__am_0024cache8;
			if (_003C_003Ef__am_0024cache9 == null)
			{
				_003C_003Ef__am_0024cache9 = _003CStart_003Em__9;
			}
			RadarObjectsSprites = radarObjectsSprites.ToDictionary(keySelector5, _003C_003Ef__am_0024cache9);
			List<ResourceObject> resourceObjectsSprites = _ResourceObjectsSprites;
			if (_003C_003Ef__am_0024cacheA == null)
			{
				_003C_003Ef__am_0024cacheA = _003CStart_003Em__A;
			}
			Func<ResourceObject, ResourceType> keySelector6 = _003C_003Ef__am_0024cacheA;
			if (_003C_003Ef__am_0024cacheB == null)
			{
				_003C_003Ef__am_0024cacheB = _003CStart_003Em__B;
			}
			ResourceObjectsSprites = resourceObjectsSprites.ToDictionary(keySelector6, _003C_003Ef__am_0024cacheB);
			List<SceneObject> sceneObjectTextures = _SceneObjectTextures;
			if (_003C_003Ef__am_0024cacheC == null)
			{
				_003C_003Ef__am_0024cacheC = _003CStart_003Em__C;
			}
			Func<SceneObject, SpawnSetupType> keySelector7 = _003C_003Ef__am_0024cacheC;
			if (_003C_003Ef__am_0024cacheD == null)
			{
				_003C_003Ef__am_0024cacheD = _003CStart_003Em__D;
			}
			SceneObjectTextures = sceneObjectTextures.ToDictionary(keySelector7, _003C_003Ef__am_0024cacheD);
			List<SlotTypes> inventorySlotSprites = _InventorySlotSprites;
			if (_003C_003Ef__am_0024cacheE == null)
			{
				_003C_003Ef__am_0024cacheE = _003CStart_003Em__E;
			}
			Func<SlotTypes, InventorySlot.Group> keySelector8 = _003C_003Ef__am_0024cacheE;
			if (_003C_003Ef__am_0024cacheF == null)
			{
				_003C_003Ef__am_0024cacheF = _003CStart_003Em__F;
			}
			InventorySlotSprites = inventorySlotSprites.ToDictionary(keySelector8, _003C_003Ef__am_0024cacheF);
			List<NotificationObject> notificationObjectsSprites = _NotificationObjectsSprites;
			if (_003C_003Ef__am_0024cache10 == null)
			{
				_003C_003Ef__am_0024cache10 = _003CStart_003Em__10;
			}
			Func<NotificationObject, CanvasUI.NotificationType> keySelector9 = _003C_003Ef__am_0024cache10;
			if (_003C_003Ef__am_0024cache11 == null)
			{
				_003C_003Ef__am_0024cache11 = _003CStart_003Em__11;
			}
			NotificationObjectsSprites = notificationObjectsSprites.ToDictionary(keySelector9, _003C_003Ef__am_0024cache11);
			List<AttachPoints> attachPointSprites = _AttachPointSprites;
			if (_003C_003Ef__am_0024cache12 == null)
			{
				_003C_003Ef__am_0024cache12 = _003CStart_003Em__12;
			}
			Func<AttachPoints, Localization.StandardInteractionTip> keySelector10 = _003C_003Ef__am_0024cache12;
			if (_003C_003Ef__am_0024cache13 == null)
			{
				_003C_003Ef__am_0024cache13 = _003CStart_003Em__13;
			}
			AttachPointSprites = attachPointSprites.ToDictionary(keySelector10, _003C_003Ef__am_0024cache13);
			List<WeaponModObject> weaponModSprites = _WeaponModSprites;
			if (_003C_003Ef__am_0024cache14 == null)
			{
				_003C_003Ef__am_0024cache14 = _003CStart_003Em__14;
			}
			Func<WeaponModObject, WeaponMod.FireMode> keySelector11 = _003C_003Ef__am_0024cache14;
			if (_003C_003Ef__am_0024cache15 == null)
			{
				_003C_003Ef__am_0024cache15 = _003CStart_003Em__15;
			}
			WeaponModSprites = weaponModSprites.ToDictionary(keySelector11, _003C_003Ef__am_0024cache15);
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
				Sprite value;
				if (SpaceObjectSprites.TryGetValue(vessel.SceneID, out value))
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
			Sprite value;
			if (SpaceObjectSprites.TryGetValue(module, out value))
			{
				return value;
			}
			return DefaultSpaceObject;
		}

		public Sprite GetSprite(MachineryPartType part)
		{
			Sprite value;
			if (MachineryPartsSprites.TryGetValue(part, out value))
			{
				return value;
			}
			return DefaultMachineryPart;
		}

		public Sprite GetSprite(GenericItemSubType generic)
		{
			Sprite value;
			if (GenericItemsSprites.TryGetValue(generic, out value))
			{
				return value;
			}
			return DefaultGenericItemSprite;
		}

		public Sprite GetSprite(InventorySlot.Group slot)
		{
			Sprite value;
			if (InventorySlotSprites.TryGetValue(slot, out value))
			{
				return value;
			}
			return DefaultInventorySlotSprite;
		}

		public Sprite GetSprite(ItemType item)
		{
			Sprite value;
			if (AllItemsSprites.TryGetValue(item, out value))
			{
				return value;
			}
			return DefaultItemSprite;
		}

		public Sprite GetSprite(Localization.StandardInteractionTip tip)
		{
			Sprite value;
			if (AttachPointSprites.TryGetValue(tip, out value))
			{
				return value;
			}
			return DefaultAttachPointSprite;
		}

		public Sprite GetSprite(SpaceObjectType spaceObject)
		{
			Sprite value;
			if (RadarObjectsSprites.TryGetValue(spaceObject, out value))
			{
				return value;
			}
			return DefaultRadarObject;
		}

		public Sprite GetSprite(ResourceType resourceType)
		{
			Sprite value;
			if (ResourceObjectsSprites.TryGetValue(resourceType, out value))
			{
				return value;
			}
			return DefaultResourceObject;
		}

		public Texture2D GetTexture(SpawnSetupType tip)
		{
			Texture2D value;
			if (SceneObjectTextures.TryGetValue(tip, out value))
			{
				return value;
			}
			return DefaultSceneTexture;
		}

		public Sprite GetSprite(CanvasUI.NotificationType notificationType)
		{
			Sprite value;
			if (NotificationObjectsSprites.TryGetValue(notificationType, out value))
			{
				return value;
			}
			return DefaultResourceObject;
		}

		public Sprite GetSprite(WeaponMod.FireMode fireMod)
		{
			Sprite value;
			if (WeaponModSprites.TryGetValue(fireMod, out value))
			{
				return value;
			}
			return DefaultWeaponMod;
		}

		[CompilerGenerated]
		private static GameScenes.SceneID _003CStart_003Em__0(SpaceObjects x)
		{
			return x.Key;
		}

		[CompilerGenerated]
		private static Sprite _003CStart_003Em__1(SpaceObjects y)
		{
			return y.Sprite;
		}

		[CompilerGenerated]
		private static MachineryPartType _003CStart_003Em__2(MachineryParts x)
		{
			return x.Key;
		}

		[CompilerGenerated]
		private static Sprite _003CStart_003Em__3(MachineryParts y)
		{
			return y.Sprite;
		}

		[CompilerGenerated]
		private static GenericItemSubType _003CStart_003Em__4(GenericItems x)
		{
			return x.Key;
		}

		[CompilerGenerated]
		private static Sprite _003CStart_003Em__5(GenericItems y)
		{
			return y.Sprite;
		}

		[CompilerGenerated]
		private static ItemType _003CStart_003Em__6(AllItems x)
		{
			return x.Key;
		}

		[CompilerGenerated]
		private static Sprite _003CStart_003Em__7(AllItems y)
		{
			return y.Sprite;
		}

		[CompilerGenerated]
		private static SpaceObjectType _003CStart_003Em__8(RadarObjects x)
		{
			return x.Key;
		}

		[CompilerGenerated]
		private static Sprite _003CStart_003Em__9(RadarObjects y)
		{
			return y.Sprite;
		}

		[CompilerGenerated]
		private static ResourceType _003CStart_003Em__A(ResourceObject x)
		{
			return x.Key;
		}

		[CompilerGenerated]
		private static Sprite _003CStart_003Em__B(ResourceObject y)
		{
			return y.Sprite;
		}

		[CompilerGenerated]
		private static SpawnSetupType _003CStart_003Em__C(SceneObject x)
		{
			return x.Key;
		}

		[CompilerGenerated]
		private static Texture2D _003CStart_003Em__D(SceneObject y)
		{
			return y.Texture;
		}

		[CompilerGenerated]
		private static InventorySlot.Group _003CStart_003Em__E(SlotTypes x)
		{
			return x.Key;
		}

		[CompilerGenerated]
		private static Sprite _003CStart_003Em__F(SlotTypes y)
		{
			return y.Sprite;
		}

		[CompilerGenerated]
		private static CanvasUI.NotificationType _003CStart_003Em__10(NotificationObject x)
		{
			return x.Key;
		}

		[CompilerGenerated]
		private static Sprite _003CStart_003Em__11(NotificationObject y)
		{
			return y.Sprite;
		}

		[CompilerGenerated]
		private static Localization.StandardInteractionTip _003CStart_003Em__12(AttachPoints x)
		{
			return x.Key;
		}

		[CompilerGenerated]
		private static Sprite _003CStart_003Em__13(AttachPoints y)
		{
			return y.Sprite;
		}

		[CompilerGenerated]
		private static WeaponMod.FireMode _003CStart_003Em__14(WeaponModObject x)
		{
			return x.Key;
		}

		[CompilerGenerated]
		private static Sprite _003CStart_003Em__15(WeaponModObject y)
		{
			return y.Sprite;
		}
	}
}
