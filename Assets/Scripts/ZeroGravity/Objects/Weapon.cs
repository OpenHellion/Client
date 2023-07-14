using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZeroGravity.CharacterMovement;
using ZeroGravity.Data;
using ZeroGravity.Effects;
using ZeroGravity.Network;

namespace ZeroGravity.Objects
{
	public class Weapon : Item
	{
		public float CrouchAimMultiplier;

		[SerializeField]
		private Transform magazinePos;

		[SerializeField]
		private Transform NormalTipOfTheGun;

		[SerializeField]
		private Transform SpecialTipOfTheGun;

		private float lastFireTime;

		private ItemSlot magazineSlot;

		[Title("Effects and anims")]
		public GameObject MuzzleFlash;

		public GameObject ImpactEffects;

		public GameObject BulletEffect;

		public MuzzleActivator currentMuzzle;

		[Title("Mods")]
		public List<WeaponMod> Mods;

		[HideInInspector]
		public WeaponMod CurrentWeaponMod;

		public bool CanZoom;

		private bool isSpecialStance;

		[Title("Sounds")]
		public SoundEffect SoundEffect;

		private int shootingLayerMask;

		private AnimatorHelper animHelper;

		public override EquipType EquipTo => EquipType.Hands;

		public override Transform TipOfItem => (!IsSpecialStance) ? NormalTipOfTheGun : SpecialTipOfTheGun;

		public Vector2 CurrentRecoil => (!IsSpecialStance) ? CurrentWeaponMod.NormalRecoil : CurrentWeaponMod.SpecialRecoil;

		public float Range => CurrentWeaponMod.Range;

		public override float Quantity => (!(Magazine != null)) ? 0f : Magazine.Quantity;

		public override float MaxQuantity => (!(Magazine != null)) ? 0f : Magazine.MaxQuantity;

		public Magazine Magazine => (!(magazineSlot != null)) ? null : (magazineSlot.Item as Magazine);

		public bool IsSpecialStance
		{
			get
			{
				return isSpecialStance;
			}
			set
			{
				if (CanZoom)
				{
					ToggleZoomCamera(value);
				}
				isSpecialStance = value;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			if (Client.IsGameBuild)
			{
				animHelper = MyPlayer.Instance.animHelper;
			}
			CurrentWeaponMod = Mods[0];
			shootingLayerMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("FirstPerson"));
			magazineSlot = Slots.FirstOrDefault((KeyValuePair<short, ItemSlot> m) => m.Value.ItemTypes.FirstOrDefault(ItemTypeRange.IsAmmo) != ItemType.None).Value;
		}

		public override void UpdateUI()
		{
			base.UpdateUI();
		}

		public override bool PrimaryFunction()
		{
			ShootingRaycast();
			return true;
		}

		public override bool SecondaryFunction()
		{
			if (animHelper.CanSwitchState && animHelper.CanSpecial)
			{
				MyPlayer instance = MyPlayer.Instance;
				if (instance.CurrentStance == MyPlayer.PlayerStance.Special)
				{
					instance.ChangeCamerasFov(Client.Instance.DefaultCameraFov);
					instance.ChangeStance(MyPlayer.PlayerStance.Active, ActiveSpeedMultiplier);
					IsSpecialStance = false;
					UpdateUI();
				}
				else
				{
					instance.ChangeCamerasFov(Client.Instance.SpecialCameraFov);
					instance.ChangeStance(MyPlayer.PlayerStance.Special, SpecialSpeedMultiplier);
					IsSpecialStance = true;
					UpdateUI();
				}
				return true;
			}
			return false;
		}

		public bool CanReloadWithType(ItemType type)
		{
			return ReloadWithTypes.Contains(type);
		}

		public void ReloadFromInventory()
		{
			List<Magazine> list = new List<Magazine>();
			foreach (InventorySlot value in MyPlayer.Instance.Inventory.GetAllSlots().Values)
			{
				if (value.Item != null && value.Item.Type == ReloadWithTypes[0] && (value.Item as Magazine).Quantity > 0f)
				{
					list.Add(value.Item as Magazine);
				}
			}
			if (list.Count > 0)
			{
				list.OrderBy((Magazine x) => x.Quantity).Reverse();
				Reload(list[0]);
			}
		}

		public override void Reload(Item newItem)
		{
			if (newItem is not null && newItem is Magazine && CanReloadWithType((newItem as Magazine).Type))
			{
				Magazine newReloadingItem = (Magazine)newItem;
				if (magazinePos is not null)
				{
					MyPlayer.Instance.ChangeCamerasFov(Client.Instance.DefaultCameraFov);
					MyPlayer.Instance.ReloadItem(newReloadingItem, Magazine, (!(Magazine != null)) ? AnimatorHelper.ReloadType.JustLoad : AnimatorHelper.ReloadType.FullReload, Type);
					ToggleZoomCamera(status: false);
				}
			}
		}

		public override void Special()
		{
		}

		private bool CanShoot()
		{
			return Magazine != null && Magazine.Quantity > 0f && Time.time - lastFireTime > CurrentWeaponMod.RateOfFire;
		}

		private void ShootingRaycast()
		{
			if (Magazine != null && Magazine.Quantity > 0f)
			{
				if (CanShoot())
				{
					lastFireTime = Time.time;
					MyCharacterController fpsController = MyPlayer.Instance.FpsController;
					Vector3 euler = CalculateRecoil(CurrentRecoil.x, CurrentRecoil.y);
					if (MyPlayer.Instance.CurrentStance == MyPlayer.PlayerStance.Active)
					{
						Vector3 vector = fpsController.MainCamera.transform.position + fpsController.MouseLookXTransform.forward * CurrentWeaponMod.Range + CalculateRecoil(CurrentRecoil.x, CurrentRecoil.y) - TipOfItem.position;
					}
					else
					{
						Vector3 vector = TipOfItem.forward;
					}
					SpaceObject spaceObject = ((!(MyPlayer.Instance.Parent is SpaceObjectVessel)) ? MyPlayer.Instance.Parent : (MyPlayer.Instance.Parent as SpaceObjectVessel).MainVessel);
					ShotData shotData = new ShotData();
					shotData.Position = (Quaternion.LookRotation(spaceObject.Forward, spaceObject.Up) * MyPlayer.Instance.FpsController.MainCamera.transform.position).ToArray();
					shotData.Orientation = (Quaternion.LookRotation(spaceObject.Forward, spaceObject.Up) * MyPlayer.Instance.FpsController.MainCamera.transform.forward.normalized).ToArray();
					shotData.parentGUID = spaceObject.GUID;
					shotData.parentType = spaceObject.Type;
					shotData.Range = Range;
					ShotData shotData2 = shotData;
					MyPlayer.Instance.Attack(shotData2, this, CurrentWeaponMod.ThrustPerShot, CurrentWeaponMod.RotationPerShot);
					animHelper.SetParameterTrigger(AnimatorHelper.Triggers.Shoot);
					StartCoroutine(MyPlayer.Instance.FpsController.AddCharacterRotationByTime(euler, 0.2f));
				}
			}
			else
			{
				ReloadFromInventory();
			}
		}

		public void ConsumeShotAndPlayEffect(RaycastHit hit, BulletImpact.BulletImpactType bulletImpactType, Vector3 direction)
		{
			Magazine.ChangeQuantity(-1f);
			currentMuzzle.ActivateMuzzle();
			UpdateUI();
			SoundEffect.Play(0);
			PlayBulletHitEffect(hit, bulletImpactType, direction);
		}

		public void PlayBulletHitEffect(RaycastHit hitInfo, BulletImpact.BulletImpactType bulletImpactType, Vector3 direction)
		{
			if (hitInfo.collider != null)
			{
				BulletImpact bulletImpact = null;
				Vector3 forward = hitInfo.normal;
				Vector3 point = hitInfo.point;
				switch (bulletImpactType)
				{
				case BulletImpact.BulletImpactType.Metal:
					bulletImpact = Client.Instance.EffectPrefabs.BulletHitMetal;
					point = hitInfo.point + hitInfo.normal * 0.1f;
					forward = hitInfo.normal;
					break;
				case BulletImpact.BulletImpactType.Flesh:
					bulletImpact = Client.Instance.EffectPrefabs.BulletHitFlesh;
					point = hitInfo.point;
					forward = direction;
					break;
				case BulletImpact.BulletImpactType.Object:
					bulletImpact = Client.Instance.EffectPrefabs.BulletHitObject;
					point = hitInfo.point + hitInfo.normal * 0.1f;
					forward = hitInfo.normal;
					break;
				}
				BulletImpact component = GameObject.Instantiate(bulletImpact.gameObject).GetComponent<BulletImpact>();
				component.transform.position = hitInfo.point + hitInfo.normal * 0.1f;
				component.transform.forward = forward;
				if (bulletImpact.Decal != null)
				{
					MeshRenderer componentInChildren = component.Decal.GetComponentInChildren<MeshRenderer>(includeInactive: true);
					MeshRenderer componentInChildren2 = bulletImpact.Decal.GetComponentInChildren<MeshRenderer>(includeInactive: true);
					componentInChildren.sharedMaterial = componentInChildren2.sharedMaterial;
					component.Decal.transform.SetParent(hitInfo.transform);
				}
				component.Play();
			}
		}

		public Vector3 CalculateRecoil(float maxHorizontal, float maxVertical)
		{
			return new Vector3(UnityEngine.Random.Range(0f - maxVertical, 0f), UnityEngine.Random.Range(0f - maxHorizontal, maxHorizontal), 0f) * ((!MyPlayer.Instance.FpsController.IsCrouch) ? 1f : CrouchAimMultiplier);
		}

		public void ChangeMod(int mod, bool send = true)
		{
			if (Mods.Count > 1)
			{
				lastFireTime = Time.time;
				CurrentWeaponMod = Mods[mod];
				if (DynamicObj.Parent is MyPlayer && (DynamicObj.Parent as MyPlayer).CurrentActiveItem == this)
				{
					SetMuzzleFlash();
				}
				if (send)
				{
					SetStatsForSending(mod);
				}
				Client.Instance.CanvasManager.CanvasUI.HelmetHud.CheckFireMod();
			}
		}

		public void IncrementMod()
		{
			if (Mods.IndexOf(CurrentWeaponMod) + 1 >= Mods.Count)
			{
				ChangeMod(0);
			}
			else
			{
				ChangeMod(Mods.IndexOf(CurrentWeaponMod) + 1);
			}
		}

		public void SetMuzzleFlash()
		{
			if (MyPlayer.Instance.MuzzleFlashTransform.childCount != 0)
			{
				GameObject.Destroy(MyPlayer.Instance.MuzzleFlashTransform.GetChild(0).gameObject);
			}
			currentMuzzle = GameObject.Instantiate(MuzzleFlash, NormalTipOfTheGun.position, NormalTipOfTheGun.rotation).GetComponent<MuzzleActivator>();
			currentMuzzle.transform.SetParent(MyPlayer.Instance.MuzzleFlashTransform);
		}

		public override void GetReloadType(out ItemType type, out bool getHighestQuantity)
		{
			type = ReloadWithTypes[0];
			getHighestQuantity = true;
		}

		private void LateUpdate()
		{
			if (DynamicObj.Parent is MyPlayer && Client.IsGameBuild && MyPlayer.Instance.CurrentActiveItem == this)
			{
				if (currentMuzzle.transform != MyPlayer.Instance.MuzzleFlashTransform)
				{
					currentMuzzle.transform.SetParent(MyPlayer.Instance.MuzzleFlashTransform);
					currentMuzzle.transform.localPosition = Vector3.zero;
					currentMuzzle.transform.localRotation = Quaternion.identity;
				}
				MyPlayer.Instance.MuzzleFlashTransform.position = NormalTipOfTheGun.position;
				MyPlayer.Instance.MuzzleFlashTransform.rotation = NormalTipOfTheGun.rotation;
			}
		}

		public override void ChangeEquip(EquipType type, Player pl)
		{
			if (pl == null)
			{
				return;
			}
			if (pl is OtherPlayer)
			{
				(pl as OtherPlayer).CurrentWeapon = ((type != EquipTo) ? null : this);
				if (NormalTipOfTheGun.childCount != 0)
				{
					GameObject.Destroy(NormalTipOfTheGun.GetChild(0).gameObject);
				}
				currentMuzzle = GameObject.Instantiate(MuzzleFlash, NormalTipOfTheGun.position, NormalTipOfTheGun.rotation).GetComponent<MuzzleActivator>();
				currentMuzzle.transform.SetParent(NormalTipOfTheGun);
				currentMuzzle.currentSmoke.SetActive(value: false);
			}
			else if (pl is MyPlayer)
			{
				if (type == EquipTo)
				{
					SetMuzzleFlash();
				}
				else
				{
					ToggleZoomCamera(status: false);
				}
			}
		}

		public void SetStatsForSending(int? currentMod = null)
		{
			DynamicObject dynamicObj = DynamicObj;
			WeaponStats statsData = new WeaponStats
			{
				CurrentMod = currentMod
			};
			dynamicObj.SendStatsMessage(null, statsData);
		}

		public override void ProcesStatsData(DynamicObjectStats dos)
		{
			base.ProcesStatsData(dos);
			WeaponStats weaponStats = dos as WeaponStats;
			if (weaponStats.CurrentMod.HasValue && weaponStats.CurrentMod.Value != Mods.IndexOf(CurrentWeaponMod))
			{
				ChangeMod(weaponStats.CurrentMod.Value, send: false);
			}
			UpdateUI();
		}

		public bool IsInRange()
		{
			return true;
		}

		public override bool ProcessSlotChange(Inventory inv, InventorySlot mySlot, InventorySlot nextSlot)
		{
			if (animHelper.GetParameterBool(AnimatorHelper.Parameter.Reloading))
			{
				return true;
			}
			if (mySlot.SlotType == InventorySlot.Type.Hands && nextSlot.CanFitItem(this))
			{
				inv.ExitCombatStance();
				return false;
			}
			if (Magazine != null && nextSlot.Item == null && nextSlot.CanFitItem(Magazine) && nextSlot.SlotType != 0)
			{
				inv.AddToInventory(Magazine, nextSlot, null);
				UpdateUI();
				return true;
			}
			return false;
		}

		public override bool CanReloadOnInteract(Item item)
		{
			return item is Magazine && Magazine == null;
		}

		public override bool CustomCollidereToggle(bool isEnabled)
		{
			GetComponent<Collider>().enabled = isEnabled;
			if (Magazine != null)
			{
				Magazine.GetComponent<Collider>().enabled = false;
			}
			return true;
		}

		public override DynamicObjectAuxData GetAuxData()
		{
			List<WeaponModData> list = new List<WeaponModData>();
			foreach (WeaponMod mod in Mods)
			{
				list.Add(new WeaponModData
				{
					Damage = mod.Damage,
					PowerCons = mod.PowerConsuption,
					Range = mod.Range,
					RateOfFire = mod.RateOfFire
				});
			}
			WeaponData baseAuxData = GetBaseAuxData<WeaponData>();
			baseAuxData.weaponMods = list;
			baseAuxData.CurrentMod = Mods.IndexOf(CurrentWeaponMod);
			return baseAuxData;
		}

		public override void ReloadStepComplete(Player pl, AnimatorHelper.ReloadStepType reloadStepType, ref Item currentReloadItem, ref Item newReloadItem)
		{
			switch (reloadStepType)
			{
			case AnimatorHelper.ReloadStepType.ReloadStart:
				currentReloadItem.AttachToBone(pl, AnimatorHelper.HumanBones.LeftInteractBone);
				break;
			case AnimatorHelper.ReloadStepType.ItemSwitch:
				newReloadItem.AttachToBone(pl, AnimatorHelper.HumanBones.LeftInteractBone);
				if (currentReloadItem != null)
				{
					currentReloadItem.AttachToBone(pl, AnimatorHelper.HumanBones.Hips, resetTransform: false);
				}
				break;
			case AnimatorHelper.ReloadStepType.ReloadEnd:
				newReloadItem.RequestAttach(magazineSlot);
				UpdateUI();
				break;
			case AnimatorHelper.ReloadStepType.UnloadEnd:
				UpdateUI();
				break;
			}
			if (IsSpecialStance && CanZoom && (reloadStepType == AnimatorHelper.ReloadStepType.ReloadEnd || reloadStepType == AnimatorHelper.ReloadStepType.UnloadEnd))
			{
				ToggleZoomCamera(status: true);
			}
		}

		public override string GetInfo()
		{
			string empty = string.Empty;
			return Localization.FireMode + ": " + CurrentWeaponMod.ModsFireMode.ToLocalizedString();
		}

		public override string QuantityCheck()
		{
			return (!(Magazine == null)) ? FormatHelper.CurrentMax(Magazine.Quantity, Magazine.MaxQuantity) : "0";
		}

		public virtual void ToggleZoomCamera(bool status)
		{
		}
	}
}
