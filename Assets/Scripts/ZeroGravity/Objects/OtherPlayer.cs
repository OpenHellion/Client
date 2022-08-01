using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using ZeroGravity.CharacterMovement;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;

namespace ZeroGravity.Objects
{
	public class OtherPlayer : Player
	{
		public delegate void InteractLockDelegate();

		[CompilerGenerated]
		private sealed class _003CPlayerStatsMessageListener_003Ec__AnonStorey0
		{
			internal PlayerStatsMessage psm;

			internal bool _003C_003Em__0(BaseSceneTrigger m)
			{
				return m.GetID() == psm.LockedToTriggerID;
			}
		}

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

		[CompilerGenerated]
		private static Predicate<DynamicObjectDetails> _003C_003Ef__am_0024cache0;

		[CompilerGenerated]
		private static Func<PlayerDamage, bool> _003C_003Ef__am_0024cache1;

		[CompilerGenerated]
		private static Func<PlayerDamage, float> _003C_003Ef__am_0024cache2;

		[CompilerGenerated]
		private static Func<PlayerDamage, bool> _003C_003Ef__am_0024cache3;

		[CompilerGenerated]
		private static Func<PlayerDamage, float> _003C_003Ef__am_0024cache4;

		[CompilerGenerated]
		private static Func<PlayerDamage, bool> _003C_003Ef__am_0024cache5;

		[CompilerGenerated]
		private static Func<PlayerDamage, float> _003C_003Ef__am_0024cache6;

		[CompilerGenerated]
		private static Func<PlayerDamage, bool> _003C_003Ef__am_0024cache7;

		[CompilerGenerated]
		private static Func<PlayerDamage, float> _003C_003Ef__am_0024cache8;

		[CompilerGenerated]
		private static Func<PlayerDamage, bool> _003C_003Ef__am_0024cache9;

		[CompilerGenerated]
		private static Func<PlayerDamage, float> _003C_003Ef__am_0024cacheA;

		[CompilerGenerated]
		private static Func<PlayerDamage, bool> _003C_003Ef__am_0024cacheB;

		[CompilerGenerated]
		private static Func<PlayerDamage, float> _003C_003Ef__am_0024cacheC;

		[CompilerGenerated]
		private static Func<PlayerDamage, bool> _003C_003Ef__am_0024cacheD;

		[CompilerGenerated]
		private static Func<PlayerDamage, float> _003C_003Ef__am_0024cacheE;

		[CompilerGenerated]
		private static Func<PlayerDamage, bool> _003C_003Ef__am_0024cacheF;

		[CompilerGenerated]
		private static Func<PlayerDamage, float> _003C_003Ef__am_0024cache10;

		public override SpaceObjectType Type
		{
			get
			{
				return SpaceObjectType.Player;
			}
		}

		public new SpaceObject Parent
		{
			get
			{
				return base.Parent;
			}
			set
			{
				base.Parent = value;
			}
		}

		public void UpdateMovement()
		{
			if (Time.time - movementReceivedTime <= 1f)
			{
				base.transform.localPosition += (movementLocalVelocity + movementLocalVelocityCorrection) * Time.deltaTime;
				base.transform.localRotation = Quaternion.Slerp(base.transform.localRotation, movementTargetLocalRotation, (float)System.Math.Pow(Time.time - movementReceivedTime, 0.5));
				if (base.CurrentRoomTrigger != null && base.CurrentRoomTrigger.GravityForce.IsNotEpsilonZero() && base.CurrentRoomTrigger.UseGravity)
				{
					base.transform.localPosition += Vector3.Project(movementTargetLocalPosition - base.transform.localPosition, base.transform.up) * 0.1f;
				}
			}
		}

		public void SetMovementData(Vector3 localPosition, Quaternion localRotation, Vector3 localVelocity, float timestamp)
		{
			movementReceivedTime = Time.time;
			float num = movementTimestamp;
			Vector3 vector = movementTargetLocalPosition;
			Quaternion quaternion = movementTargetLocalRotation;
			Vector3 vector2 = movementLocalVelocity;
			movementTimestamp = timestamp;
			float num2 = ((!(num > 0f)) ? 0f : (movementTimestamp - num));
			movementTargetLocalPosition = localPosition;
			movementLocalVelocity = localVelocity;
			movementLocalVelocityCorrection = ((!(num2 < 1f)) ? Vector3.zero : ((vector - base.transform.localPosition) * num2));
			movementTargetLocalRotation = localRotation;
			if (movementLocalVelocity == Vector3.zero || Vector3.Dot(movementLocalVelocity.normalized, vector2.normalized) < 0f)
			{
				base.transform.localPosition = movementTargetLocalPosition;
			}
			if (movementTargetLocalRotation == quaternion)
			{
				base.transform.localRotation = movementTargetLocalRotation;
			}
		}

		protected void Awake()
		{
			Client.Instance.NetworkController.EventSystem.AddListener(typeof(KillPlayerMessage), KillPlayerMessageListener);
			Client.Instance.NetworkController.EventSystem.AddListener(typeof(PlayerDrillingMessage), PlayerDrillingMessageListener);
			Client.Instance.NetworkController.EventSystem.AddListener(typeof(PlayerStatsMessage), PlayerStatsMessageListener);
			if (tpsController == null)
			{
				tpsController = base.transform.GetComponent<OtherCharacterController>();
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
				child.localRotation = Quaternion.Euler((!(child.name == "Root")) ? Vector3.zero : new Vector3(0f, 90f, -90f));
				child.gameObject.SetActive(activeGeometry);
			}
		}

		public void EquipOutfit(Outfit o)
		{
			o.FoldedOutfitTrans.gameObject.SetActive(false);
			o.transform.parent = base.transform;
			RemoveOutfit();
			tpsController.CurrentOutfit = o;
			SetOutfitParent(o.OutfitTrans.GetChildren(), tpsController.Outfit, true);
			RefreshOutfitData();
			tpsController.TransitionHelperGO.transform.parent = base.AnimHelper.GetBone(AnimatorHelper.HumanBones.Spine2);
			tpsController.TransitionHelperGO.transform.Reset();
			Inventory.SetOutfit(o);
			InventorySlot inventorySlot = tpsController.CurrentOutfit.GetSlotsByGroup(InventorySlot.Group.Helmet).Values.FirstOrDefault();
			if (inventorySlot != null && inventorySlot.Item != null)
			{
				Helmet helmet = inventorySlot.Item as Helmet;
				helmet.ChangeEquip(Item.EquipType.EquipInventory, this);
				helmet.gameObject.SetActive(true);
				helmet.AttachToObject(inventorySlot, false);
			}
			InventorySlot inventorySlot2 = tpsController.CurrentOutfit.GetSlotsByGroup(InventorySlot.Group.Jetpack).Values.FirstOrDefault();
			if (inventorySlot2 != null && inventorySlot2.Item != null)
			{
				Jetpack jetpack = inventorySlot2.Item as Jetpack;
				jetpack.ChangeEquip(Item.EquipType.EquipInventory, this);
				jetpack.gameObject.SetActive(true);
				jetpack.AttachToObject(inventorySlot2, false);
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
				tpsController.CurrentOutfit.SetOutfitParent(tpsController.Outfit.GetChildren(), tpsController.CurrentOutfit.OutfitTrans, false);
				tpsController.CurrentOutfit.FoldedOutfitTrans.gameObject.SetActive(true);
				return;
			}
			foreach (Transform child in tpsController.Outfit.GetChildren())
			{
				child.parent = tpsController.BasicOutfitHolder;
				child.gameObject.SetActive(false);
			}
		}

		public void TakeOffOutfit()
		{
			InventorySlot inventorySlot = tpsController.CurrentOutfit.GetSlotsByGroup(InventorySlot.Group.Helmet).Values.FirstOrDefault();
			if (inventorySlot != null && inventorySlot.Item != null)
			{
				Helmet helmet = inventorySlot.Item as Helmet;
				helmet.ChangeEquip(Item.EquipType.Inventory, this);
				helmet.gameObject.SetActive(false);
				helmet.transform.parent = tpsController.CurrentOutfit.transform;
			}
			InventorySlot inventorySlot2 = tpsController.CurrentOutfit.GetSlotsByGroup(InventorySlot.Group.Jetpack).Values.FirstOrDefault();
			if (inventorySlot2 != null && inventorySlot2.Item != null)
			{
				Jetpack jetpack = inventorySlot2.Item as Jetpack;
				jetpack.ChangeEquip(Item.EquipType.Inventory, this);
				jetpack.gameObject.SetActive(false);
				jetpack.transform.parent = tpsController.CurrentOutfit.transform;
			}
			RemoveOutfit();
			foreach (Transform child in tpsController.BasicOutfitHolder.GetChildren())
			{
				child.parent = tpsController.Outfit;
				child.localPosition = Vector3.zero;
				child.localRotation = Quaternion.Euler((!(child.name == "Root")) ? Vector3.zero : new Vector3(0f, 90f, -90f));
				child.gameObject.SetActive(true);
			}
			tpsController.CurrentOutfit = null;
			RefreshOutfitData();
			Inventory.SetOutfit(null);
			tpsController.TransitionHelperGO.transform.parent = base.AnimHelper.GetBone(AnimatorHelper.HumanBones.Spine2);
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
			if (cmm.GUID != base.GUID || cmm == null)
			{
				return;
			}
			if (cmm.ParentType == SpaceObjectType.PlayerPivot && Parent is SpaceObjectVessel)
			{
				Pivot pivot = (Pivot)(Parent = Client.Instance.SolarSystem.GetArtificialBody(base.GUID) as Pivot);
			}
			else if (cmm.ParentType != SpaceObjectType.PlayerPivot && Parent is Pivot)
			{
				Pivot pivot2 = Parent as Pivot;
				Parent = Client.Instance.GetVessel(cmm.ParentGUID);
				Client.Instance.SolarSystem.RemoveArtificialBody(base.GUID);
				UnityEngine.Object.Destroy(pivot2.gameObject);
			}
			if (cmm.PivotReset && Parent is Pivot)
			{
				Vector3 value = cmm.PivotPositionCorrection.ToVector3();
				Parent.ModifyPositionAndRotation(value);
				ModifyPositionAndRotation(cmm.TransformData.LocalPosition.ToVector3() - base.transform.localPosition);
			}
			else if (Parent is SpaceObjectVessel && Parent.GUID != cmm.ParentGUID)
			{
				SpaceObjectVessel vessel = Client.Instance.GetVessel(cmm.ParentGUID);
				if (vessel != null)
				{
					Parent = vessel;
				}
			}
			tpsController.MovementMessageReceived(cmm);
		}

		private void UpdateReferenceHead()
		{
			Transform[] array = new Transform[tpsController.ReferenceHead.bones.Length];
			Transform bone = base.AnimHelper.GetBone(AnimatorHelper.HumanBones.Spine2);
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
			if (killPlayerMessage.GUID == base.GUID)
			{
				if (Inventory.Outfit != null)
				{
					RemoveOutfit();
				}
				if (Inventory.ItemInHands != null && Inventory.ItemInHands is HandDrill)
				{
					UnityEngine.Object.Destroy((Inventory.ItemInHands as HandDrill).effectScript.gameObject);
				}
				base.gameObject.SetActive(false);
				if (killPlayerMessage.CorpseDetails != null)
				{
					Corpse.SpawnCorpse(killPlayerMessage.CorpseDetails, this);
				}
				UnityEngine.Object.Destroy(base.gameObject);
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
			if (characterDetails.GUID == MyPlayer.Instance.GUID)
			{
				return null;
			}
			if (Client.Instance.GetPlayer(characterDetails.GUID) != null)
			{
				return null;
			}
			if (parent == null)
			{
				if (characterDetails.ParentID == MyPlayer.Instance.Parent.GUID)
				{
					parent = MyPlayer.Instance.Parent;
				}
				else if (characterDetails.ParentType == SpaceObjectType.Ship || characterDetails.ParentType == SpaceObjectType.Asteroid || characterDetails.ParentType == SpaceObjectType.PlayerPivot || characterDetails.ParentType == SpaceObjectType.Station)
				{
					parent = Client.Instance.SolarSystem.GetArtificialBody(characterDetails.ParentID);
				}
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Models/Units/Characters/ThirdPersonCharacter"), new Vector3(20000f, 20000f, 20000f), Quaternion.identity) as GameObject;
			gameObject.SetActive(false);
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
					UnityEngine.Object.Destroy(setting.Outfit.gameObject);
				}
				else
				{
					genderItem = setting;
				}
			}
			if (genderItem == null)
			{
				Dbg.Error("AAAAAAAAAAAAAA, trece lice dzenderica prsla");
				return null;
			}
			b2 = (byte)((characterDetails.Gender != 0) ? 1 : 0);
			otherPlayer.AnimatorHelperHair = genderItem.Outfit.GetComponent<AnimatorHelper>();
			otherPlayer.tpsController.Outfit = genderItem.Outfit;
			GameObject gameObject2 = UnityEngine.Object.Instantiate(Resources.Load("Models/Units/Characters/Heads/" + characterDetails.Gender.ToString() + "/Head" + b)) as GameObject;
			gameObject2.transform.parent = gameObject.transform;
			gameObject2.transform.localPosition = new Vector3(0f, -1.34f, 0f);
			gameObject2.transform.localRotation = Quaternion.identity;
			gameObject2.transform.localScale = Vector3.one;
			gameObject2.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = ShadowCastingMode.On;
			if (b2 != 0)
			{
				otherPlayer.HairObject = UnityEngine.Object.Instantiate(Resources.Load("Models/Units/Characters/Hairs/" + characterDetails.Gender.ToString() + "/Hair" + b2)) as GameObject;
				otherPlayer.HairObject.transform.parent = otherPlayer.AnimatorHelperHair.GetBone(AnimatorHelper.HumanBones.Head);
				otherPlayer.HairObject.transform.localPosition = Vector3.zero;
				otherPlayer.HairObject.transform.localScale = Vector3.one;
				otherPlayer.hairMesh = otherPlayer.HairObject.GetComponent<SkinnedMeshRenderer>();
			}
			otherPlayer.tpsController.HeadSkin = gameObject2.GetComponent<SkinnedMeshRenderer>();
			otherPlayer.tpsController.HeadSkin.rootBone = otherPlayer.AnimatorHelperHair.GetBone(AnimatorHelper.HumanBones.Spine2);
			otherPlayer.tpsController.TPSAnimator = otherPlayer.AnimatorHelperHair.GetComponent<Animator>();
			otherPlayer.tpsController.RagdollComponent = genderItem.Outfit.GetComponent<RagdollHelper>();
			otherPlayer.tpsController.animHelper = otherPlayer.AnimatorHelperHair;
			otherPlayer.tpsController.hips = otherPlayer.AnimatorHelperHair.GetBone(AnimatorHelper.HumanBones.Hips);
			otherPlayer.tpsController.spine2 = otherPlayer.AnimatorHelperHair.GetBone(AnimatorHelper.HumanBones.Spine2);
			otherPlayer.tpsController.ReferenceHead.rootBone = otherPlayer.AnimatorHelperHair.GetBone(AnimatorHelper.HumanBones.Spine2);
			otherPlayer.UpdateReferenceHead();
			otherPlayer.RefreshOutfitData();
			otherPlayer.tpsController.TransitionHelperGO.transform.parent = otherPlayer.AnimatorHelperHair.GetBone(AnimatorHelper.HumanBones.Spine2);
			otherPlayer.tpsController.TransitionHelperGO.GetComponent<TransitionTriggerHelper>().SetTransferableObject(otherPlayer);
			otherPlayer.tpsController.SetPlayer(otherPlayer);
			otherPlayer.GUID = characterDetails.GUID;
			otherPlayer.PlayerName = characterDetails.Name;
			otherPlayer.SteamId = characterDetails.SteamId;
			gameObject.name = "Character_" + otherPlayer.GUID;
			otherPlayer.Parent = parent;
			TargetingPoint[] componentsInChildren = otherPlayer.GetComponentsInChildren<TargetingPoint>();
			foreach (TargetingPoint targetingPoint in componentsInChildren)
			{
				targetingPoint.MainObject = otherPlayer;
			}
			Client.Instance.AddPlayer(otherPlayer.GUID, otherPlayer);
			SceneSpawnPoint sceneSpawnPoint = null;
			if (characterDetails.SpawnPointID > 0)
			{
				if (parent.Type == SpaceObjectType.Ship)
				{
					sceneSpawnPoint = (parent as Ship).GetStructureObject<SceneSpawnPoint>(characterDetails.SpawnPointID);
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
			otherPlayer.tpsController.SetTargetPositionAndRotation(otherPlayer.transform.localPosition, otherPlayer.transform.localRotation, true);
			gameObject.SetActive(true);
			otherPlayer.PlayerStatsMessageListener(new PlayerStatsMessage
			{
				GUID = otherPlayer.GUID,
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
					List<DynamicObjectDetails> dynamicObjects = characterDetails.DynamicObjects;
					if (_003C_003Ef__am_0024cache0 == null)
					{
						_003C_003Ef__am_0024cache0 = _003CSpawnPlayer_003Em__0;
					}
					DynamicObjectDetails dynamicObjectDetails = dynamicObjects.Find(_003C_003Ef__am_0024cache0);
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
			if (sceneSpawnPoint != null && sceneSpawnPoint.Executer != null)
			{
				if (otherPlayer.tpsController == null)
				{
					otherPlayer.tpsController = otherPlayer.transform.GetComponent<OtherCharacterController>();
				}
				sceneSpawnPoint.Executer.SetExecuterDetails(new SceneTriggerExecuterDetails
				{
					PlayerThatActivated = otherPlayer.GUID,
					InSceneID = sceneSpawnPoint.Executer.InSceneID,
					IsImmediate = true,
					IsFail = false,
					CurrentStateID = sceneSpawnPoint.Executer.CurrentStateID,
					NewStateID = sceneSpawnPoint.Executer.GetStateID(sceneSpawnPoint.ExecuterState)
				}, false, null, false);
			}
			if (Client.Instance.CharacterInteractionStatesQueue.ContainsKey(otherPlayer.GUID))
			{
				Client.Instance.CharacterInteractionStatesQueue[otherPlayer.GUID].Executer.CharacterInteractInstant(Client.Instance.CharacterInteractionStatesQueue[otherPlayer.GUID], otherPlayer.GUID);
				Client.Instance.CharacterInteractionStatesQueue.Remove(otherPlayer.GUID);
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
			Client.Instance.NetworkController.EventSystem.RemoveListener(typeof(KillPlayerMessage), KillPlayerMessageListener);
			Client.Instance.NetworkController.EventSystem.RemoveListener(typeof(PlayerDrillingMessage), PlayerDrillingMessageListener);
			Client.Instance.NetworkController.EventSystem.RemoveListener(typeof(PlayerStatsMessage), PlayerStatsMessageListener);
			Client.Instance.RemovePlayer(base.GUID);
		}

		private void SwitchParentResponseListener(NetworkData data)
		{
		}

		public void PlayerDrillingMessageListener(NetworkData data)
		{
			PlayerDrillingMessage playerDrillingMessage = data as PlayerDrillingMessage;
			if (playerDrillingMessage.DrillersGUID == base.GUID && Inventory.CheckIfItemInHandsIsType<HandDrill>())
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
			_003CPlayerStatsMessageListener_003Ec__AnonStorey0 _003CPlayerStatsMessageListener_003Ec__AnonStorey = new _003CPlayerStatsMessageListener_003Ec__AnonStorey0();
			_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm = data as PlayerStatsMessage;
			if (base.GUID != _003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.GUID)
			{
				return;
			}
			tpsController.animHelper.animationData.ReloadType = _003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.ReloadType;
			tpsController.animHelper.animationData.IsCrouch = (_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.AnimationStatesMask & 1) != 0;
			tpsController.animHelper.animationData.IsJump = (_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.AnimationStatesMask & 2) != 0;
			tpsController.animHelper.animationData.IsZeroG = (_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.AnimationStatesMask & 4) != 0;
			tpsController.animHelper.animationData.isInStance = (_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.AnimationStatesMask & 8) != 0;
			tpsController.animHelper.animationData.IsReloading = (_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.AnimationStatesMask & 0x10) != 0;
			tpsController.animHelper.animationData.IsGrounded = (_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.AnimationStatesMask & 0x20) != 0;
			tpsController.animHelper.animationData.IsHolster = (_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.AnimationStatesMask & 0x40) != 0;
			tpsController.animHelper.animationData.IsDraw = (_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.AnimationStatesMask & 0x80) != 0;
			tpsController.animHelper.animationData.CancelInteract = (_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.AnimationStatesMask & 0x100) != 0;
			tpsController.animHelper.animationData.IsFalling = (_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.AnimationStatesMask & 0x400) != 0;
			tpsController.animHelper.animationData.isEquipping = (_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.AnimationStatesMask & 0x800) != 0;
			tpsController.animHelper.animationData.TouchingFloor = (_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.AnimationStatesMask & 0x1000) != 0;
			tpsController.animHelper.animationData.UsingTool = (_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.AnimationStatesMask & 0x2000) != 0;
			tpsController.animHelper.animationData.IsEmote = (_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.AnimationStatesMask & 0x4000) != 0;
			tpsController.animHelper.animationData.IsMelee = (_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.AnimationStatesMask & 0x8000) != 0;
			tpsController.animHelper.animationData.UsingLadder = (_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.AnimationStatesMask & 0x10000) != 0;
			tpsController.animHelper.animationData.UseConsumable = (_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.AnimationStatesMask & 0x20000) != 0;
			tpsController.animHelper.animationData.WeaponActivated = (_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.AnimationStatesMask & 0x40000) != 0;
			tpsController.UpdateAnimatorOneFrame();
			if (_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.LockedToTriggerID != null && Parent is SpaceObjectVessel)
			{
				LockedToTrigger = Parent.GeometryRoot.GetComponentsInChildren<BaseSceneTrigger>(true).FirstOrDefault(_003CPlayerStatsMessageListener_003Ec__AnonStorey._003C_003Em__0);
			}
			else
			{
				LockedToTrigger = null;
			}
			float num;
			if (_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.DamageList != null)
			{
				List<PlayerDamage> damageList = _003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.DamageList;
				if (_003C_003Ef__am_0024cache1 == null)
				{
					_003C_003Ef__am_0024cache1 = _003CPlayerStatsMessageListener_003Em__1;
				}
				IEnumerable<PlayerDamage> source = damageList.Where(_003C_003Ef__am_0024cache1);
				if (_003C_003Ef__am_0024cache2 == null)
				{
					_003C_003Ef__am_0024cache2 = _003CPlayerStatsMessageListener_003Em__2;
				}
				num = source.Sum(_003C_003Ef__am_0024cache2);
			}
			else
			{
				num = 0f;
			}
			float num2 = num;
			if (num2 > float.Epsilon)
			{
				tpsController.HealthSounds.Play(1);
			}
			float num3;
			if (_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.DamageList != null)
			{
				List<PlayerDamage> damageList2 = _003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.DamageList;
				if (_003C_003Ef__am_0024cache3 == null)
				{
					_003C_003Ef__am_0024cache3 = _003CPlayerStatsMessageListener_003Em__3;
				}
				IEnumerable<PlayerDamage> source2 = damageList2.Where(_003C_003Ef__am_0024cache3);
				if (_003C_003Ef__am_0024cache4 == null)
				{
					_003C_003Ef__am_0024cache4 = _003CPlayerStatsMessageListener_003Em__4;
				}
				num3 = source2.Sum(_003C_003Ef__am_0024cache4);
			}
			else
			{
				num3 = 0f;
			}
			float num4 = num3;
			if (num4 > float.Epsilon)
			{
				tpsController.HealthSounds.Play(0);
			}
			float num5;
			if (_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.DamageList != null)
			{
				List<PlayerDamage> damageList3 = _003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.DamageList;
				if (_003C_003Ef__am_0024cache5 == null)
				{
					_003C_003Ef__am_0024cache5 = _003CPlayerStatsMessageListener_003Em__5;
				}
				IEnumerable<PlayerDamage> source3 = damageList3.Where(_003C_003Ef__am_0024cache5);
				if (_003C_003Ef__am_0024cache6 == null)
				{
					_003C_003Ef__am_0024cache6 = _003CPlayerStatsMessageListener_003Em__6;
				}
				num5 = source3.Sum(_003C_003Ef__am_0024cache6);
			}
			else
			{
				num5 = 0f;
			}
			float num6 = num5;
			if (num6 > float.Epsilon)
			{
				tpsController.HealthSounds.Play(1);
			}
			float num7;
			if (_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.DamageList != null)
			{
				List<PlayerDamage> damageList4 = _003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.DamageList;
				if (_003C_003Ef__am_0024cache7 == null)
				{
					_003C_003Ef__am_0024cache7 = _003CPlayerStatsMessageListener_003Em__7;
				}
				IEnumerable<PlayerDamage> source4 = damageList4.Where(_003C_003Ef__am_0024cache7);
				if (_003C_003Ef__am_0024cache8 == null)
				{
					_003C_003Ef__am_0024cache8 = _003CPlayerStatsMessageListener_003Em__8;
				}
				num7 = source4.Sum(_003C_003Ef__am_0024cache8);
			}
			else
			{
				num7 = 0f;
			}
			float num8 = num7;
			if (num8 > float.Epsilon)
			{
				tpsController.HealthSounds.Play(0);
			}
			float num9;
			if (_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.DamageList != null)
			{
				List<PlayerDamage> damageList5 = _003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.DamageList;
				if (_003C_003Ef__am_0024cache9 == null)
				{
					_003C_003Ef__am_0024cache9 = _003CPlayerStatsMessageListener_003Em__9;
				}
				IEnumerable<PlayerDamage> source5 = damageList5.Where(_003C_003Ef__am_0024cache9);
				if (_003C_003Ef__am_0024cacheA == null)
				{
					_003C_003Ef__am_0024cacheA = _003CPlayerStatsMessageListener_003Em__A;
				}
				num9 = source5.Sum(_003C_003Ef__am_0024cacheA);
			}
			else
			{
				num9 = 0f;
			}
			float num10 = num9;
			if (num10 > float.Epsilon)
			{
			}
			float num11;
			if (_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.DamageList != null)
			{
				List<PlayerDamage> damageList6 = _003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.DamageList;
				if (_003C_003Ef__am_0024cacheB == null)
				{
					_003C_003Ef__am_0024cacheB = _003CPlayerStatsMessageListener_003Em__B;
				}
				IEnumerable<PlayerDamage> source6 = damageList6.Where(_003C_003Ef__am_0024cacheB);
				if (_003C_003Ef__am_0024cacheC == null)
				{
					_003C_003Ef__am_0024cacheC = _003CPlayerStatsMessageListener_003Em__C;
				}
				num11 = source6.Sum(_003C_003Ef__am_0024cacheC);
			}
			else
			{
				num11 = 0f;
			}
			float num12 = num11;
			if (num12 > float.Epsilon)
			{
				tpsController.HealthSounds.Play(1);
			}
			float num13;
			if (_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.DamageList != null)
			{
				List<PlayerDamage> damageList7 = _003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.DamageList;
				if (_003C_003Ef__am_0024cacheD == null)
				{
					_003C_003Ef__am_0024cacheD = _003CPlayerStatsMessageListener_003Em__D;
				}
				IEnumerable<PlayerDamage> source7 = damageList7.Where(_003C_003Ef__am_0024cacheD);
				if (_003C_003Ef__am_0024cacheE == null)
				{
					_003C_003Ef__am_0024cacheE = _003CPlayerStatsMessageListener_003Em__E;
				}
				num13 = source7.Sum(_003C_003Ef__am_0024cacheE);
			}
			else
			{
				num13 = 0f;
			}
			float num14 = num13;
			if (num14 > float.Epsilon)
			{
				tpsController.HealthSounds.Play(1);
			}
			float num15;
			if (_003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.DamageList != null)
			{
				List<PlayerDamage> damageList8 = _003CPlayerStatsMessageListener_003Ec__AnonStorey.psm.DamageList;
				if (_003C_003Ef__am_0024cacheF == null)
				{
					_003C_003Ef__am_0024cacheF = _003CPlayerStatsMessageListener_003Em__F;
				}
				IEnumerable<PlayerDamage> source8 = damageList8.Where(_003C_003Ef__am_0024cacheF);
				if (_003C_003Ef__am_0024cache10 == null)
				{
					_003C_003Ef__am_0024cache10 = _003CPlayerStatsMessageListener_003Em__10;
				}
				num15 = source8.Sum(_003C_003Ef__am_0024cache10);
			}
			else
			{
				num15 = 0f;
			}
			float num16 = num15;
			if (num16 > float.Epsilon)
			{
				tpsController.HealthSounds.Play(1);
			}
		}

		public void SetGlobalPositionAndRotation(Vector3 position, Quaternion rotation)
		{
			base.transform.position = position;
			base.transform.rotation = rotation;
			tpsController.SetTargetPositionAndRotation(base.transform.localPosition, base.transform.localRotation, true);
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

		public override void SetTargetPositionAndRotation(Vector3? position, Vector3? forward, Vector3? up, bool instant = false, double time = -1.0)
		{
			IsInVisibilityRange = true;
			if (forward.HasValue && up.HasValue)
			{
				tpsController.SetTargetPositionAndRotation(position, Quaternion.LookRotation(forward.Value, up.Value), instant);
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

		[CompilerGenerated]
		private static bool _003CSpawnPlayer_003Em__0(DynamicObjectDetails x)
		{
			return x.AttachData.IsAttached && x.AttachData.InventorySlotID == -2;
		}

		[CompilerGenerated]
		private static bool _003CPlayerStatsMessageListener_003Em__1(PlayerDamage m)
		{
			return m.HurtType == HurtType.Shot;
		}

		[CompilerGenerated]
		private static float _003CPlayerStatsMessageListener_003Em__2(PlayerDamage m)
		{
			return m.Amount;
		}

		[CompilerGenerated]
		private static bool _003CPlayerStatsMessageListener_003Em__3(PlayerDamage m)
		{
			return m.HurtType == HurtType.Pressure;
		}

		[CompilerGenerated]
		private static float _003CPlayerStatsMessageListener_003Em__4(PlayerDamage m)
		{
			return m.Amount;
		}

		[CompilerGenerated]
		private static bool _003CPlayerStatsMessageListener_003Em__5(PlayerDamage m)
		{
			return m.HurtType == HurtType.Impact;
		}

		[CompilerGenerated]
		private static float _003CPlayerStatsMessageListener_003Em__6(PlayerDamage m)
		{
			return m.Amount;
		}

		[CompilerGenerated]
		private static bool _003CPlayerStatsMessageListener_003Em__7(PlayerDamage m)
		{
			return m.HurtType == HurtType.Suffocate;
		}

		[CompilerGenerated]
		private static float _003CPlayerStatsMessageListener_003Em__8(PlayerDamage m)
		{
			return m.Amount;
		}

		[CompilerGenerated]
		private static bool _003CPlayerStatsMessageListener_003Em__9(PlayerDamage m)
		{
			return m.HurtType == HurtType.Frost;
		}

		[CompilerGenerated]
		private static float _003CPlayerStatsMessageListener_003Em__A(PlayerDamage m)
		{
			return m.Amount;
		}

		[CompilerGenerated]
		private static bool _003CPlayerStatsMessageListener_003Em__B(PlayerDamage m)
		{
			return m.HurtType == HurtType.Heat;
		}

		[CompilerGenerated]
		private static float _003CPlayerStatsMessageListener_003Em__C(PlayerDamage m)
		{
			return m.Amount;
		}

		[CompilerGenerated]
		private static bool _003CPlayerStatsMessageListener_003Em__D(PlayerDamage m)
		{
			return m.HurtType == HurtType.Shred;
		}

		[CompilerGenerated]
		private static float _003CPlayerStatsMessageListener_003Em__E(PlayerDamage m)
		{
			return m.Amount;
		}

		[CompilerGenerated]
		private static bool _003CPlayerStatsMessageListener_003Em__F(PlayerDamage m)
		{
			return m.HurtType == HurtType.SpaceExposure;
		}

		[CompilerGenerated]
		private static float _003CPlayerStatsMessageListener_003Em__10(PlayerDamage m)
		{
			return m.Amount;
		}
	}
}
