using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using ZeroGravity.CharacterMovement;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;
using OpenHellion.Net;

namespace ZeroGravity.Objects
{
	public class OtherPlayer : Player
	{
		public delegate void InteractLockDelegate();

		public InteractLockDelegate OnIteractStart;

		public InteractLockDelegate OnIteractComplete;

		public InteractLockDelegate OnLockStart;

		public InteractLockDelegate OnLockComplete;

		public OtherCharacterController tpsController;

		public List<DynamicObject> DynamicObjects = new List<DynamicObject>();

		public List<DynamicObjectDetails> DynamicObjectDetailsQueue;

		public Jetpack CurrentJetpack;

		public Helmet CurrentHelmet;

		public Weapon CurrentWeapon;

		public bool isDrilling;

		public AnimatorHelper AnimatorHelperHair;

		public SkinnedMeshRenderer hairMesh;

		public GameObject HairObject;

		private float movementReceivedTime = -1f;

		private Vector3 movementTargetLocalPosition;

		private Quaternion movementTargetLocalRotation;

		private float movementTimestamp = -1f;

		private Vector3 movementLocalVelocity;

		private Vector3 movementLocalVelocityCorrection;

		public override SpaceObjectType Type => SpaceObjectType.Player;

		public new SpaceObject Parent
		{
			get => base.Parent;
			set => base.Parent = value;
		}

		public void UpdateMovement()
		{
			if (Time.time - movementReceivedTime <= 1f)
			{
				transform.localPosition +=
					(movementLocalVelocity + movementLocalVelocityCorrection) * Time.deltaTime;
				transform.localRotation = Quaternion.Slerp(transform.localRotation,
					movementTargetLocalRotation, Mathf.Pow(Time.time - movementReceivedTime, 0.5f));
				if (CurrentRoomTrigger != null && CurrentRoomTrigger.GravityForce.IsNotEpsilonZero() &&
				    CurrentRoomTrigger.UseGravity)
				{
					transform.localPosition +=
						Vector3.Project(movementTargetLocalPosition - transform.localPosition, transform.up) *
						0.1f;
				}
			}
		}

		public void SetMovementData(Vector3 localPosition, Quaternion localRotation, Vector3 localVelocity,
			float timestamp)
		{
			movementReceivedTime = Time.time;
			float num = movementTimestamp;
			Vector3 vector = movementTargetLocalPosition;
			Quaternion quaternion = movementTargetLocalRotation;
			Vector3 vector2 = movementLocalVelocity;
			movementTimestamp = timestamp;
			float num2 = !(num > 0f) ? 0f : movementTimestamp - num;
			movementTargetLocalPosition = localPosition;
			movementLocalVelocity = localVelocity;
			movementLocalVelocityCorrection =
				!(num2 < 1f) ? Vector3.zero : (vector - transform.localPosition) * num2;
			movementTargetLocalRotation = localRotation;
			if (movementLocalVelocity == Vector3.zero ||
			    Vector3.Dot(movementLocalVelocity.normalized, vector2.normalized) < 0f)
			{
				transform.localPosition = movementTargetLocalPosition;
			}

			if (movementTargetLocalRotation == quaternion)
			{
				transform.localRotation = movementTargetLocalRotation;
			}
		}

		protected void Awake()
		{
			EventSystem.AddListener(typeof(KillPlayerMessage), KillPlayerMessageListener);
			EventSystem.AddListener(typeof(PlayerDrillingMessage), PlayerDrillingMessageListener);
			EventSystem.AddListener(typeof(PlayerStatsMessage), PlayerStatsMessageListener);
			if (tpsController == null)
			{
				tpsController = transform.GetComponent<OtherCharacterController>();
			}

			InitInventory();
			tpsController.PlayerName = PlayerName;
		}

		private void InitInventory()
		{
			if (Inventory == null)
			{
				tpsController.RecreateRig();
				Inventory = new Inventory(this, tpsController.animHelper);
			}
		}

		public void SetOutfitParent(List<Transform> children, Transform parentTransform, bool activeGeometry)
		{
			foreach (Transform child in children)
			{
				child.parent = parentTransform;
				child.localScale = Vector3.one;
				child.localPosition = Vector3.zero;
				child.localRotation =
					Quaternion.Euler(!(child.name == "Root") ? Vector3.zero : new Vector3(0f, 90f, -90f));
				child.gameObject.SetActive(activeGeometry);
			}
		}

		public void EquipOutfit(Outfit o)
		{
			o.FoldedOutfitTrans.gameObject.SetActive(value: false);
			o.transform.parent = transform;
			RemoveOutfit();
			tpsController.CurrentOutfit = o;
			SetOutfitParent(o.OutfitTrans.GetChildren(), tpsController.Outfit, activeGeometry: true);
			RefreshOutfitData();
			tpsController.TransitionHelperGO.transform.parent =
				AnimHelper.GetBone(AnimatorHelper.HumanBones.Spine2);
			tpsController.TransitionHelperGO.transform.Reset();
			Inventory.SetOutfit(o);
			InventorySlot inventorySlot = tpsController.CurrentOutfit.GetSlotsByGroup(InventorySlot.Group.Helmet).Values
				.FirstOrDefault();
			if (inventorySlot != null && inventorySlot.Item != null)
			{
				Helmet helmet = inventorySlot.Item as Helmet;
				helmet.ChangeEquip(Item.EquipType.EquipInventory, this);
				helmet.gameObject.SetActive(value: true);
				helmet.AttachToObject(inventorySlot, sendAttachMessage: false);
			}

			InventorySlot inventorySlot2 = tpsController.CurrentOutfit.GetSlotsByGroup(InventorySlot.Group.Jetpack)
				.Values.FirstOrDefault();
			if (inventorySlot2 != null && inventorySlot2.Item != null)
			{
				Jetpack jetpack = inventorySlot2.Item as Jetpack;
				jetpack.ChangeEquip(Item.EquipType.EquipInventory, this);
				jetpack.gameObject.SetActive(value: true);
				jetpack.AttachToObject(inventorySlot2, sendAttachMessage: false);
			}

			if (HairObject != null)
			{
				HairObject.transform.parent = AnimatorHelperHair.GetBone(AnimatorHelper.HumanBones.Head);
				HairObject.transform.localPosition = Vector3.zero;
				HairObject.transform.localScale = Vector3.one;
				hairMesh = HairObject.GetComponent<SkinnedMeshRenderer>();
			}
		}

		private void RemoveOutfit()
		{
			if (tpsController.CurrentOutfit != null)
			{
				tpsController.CurrentOutfit.SetOutfitParent(tpsController.Outfit.GetChildren(),
					tpsController.CurrentOutfit.OutfitTrans, activateGeometry: false);
				tpsController.CurrentOutfit.FoldedOutfitTrans.gameObject.SetActive(value: true);
				return;
			}

			foreach (Transform child in tpsController.Outfit.GetChildren())
			{
				child.parent = tpsController.BasicOutfitHolder;
				child.gameObject.SetActive(value: false);
			}
		}

		public void TakeOffOutfit()
		{
			InventorySlot inventorySlot = tpsController.CurrentOutfit.GetSlotsByGroup(InventorySlot.Group.Helmet).Values
				.FirstOrDefault();
			if (inventorySlot != null && inventorySlot.Item != null)
			{
				Helmet helmet = inventorySlot.Item as Helmet;
				helmet.ChangeEquip(Item.EquipType.Inventory, this);
				helmet.gameObject.SetActive(value: false);
				helmet.transform.parent = tpsController.CurrentOutfit.transform;
			}

			InventorySlot inventorySlot2 = tpsController.CurrentOutfit.GetSlotsByGroup(InventorySlot.Group.Jetpack)
				.Values.FirstOrDefault();
			if (inventorySlot2 != null && inventorySlot2.Item != null)
			{
				Jetpack jetpack = inventorySlot2.Item as Jetpack;
				jetpack.ChangeEquip(Item.EquipType.Inventory, this);
				jetpack.gameObject.SetActive(value: false);
				jetpack.transform.parent = tpsController.CurrentOutfit.transform;
			}

			RemoveOutfit();
			foreach (Transform child in tpsController.BasicOutfitHolder.GetChildren())
			{
				child.parent = tpsController.Outfit;
				child.localPosition = Vector3.zero;
				child.localRotation =
					Quaternion.Euler(!(child.name == "Root") ? Vector3.zero : new Vector3(0f, 90f, -90f));
				child.gameObject.SetActive(value: true);
			}

			tpsController.CurrentOutfit = null;
			RefreshOutfitData();
			Inventory.SetOutfit(null);
			tpsController.TransitionHelperGO.transform.parent =
				AnimHelper.GetBone(AnimatorHelper.HumanBones.Spine2);
			tpsController.TransitionHelperGO.transform.Reset();
			if (HairObject != null)
			{
				HairObject.transform.parent = AnimatorHelperHair.GetBone(AnimatorHelper.HumanBones.Head);
				HairObject.transform.localPosition = Vector3.zero;
				HairObject.transform.localScale = Vector3.one;
				hairMesh = HairObject.GetComponent<SkinnedMeshRenderer>();
			}
		}

		public void ProcessMovementMessage(CharacterMovementMessage cmm)
		{
			if (cmm.GUID != Guid)
			{
				return;
			}

			if (cmm.ParentType == SpaceObjectType.PlayerPivot && Parent is SpaceObjectVessel)
			{
				Parent = World.SolarSystem.GetArtificialBody(Guid);
			}
			else if (cmm.ParentType != SpaceObjectType.PlayerPivot && Parent is Pivot)
			{
				Pivot pivot = Parent as Pivot;
				Parent = World.GetVessel(cmm.ParentGUID);
				World.SolarSystem.RemoveArtificialBody(Guid);
				Destroy(pivot.gameObject);
			}

			if (cmm.PivotReset && Parent is Pivot)
			{
				Vector3 value = cmm.PivotPositionCorrection.ToVector3();
				Parent.ModifyPositionAndRotation(value);
				ModifyPositionAndRotation(cmm.TransformData.LocalPosition.ToVector3() - transform.localPosition);
			}
			else if (Parent is SpaceObjectVessel && Parent.Guid != cmm.ParentGUID)
			{
				SpaceObjectVessel vessel = World.GetVessel(cmm.ParentGUID);
				if (vessel is not null)
				{
					Parent = vessel;
				}
			}

			tpsController.MovementMessageReceived(cmm);
		}

		private void UpdateReferenceHead()
		{
			Transform[] array = new Transform[tpsController.ReferenceHead.bones.Length];
			Transform bone = AnimHelper.GetBone(AnimatorHelper.HumanBones.Spine2);
			for (int i = 0; i < 6; i++)
			{
				array[i] = bone.FindChildByName(tpsController.ReferenceHead.bones[i].name);
			}

			tpsController.ReferenceHead.bones = array;
		}

		public void RefreshOutfitData()
		{
			tpsController.animHelper.RebindAndReload();
			tpsController.RagdollComponent.RefreshRagdollVariables();
			Transform[] array = new Transform[tpsController.ReferenceHead.bones.Length];
			Transform bone = tpsController.animHelper.GetBone(AnimatorHelper.HumanBones.Spine2);
			for (int i = 0; i < tpsController.HeadSkin.bones.Length; i++)
			{
				array[i] = bone.FindChildByName(tpsController.ReferenceHead.bones[i].name);
			}

			tpsController.HeadSkin.bones = array;
			tpsController.animHelper.aimIKController.UpdateIKBones();
		}

		protected void KillPlayerMessageListener(NetworkData data)
		{
			KillPlayerMessage killPlayerMessage = data as KillPlayerMessage;
			if (killPlayerMessage.GUID == Guid)
			{
				if (Inventory.Outfit != null)
				{
					RemoveOutfit();
				}

				if (Inventory.ItemInHands != null && Inventory.ItemInHands is HandDrill)
				{
					Destroy((Inventory.ItemInHands as HandDrill).effectScript.gameObject);
				}

				gameObject.SetActive(value: false);
				if (killPlayerMessage.CorpseDetails != null)
				{
					Corpse.SpawnCorpse(killPlayerMessage.CorpseDetails, this);
				}

				Destroy(gameObject);
			}
		}

		public void AttachDynamicObjectsOnPlayer()
		{
			if (DynamicObjectDetailsQueue != null)
			{
				foreach (DynamicObjectDetails item in DynamicObjectDetailsQueue)
				{
					DynamicObjects.Add(DynamicObject.SpawnDynamicObject(item, this));
				}
			}

			DynamicObjectDetailsQueue = null;
		}

		private void Update()
		{
			if (!isDrilling || Inventory.CheckIfItemInHandsIsType<HandDrill>())
			{
			}

			tpsController.animHelper.UpdateVelocities();
		}

		public static OtherPlayer SpawnPlayer(SpawnObjectResponseData data)
		{
			SpawnCharacterResponseData spawnCharacterResponseData = data as SpawnCharacterResponseData;
			return SpawnPlayer(spawnCharacterResponseData.Details);
		}

		public static OtherPlayer SpawnPlayer(CharacterDetails characterDetails, SpaceObject parent = null)
		{
			if (characterDetails.GUID == MyPlayer.Instance.Guid)
			{
				return null;
			}

			if (World.GetPlayer(characterDetails.GUID) != null)
			{
				return null;
			}

			if (parent == null)
			{
				if (characterDetails.ParentID == MyPlayer.Instance.Parent.Guid)
				{
					parent = MyPlayer.Instance.Parent;
				}
				else if (characterDetails.ParentType == SpaceObjectType.Ship ||
				         characterDetails.ParentType == SpaceObjectType.Asteroid ||
				         characterDetails.ParentType == SpaceObjectType.PlayerPivot ||
				         characterDetails.ParentType == SpaceObjectType.Station)
				{
					parent = World.SolarSystem.GetArtificialBody(characterDetails.ParentID);
				}
			}

			GameObject gameObject =
				Instantiate(Resources.Load("Models/Units/Characters/ThirdPersonCharacter"),
					new Vector3(20000f, 20000f, 20000f), Quaternion.identity) as GameObject;
			gameObject.SetActive(value: false);
			OtherPlayer otherPlayer = gameObject.AddComponent<OtherPlayer>();
			otherPlayer.tpsController = otherPlayer.GetComponent<OtherCharacterController>();
			byte b = 1;
			byte b2 = 0;
			GenderSettings component = gameObject.GetComponent<GenderSettings>();
			GenderSettings.GenderItem genderItem = null;
			foreach (GenderSettings.GenderItem setting in component.settings)
			{
				if (setting.Gender != characterDetails.Gender)
				{
					Destroy(setting.Outfit.gameObject);
				}
				else
				{
					genderItem = setting;
				}
			}

			if (genderItem == null)
			{
				Debug.LogError("AAAAAAAAAAAAAA, trece lice dzenderica prsla");
				return null;
			}

			b2 = (byte)(characterDetails.Gender != 0 ? 1 : 0);
			otherPlayer.AnimatorHelperHair = genderItem.Outfit.GetComponent<AnimatorHelper>();
			otherPlayer.tpsController.Outfit = genderItem.Outfit;
			GameObject gameObject2 =
				Instantiate(Resources.Load("Models/Units/Characters/Heads/" +
				                                              characterDetails.Gender + "/Head" +
				                                              b)) as GameObject;
			gameObject2.transform.parent = gameObject.transform;
			gameObject2.transform.localPosition = new Vector3(0f, -1.34f, 0f);
			gameObject2.transform.localRotation = Quaternion.identity;
			gameObject2.transform.localScale = Vector3.one;
			gameObject2.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = ShadowCastingMode.On;
			if (b2 != 0)
			{
				otherPlayer.HairObject =
					Instantiate(Resources.Load("Models/Units/Characters/Hairs/" +
					                                              characterDetails.Gender + "/Hair" +
					                                              b2)) as GameObject;
				otherPlayer.HairObject.transform.parent =
					otherPlayer.AnimatorHelperHair.GetBone(AnimatorHelper.HumanBones.Head);
				otherPlayer.HairObject.transform.localPosition = Vector3.zero;
				otherPlayer.HairObject.transform.localScale = Vector3.one;
				otherPlayer.hairMesh = otherPlayer.HairObject.GetComponent<SkinnedMeshRenderer>();
			}

			otherPlayer.tpsController.HeadSkin = gameObject2.GetComponent<SkinnedMeshRenderer>();
			otherPlayer.tpsController.HeadSkin.rootBone =
				otherPlayer.AnimatorHelperHair.GetBone(AnimatorHelper.HumanBones.Spine2);
			otherPlayer.tpsController.TPSAnimator = otherPlayer.AnimatorHelperHair.GetComponent<Animator>();
			otherPlayer.tpsController.RagdollComponent = genderItem.Outfit.GetComponent<RagdollHelper>();
			otherPlayer.tpsController.animHelper = otherPlayer.AnimatorHelperHair;
			otherPlayer.tpsController.hips = otherPlayer.AnimatorHelperHair.GetBone(AnimatorHelper.HumanBones.Hips);
			otherPlayer.tpsController.spine2 = otherPlayer.AnimatorHelperHair.GetBone(AnimatorHelper.HumanBones.Spine2);
			otherPlayer.tpsController.ReferenceHead.rootBone =
				otherPlayer.AnimatorHelperHair.GetBone(AnimatorHelper.HumanBones.Spine2);
			otherPlayer.UpdateReferenceHead();
			otherPlayer.RefreshOutfitData();
			otherPlayer.tpsController.TransitionHelperGO.transform.parent =
				otherPlayer.AnimatorHelperHair.GetBone(AnimatorHelper.HumanBones.Spine2);
			otherPlayer.tpsController.TransitionHelperGO.GetComponent<TransitionTriggerHelper>()
				.SetTransferableObject(otherPlayer);
			otherPlayer.tpsController.SetPlayer(otherPlayer);
			otherPlayer.Guid = characterDetails.GUID;
			otherPlayer.PlayerName = characterDetails.Name;
			otherPlayer.PlayerId = characterDetails.PlayerId;
			gameObject.name = "Character_" + otherPlayer.Guid;
			otherPlayer.Parent = parent;
			TargetingPoint[] componentsInChildren = otherPlayer.GetComponentsInChildren<TargetingPoint>();
			foreach (TargetingPoint targetingPoint in componentsInChildren)
			{
				targetingPoint.MainObject = otherPlayer;
			}

			World.AddPlayer(otherPlayer.Guid, otherPlayer);
			SceneSpawnPoint sceneSpawnPoint = null;
			if (characterDetails.SpawnPointID > 0)
			{
				if (parent.Type == SpaceObjectType.Ship)
				{
					sceneSpawnPoint =
						(parent as Ship).GetStructureObject<SceneSpawnPoint>(characterDetails.SpawnPointID);
				}

				if (sceneSpawnPoint != null)
				{
					otherPlayer.transform.position = sceneSpawnPoint.transform.position;
					otherPlayer.transform.rotation = sceneSpawnPoint.transform.rotation;
				}
			}
			else
			{
				otherPlayer.transform.localPosition = characterDetails.TransformData.LocalPosition.ToVector3();
				otherPlayer.transform.localRotation = characterDetails.TransformData.LocalRotation.ToQuaternion();
			}

			otherPlayer.tpsController.SetTargetPositionAndRotation(otherPlayer.transform.localPosition,
				otherPlayer.transform.localRotation, instant: true);
			gameObject.SetActive(value: true);
			otherPlayer.PlayerStatsMessageListener(new PlayerStatsMessage
			{
				GUID = otherPlayer.Guid,
				AnimationStatesMask = characterDetails.AnimationStatsMask,
				LockedToTriggerID = characterDetails.LockedToTriggerID
			});
			if (characterDetails.DynamicObjects != null)
			{
				if (otherPlayer.tpsController == null)
				{
					otherPlayer.DynamicObjectDetailsQueue = characterDetails.DynamicObjects;
				}
				else
				{
					if (otherPlayer.Inventory == null)
					{
						otherPlayer.InitInventory();
					}

					DynamicObjectDetails dynamicObjectDetails =
						characterDetails.DynamicObjects.Find((DynamicObjectDetails x) =>
							x.AttachData.IsAttached && x.AttachData.InventorySlotID == -2);
					if (dynamicObjectDetails != null)
					{
						DynamicObject.SpawnDynamicObject(dynamicObjectDetails, otherPlayer);
					}

					foreach (DynamicObjectDetails dynamicObject in characterDetails.DynamicObjects)
					{
						if (dynamicObject != dynamicObjectDetails)
						{
							DynamicObject.SpawnDynamicObject(dynamicObject, otherPlayer);
						}
					}
				}
			}

			if (sceneSpawnPoint != null && sceneSpawnPoint.Executor != null)
			{
				if (otherPlayer.tpsController == null)
				{
					otherPlayer.tpsController = otherPlayer.transform.GetComponent<OtherCharacterController>();
				}

				sceneSpawnPoint.Executor.SetExecutorDetails(new SceneTriggerExecutorDetails
				{
					PlayerThatActivated = otherPlayer.Guid,
					InSceneID = sceneSpawnPoint.Executor.InSceneID,
					IsImmediate = true,
					IsFail = false,
					CurrentStateID = sceneSpawnPoint.Executor.CurrentStateID,
					NewStateID = sceneSpawnPoint.Executor.GetStateID(sceneSpawnPoint.ExecutorState)
				}, isInstant: false, null, checkCurrentState: false);
			}

			if (World.CharacterInteractionStatesQueue.ContainsKey(otherPlayer.Guid))
			{
				World.CharacterInteractionStatesQueue[otherPlayer.Guid].Executor
					.CharacterInteractInstant(World.CharacterInteractionStatesQueue[otherPlayer.Guid],
						otherPlayer.Guid);
				World.CharacterInteractionStatesQueue.Remove(otherPlayer.Guid);
			}

			return otherPlayer;
		}

		public void AnimInteraction_LockEnter()
		{
			if (OnLockStart != null)
			{
				OnLockStart();
			}
		}

		public void AnimInteraction_LockExit()
		{
			if (OnLockComplete != null)
			{
				OnLockComplete();
			}
		}

		public void AnimInteraction_InteractEnter()
		{
			if (OnIteractStart != null)
			{
				OnIteractStart();
			}
		}

		public void AnimInteraction_InteractExit()
		{
			if (OnIteractComplete != null)
			{
				OnIteractComplete();
			}
		}

		public void AnimInteraction_NoneEnter()
		{
		}

		public void AnimInteraction_NoneExit()
		{
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			EventSystem.RemoveListener(typeof(KillPlayerMessage), KillPlayerMessageListener);
			EventSystem.RemoveListener(typeof(PlayerDrillingMessage), PlayerDrillingMessageListener);
			EventSystem.RemoveListener(typeof(PlayerStatsMessage), PlayerStatsMessageListener);
			World.RemovePlayer(Guid);
		}

		public void PlayerDrillingMessageListener(NetworkData data)
		{
			PlayerDrillingMessage playerDrillingMessage = data as PlayerDrillingMessage;
			if (playerDrillingMessage.DrillersGUID == Guid && Inventory.CheckIfItemInHandsIsType<HandDrill>())
			{
				HandDrill handDrill = Inventory.ItemInHands as HandDrill;
				isDrilling = playerDrillingMessage.isDrilling;
				handDrill.effectScript.ToggleEffect(!playerDrillingMessage.dontPlayEffect);
				handDrill.HurtTrigger.Activate(isDrilling);
				handDrill.drillAnimator.SetBool("Drilling", isDrilling);
			}
		}

		private void PlayerStatsMessageListener(NetworkData data)
		{
			PlayerStatsMessage psm = data as PlayerStatsMessage;
			if (Guid != psm.GUID)
			{
				return;
			}

			tpsController.animHelper.animationData.ReloadType = psm.ReloadType;
			tpsController.animHelper.animationData.IsCrouch = (psm.AnimationStatesMask & 1) != 0;
			tpsController.animHelper.animationData.IsJump = (psm.AnimationStatesMask & 2) != 0;
			tpsController.animHelper.animationData.IsZeroG = (psm.AnimationStatesMask & 4) != 0;
			tpsController.animHelper.animationData.isInStance = (psm.AnimationStatesMask & 8) != 0;
			tpsController.animHelper.animationData.IsReloading = (psm.AnimationStatesMask & 0x10) != 0;
			tpsController.animHelper.animationData.IsGrounded = (psm.AnimationStatesMask & 0x20) != 0;
			tpsController.animHelper.animationData.IsHolster = (psm.AnimationStatesMask & 0x40) != 0;
			tpsController.animHelper.animationData.IsDraw = (psm.AnimationStatesMask & 0x80) != 0;
			tpsController.animHelper.animationData.CancelInteract = (psm.AnimationStatesMask & 0x100) != 0;
			tpsController.animHelper.animationData.IsFalling = (psm.AnimationStatesMask & 0x400) != 0;
			tpsController.animHelper.animationData.isEquipping = (psm.AnimationStatesMask & 0x800) != 0;
			tpsController.animHelper.animationData.TouchingFloor = (psm.AnimationStatesMask & 0x1000) != 0;
			tpsController.animHelper.animationData.UsingTool = (psm.AnimationStatesMask & 0x2000) != 0;
			tpsController.animHelper.animationData.IsEmote = (psm.AnimationStatesMask & 0x4000) != 0;
			tpsController.animHelper.animationData.IsMelee = (psm.AnimationStatesMask & 0x8000) != 0;
			tpsController.animHelper.animationData.UsingLadder = (psm.AnimationStatesMask & 0x10000) != 0;
			tpsController.animHelper.animationData.UseConsumable = (psm.AnimationStatesMask & 0x20000) != 0;
			tpsController.animHelper.animationData.WeaponActivated = (psm.AnimationStatesMask & 0x40000) != 0;
			tpsController.UpdateAnimatorOneFrame();
			if (psm.LockedToTriggerID != null && Parent is SpaceObjectVessel)
			{
				LockedToTrigger = Parent.GeometryRoot.GetComponentsInChildren<BaseSceneTrigger>(includeInactive: true)
					.FirstOrDefault((BaseSceneTrigger m) => m.GetID() == psm.LockedToTriggerID);
			}
			else
			{
				LockedToTrigger = null;
			}

			float num = psm.DamageList == null
				? 0f
				: psm.DamageList.Where((PlayerDamage m) => m.HurtType == HurtType.Shot)
					.Sum((PlayerDamage m) => m.Amount);
			if (num > float.Epsilon)
			{
				tpsController.HealthSounds.Play(1);
			}

			float num2 = psm.DamageList == null
				? 0f
				: psm.DamageList.Where((PlayerDamage m) => m.HurtType == HurtType.Pressure)
					.Sum((PlayerDamage m) => m.Amount);
			if (num2 > float.Epsilon)
			{
				tpsController.HealthSounds.Play(0);
			}

			float num3 = psm.DamageList == null
				? 0f
				: psm.DamageList.Where((PlayerDamage m) => m.HurtType == HurtType.Impact)
					.Sum((PlayerDamage m) => m.Amount);
			if (num3 > float.Epsilon)
			{
				tpsController.HealthSounds.Play(1);
			}

			float num4 = psm.DamageList == null
				? 0f
				: psm.DamageList.Where((PlayerDamage m) => m.HurtType == HurtType.Suffocate)
					.Sum((PlayerDamage m) => m.Amount);
			if (num4 > float.Epsilon)
			{
				tpsController.HealthSounds.Play(0);
			}

			float num5 = psm.DamageList == null
				? 0f
				: psm.DamageList.Where((PlayerDamage m) => m.HurtType == HurtType.Frost)
					.Sum((PlayerDamage m) => m.Amount);
			if (num5 > float.Epsilon)
			{
			}

			float num6 = psm.DamageList == null
				? 0f
				: psm.DamageList.Where((PlayerDamage m) => m.HurtType == HurtType.Heat)
					.Sum((PlayerDamage m) => m.Amount);
			if (num6 > float.Epsilon)
			{
				tpsController.HealthSounds.Play(1);
			}

			float num7 = psm.DamageList == null
				? 0f
				: psm.DamageList.Where((PlayerDamage m) => m.HurtType == HurtType.Shred)
					.Sum((PlayerDamage m) => m.Amount);
			if (num7 > float.Epsilon)
			{
				tpsController.HealthSounds.Play(1);
			}

			float num8 = psm.DamageList == null
				? 0f
				: psm.DamageList.Where((PlayerDamage m) => m.HurtType == HurtType.SpaceExposure)
					.Sum((PlayerDamage m) => m.Amount);
			if (num8 > float.Epsilon)
			{
				tpsController.HealthSounds.Play(1);
			}
		}

		public void SetGlobalPositionAndRotation(Vector3 position, Quaternion rotation)
		{
			transform.position = position;
			transform.rotation = rotation;
			tpsController.SetTargetPositionAndRotation(transform.localPosition, transform.localRotation,
				instant: true);
		}

		public override void EnterVessel(SpaceObjectVessel vessel)
		{
			tpsController.UpdateMovementPosition = true;
		}

		public override void ExitVessel(bool forceExit)
		{
			tpsController.UpdateMovementPosition = true;
		}

		public override void ModifyPositionAndRotation(Vector3? position = null, Quaternion? rotation = null)
		{
			tpsController.ModifyPositionAndRotation(position, rotation);
		}

		public override void SetTargetPositionAndRotation(Vector3? position, Vector3? forward, Vector3? up,
			bool instant = false, double time = -1.0)
		{
			IsInVisibilityRange = true;
			if (forward.HasValue && up.HasValue)
			{
				tpsController.SetTargetPositionAndRotation(position, Quaternion.LookRotation(forward.Value, up.Value),
					instant);
			}
			else
			{
				tpsController.SetTargetPositionAndRotation(position, null, instant);
			}
		}

		public override void DockedVesselParentChanged(SpaceObjectVessel vessel)
		{
		}

		public override void OnGravityChanged(Vector3 oldGravity)
		{
		}

		public override void RoomChanged(SceneTriggerRoom prevRoomTrigger)
		{
			base.RoomChanged(prevRoomTrigger);
		}
	}
}
