using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;

namespace ZeroGravity.Objects
{
	public class HandDrill : Item, IBatteryConsumer
	{
		private ItemSlot canisterSlot;

		private ItemSlot drillBitSlot;

		public GameObject HurtTrigger;

		[SerializeField]
		private Transform drillTip;

		public Transform BatteryPos;

		public Transform CanisterPos;

		public GameObject drillingParticle;

		public GameObject drillingParticle3rd;

		public Transform drillEffectTransform;

		public DrillEffectScript effectScript;

		public float BatteryUsage;

		public float DrillingStrength;

		private bool initiateDrilling;

		[SerializeField]
		public Animator drillAnimator;

		public Transform rayTransform;

		private UpdateTimer Timer = new UpdateTimer(3f);

		private int currentHandDrillResourceIndex;

		public GameObject NotAttached;

		public GameObject DrillCanvas;

		public Image EnergyFiller;

		public Image ResourceBarFiller;

		public Text ResourceName;

		public Text ResourceValue;

		public Text NoOfElements;

		public GameObject CanisterFull;

		public GameObject LowPowerMask;

		public GameObject NoPower;

		public bool IsDrilling;

		private bool isPlayingSpin;

		private bool isPlayingDrilling;

		private float miningTime;

		private AsteroidMiningPoint miningPoint;

		private Vector3 winningPos = new Vector3(0.16f, -0.04f, 1.34f);

		public SoundEffect DrillSoundEffect;

		public SoundEffect RockDrillingSoundEffect;

		[CompilerGenerated]
		private static Func<KeyValuePair<short, ItemSlot>, bool> _003C_003Ef__am_0024cache0;

		[CompilerGenerated]
		private static Func<KeyValuePair<short, ItemSlot>, bool> _003C_003Ef__am_0024cache1;

		public override float Quantity
		{
			get
			{
				return (!(Canister != null)) ? 0f : Canister.Quantity;
			}
		}

		public override float MaxQuantity
		{
			get
			{
				return (!(Canister != null)) ? 0f : Canister.MaxQuantity;
			}
		}

		public override EquipType EquipTo
		{
			get
			{
				return EquipType.Hands;
			}
		}

		public ItemSlot BatterySlot { get; set; }

		public Battery Battery
		{
			get
			{
				return (!(BatterySlot != null)) ? null : (BatterySlot.Item as Battery);
			}
		}

		public float BatteryPower
		{
			get
			{
				return (!(Battery != null)) ? 0f : Mathf.Clamp01(Battery.CurrentPower / Battery.MaxPower);
			}
		}

		public Canister Canister
		{
			get
			{
				return (!(canisterSlot != null)) ? null : (canisterSlot.Item as Canister);
			}
		}

		public Item DrillBit
		{
			get
			{
				return (!(drillBitSlot != null)) ? null : (drillBitSlot.Item as GenericItem);
			}
		}

		public override Transform TipOfItem
		{
			get
			{
				return drillTip;
			}
		}

		public bool CanDrill
		{
			get
			{
				Battery battery = Battery;
				float? num = (((object)battery != null) ? new float?(battery.CurrentPower) : null);
				int result;
				if (num.HasValue && num.GetValueOrDefault() > float.Epsilon && Canister.HasSpace)
				{
					Item drillBit = DrillBit;
					float? num2 = (((object)drillBit != null) ? new float?(drillBit.Health) : null);
					result = ((num2.HasValue && num2.GetValueOrDefault() > float.Epsilon) ? 1 : 0);
				}
				else
				{
					result = 0;
				}
				return (byte)result != 0;
			}
		}

		public override bool CanReloadOnInteract(Item item)
		{
			return (Battery == null && item is Battery) || (Canister == null && item is Canister) || (DrillBit == null && item is GenericItem && (item as GenericItem).SubType == GenericItemSubType.DiamondCoreDrillBit);
		}

		public void CreateParticle()
		{
			drillEffectTransform = UnityEngine.Object.Instantiate((!(DynamicObj.Parent is MyPlayer)) ? drillingParticle3rd : drillingParticle, (!(DynamicObj.Parent is MyPlayer)) ? TipOfItem : MyPlayer.Instance.MuzzleFlashTransform).transform;
			drillEffectTransform.Reset();
			effectScript = drillEffectTransform.GetComponent<DrillEffectScript>();
		}

		public override bool PrimaryFunction()
		{
			if (CanDrill)
			{
				PlayerDrillingMessage playerDrillingMessage = new PlayerDrillingMessage();
				bool flag = !IsDrilling;
				IsDrilling = true;
				drillAnimator.SetBool("Drilling", IsDrilling);
				if (!isPlayingSpin)
				{
					isPlayingSpin = true;
					DrillSoundEffect.Play(0);
				}
				playerDrillingMessage.isDrilling = true;
				AsteroidMiningPoint asteroidMiningPoint = null;
				Collider[] array = Physics.OverlapSphere(drillTip.position, 0.05f);
				foreach (Collider collider in array)
				{
					asteroidMiningPoint = collider.GetComponent<AsteroidMiningPoint>();
					if (asteroidMiningPoint != null)
					{
						break;
					}
				}
				if (asteroidMiningPoint != null && asteroidMiningPoint.Quantity > 0f)
				{
					RaycastHit hitInfo;
					if (Physics.Raycast(rayTransform.position, TipOfItem.forward, out hitInfo, 1f, Client.DefaultLayerMask))
					{
						drillEffectTransform.forward = hitInfo.normal;
						effectScript.ToggleEffect(true);
						if (!isPlayingDrilling)
						{
							isPlayingDrilling = true;
							RockDrillingSoundEffect.Play(0);
						}
						playerDrillingMessage.dontPlayEffect = false;
					}
					else if (effectScript != null)
					{
						if (!isPlayingDrilling)
						{
							isPlayingDrilling = true;
							RockDrillingSoundEffect.Play(0);
						}
						effectScript.ToggleEffect(false);
						playerDrillingMessage.dontPlayEffect = true;
					}
					miningTime += Time.deltaTime;
					miningPoint = asteroidMiningPoint;
					if (DynamicObj.Parent is MyPlayer)
					{
						MyPlayer.Instance.IsUsingItemInHands = true;
						MyPlayer.Instance.animHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, true);
					}
					if ((double)miningTime > 0.02)
					{
						playerDrillingMessage.MiningTime = miningTime;
						playerDrillingMessage.MiningPointID = new VesselObjectID
						{
							InSceneID = miningPoint.InSceneID,
							VesselGUID = miningPoint.ParentVessel.GUID
						};
						Client.Instance.NetworkController.SendToGameServer(playerDrillingMessage);
						miningTime = 0f;
					}
				}
				else
				{
					if (miningTime > 0f)
					{
						Client.Instance.NetworkController.SendToGameServer(playerDrillingMessage);
					}
					miningTime = 0f;
					miningPoint = null;
					playerDrillingMessage.dontPlayEffect = true;
					effectScript.ToggleEffect(false);
					if (isPlayingDrilling)
					{
						isPlayingDrilling = false;
						RockDrillingSoundEffect.Play(1);
					}
					MyPlayer.Instance.animHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, false);
				}
				if ((double)miningTime > 0.02 || flag)
				{
					playerDrillingMessage.MiningTime = miningTime;
					Client.Instance.NetworkController.SendToGameServer(playerDrillingMessage);
					miningTime = 0f;
				}
			}
			else
			{
				PrimaryReleased();
			}
			return false;
		}

		public override void TriggerItemAnimation(AnimatorHelper.ReloadType rType, string triggerName, string blendTreeParamName)
		{
			drillAnimator.SetFloat(blendTreeParamName, (float)rType);
			drillAnimator.SetTrigger(triggerName);
		}

		public override void Reload(Item item)
		{
			if (item is Canister && item.Type == ItemType.AltairHandDrillCanister)
			{
				item.RequestAttach(canisterSlot);
			}
			else if (item is Battery && item.Type == ItemType.AltairHandDrillBattery)
			{
				item.RequestAttach(BatterySlot);
			}
			else if (item is GenericItem && (item as GenericItem).SubType == GenericItemSubType.DiamondCoreDrillBit)
			{
				item.RequestAttach(drillBitSlot);
			}
		}

		public override void ReloadStepComplete(Player pl, AnimatorHelper.ReloadStepType reloadStepType, ref Item currentReloadItem, ref Item newReloadItem)
		{
			switch (reloadStepType)
			{
			case AnimatorHelper.ReloadStepType.ReloadStart:
				currentReloadItem.AttachToBone(pl, AnimatorHelper.HumanBones.LeftInteractBone);
				UpdateUI();
				break;
			case AnimatorHelper.ReloadStepType.ItemSwitch:
				newReloadItem.AttachToBone(pl, AnimatorHelper.HumanBones.LeftInteractBone);
				if (currentReloadItem != null)
				{
					currentReloadItem.AttachToBone(pl, AnimatorHelper.HumanBones.Hips, false);
				}
				break;
			case AnimatorHelper.ReloadStepType.ReloadEnd:
				if (pl is MyPlayer)
				{
					pl.Inventory.HandsSlot.UI.UpdateSlot();
					if (currentReloadItem != null)
					{
						pl.Inventory.AddToInventoryOrDrop(currentReloadItem, null);
					}
				}
				UpdateUI();
				break;
			case AnimatorHelper.ReloadStepType.UnloadEnd:
				if (currentReloadItem != null && pl is MyPlayer)
				{
					pl.Inventory.AddToInventoryOrDrop(currentReloadItem, null);
				}
				UpdateUI();
				break;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			Dictionary<short, ItemSlot> slots = Slots;
			if (_003C_003Ef__am_0024cache0 == null)
			{
				_003C_003Ef__am_0024cache0 = _003CAwake_003Em__0;
			}
			canisterSlot = slots.FirstOrDefault(_003C_003Ef__am_0024cache0).Value;
			Dictionary<short, ItemSlot> slots2 = Slots;
			if (_003C_003Ef__am_0024cache1 == null)
			{
				_003C_003Ef__am_0024cache1 = _003CAwake_003Em__1;
			}
			drillBitSlot = slots2.FirstOrDefault(_003C_003Ef__am_0024cache1).Value;
		}

		protected override void Start()
		{
			base.Start();
			if (MyPlayer.Instance.IsAlive)
			{
				DrillCanvas.GetComponent<Canvas>().worldCamera = MyPlayer.Instance.FpsController.MainCamera;
			}
			DrillCanvas.SetActive(Battery != null);
		}

		private void LateUpdate()
		{
			SetPositionOfActiveEffect();
		}

		public override bool CustomCollidereToggle(bool isEnabled)
		{
			if ((bool)DynamicObj.OnPlatform)
			{
				DynamicObj.OnPlatform.RemoveFromPlatform(DynamicObj);
			}
			GetComponent<Collider>().enabled = isEnabled;
			if (Battery != null)
			{
				Battery.GetComponent<Collider>().enabled = false;
			}
			if (Canister != null)
			{
				Canister.GetComponent<Collider>().enabled = false;
			}
			return true;
		}

		public override void Special()
		{
		}

		public void RemoveBattery()
		{
			if (Battery != null && DynamicObj.Parent is MyPlayer)
			{
				if (MyPlayer.Instance.CurrentOutfit == null)
				{
					Battery.RequestDrop();
				}
				else if (MyPlayer.Instance.Inventory.FindEmptyOutfitSlot(Battery) != null)
				{
					Battery.RequestPickUp();
				}
			}
		}

		public void RemoveCanister()
		{
			if (MyPlayer.Instance.CurrentOutfit == null)
			{
				Canister.RequestDrop();
			}
			else if (MyPlayer.Instance.Inventory.FindEmptyOutfitSlot(Canister) != null)
			{
				Canister.RequestPickUp();
			}
		}

		private void Update()
		{
			if (IsDrilling)
			{
				UpdateUI();
			}
			if (!Timer.Update() || !(Battery != null) || !(Canister != null))
			{
				return;
			}
			UpdateUI();
			if (Canister.CargoCompartment.Resources != null)
			{
				if (currentHandDrillResourceIndex + 1 > Canister.CargoCompartment.Resources.Count - 1)
				{
					currentHandDrillResourceIndex = 0;
				}
				else
				{
					currentHandDrillResourceIndex++;
				}
			}
			UpdateResources();
		}

		public override void UpdateUI()
		{
			base.UpdateUI();
			NoPower.Activate(Battery == null || Battery.BatteryPrecentage <= 0f);
			if (Battery == null)
			{
				return;
			}
			EnergyFiller.fillAmount = Battery.BatteryPrecentage;
			LowPowerMask.SetActive(Battery.BatteryPrecentage <= 0.1f);
			if (Canister != null)
			{
				NotAttached.SetActive(false);
				ResourceBarFiller.fillAmount = Canister.ResourcePercentage;
				CanisterFull.SetActive(Canister.ResourcePercentage >= 1f);
				if (Canister.ResourcePercentage == 0f)
				{
					ResourceName.text = Localization.Empty.ToUpper();
					ResourceValue.text = "0";
					NoOfElements.text = "0 / 0";
				}
			}
			else
			{
				ResourceBarFiller.fillAmount = 0f;
				NotAttached.SetActive(true);
			}
		}

		private void UpdateResources()
		{
			if (Canister.CargoCompartment != null && Canister.CargoCompartment.Resources != null && currentHandDrillResourceIndex < Canister.CargoCompartment.Resources.Count)
			{
				ResourceName.text = Canister.CargoCompartment.Resources[currentHandDrillResourceIndex].ResourceType.ToString().CamelCaseToSpaced().ToUpper();
				ResourceValue.text = Canister.CargoCompartment.Resources[currentHandDrillResourceIndex].Quantity.ToString("f0");
				NoOfElements.text = currentHandDrillResourceIndex + 1 + "/" + Canister.CargoCompartment.Resources.Count;
			}
		}

		public override void PrimaryReleased()
		{
			IsDrilling = false;
			miningTime = 0f;
			drillAnimator.SetBool("Drilling", IsDrilling);
			if (isPlayingSpin)
			{
				DrillSoundEffect.Play(1);
				isPlayingSpin = false;
			}
			if (isPlayingDrilling)
			{
				RockDrillingSoundEffect.Play(1);
				isPlayingDrilling = false;
			}
			if (effectScript != null)
			{
				effectScript.ToggleEffect(false);
			}
			if (DynamicObj.Parent is Player)
			{
				(DynamicObj.Parent as Player).IsUsingItemInHands = false;
			}
			if (DynamicObj.Parent is MyPlayer)
			{
				MyPlayer.Instance.animHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, false);
				PlayerDrillingMessage playerDrillingMessage = new PlayerDrillingMessage();
				playerDrillingMessage.isDrilling = false;
				playerDrillingMessage.dontPlayEffect = true;
				PlayerDrillingMessage data = playerDrillingMessage;
				Client.Instance.NetworkController.SendToGameServer(data);
			}
			UpdateUI();
		}

		public override void ChangeEquip(EquipType type, Player pl)
		{
			if (pl == null)
			{
				return;
			}
			if (type != EquipTo)
			{
				PrimaryReleased();
				if (drillEffectTransform != null)
				{
					UnityEngine.Object.Destroy(drillEffectTransform.gameObject);
				}
			}
			else if (type == EquipTo)
			{
				CreateParticle();
			}
			DrillCanvas.SetActive(Battery != null);
			UpdateUI();
		}

		public override DynamicObjectAuxData GetAuxData()
		{
			HandDrillData baseAuxData = GetBaseAuxData<HandDrillData>();
			baseAuxData.BatteryConsumption = BatteryUsage;
			baseAuxData.DrillingStrength = DrillingStrength;
			return baseAuxData;
		}

		public override string QuantityCheck()
		{
			return (!(Canister == null)) ? FormatHelper.CurrentMax(Canister.Quantity, Canister.MaxQuantity) : "0";
		}

		[CompilerGenerated]
		private static bool _003CAwake_003Em__0(KeyValuePair<short, ItemSlot> m)
		{
			return m.Value.ItemTypes.Contains(ItemType.AltairHandDrillCanister);
		}

		[CompilerGenerated]
		private static bool _003CAwake_003Em__1(KeyValuePair<short, ItemSlot> m)
		{
			return m.Value.GenericSubTypes.Contains(GenericItemSubType.DiamondCoreDrillBit);
		}
	}
}
