using System;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	// TODO: The overrides under here are temporary data containers to keep
	// data generation for structures correct. It should be removed when
	// a larger rewrite of this system is done.
	public class DynamicSceneObject : MonoBehaviour
	{
		[Serializable]
		public struct SlotSpawnOverride
		{
			public short SlotID;

			public ItemCompoundType SpawnItem;
		}

		[Serializable]
		public struct CompartmentOverride
		{
			public CargoCompartmentType Compartment;

			public CargoResourceData[] Resources;
		}

		[Serializable]
		public struct SlotItemTypesOverride
		{
			public short SlotID;

			public ItemType[] ItemTypes;
		}

		[Serializable]
		public struct OutfitStatsOverride
		{
			public bool Enabled;

			public ItemCategory Category;

			public float Armor;

			public float[] TierMultipliers;

			public float DamageReductionTorso;

			public float DamageReductionAbdomen;

			public float DamageReductionArms;

			public float DamageReductionLegs;

			public float DamageResistanceTorso;

			public float DamageResistanceAbdomen;

			public float DamageResistanceArms;

			public float DamageResistanceLegs;

			public float CollisionResistance;

			public SlotItemTypesOverride[] SlotItemTypes;
		}

		[HideInInspector] public GameObject PrefabObject;

		[HideInInspector] public MachineryPartType MachineryPartType;

		[Range(1, 4)] public int Tier = 1;

		public bool LightActive;

		public SlotSpawnOverride[] SlotOverrides;

		public CompartmentOverride[] CompartmentOverrides;

		public OutfitStatsOverride OutfitOverride;

		public DynaminObjectSpawnSettings[] SpawnSettings;

		public ItemType ItemType
		{
			get
			{
				Item componentInChildren = GetComponentInChildren<Item>();
				return (componentInChildren != null) ? componentInChildren.Type : ItemType.None;
			}
		}

		public Item Item => GetComponentInChildren<Item>();

		private void Awake()
		{
			if (Application.isPlaying && Application.isEditor)
			{
				Destroy(gameObject);
			}
		}
	}
}
