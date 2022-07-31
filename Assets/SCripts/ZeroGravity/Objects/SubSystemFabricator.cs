using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.Objects
{
	public class SubSystemFabricator : SubSystem, IPowerConsumer, ICargo
	{
		public class CraftableItemData
		{
			public short ItemID;

			public ItemCompoundType CompoundType;

			public Dictionary<ResourceType, float> Resources;
		}

		[CompilerGenerated]
		private sealed class _003CGetCompartment_003Ec__AnonStorey0
		{
			internal short? id;

			internal bool _003C_003Em__0(ICargoCompartment m)
			{
				return m.ID == id.Value;
			}
		}

		[CompilerGenerated]
		private sealed class _003CSetAuxDetails_003Ec__AnonStorey1
		{
			internal CargoCompartmentDetails ccd;

			internal bool _003C_003Em__0(ICargoCompartment m)
			{
				return m.ID == ccd.ID;
			}
		}

		[CompilerGenerated]
		private sealed class _003CHasEnoughResources_003Ec__AnonStorey2
		{
			internal KeyValuePair<ResourceType, float> kv;

			internal bool _003C_003Em__0(CargoResourceData m)
			{
				return m.ResourceType == kv.Key;
			}
		}

		[CompilerGenerated]
		private sealed class _003CGetCraftableItems_003Ec__AnonStorey3
		{
			internal ItemType type;

			internal GenericItemSubType subType;

			internal MachineryPartType partType;

			internal int index;

			internal bool _003C_003Em__0(ItemCompoundType m)
			{
				return m.Type == type && m.SubType == subType && m.PartType == partType;
			}

			internal int _003C_003Em__1(float m)
			{
				return index++;
			}
		}

		[SerializeField]
		private string _Name = "Fabricator";

		[SerializeField]
		private ResourceRequirement[] _ResourceRequirements = new ResourceRequirement[1]
		{
			new ResourceRequirement
			{
				ResourceType = DistributionSystemType.Power
			}
		};

		public float TimePerResourceUnit = 1f;

		public List<ItemCompoundType> AllowedItemTypes;

		public List<SceneAttachPoint> AttachPoints;

		[NonSerialized]
		public List<ItemCompoundType> ItemsInQueue;

		public float CurrentTimeLeft;

		[NonSerialized]
		public float TotalTimeLeft;

		public CargoCompartment CargoResources;

		private List<ICargoCompartment> _Compartments;

		public List<CraftableItemData> CraftableItems;

		public Animator FabricatorAnimator;

		private bool initialized;

		[CompilerGenerated]
		private static Func<SceneAttachPoint, bool> _003C_003Ef__am_0024cache0;

		[CompilerGenerated]
		private static Func<SceneAttachPoint, int> _003C_003Ef__am_0024cache1;

		[CompilerGenerated]
		private static Func<CraftableItemData, bool> _003C_003Ef__am_0024cache2;

		[CompilerGenerated]
		private static Func<CraftableItemData, bool> _003C_003Ef__am_0024cache3;

		public override SubSystemType Type
		{
			get
			{
				return SubSystemType.Fabricator;
			}
		}

		public string Name
		{
			get
			{
				return _Name;
			}
		}

		public override ResourceRequirement[] ResourceRequirements
		{
			get
			{
				return _ResourceRequirements;
			}
		}

		public List<ICargoCompartment> Compartments
		{
			get
			{
				return _Compartments;
			}
		}

		protected override void Start()
		{
			base.Start();
			_Compartments = new List<ICargoCompartment> { CargoResources };
			CraftableItems = GetCraftableItems();
		}

		public ICargoCompartment GetCompartment(short? id = null)
		{
			_003CGetCompartment_003Ec__AnonStorey0 _003CGetCompartment_003Ec__AnonStorey = new _003CGetCompartment_003Ec__AnonStorey0();
			_003CGetCompartment_003Ec__AnonStorey.id = id;
			if (_003CGetCompartment_003Ec__AnonStorey.id.HasValue)
			{
				return _Compartments.Find(_003CGetCompartment_003Ec__AnonStorey._003C_003Em__0);
			}
			return _Compartments[0];
		}

		public override SystemAuxData GetAuxData()
		{
			SubSystemFabricatorAuxData subSystemFabricatorAuxData = new SubSystemFabricatorAuxData();
			subSystemFabricatorAuxData.TimePerResourceUnit = TimePerResourceUnit;
			subSystemFabricatorAuxData.AllowedItemTypes = AllowedItemTypes;
			List<SceneAttachPoint> attachPoints = AttachPoints;
			if (_003C_003Ef__am_0024cache0 == null)
			{
				_003C_003Ef__am_0024cache0 = _003CGetAuxData_003Em__0;
			}
			IEnumerable<SceneAttachPoint> source = attachPoints.Where(_003C_003Ef__am_0024cache0);
			if (_003C_003Ef__am_0024cache1 == null)
			{
				_003C_003Ef__am_0024cache1 = _003CGetAuxData_003Em__1;
			}
			subSystemFabricatorAuxData.AttachPoints = source.Select(_003C_003Ef__am_0024cache1).ToList();
			subSystemFabricatorAuxData.CargoCompartments = new List<CargoCompartmentData> { (!(CargoResources == null)) ? CargoResources.GetData() : null };
			return subSystemFabricatorAuxData;
		}

		public override void SetDetails(SubSystemDetails details, bool instant = false)
		{
			base.SetDetails(details, instant);
			if (Status == SystemStatus.OnLine)
			{
				FabricatorAnimator.SetBool("Work", true);
				FabricatorAnimator.SetBool("Initialized", initialized);
			}
			else
			{
				FabricatorAnimator.SetBool("Work", false);
				FabricatorAnimator.SetBool("Initialized", initialized);
			}
			initialized = true;
		}

		public override void SetAuxDetails(IAuxDetails auxDetails)
		{
			base.SetAuxDetails(auxDetails);
			FabricatorAuxDetails fabricatorAuxDetails = auxDetails as FabricatorAuxDetails;
			ItemsInQueue = fabricatorAuxDetails.ItemsInQueue;
			CurrentTimeLeft = fabricatorAuxDetails.CurrentTimeLeft;
			TotalTimeLeft = fabricatorAuxDetails.TotalTimeLeft;
			if (_Compartments == null)
			{
				_Compartments = new List<ICargoCompartment> { CargoResources };
			}
			using (List<CargoCompartmentDetails>.Enumerator enumerator = fabricatorAuxDetails.CargoCompartments.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					_003CSetAuxDetails_003Ec__AnonStorey1 _003CSetAuxDetails_003Ec__AnonStorey = new _003CSetAuxDetails_003Ec__AnonStorey1();
					_003CSetAuxDetails_003Ec__AnonStorey.ccd = enumerator.Current;
					ICargoCompartment cargoCompartment = _Compartments.Find(_003CSetAuxDetails_003Ec__AnonStorey._003C_003Em__0);
					if (cargoCompartment != null)
					{
						cargoCompartment.Resources = _003CSetAuxDetails_003Ec__AnonStorey.ccd.Resources;
					}
				}
			}
		}

		public void FabricateItem(ItemType type, GenericItemSubType subType, MachineryPartType partType, int tier)
		{
			Client.Instance.NetworkController.SendToGameServer(new FabricateItemMessage
			{
				ID = new VesselObjectID(base.ParentVessel.GUID, base.InSceneID),
				ItemType = new ItemCompoundType
				{
					Type = type,
					SubType = subType,
					PartType = partType,
					Tier = tier
				}
			});
			Client.LogCustomEvent("fabricate", new Dictionary<string, object> { 
			{
				Item.GetName(type, subType, partType, false),
				tier
			} });
		}

		public void CancelFabrication(bool currentItemOnly = false)
		{
			Client.Instance.NetworkController.SendToGameServer(new CancelFabricationMessage
			{
				ID = new VesselObjectID(base.ParentVessel.GUID, base.InSceneID),
				CurrentItemOnly = currentItemOnly
			});
			Client.LogCustomEvent("cancel_fabrication", null);
		}

		public bool HasEnoughResources(ItemType itemType, GenericItemSubType subType, MachineryPartType partType, int tier)
		{
			Dictionary<ResourceType, float> craftingResources = Item.GetCraftingResources(itemType, subType, partType, tier);
			using (Dictionary<ResourceType, float>.Enumerator enumerator = craftingResources.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					_003CHasEnoughResources_003Ec__AnonStorey2 _003CHasEnoughResources_003Ec__AnonStorey = new _003CHasEnoughResources_003Ec__AnonStorey2();
					_003CHasEnoughResources_003Ec__AnonStorey.kv = enumerator.Current;
					CargoResourceData cargoResourceData = GetCompartment().Resources.FirstOrDefault(_003CHasEnoughResources_003Ec__AnonStorey._003C_003Em__0);
					if (cargoResourceData == null || cargoResourceData.Quantity < _003CHasEnoughResources_003Ec__AnonStorey.kv.Value)
					{
						return false;
					}
				}
			}
			return true;
		}

		private List<CraftableItemData> GetCraftableItems()
		{
			List<CraftableItemData> list = new List<CraftableItemData>();
			foreach (DynamicObjectData value in StaticData.DynamicObjectsDataList.Values)
			{
				_003CGetCraftableItems_003Ec__AnonStorey3 _003CGetCraftableItems_003Ec__AnonStorey = new _003CGetCraftableItems_003Ec__AnonStorey3();
				_003CGetCraftableItems_003Ec__AnonStorey.type = value.ItemType;
				_003CGetCraftableItems_003Ec__AnonStorey.subType = GenericItemSubType.None;
				_003CGetCraftableItems_003Ec__AnonStorey.partType = MachineryPartType.None;
				if (value.ItemType == ItemType.GenericItem)
				{
					_003CGetCraftableItems_003Ec__AnonStorey.subType = (value.DefaultAuxData as GenericItemData).SubType;
				}
				else if (value.ItemType == ItemType.MachineryPart)
				{
					_003CGetCraftableItems_003Ec__AnonStorey.partType = (value.DefaultAuxData as MachineryPartData).PartType;
				}
				if (AllowedItemTypes.Count > 0 && AllowedItemTypes.FirstOrDefault(_003CGetCraftableItems_003Ec__AnonStorey._003C_003Em__0) == null)
				{
					continue;
				}
				_003CGetCraftableItems_003Ec__AnonStorey.index = 1;
				List<int> list2 = value.DefaultAuxData.TierMultipliers.Select(_003CGetCraftableItems_003Ec__AnonStorey._003C_003Em__1).ToList();
				if (list2.Count == 0)
				{
					list2.Add(1);
				}
				foreach (int item in list2)
				{
					Dictionary<ResourceType, float> craftingResources = Item.GetCraftingResources(_003CGetCraftableItems_003Ec__AnonStorey.type, _003CGetCraftableItems_003Ec__AnonStorey.subType, _003CGetCraftableItems_003Ec__AnonStorey.partType, item);
					ItemCompoundType itemCompoundType = new ItemCompoundType();
					itemCompoundType.Type = _003CGetCraftableItems_003Ec__AnonStorey.type;
					itemCompoundType.SubType = _003CGetCraftableItems_003Ec__AnonStorey.subType;
					itemCompoundType.PartType = _003CGetCraftableItems_003Ec__AnonStorey.partType;
					itemCompoundType.Tier = item;
					ItemCompoundType compoundType = itemCompoundType;
					int? num = ((craftingResources != null) ? new int?(craftingResources.Count) : null);
					if (num.HasValue && num.GetValueOrDefault() > 0)
					{
						list.Add(new CraftableItemData
						{
							ItemID = value.ItemID,
							CompoundType = compoundType,
							Resources = craftingResources
						});
					}
				}
			}
			if (_003C_003Ef__am_0024cache2 == null)
			{
				_003C_003Ef__am_0024cache2 = _003CGetCraftableItems_003Em__2;
			}
			IOrderedEnumerable<CraftableItemData> source = list.OrderBy(_003C_003Ef__am_0024cache2);
			if (_003C_003Ef__am_0024cache3 == null)
			{
				_003C_003Ef__am_0024cache3 = _003CGetCraftableItems_003Em__3;
			}
			return source.ThenBy(_003C_003Ef__am_0024cache3).ToList();
		}

		[CompilerGenerated]
		private static bool _003CGetAuxData_003Em__0(SceneAttachPoint m)
		{
			return m != null;
		}

		[CompilerGenerated]
		private static int _003CGetAuxData_003Em__1(SceneAttachPoint m)
		{
			return m.InSceneID;
		}

		[CompilerGenerated]
		private static bool _003CGetCraftableItems_003Em__2(CraftableItemData m)
		{
			return m.CompoundType.Type != ItemType.MachineryPart;
		}

		[CompilerGenerated]
		private static bool _003CGetCraftableItems_003Em__3(CraftableItemData m)
		{
			return m.CompoundType.Type == ItemType.GenericItem;
		}
	}
}
