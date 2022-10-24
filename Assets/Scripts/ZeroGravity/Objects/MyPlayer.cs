using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using ZeroGravity.CharacterMovement;
using ZeroGravity.Data;
using ZeroGravity.Effects;
using ZeroGravity.LevelDesign;
using ZeroGravity.Math;
using ZeroGravity.Network;
using ZeroGravity.ShipComponents;
using ZeroGravity.UI;

namespace ZeroGravity.Objects
{
	public class MyPlayer : Player
	{
		public delegate void InteractLockDelegate();

		public enum IKHandsInUse
		{
			LeftHand = -1,
			BothHands,
			RightHand,
			None
		}

		public enum HandAnimationStates
		{
			Clear,
			Left,
			Right,
			Forward,
			Back,
			Top,
			Bottom
		}

		public enum IKType
		{
			None,
			Walls,
			Weapon
		}

		public enum PlayerStance
		{
			Passive = 1,
			Active,
			Special
		}

		public static float SendMovementInterval = 0.1f;

		private float sendMovementTime;

		private Rigidbody VesselChangeHelperRb;

		[HideInInspector]
		public InteractLockDelegate OnIteractStart;

		[HideInInspector]
		public InteractLockDelegate OnIteractComplete;

		[HideInInspector]
		public InteractLockDelegate OnLockStart;

		[HideInInspector]
		public InteractLockDelegate OnLockComplete;

		private bool wasJetpackOn;

		public bool SittingOnPilotSeat;

		public float MeleeRange;

		[HideInInspector]
		public EventSystem EventSystem;

		[HideInInspector]
		public Vector3 OldGravity;

		[SerializeField]
		private MyCharacterController characterController;

		public bool PlayerReady;

		public int Health;

		[SerializeField]
		private RagdollHelper ragdollComponent;

		[HideInInspector]
		public Transform SunCameraRoot;

		private Transform SunCamera;

		[HideInInspector]
		public Transform PlanetsCameraRoot;

		private Transform PlanetsCamera;

		private Transform ShipExteriorCameraRoot;

		private Transform ShipExteriorCamera;

		private Transform ShipSunLight;

		public BaseSceneTrigger LookingAtTrigger;

		public HealthPostEffect healthEffect;

		public EnterAtmosphereEffect burningEffect;

		private float pressure;

		public SoundEffect HealthSounds;

		private Vector3 shipThrust;

		private Vector3 shipRotation;

		private float shipRotationStrength = 0.01f;

		private float shipThrustStrength = 0.01f;

		public Player LookingAtPlayer;

		private Item _LookingAtItem;

		private Item PrevLookingAtItem;

		private RagdollCollider LookingAtCorpseCollider;

		private float nearRaycastDistance = 1.3f;

		private float farRaycastDistance = 2.2f;

		public float HideCanvasDistance = 5f;

		[Header("Outfit")]
		public InventoryUI inventoryUI;

		public Outfit CurrentOutfit;

		public Helmet CurrentHelmet;

		public Transform Outfit;

		public Transform BasicOutfitHolder;

		public SkinnedMeshRenderer HeadSkin;

		public SkinnedMeshRenderer ReferenceHead;

		public List<SkinnedMeshRenderer> ArmSkins;

		private float inventoryTreshold = 1f;

		private float inventoryQuickTime = -1f;

		private Vector3 lootRaycastPoint;

		private float dropThrowStartTime;

		[SerializeField]
		private float pickUpWhenFullTreshold = 0.5f;

		private float pickUpWhenFullCounter;

		private float switchItemTime;

		private float cameraFovZoomTreshold = 0.2f;

		private float cameraFovZoomCounter;

		private float cameraFovZoomMinValue = 30f;

		private float cameraFovLerpFrom;

		private float cameraFovLerpTo;

		private float cameraFovLerpValue;

		private float cameraFovLerpStrength = 7.5f;

		public float CurrentPanelFov = Client.DefaultCameraFov;

		public bool InLadderTrigger;

		public SceneTriggerLadder LadderTrigger;

		private PlayerStance currentStance = PlayerStance.Passive;

		private Dictionary<ResourceType, float> ResDbg;

		private bool reloadCalled;

		private float reloadButtonPressedTime;

		private float specialMenuTreshold = 0.4f;

		[SerializeField]
		public Rigidbody rigidBody;

		[SerializeField]
		private SunFlarePostEffect sunFlareEffect;

		private Light shipExteriorSunLight;

		private int planetsRaycastLayer;

		private int dropThrowLayerMask;

		private int playerLookRaycastMask;

		private int shootingRaycastMask;

		private int highlightAttachPointMask;

		public AnimatorHelper animHelper;

		[SerializeField]
		private List<AudioMixerSnapshot> NearSnapshots;

		[SerializeField]
		private List<AudioMixerSnapshot> FarSnapshots;

		public Transform HelmetPlacementTransform;

		public Transform MuzzleFlashTransform;

		public GameObject BulletHoleEffect;

		private int sunRaycastLayerMask;

		public float RcsThrustModifier = 1f;

		public SceneTriggerRoom MyRoomTrigger;

		private static MyPlayer _instance;

		public bool PivotReset;

		private float timeCorr;

		private int prevStatsMask;

		private int triggersMask;

		public SpaceObjectVessel NearestVessel;

		public float NearestVesselSqDistance;

		public long HomeStationGUID = -1L;

		public bool SendDockUndockMsg;

		[Multiline(20)]
		[ContextMenuItem("reset", "ResetStatistics")]
		public string SentPacketStatistics = string.Empty;

		[Multiline(20)]
		[ContextMenuItem("reset", "ResetStatistics")]
		public string ReceivedPacketStatistics = string.Empty;

		private GameObject[] DefaultBloodEffects;

		[HideInInspector]
		public GameObject bloodCloudEffect;

		private List<GameObject> higlightedAttachPoints = new List<GameObject>();

		private float highlightAttachPointsRange = 5f;

		private bool cruiseControl;

		private float prevEngineThrustDirection;

		private bool pickingUpItem;

		public DebrisField InDebrisField;

		public Vector3D DebrisFieldVelocityDirection = Vector3D.Zero;

		public ShipControlMode ShipControlMode;

		private bool sendStats;

		private bool QuickSwitchItemRunning;

		[NonSerialized]
		public List<ItemCompoundType> Blueprints = new List<ItemCompoundType>();

		[NonSerialized]
		public bool IsAdmin;

		[NonSerialized]
		public Vector3 ShipRotationCursor = Vector3.zero;

		private float lastShipRotationCursorChangeTime;

		private UpdateTimer passiveScanTimer = new UpdateTimer(10f);

		private List<DynamicObjectDetails> equipSpawnDynamicObjects;

		private Item itemToPickup;

		private Item itemToDrop;

		private bool itemToDropIsThrow;

		private bool itemToDropIsResetStance;

		public Queue<string> ShotDebugList = new Queue<string>();

		private float healthLerpHelper;

		private float healthStartVal;

		private float healthEndVal;

		public static bool isAudioDebug;

		private Item newReloadingItem;

		private Item currentReloadingItem;

		private ItemType reloadItemType;

		private bool changeEngineThrust;

		private float changeEngineThrustTime;

		private bool isEmoting;

		public int currentDockIndex;

		private int showSystemsDetails;

		public bool ShowGUIElements;

		private bool allowVesselChange = true;

		private SpaceObjectVessel vesselChangeQueue;

		private bool vesselChangeIsEnter;

		private Vector3? cameraLerpPosFrom;

		private Vector3? cameraLerpPosTo;

		private Quaternion? cameraLerpRotFrom;

		private Quaternion? cameraLerpRotTo;

		private bool cameraLerpLocal;

		private float cameraLerpHelper;

		private bool isRagdolled;

		private bool isRagdollFinished = true;

		private bool wasInStance;

		private OcSector currOcSector;

		private bool low;

		private bool high;

		private Vector3 prevGlobalPos = Vector3.zero;

		private float prevTimeStamp;

		private SpaceObjectVessel prevStickToVessel;

		[NonSerialized]
		public bool IsAlive = true;

		public bool InIteractLayer { get; private set; }

		public bool InLockState { get; private set; }

		public bool InInteractState { get; private set; }

		public bool InLerpingState { get; set; }

		public override SpaceObjectType Type => SpaceObjectType.Player;

		public MyCharacterController FpsController => characterController;

		public bool IsDrivingShip => ShipControlMode == ShipControlMode.Piloting;

		public override BaseSceneTrigger LockedToTrigger
		{
			get
			{
				return base.LockedToTrigger;
			}
			set
			{
				if (base.LockedToTrigger != value)
				{
					sendStats = true;
				}
				base.LockedToTrigger = value;
			}
		}

		public float Pressure
		{
			get
			{
				return pressure;
			}
			set
			{
				pressure = value;
				AkSoundEngine.SetRTPCValue(SoundManager.instance.Pressure, value);
			}
		}

		private Item LookingAtItem
		{
			get
			{
				return _LookingAtItem;
			}
			set
			{
				if (_LookingAtItem != value)
				{
					PrevLookingAtItem = _LookingAtItem;
					_LookingAtItem = value;
				}
			}
		}

		public float FarRaycastDistance => farRaycastDistance;

		public PlayerStance CurrentStance => currentStance;

		public static MyPlayer Instance => _instance;

		public bool UseGravity => base.CurrentRoomTrigger != null && base.CurrentRoomTrigger.UseGravity;

		public new bool IsInsideSpaceObject => Parent is SpaceObjectVessel || !Client.IsGameBuild;

		public Vector3 MyVelocity => rigidBody.velocity;

		public SceneTriggerExecuter CancelInteractExecuter { get; set; }

		public override SpaceObject Parent
		{
			get
			{
				return base.Parent;
			}
			set
			{
				bool flag = !(Parent is SpaceObjectVessel) || !(value is SpaceObjectVessel) || !((Parent as SpaceObjectVessel).MainVessel == (value as SpaceObjectVessel).MainVessel);
				base.Parent = value;
				if (flag)
				{
					Client.Instance.Discord.UpdateStatus();
				}
			}
		}

		private Vector3 ThrustForward
		{
			get
			{
				if (Parent is SpaceObjectVessel && (ShipControlMode == ShipControlMode.Docking || LockedToTrigger is SceneTriggerDockingPanel))
				{
					DockingPanel docking = Client.Instance.InGamePanels.Docking;
					Transform cameraPosition = docking.DockingPort.CameraPosition;
					SpaceObjectVessel mainVessel = (Parent as SpaceObjectVessel).MainVessel;
					return Quaternion.LookRotation(mainVessel.Forward, mainVessel.Up) * Quaternion.LookRotation(cameraPosition.forward, cameraPosition.up) * Vector3.forward * docking.ThrustModifier;
				}
				return Parent.Forward;
			}
		}

		private Vector3 ThrustUp
		{
			get
			{
				if (Parent is SpaceObjectVessel && (ShipControlMode == ShipControlMode.Docking || LockedToTrigger is SceneTriggerDockingPanel))
				{
					DockingPanel docking = Client.Instance.InGamePanels.Docking;
					Transform cameraPosition = docking.DockingPort.CameraPosition;
					SpaceObjectVessel mainVessel = (Parent as SpaceObjectVessel).MainVessel;
					return Quaternion.LookRotation(mainVessel.Forward, mainVessel.Up) * Quaternion.LookRotation(cameraPosition.forward, cameraPosition.up) * Vector3.up * docking.ThrustModifier;
				}
				return Parent.Up;
			}
		}

		private Vector3 ThrustRight
		{
			get
			{
				if (Parent is SpaceObjectVessel && (ShipControlMode == ShipControlMode.Docking || LockedToTrigger is SceneTriggerDockingPanel))
				{
					DockingPanel docking = Client.Instance.InGamePanels.Docking;
					Transform cameraPosition = docking.DockingPort.CameraPosition;
					SpaceObjectVessel mainVessel = (Parent as SpaceObjectVessel).MainVessel;
					return Quaternion.LookRotation(mainVessel.Forward, mainVessel.Up) * Quaternion.LookRotation(cameraPosition.forward, cameraPosition.up) * Vector3.Cross(-Vector3.forward, Vector3.up).normalized * docking.ThrustModifier;
				}
				return Vector3.Cross(-Parent.Forward, Parent.Up).normalized;
			}
		}

		public bool MeshRenderersEnabled { get; private set; }

		public bool ShouldMoveCamera
		{
			get
			{
				if (LockedToTrigger == null)
				{
					if (Client.IsGameBuild && Client.Instance.CanvasManager.PlayerOverview.gameObject.activeInHierarchy)
					{
						return false;
					}
					return true;
				}
				return LockedToTrigger.CameraMovementAllowed;
			}
		}

		public GameObject GetDefaultBlood()
		{
			return DefaultBloodEffects[UnityEngine.Random.Range(0, DefaultBloodEffects.Length - 1)];
		}

		private void Awake()
		{
			_instance = this;
			sunRaycastLayerMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("FirstPerson"));
			planetsRaycastLayer = 1 << LayerMask.NameToLayer("Planets");
			dropThrowLayerMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("DynamicObjectCollision"));
			playerLookRaycastMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("DynamicObject")) | (1 << LayerMask.NameToLayer("InteractiveTriggers")) | (1 << LayerMask.NameToLayer("Player"));
			shootingRaycastMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("DynamicObject")) | (1 << LayerMask.NameToLayer("Player"));
			triggersMask = 1 << LayerMask.NameToLayer("Triggers");
			highlightAttachPointMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("InteractiveTriggers"));
			if (Client.IsGameBuild)
			{
				EventSystem = Client.Instance.NetworkController.EventSystem;
				Client.Instance.CanvasManager.CanvasUI.ToggleCroshair(show: false);
			}
			else
			{
				EventSystem = new EventSystem();
				SetGravity(new Vector3(0f, -9.81f, 0f));
			}
			if (Client.IsGameBuild)
			{
				Client.Instance.NetworkController.EventSystem.AddListener(typeof(PlayerShootingMessage), PlayerShootingMessageListener);
				Client.Instance.NetworkController.EventSystem.AddListener(typeof(PlayerStatsMessage), PlayerStatMessageListener);
				Client.Instance.NetworkController.EventSystem.AddListener(typeof(TextChatMessage), TextChatMessageListener);
				Client.Instance.NetworkController.EventSystem.AddListener(EventSystem.InternalEventType.EquipAnimationEnd, EquipAnimationEndListener);
				Client.Instance.NetworkController.EventSystem.AddListener(typeof(LockToTriggerMessage), LockToTriggerMessageListener);
				Client.Instance.NetworkController.EventSystem.AddListener(typeof(QuestStatsMessage), QuestStatsMessageListener);
				Client.Instance.NetworkController.EventSystem.AddListener(typeof(UpdateBlueprintsMessage), UpdateBlueprintsMessageListener);
			}
			DefaultBloodEffects = Resources.LoadAll<GameObject>("Effects/DefaultBloodEffects/");
			bloodCloudEffect = Resources.Load<GameObject>("Test/Effekc/BloodCloud");
			Client.Instance.AmbientSounds.SwitchAmbience(SoundManager.instance.SpaceAmbience);
		}

		private void TextChatMessageListener(NetworkData data)
		{
			TextChatMessage textChatMessage = data as TextChatMessage;
			if (textChatMessage.GUID == -1 && textChatMessage.MessageType.HasValue)
			{
				Client.Instance.CanvasManager.TextChat.CreateSystemMessage(textChatMessage.MessageType.Value, textChatMessage.MessageParam);
			}
			else
			{
				Client.Instance.CanvasManager.TextChat.CreateMessage(textChatMessage.Name, textChatMessage.MessageText, textChatMessage.Local);
			}
		}

		protected void Start()
		{
			if (Client.IsGameBuild)
			{
				InitializeInventory();
				EquipSpawnDynamicObjects();
				if (CurrentHelmet != null && (Parent is Pivot || Parent is Asteroid))
				{
					CurrentHelmet.HudUI.Radar.CanRadarWork = true;
					CurrentHelmet.HudUI.Radar.ToggleTargeting(val: true);
				}
				CheckCameraShake();
				CheckRoomTrigger(null);
				FpsController.HeadBobStrength = Client.Instance.HeadbobStrength;
				InitializeCameraEffects();
				StartCoroutine(DebrisFieldCheckCoroutine());
				Client.Instance.Invoke(Client.Instance.LatencyTestMessage, 1f);
			}
		}

		public void InitializeCameraEffects()
		{
			PostProcessLayer.Antialiasing antialiasingOption = (PostProcessLayer.Antialiasing)Client.Instance.AntialiasingOption;
			FpsController.MainCamera.GetComponent<PostProcessLayer>().antialiasingMode = antialiasingOption;
			FpsController.MainCamera.GetComponent<HxVolumetricCamera>().enabled = Client.Instance.VolumetricOption;
		}

		private void InitializeInventory()
		{
			if (Inventory == null)
			{
				Inventory = new Inventory(this, animHelper);
				inventoryUI = Client.Instance.CanvasManager.InventoryUI;
			}
		}

		private void EquipSpawnDynamicObjects()
		{
			if (equipSpawnDynamicObjects == null)
			{
				return;
			}
			DynamicObjectDetails dynamicObjectDetails = equipSpawnDynamicObjects.Find((DynamicObjectDetails x) => x.AttachData.IsAttached && x.AttachData.InventorySlotID == -2);
			if (dynamicObjectDetails != null)
			{
				DynamicObject dynamicObject = DynamicObject.SpawnDynamicObject(dynamicObjectDetails, this);
				(dynamicObject.Item as Outfit).EquipOutfit(this, checkHands: false);
			}
			foreach (DynamicObjectDetails equipSpawnDynamicObject in equipSpawnDynamicObjects)
			{
				try
				{
					if (equipSpawnDynamicObject != dynamicObjectDetails)
					{
						DynamicObject.SpawnDynamicObject(equipSpawnDynamicObject, this);
					}
				}
				catch (Exception ex)
				{
					Dbg.Error("Unable to equip item on spawn", equipSpawnDynamicObject.GUID, ex.Message, ex.StackTrace);
				}
			}
			equipSpawnDynamicObjects.Clear();
			equipSpawnDynamicObjects = null;
		}

		public void ItemAddedToHands(Item item)
		{
			animHelper.SetParameter(null, null, null, null, null, null, null, null, true);
			ChangeStance(PlayerStance.Passive, item.PassiveSpeedMultiplier);
			ResetCameraFov();
		}

		public void SetHandsBoxCollider(BoxCollider collider)
		{
			FpsController.SetHandsBoxCollider(collider);
		}

		public void ChangeStance(PlayerStance newStance, float speedMultiplier)
		{
			currentStance = newStance;
			FpsController.SetStateSpeedMultiplier(speedMultiplier);
			AnimatorHelper animatorHelper = animHelper;
			PlayerStance? playerStance = currentStance;
			animatorHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, playerStance);
			Client.Instance.CanvasManager.CanvasUI.CheckDotCroshair();
		}

		public void Attack(ShotData shotData, Item activeItem, float thrust, float rotation, bool otherPlayer = false)
		{
			if (shotData.Range == 1f)
			{
				shotData.Range = MeleeRange;
				shotData.IsMeleeAttack = true;
			}
			SpaceObject @object = Client.Instance.GetObject(shotData.parentGUID, shotData.parentType);
			SpaceObject spaceObject = ((!(Parent is SpaceObjectVessel)) ? Parent : (Parent as SpaceObjectVessel).MainVessel);
			Vector3 vector = Quaternion.LookRotation(spaceObject.Forward, spaceObject.Up).Inverse() * shotData.Position.ToVector3() + @object.transform.position;
			Vector3 vector2 = Quaternion.LookRotation(spaceObject.Forward, spaceObject.Up).Inverse() * shotData.Orientation.ToVector3();
			float range = shotData.Range;
			if (!UseGravity)
			{
				FpsController.AddForce(vector2.normalized * thrust, ForceMode.Acceleration);
				FpsController.AddTorque(base.transform.up * (rotation * ((float) Mathf.PI / 180f)), ForceMode.Acceleration);
			}
			OtherPlayer playerHit = null;
			bool corpseHit = false;
			long hitGUID = -1L;
			RaycastHit[] array;
			if (shotData.IsMeleeAttack)
			{
				array = (from x in Physics.SphereCastAll(vector, 0.1f, vector2, range, shootingRaycastMask)
					orderby x.distance
					select x).ToArray();
			}
			else
			{
				Debug.DrawRay(vector, vector2 * range, Color.gray, 2f);
				array = (from x in Physics.RaycastAll(vector, vector2, range, shootingRaycastMask, QueryTriggerInteraction.Collide)
					orderby x.distance
					select x).ToArray();
			}
			if (array != null && array.Length > 0)
			{
				RaycastHit hit = array.FirstOrDefault(delegate(RaycastHit m)
				{
					bool flag = m.transform.GetComponentInParent<MyPlayer>() != null;
					bool flag2 = m.transform.GetComponentInParent<RagdollCollider>() != null;
					playerHit = m.transform.gameObject.GetComponentInParent<OtherPlayer>();
					corpseHit = m.transform.gameObject.GetComponentInParent<Corpse>() != null;
					return !flag && ((flag2 && (playerHit != null || corpseHit)) || (!flag2 && playerHit == null && !corpseHit));
				});
				if (hit.collider != null)
				{
					if (playerHit != null || corpseHit)
					{
						if (playerHit != null && !corpseHit)
						{
							hitGUID = playerHit.GUID;
						}
						RagdollCollider component = hit.transform.gameObject.GetComponent<RagdollCollider>();
						if (component != null)
						{
							shotData.colliderType = (byte)component.ColType;
						}
						if (activeItem as Weapon != null)
						{
							(activeItem as Weapon).ConsumeShotAndPlayEffect(hit, BulletImpact.BulletImpactType.Flesh, vector2);
						}
						if (corpseHit)
						{
							Rigidbody component2 = hit.transform.GetComponent<Rigidbody>();
							if (component2 != null)
							{
								component2.AddForce(Quaternion.LookRotation(FpsController.MainCamera.transform.forward, FpsController.MainCamera.transform.up) * new Vector3(0f, 0f, 400f), ForceMode.Force);
							}
						}
					}
					else
					{
						DynamicObject componentInChildren = hit.transform.GetComponentInChildren<DynamicObject>();
						if (componentInChildren != null)
						{
							if (!componentInChildren.IsAttached && componentInChildren.IsKinematic)
							{
								componentInChildren.Master = true;
								componentInChildren.ToggleKinematic(value: false);
							}
							hitGUID = componentInChildren.GUID;
							componentInChildren.AddForce(Quaternion.LookRotation(FpsController.MainCamera.transform.forward, FpsController.MainCamera.transform.up) * new Vector3(0f, 0f, 400f), ForceMode.Force);
							if (activeItem as Weapon != null)
							{
								(activeItem as Weapon).ConsumeShotAndPlayEffect(hit, BulletImpact.BulletImpactType.Object, vector2);
							}
						}
						else if (activeItem as Weapon != null)
						{
							(activeItem as Weapon).ConsumeShotAndPlayEffect(hit, BulletImpact.BulletImpactType.Metal, vector2);
						}
					}
				}
				else if (activeItem as Weapon != null)
				{
					(activeItem as Weapon).ConsumeShotAndPlayEffect(hit, BulletImpact.BulletImpactType.Object, vector2);
				}
			}
			else if (activeItem as Weapon != null)
			{
				(activeItem as Weapon).ConsumeShotAndPlayEffect(default(RaycastHit), BulletImpact.BulletImpactType.Object, vector2);
			}
			if (!otherPlayer)
			{
				PlayerShootingMessage playerShootingMessage = new PlayerShootingMessage();
				playerShootingMessage.HitGUID = hitGUID;
				playerShootingMessage.ShotData = shotData;
				playerShootingMessage.GUID = base.GUID;
				PlayerShootingMessage data = playerShootingMessage;
				Client.Instance.NetworkController.SendToGameServer(data);
			}
		}

		private void PlayerShootingMessageListener(NetworkData data)
		{
			PlayerShootingMessage playerShootingMessage = data as PlayerShootingMessage;
			OtherPlayer player = Client.Instance.GetPlayer(playerShootingMessage.GUID);
			if (player != null)
			{
				Attack(playerShootingMessage.ShotData, player.CurrentActiveItem, 0f, 0f, otherPlayer: true);
			}
		}

		private void FixedUpdate()
		{
			if (!Client.IsGameBuild || !Client.Instance.IsInGame || !PlayerReady)
			{
				return;
			}
			int AnimationStatsMask;
			CharacterAnimationData animationData = animHelper.GetAnimationData(FpsController.IsJump, isDraw: false, isHolster: false, cancelInteract: false, FpsController.AirTime, FpsController.IsEquippingAnimationTriggered, FpsController.MeleeTriggered, FpsController.UseConsumableTriggered, out AnimationStatsMask);
			if (AnimationStatsMask != prevStatsMask || sendStats)
			{
				sendStats = false;
				PlayerStatsMessage playerStatsMessage = new PlayerStatsMessage();
				playerStatsMessage.GUID = base.GUID;
				if (AnimationStatsMask != prevStatsMask)
				{
					prevStatsMask = AnimationStatsMask;
					playerStatsMessage.AnimationMaskChanged = true;
					playerStatsMessage.Health = Health;
					playerStatsMessage.ReloadType = (int)animHelper.GetParameterFloat(AnimatorHelper.Parameter.ReloadType);
					playerStatsMessage.AnimationStatesMask = AnimationStatsMask;
				}
				if (LockedToTrigger != null)
				{
					playerStatsMessage.LockedToTriggerID = LockedToTrigger.GetID();
					playerStatsMessage.IsPilotingVessel = LockedToTrigger.TriggerType == SceneTriggerType.ShipControl || LockedToTrigger.TriggerType == SceneTriggerType.DockingPanel;
				}
				else
				{
					playerStatsMessage.LockedToTriggerID = null;
				}
				Client.Instance.NetworkController.SendToGameServer(playerStatsMessage);
			}
			ResetTriggerBools();
			if (PivotReset || ImpactVelocity > 0f || SendDockUndockMsg || (FpsController.StickToVessel != null && prevStickToVessel != FpsController.StickToVessel) || sendMovementTime + SendMovementInterval <= Time.fixedTime)
			{
				sendMovementTime = Time.fixedTime;
				prevStickToVessel = FpsController.StickToVessel;
				CharacterMovementMessage characterMovementMessage = new CharacterMovementMessage();
				CharacterTransformData characterTransformData = new CharacterTransformData();
				characterTransformData.LocalPosition = base.transform.localPosition.ToArray();
				characterTransformData.LocalRotation = base.transform.localRotation.ToArray();
				characterTransformData.LocalVelocity = (Parent.TransferableObjectsRoot.transform.rotation.Inverse() * rigidBody.velocity).ToArray();
				characterTransformData.Timestamp = Time.time;
				characterTransformData.PlatformRelativePos = ((!(OnPlatform != null)) ? null : (base.transform.position - OnPlatform.transform.position).ToArray());
				CharacterTransformData characterTransformData2 = characterTransformData;
				if (FpsController.IsFreeLook)
				{
					Vector2 freeLookAngle = FpsController.FreeLookAngle;
					characterTransformData2.FreeLookY = 0f - freeLookAngle.y;
					characterTransformData2.FreeLookX = 0f - freeLookAngle.x;
				}
				characterTransformData2.MouseLook = FpsController.MouseLookXAngle;
				characterMovementMessage.TransformData = characterTransformData2;
				if (FpsController.StickToVessel != null)
				{
					characterMovementMessage.NearestVesselGUID = FpsController.StickToVessel.GUID;
					characterMovementMessage.NearestVesselDistance = 0f;
					characterMovementMessage.StickToVessel = true;
				}
				else
				{
					characterMovementMessage.NearestVesselGUID = ((!(NearestVessel != null)) ? (-1) : NearestVessel.GUID);
					characterMovementMessage.NearestVesselDistance = Mathf.Sqrt(NearestVesselSqDistance);
					characterMovementMessage.StickToVessel = false;
				}
				if (PivotReset)
				{
					characterMovementMessage.PivotReset = true;
					PivotReset = false;
				}
				if (Parent != null)
				{
					characterMovementMessage.ParentGUID = Parent.GUID;
					characterMovementMessage.ParentType = Parent.Type;
				}
				if (SendDockUndockMsg)
				{
					characterMovementMessage.DockUndockMsg = true;
					SendDockUndockMsg = false;
				}
				if (ImpactVelocity > 0f)
				{
					characterMovementMessage.ImpactVelocity = ImpactVelocity;
					ImpactVelocity = 0f;
				}
				characterMovementMessage.Gravity = base.Gravity.ToArray();
				characterMovementMessage.AnimationData = animationData;
				if (FpsController.IsZeroG && FpsController.IsJetpackOn)
				{
					characterMovementMessage.JetpackDirection = new sbyte[4]
					{
						FpsController.CurrentJetpack.NozzleDir[0],
						FpsController.CurrentJetpack.NozzleDir[1],
						FpsController.CurrentJetpack.NozzleDir[2],
						FpsController.CurrentJetpack.NozzleDir[3]
					};
				}
				if (!isRagdollFinished)
				{
					characterMovementMessage.RagdollData = GetRagdollData();
				}
				try
				{
					Client.Instance.NetworkController.SendToGameServer(characterMovementMessage);
				}
				catch (Exception)
				{
					Client.Instance.NetworkController.Disconnect();
					Client.Instance.OpenMainScreen();
				}
			}
			if (FpsController.StickToVessel != null && base.CurrentRoomTrigger == null)
			{
				Quaternion quaternion = FpsController.StickToVessel.transform.rotation * FpsController.StickToVesselRotation.Inverse();
				FpsController.StickToVesselRotation = FpsController.StickToVessel.transform.rotation;
				Vector3 position = base.transform.position;
				base.transform.position = quaternion * base.transform.position;
				base.transform.rotation = quaternion * base.transform.rotation;
				FpsController.StickToVesselTangentialVelocity = (base.transform.position - position) / Time.deltaTime;
			}
		}

		private void LateUpdate()
		{
			FpsController.CameraController.UpdateSpineTransform();
			if (Client.IsGameBuild)
			{
				UpdateCameraPositions();
			}
		}

		private void ResetTriggerBools()
		{
			FpsController.IsEquippingAnimationTriggered = false;
			FpsController.MeleeTriggered = false;
			FpsController.UseConsumableTriggered = false;
		}

		public void EquipAnimStart()
		{
			Inventory.AnimationItem_EventStart();
		}

		private void EquipAnimationEndListener(EventSystem.InternalEventData data)
		{
			Inventory.AnimationItem_EventEnd((int)data.Objects[0]);
		}

		private IEnumerator LerpHealthEffect()
		{
			while (healthLerpHelper < 1f)
			{
				healthLerpHelper += Time.deltaTime;
				yield return new WaitForEndOfFrame();
			}
			yield return null;
		}

		private void PlayerStatMessageListener(NetworkData data)
		{
			PlayerStatsMessage playerStatsMessage = data as PlayerStatsMessage;
			if (playerStatsMessage.GUID != base.GUID)
			{
				return;
			}
			float num = ((playerStatsMessage.DamageList == null) ? 0f : playerStatsMessage.DamageList.Where((PlayerDamage m) => m.HurtType == HurtType.Shot).Sum((PlayerDamage m) => m.Amount));
			if (num > float.Epsilon)
			{
				if (playerStatsMessage.ShotDirection != null)
				{
					Vector3 vector = playerStatsMessage.ShotDirection.ToVector3();
					Vector3 from = Vector3.ProjectOnPlane(vector, base.transform.up);
					if (Vector3.Angle(from, -base.transform.forward) < 45f)
					{
						healthEffect.Hit(num, HealthPostEffect.side.Front);
					}
					if (Vector3.Angle(from, base.transform.forward) < 45f)
					{
						healthEffect.Hit(num, HealthPostEffect.side.Back);
					}
					if (Vector3.Angle(from, -base.transform.right) < 45f)
					{
						healthEffect.Hit(num, HealthPostEffect.side.Right);
					}
					if (Vector3.Angle(from, base.transform.right) < 45f)
					{
						healthEffect.Hit(num, HealthPostEffect.side.Left);
					}
				}
				HealthSounds.Play(3);
			}
			float num2 = ((playerStatsMessage.DamageList == null) ? 0f : playerStatsMessage.DamageList.Where((PlayerDamage m) => m.HurtType == HurtType.Pressure).Sum((PlayerDamage m) => m.Amount));
			if (num2 > float.Epsilon)
			{
				healthEffect.LowPressureHit(num2);
				HealthSounds.Play(1);
			}
			float num3 = ((playerStatsMessage.DamageList == null) ? 0f : playerStatsMessage.DamageList.Where((PlayerDamage m) => m.HurtType == HurtType.Impact).Sum((PlayerDamage m) => m.Amount));
			if (num3 > float.Epsilon)
			{
				int num4 = UnityEngine.Random.Range(0, 3);
				if (num4 == 0)
				{
					healthEffect.Hit(num3, HealthPostEffect.side.Front);
				}
				if (num4 == 1)
				{
					healthEffect.Hit(num3, HealthPostEffect.side.Back);
				}
				if (num4 == 2)
				{
					healthEffect.Hit(num3, HealthPostEffect.side.Left);
				}
				if (num4 == 3)
				{
					healthEffect.Hit(num3, HealthPostEffect.side.Right);
				}
				HealthSounds.Play(3);
			}
			float num5 = ((playerStatsMessage.DamageList == null) ? 0f : playerStatsMessage.DamageList.Where((PlayerDamage m) => m.HurtType == HurtType.Suffocate).Sum((PlayerDamage m) => m.Amount));
			if (num5 > float.Epsilon)
			{
				healthEffect.SuffocationHit(num5);
				HealthSounds.Play(1);
			}
			float num6 = ((playerStatsMessage.DamageList == null) ? 0f : playerStatsMessage.DamageList.Where((PlayerDamage m) => m.HurtType == HurtType.Frost).Sum((PlayerDamage m) => m.Amount));
			if (num6 > float.Epsilon)
			{
			}
			float num7 = ((playerStatsMessage.DamageList == null) ? 0f : playerStatsMessage.DamageList.Where((PlayerDamage m) => m.HurtType == HurtType.Heat).Sum((PlayerDamage m) => m.Amount));
			if (num7 > float.Epsilon)
			{
				burningEffect.BurnEffect(3f);
				healthEffect.LowPressureHit(num7 * 2f);
				HealthSounds.Play(3);
			}
			float num8 = ((playerStatsMessage.DamageList == null) ? 0f : playerStatsMessage.DamageList.Where((PlayerDamage m) => m.HurtType == HurtType.Shred).Sum((PlayerDamage m) => m.Amount));
			if (num8 > float.Epsilon)
			{
				burningEffect.BurnEffect(3f);
				healthEffect.LowPressureHit(num8 * 2f);
				HealthSounds.Play(3);
			}
			float num9 = ((playerStatsMessage.DamageList == null) ? 0f : playerStatsMessage.DamageList.Where((PlayerDamage m) => m.HurtType == HurtType.SpaceExposure).Sum((PlayerDamage m) => m.Amount));
			if (num9 > float.Epsilon)
			{
				burningEffect.BurnEffect(3f);
				healthEffect.LowPressureHit(num7 * 2f);
				HealthSounds.Play(3);
			}
			Health = playerStatsMessage.Health;
			healthEffect.Health = Health;
			AkSoundEngine.SetRTPCValue(SoundManager.instance.Health, Health);
			if (inventoryUI.gameObject.activeInHierarchy)
			{
				inventoryUI.UpdateArmorAndHealth();
			}
		}

		public void UpdateCameraPositions()
		{
			if (!(Parent == null))
			{
				Quaternion quaternion = Quaternion.LookRotation(Parent.Forward, Parent.Up);
				Vector3D position = Parent.Position;
				Vector3D position2 = Parent.Position;
				if (Parent is SpaceObjectVessel)
				{
					SpaceObjectVessel mainVessel = (Parent as SpaceObjectVessel).MainVessel;
					quaternion = Quaternion.LookRotation(mainVessel.Forward, mainVessel.Up);
					position = mainVessel.Position;
				}
				position2 = position + (quaternion * base.transform.position).ToVector3D();
				SunCameraRoot.position = (position2 / 149597870.7).ToVector3();
				SunCamera.localRotation = FpsController.MainCamera.transform.rotation;
				SunCameraRoot.rotation = quaternion;
				PlanetsCamera.localPosition = (new Vector3D(base.transform.position) / 1000000.0).ToVector3();
				if (CurrentStance == PlayerStance.Special && Inventory.ItemInHands is Weapon && (Inventory.ItemInHands as Weapon).CanZoom)
				{
					PlanetsCamera.localRotation = (Inventory.ItemInHands as MilitaryCorpRailgunSniper).zoomCamera.transform.rotation;
				}
				else
				{
					PlanetsCamera.localRotation = FpsController.MainCamera.transform.rotation;
				}
				Client.Instance.ShipExteriorRoot.transform.rotation = Quaternion.Inverse(quaternion);
				Client.Instance.PlanetsRootTransform.rotation = Quaternion.Inverse(quaternion);
				Client.Instance.PlanetsSunLightTransform.forward = Client.Instance.PlanetsRootTransform.TransformDirection(position2.Normalized.ToVector3());
				Client.Instance.ShipSunLightTransform.forward = Client.Instance.PlanetsRootTransform.TransformDirection(position2.Normalized.ToVector3());
				Client.Instance.ShipSunLightTransform.position = base.transform.position;
				UpdateSunFlareAndItensity(quaternion, position2);
			}
		}

		private void UpdateSunFlareAndItensity(Quaternion lookRotation, Vector3D playerPosition)
		{
			Vector3 vector = lookRotation.Inverse() * -playerPosition.Normalized.ToVector3();
			Vector3 vector2 = vector * 100000f;
			float num = Vector3.Angle(vector, FpsController.MainCamera.transform.forward);
			bool flag = true;
			if (Physics.Raycast(PlanetsCamera.position, vector, out var _, float.PositiveInfinity, planetsRaycastLayer))
			{
				flag = false;
			}
			if (flag && shipExteriorSunLight.intensity < 1f)
			{
				shipExteriorSunLight.intensity = Mathf.Clamp01(shipExteriorSunLight.intensity + Time.deltaTime * 1f);
				if (shipExteriorSunLight.GetComponentsInChildren<SunFlareEffect>().Length > 0)
				{
					shipExteriorSunLight.GetComponentsInChildren<SunFlareEffect>()[0].UpdateFlareBrightness(shipExteriorSunLight.intensity);
				}
			}
			else if (!flag && shipExteriorSunLight.intensity > 0f)
			{
				shipExteriorSunLight.intensity = Mathf.Clamp01(shipExteriorSunLight.intensity - Time.deltaTime * 1f);
				if (shipExteriorSunLight.GetComponentsInChildren<SunFlareEffect>().Length > 0)
				{
					shipExteriorSunLight.GetComponentsInChildren<SunFlareEffect>()[0].UpdateFlareBrightness(shipExteriorSunLight.intensity);
				}
			}
		}

		private void Update()
		{
			if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.I))
			{
				isAudioDebug = !isAudioDebug;
			}
			TriggerRaycast();
			UpdateInputKeys();
			if (cameraFovLerpValue.IsNotEpsilonZero())
			{
				LerpCameraFov();
			}
			if (Parent == null)
			{
				return;
			}
			if ((ShipControlMode == ShipControlMode.Piloting || ShipControlMode == ShipControlMode.Docking || LockedToTrigger is SceneTriggerDockingPanel) && Parent is SpaceObjectVessel)
			{
				SpaceObjectVessel spaceObjectVessel = ((!(LockedToTrigger != null)) ? (Parent as SpaceObjectVessel) : LockedToTrigger.ParentShip);
				if (ShipControlMode == ShipControlMode.Docking || LockedToTrigger is SceneTriggerDockingPanel)
				{
					if (!(LockedToTrigger is SceneTriggerDockingPanel) || (LockedToTrigger as SceneTriggerDockingPanel).MyDockingPanel.IsDockingEnabled)
					{
						SceneDockingPort dockingPort = Client.Instance.InGamePanels.Docking.DockingPort;
						if (dockingPort != null)
						{
							Quaternion quaternion = Quaternion.LookRotation(Parent.transform.forward, Parent.transform.up).Inverse() * Quaternion.LookRotation(dockingPort.transform.forward, dockingPort.transform.up);
							spaceObjectVessel.ChangeStats(shipThrust * shipThrustStrength, quaternion * shipRotation * shipRotationStrength);
						}
					}
				}
				else if (ShipControlMode == ShipControlMode.Piloting && !FpsController.IsFreeLook && !Client.Instance.CanvasManager.ConsoleIsUp)
				{
					Vector3 shipRotationCursor = ShipRotationCursor;
					ShipRotationCursor.x -= Mathf.Clamp((float)((!Client.Instance.InvertMouseWhileDriving) ? 1 : (-1)) * TeamUtility.IO.InputManager.GetAxis("LookVertical"), -1f, 1f);
					ShipRotationCursor.y += Mathf.Clamp(TeamUtility.IO.InputManager.GetAxis("LookHorizontal"), -1f, 1f);
					if (ZeroGravity.UI.InputManager.GetButton(ZeroGravity.UI.InputManager.AxisNames.LeftShift))
					{
						ShipRotationCursor *= spaceObjectVessel.RCS.RotationAcceleration / spaceObjectVessel.RCS.RotationStabilization;
					}
					Vector3 vector = ShipRotationCursor - shipRotationCursor;
					float magnitude = vector.magnitude;
					if (magnitude > spaceObjectVessel.RCS.MaxOperationRate)
					{
						vector = vector.normalized * spaceObjectVessel.RCS.MaxOperationRate;
						ShipRotationCursor += vector;
					}
					if (vector.IsNotEpsilonZero() && !ZeroGravity.UI.InputManager.GetButton(ZeroGravity.UI.InputManager.AxisNames.LeftShift))
					{
						lastShipRotationCursorChangeTime = Time.realtimeSinceStartup;
						shipRotation.x = vector.x;
						shipRotation.y = vector.y;
						if (shipRotation.IsNotEpsilonZero())
						{
							(spaceObjectVessel as Ship).IsRotationStabilized = false;
						}
						else
						{
							ShipRotationCursor = Vector3.zero;
						}
					}
					else
					{
						magnitude = ShipRotationCursor.magnitude;
						ShipRotationCursor = Vector3.Lerp(ShipRotationCursor, Quaternion.LookRotation(spaceObjectVessel.Forward, spaceObjectVessel.Up).Inverse() * spaceObjectVessel.MainVessel.AngularVelocity, (Time.realtimeSinceStartup - lastShipRotationCursorChangeTime) / 5f);
						ShipRotationCursor.z = 0f;
					}
					Vector3 value = shipThrust * RcsThrustModifier * shipThrustStrength;
					if (value.IsNotEpsilonZero())
					{
						if (ShipControlMode == ShipControlMode.Piloting && Client.Instance.InGamePanels.Pilot.SelectedTarget != null && Mathf.Abs(Vector3.Dot(shipThrust.normalized, spaceObjectVessel.Forward)) > 0.9f && Client.Instance.OffSpeedHelper)
						{
							Vector3 from = (Client.Instance.InGamePanels.Pilot.SelectedTarget.AB.Position - spaceObjectVessel.Position).ToVector3();
							Vector3 vector2 = (Client.Instance.InGamePanels.Pilot.SelectedTarget.AB.Velocity - spaceObjectVessel.Velocity).ToVector3();
							Vector3 vector3 = Vector3.ProjectOnPlane(vector2, from.normalized);
							float num = Vector3.Angle(from, spaceObjectVessel.Forward);
							if (num <= 3f)
							{
								shipThrust += vector3 * 0.1f * MathHelper.Clamp(1f - num / 3f, 0f, 1f);
								shipThrust.x = MathHelper.Clamp(shipThrust.x, -1f, 1f);
								shipThrust.y = MathHelper.Clamp(shipThrust.y, -1f, 1f);
								shipThrust.z = MathHelper.Clamp(shipThrust.z, -1f, 1f);
								Client.Instance.InGamePanels.Pilot.CancelInvoke(Client.Instance.InGamePanels.Pilot.OffSpeedHelperInactive);
								Client.Instance.InGamePanels.Pilot.OffSpeedHelperActive();
								Client.Instance.InGamePanels.Pilot.Invoke(Client.Instance.InGamePanels.Pilot.OffSpeedHelperInactive, 1f);
							}
						}
						spaceObjectVessel.ChangeStats(shipThrust * RcsThrustModifier * shipThrustStrength);
					}
					if (shipRotation.IsNotEpsilonZero())
					{
						Vector3? rotation = shipRotation * shipRotationStrength;
						spaceObjectVessel.ChangeStats(null, rotation);
					}
				}
			}
			if (cameraLerpHelper < 1f && cameraLerpPosTo.HasValue && cameraLerpPosFrom.HasValue && cameraLerpRotFrom.HasValue && cameraLerpRotTo.HasValue)
			{
				cameraLerpHelper += Mathf.Clamp01(Time.deltaTime * 2f);
				if (cameraLerpLocal)
				{
					FpsController.MainCamera.transform.localPosition = Vector3.Lerp(cameraLerpPosFrom.Value, cameraLerpPosTo.Value, cameraLerpHelper);
					FpsController.MainCamera.transform.localRotation = Quaternion.Lerp(cameraLerpRotFrom.Value, cameraLerpRotTo.Value, cameraLerpHelper);
				}
				else
				{
					FpsController.MainCamera.transform.position = Vector3.Lerp(cameraLerpPosFrom.Value, cameraLerpPosTo.Value, cameraLerpHelper);
					FpsController.MainCamera.transform.rotation = Quaternion.Lerp(cameraLerpRotFrom.Value, cameraLerpRotTo.Value, cameraLerpHelper);
				}
				if (cameraLerpHelper >= 1f)
				{
					if (cameraLerpLocal)
					{
						FpsController.MainCamera.transform.localPosition = cameraLerpPosTo.Value;
						FpsController.MainCamera.transform.localRotation = cameraLerpRotTo.Value;
					}
					else
					{
						FpsController.MainCamera.transform.position = cameraLerpPosTo.Value;
						FpsController.MainCamera.transform.rotation = cameraLerpRotTo.Value;
					}
					cameraLerpPosFrom = null;
					cameraLerpRotFrom = null;
					cameraLerpPosTo = null;
					cameraLerpRotTo = null;
					cameraLerpLocal = false;
					cameraLerpHelper = 10f;
				}
			}
			if (base.CurrentRoomTrigger != null)
			{
				if (Pressure != base.CurrentRoomTrigger.InterpolatedAirPressure)
				{
					Pressure = base.CurrentRoomTrigger.InterpolatedAirPressure;
				}
			}
			else if (Pressure != 0f)
			{
				Pressure = 0f;
			}
			if (passiveScanTimer.Update())
			{
				DoPassiveScan();
			}
			Debug.DrawRay(Vector3.zero, Client.Instance.ShipExteriorRoot.transform.rotation * DebrisFieldVelocityDirection.ToVector3() * 200f, Color.magenta);
		}

		public void DoPassiveScan()
		{
			IEnumerable<SubSystemRadar> source = (from m in Client.Instance.ActiveVessels.Values
				where m.RadarSystem != null
				select m.RadarSystem into m
				orderby m.PassiveScanSensitivity
				select m).Reverse();
			SubSystemRadar subSystemRadar = source.FirstOrDefault((SubSystemRadar m) => m.ParentVessel != null && m.ParentVessel.VesselBaseSystem.Status == SystemStatus.OnLine && m.ParentVessel.IsPlayerAuthorized(this));
			if (subSystemRadar != null)
			{
				subSystemRadar.PassiveScan();
			}
		}

		private void TriggerRaycast()
		{
			LookingAtPlayer = null;
			if (Physics.Raycast(FpsController.CameraPosition, FpsController.CameraForward, out var hitInfo, farRaycastDistance, playerLookRaycastMask))
			{
				LookingAtPlayer = hitInfo.collider.GetComponent<Player>();
				LookingAtItem = hitInfo.collider.GetComponent<Item>();
				LookingAtCorpseCollider = hitInfo.collider.GetComponent<RagdollCollider>();
				lootRaycastPoint = hitInfo.point;
				if (LookingAtItem == null)
				{
					LookingAtTrigger = hitInfo.collider.GetComponent<BaseSceneTrigger>();
					if (LookingAtTrigger != null && LookingAtTrigger is SceneTrigger)
					{
						SceneSpawnPoint componentInChildren = (LookingAtTrigger as SceneTrigger).transform.parent.GetComponentInChildren<SceneSpawnPoint>();
						if (componentInChildren != null && componentInChildren.PlayerGUID > 0 && componentInChildren.PlayerGUID != base.GUID)
						{
							LookingAtTrigger = null;
						}
					}
					if (LookingAtTrigger != null && LockedToTrigger == null && LookingAtTrigger.Glossary != null && ZeroGravity.UI.InputManager.GetKey(KeyCode.F1))
					{
						AbstractGlossaryElement glossary = LookingAtTrigger.Glossary;
						Client.Instance.CanvasManager.PlayerOverview.Toggle(val: true, 0, gloss: true);
						Client.Instance.CanvasManager.PlayerOverview.Glossary.OpenElement(glossary);
					}
					if (LookingAtTrigger != null && LookingAtTrigger.IsNearTrigger && hitInfo.distance > nearRaycastDistance)
					{
						LookingAtTrigger = null;
					}
				}
				else
				{
					LookingAtTrigger = null;
				}
				if (((LookingAtItem != null || LookingAtTrigger != null) && (LockedToTrigger == null || FpsController.IsFreeLook)) || (LookingAtCorpseCollider != null && LookingAtCorpseCollider.CorpseObject != null))
				{
					if (LookingAtCorpseCollider != null && LookingAtCorpseCollider.CorpseObject != null)
					{
						Client.Instance.CanvasManager.CanvasUI.ToggleCroshair(show: true, null, canLoot: true);
					}
					else if (PrevLookingAtItem != LookingAtItem && LookingAtTrigger == null)
					{
						Client.Instance.CanvasManager.CanvasUI.SetItemName(LookingAtItem);
						if ((LookingAtItem is Outfit && (LookingAtItem as Outfit).AllItems() != null && (LookingAtItem as Outfit).AllItems().Count > 0) || (LookingAtItem.IsSlotContainer && LookingAtItem.Slots.Count > 0))
						{
							Client.Instance.CanvasManager.CanvasUI.ToggleCroshair(show: true, null, canLoot: true);
						}
						else
						{
							Client.Instance.CanvasManager.CanvasUI.ToggleCroshair(show: true);
						}
					}
					else if (LookingAtTrigger != null)
					{
						Client.Instance.CanvasManager.CanvasUI.SetItemName(null);
						Client.Instance.CanvasManager.CanvasUI.ToggleCroshair(show: true, LookingAtTrigger);
					}
				}
				else if (Client.Instance.CanvasManager.CanvasUI.IsCroshairActive)
				{
					Client.Instance.CanvasManager.CanvasUI.ToggleCroshair(show: false);
				}
			}
			else
			{
				LookingAtTrigger = null;
				LookingAtItem = null;
				if (Client.Instance.CanvasManager.CanvasUI.IsCroshairActive)
				{
					Client.Instance.CanvasManager.CanvasUI.ToggleCroshair(show: false);
				}
			}
		}

		public void ReloadItem(Item newReloadingItem, Item currentReloadingItem, AnimatorHelper.ReloadType reloadType, ItemType itemType)
		{
			this.newReloadingItem = newReloadingItem;
			this.currentReloadingItem = currentReloadingItem;
			AnimatorHelper animatorHelper = animHelper;
			AnimatorHelper.ReloadType? reloadType2 = reloadType;
			animatorHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, reloadType2);
			AnimatorHelper animatorHelper2 = animHelper;
			ItemType? itemType2 = itemType;
			animatorHelper2.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, itemType2);
			animHelper.SetParameter(null, null, null, null, null, null, null, null, null, true);
		}

		public void ReloadStepComplete(AnimatorHelper.ReloadStepType type)
		{
			switch (type)
			{
			case AnimatorHelper.ReloadStepType.ReloadStart:
				base.CurrentActiveItem.ReloadStepComplete(this, type, ref currentReloadingItem, ref newReloadingItem);
				break;
			case AnimatorHelper.ReloadStepType.ItemSwitch:
				base.CurrentActiveItem.ReloadStepComplete(this, type, ref currentReloadingItem, ref newReloadingItem);
				break;
			case AnimatorHelper.ReloadStepType.ReloadEnd:
				base.CurrentActiveItem.ReloadStepComplete(this, type, ref currentReloadingItem, ref newReloadingItem);
				newReloadingItem = null;
				currentReloadingItem = null;
				break;
			case AnimatorHelper.ReloadStepType.UnloadEnd:
				base.CurrentActiveItem.ReloadStepComplete(this, type, ref currentReloadingItem, ref newReloadingItem);
				newReloadingItem = null;
				currentReloadingItem = null;
				break;
			}
		}

		private void UpdateInputKeys()
		{
			if (Client.IsGameBuild && (Client.Instance.CanvasManager.DeadScreen.activeInHierarchy || Client.Instance.IsChatOpened || Client.Instance.CanvasManager.IsInputFieldIsActive || Client.Instance.CanvasManager.Console.gameObject.activeInHierarchy))
			{
				return;
			}
			shipThrust = Vector3.zero;
			shipRotation = Vector3.zero;
			if (Client.Instance.SinglePlayerMode)
			{
				if (Input.GetKeyUp(KeyCode.F5))
				{
					Client.Instance.QuickSave();
				}
				else if (Input.GetKeyUp(KeyCode.F9))
				{
					Client.Instance.QuickLoad();
				}
			}
			if (ZeroGravity.UI.InputManager.GetButtonDown(ZeroGravity.UI.InputManager.AxisNames.J))
			{
				FpsController.ToggleJetPack();
			}
			if (ZeroGravity.UI.InputManager.GetButtonDown(ZeroGravity.UI.InputManager.AxisNames.LeftAlt))
			{
				if (Inventory.ItemInHands == null)
				{
					HighlightAttachPoints();
				}
				else
				{
					Item itemInHands = Inventory.ItemInHands;
					GenericItemSubType? generic = null;
					MachineryPartType? part = null;
					int? partTier = null;
					if (itemInHands is GenericItem)
					{
						generic = (itemInHands as GenericItem).SubType;
					}
					if (itemInHands is MachineryPart)
					{
						part = (itemInHands as MachineryPart).PartType;
						partTier = (itemInHands as MachineryPart).Tier;
					}
					HighlightAttachPoints(itemInHands.Type, generic, part, partTier);
				}
			}
			else if (ZeroGravity.UI.InputManager.GetButtonUp(ZeroGravity.UI.InputManager.AxisNames.LeftAlt))
			{
				Invoke("HideHiglightedAttachPoints", 5f);
			}
			if (ZeroGravity.UI.InputManager.GetButtonDown(ZeroGravity.UI.InputManager.AxisNames.V) && animHelper.CanMelee && (base.CurrentActiveItem == null || base.CurrentActiveItem.HasMelee))
			{
				animHelper.SetParameterTrigger(AnimatorHelper.Triggers.Melee);
			}

			// Open the in game meny when pressing escape.
			if (ZeroGravity.UI.InputManager.GetKeyDown(KeyCode.Escape) && LockedToTrigger == null && !Client.Instance.CanvasManager.IsGameMenuOpen && !Client.Instance.CanvasManager.ScreenShootMod.activeInHierarchy)
			{
				if (Client.Instance.CanvasManager.IsPlayerOverviewOpen)
				{
					Client.Instance.CanvasManager.PlayerOverview.Toggle(val: false);
				}
				else
				{
					Client.Instance.CanvasManager.OpenInGameMenu();
				}
			}
			if (ZeroGravity.UI.InputManager.GetButtonDown(ZeroGravity.UI.InputManager.AxisNames.Mouse3) && !FpsController.IsZeroG)
			{
				Vector3 position = base.transform.position + base.Gravity.normalized + base.transform.forward * 0.7f;
				Collider[] array = Physics.OverlapSphere(position, 0.7f);
				Collider[] array2 = array;
				foreach (Collider collider in array2)
				{
					GenericItem componentInParent = collider.GetComponentInParent<GenericItem>();
					if (componentInParent != null && componentInParent.AttachPoint == null && componentInParent.SubType == GenericItemSubType.BasketBall)
					{
						componentInParent.DynamicObj.ToggleKinematic(value: false);
						componentInParent.DynamicObj.rigidBody.AddForce(base.transform.forward * 4f + base.transform.up * 2f, ForceMode.VelocityChange);
						ImpactDetector componentInChildren = componentInParent.DynamicObj.GetComponentInChildren<ImpactDetector>(includeInactive: true);
						if (componentInChildren != null)
						{
							componentInChildren.PlayImpactSound(4f);
						}
						break;
					}
				}
			}
			if ((ZeroGravity.UI.InputManager.GetKeyDown(KeyCode.Tab) || ZeroGravity.UI.InputManager.GetKeyDown(KeyCode.Escape)) && (LockedToTrigger != null || CancelInteractExecuter != null))
			{
				CancelInteract();
			}
			else if (LockedToTrigger == null && !InIteractLayer && !InLerpingState && !Instance.FpsController.IsOnLadder && Client.IsGameBuild)
			{
				if (ZeroGravity.UI.InputManager.GetButtonDown(ZeroGravity.UI.InputManager.AxisNames.O))
				{
					Client.Instance.CanvasManager.PlayerOverview.Toggle(!Client.Instance.CanvasManager.IsPlayerOverviewOpen, 1);
				}
				if (ZeroGravity.UI.InputManager.GetButtonDown(ZeroGravity.UI.InputManager.AxisNames.Tab) && !Client.Instance.CanvasManager.IsGameMenuOpen)
				{
					inventoryQuickTime = Time.time;
					Client.Instance.CanvasManager.PlayerOverview.Toggle(!Client.Instance.CanvasManager.IsPlayerOverviewOpen);
				}
				else if (ZeroGravity.UI.InputManager.GetButtonUp(ZeroGravity.UI.InputManager.AxisNames.Tab) && !Client.Instance.CanvasManager.IsGameMenuOpen && inventoryQuickTime > 0f)
				{
					if (Time.time - inventoryQuickTime > inventoryTreshold && Client.Instance.CanvasManager.IsPlayerOverviewOpen)
					{
						Client.Instance.CanvasManager.PlayerOverview.Toggle(val: false);
					}
					inventoryQuickTime = -1f;
				}
			}
			if (Client.IsGameBuild && (Client.Instance.CanvasManager.IsGameMenuOpen || Client.Instance.CanvasManager.IsPlayerOverviewOpen))
			{
				return;
			}
			if (CurrentHelmet != null)
			{
				if (ZeroGravity.UI.InputManager.GetButtonDown(ZeroGravity.UI.InputManager.AxisNames.H))
				{
					CurrentHelmet.ToggleVisor();
				}
				if (ZeroGravity.UI.InputManager.GetButtonDown(ZeroGravity.UI.InputManager.AxisNames.L) && !IsDrivingShip)
				{
					CurrentHelmet.ToggleLight(!CurrentHelmet.LightOn);
				}
			}
			if (ZeroGravity.UI.InputManager.GetButtonDown(ZeroGravity.UI.InputManager.AxisNames.G) && Inventory.ItemInHands != null && animHelper.CanDrop)
			{
				dropThrowStartTime = Time.time;
			}
			else if (ZeroGravity.UI.InputManager.GetButton(ZeroGravity.UI.InputManager.AxisNames.G) && Inventory.ItemInHands != null && animHelper.CanDrop)
			{
				if (!Client.Instance.CanvasManager.CanvasUI.ThrowingItem.activeInHierarchy && Time.time - dropThrowStartTime >= Client.DROP_THRESHOLD)
				{
					Client.Instance.CanvasManager.CanvasUI.ThrowingItemToggle(val: true);
				}
			}
			else if (ZeroGravity.UI.InputManager.GetButtonUp(ZeroGravity.UI.InputManager.AxisNames.G))
			{
				Client.Instance.CanvasManager.CanvasUI.ThrowingItemToggle(val: false);
				if (Inventory.ItemInHands != null)
				{
					Inventory.ItemInHands.RequestDrop(Mathf.Clamp(Time.time - dropThrowStartTime - Client.DROP_THRESHOLD, 0f, Client.DROP_MAX_TIME));
				}
				dropThrowStartTime = -1f;
			}
			if (InLadderTrigger && !FpsController.IsZeroG)
			{
				if (ZeroGravity.UI.InputManager.GetButtonDown(ZeroGravity.UI.InputManager.AxisNames.F) && FpsController.Velocity.magnitude < LadderTrigger.MaxAttachVelocity)
				{
					LockedToTrigger = LookingAtTrigger;
					LadderTrigger.LadderAttach(this);
				}
				if ((ZeroGravity.UI.InputManager.GetButtonDown(ZeroGravity.UI.InputManager.AxisNames.Space) || ZeroGravity.UI.InputManager.GetKeyDown(KeyCode.Tab)) && FpsController.IsOnLadder)
				{
					LadderTrigger.LadderDetach(Instance);
				}
			}
			if (ZeroGravity.UI.InputManager.GetButtonDown(ZeroGravity.UI.InputManager.AxisNames.F) && LookingAtItem != null && animHelper.CanPickUp)
			{
				pickUpWhenFullCounter = 0f;
				switchItemTime = Time.time;
			}
			else if (ZeroGravity.UI.InputManager.GetButton(ZeroGravity.UI.InputManager.AxisNames.F) && (LookingAtItem != null || LookingAtCorpseCollider != null) && animHelper.CanPickUp)
			{
				if (LookingAtItem != null || LookingAtCorpseCollider != null)
				{
					pickUpWhenFullCounter += Time.deltaTime;
					if (pickUpWhenFullCounter > pickUpWhenFullTreshold)
					{
						if ((LookingAtItem != null && LookingAtItem is Outfit && (LookingAtItem as Outfit).AllItems() != null) || (LookingAtCorpseCollider != null && LookingAtCorpseCollider.CorpseObject != null) || (LookingAtItem.IsSlotContainer && LookingAtItem.Slots.Count > 0))
						{
							if (LookingAtItem != null)
							{
								if (LookingAtItem is Outfit)
								{
									Client.Instance.CanvasManager.PlayerOverview.Inventory.LootingTarget = LookingAtItem as Outfit;
									Client.Instance.CanvasManager.PlayerOverview.Toggle(val: true);
								}
								else if (LookingAtItem.IsSlotContainer && LookingAtItem.Slots.Count > 0)
								{
									Client.Instance.CanvasManager.PlayerOverview.Inventory.LootingTarget = LookingAtItem;
									Client.Instance.CanvasManager.PlayerOverview.Toggle(val: true);
								}
							}
							else if (Vector3.Distance(base.transform.position, LookingAtCorpseCollider.CorpseObject.transform.position) < FarRaycastDistance)
							{
								Client.Instance.CanvasManager.PlayerOverview.Inventory.LootingTarget = LookingAtCorpseCollider.CorpseObject.GetComponentInParent<Corpse>().Inventory;
								Client.Instance.CanvasManager.PlayerOverview.Toggle(val: true);
							}
							pickUpWhenFullCounter = 0f;
						}
						else if (LookingAtItem.CanPlayerPickUp(this))
						{
							LookingAtItem.RequestPickUp(handsFirst: true);
							LookingAtItem = null;
							pickUpWhenFullCounter = 0f;
						}
						pickingUpItem = true;
					}
				}
				else
				{
					pickUpWhenFullCounter = 0f;
				}
			}
			else if (ZeroGravity.UI.InputManager.GetButtonUp(ZeroGravity.UI.InputManager.AxisNames.F) && LookingAtItem != null && animHelper.CanPickUp && Time.time - switchItemTime < pickUpWhenFullTreshold && LookingAtItem.CanPlayerPickUp(this))
			{
				LookingAtItem.RequestPickUp();
			}
			if (ZeroGravity.UI.InputManager.GetButtonUp(ZeroGravity.UI.InputManager.AxisNames.F) && pickingUpItem)
			{
				pickingUpItem = false;
				return;
			}
			if (ZeroGravity.UI.InputManager.GetButtonUp(ZeroGravity.UI.InputManager.AxisNames.F) && LookingAtTrigger != null && !pickingUpItem && LockedToTrigger == null && !LookingAtTrigger.OtherPlayerLockedToTrigger() && LookingAtTrigger.IsInteractable && (FpsController.IsZeroG || FpsController.IsGrounded) && (LookingAtTrigger.PlayerHandsCheck == PlayerHandsCheckType.DontCheck || (LookingAtTrigger.PlayerHandsCheck == PlayerHandsCheckType.HandsMustBeEmpty && (Inventory == null || Inventory.ItemInHands == null)) || (LookingAtTrigger.PlayerHandsCheck == PlayerHandsCheckType.StoreItemInHands && (Inventory == null || Inventory.StoreItemInHands())) || (LookingAtTrigger.PlayerHandsCheck == PlayerHandsCheckType.MustHaveItemInHands && LookingAtTrigger.PlayerHandsItemType != null && Inventory != null && Inventory.ItemInHands != null && LookingAtTrigger.PlayerHandsItemType.Contains(Inventory.ItemInHands.Type))))
			{
				if (LookingAtTrigger.ExclusivePlayerLocking)
				{
					Client.Instance.NetworkController.SendToGameServer(new LockToTriggerMessage
					{
						TriggerID = LookingAtTrigger.GetID(),
						IsPilotingVessel = (LookingAtTrigger.TriggerType == SceneTriggerType.ShipControl || LookingAtTrigger.TriggerType == SceneTriggerType.DockingPanel)
					});
				}
				else
				{
					LookingAtTrigger.Interact(this);
				}
			}
			if (ZeroGravity.UI.InputManager.GetButtonDown(ZeroGravity.UI.InputManager.AxisNames.R) && Client.IsGameBuild)
			{
				reloadCalled = false;
				reloadButtonPressedTime = 0f;
			}
			if (ZeroGravity.UI.InputManager.GetButton(ZeroGravity.UI.InputManager.AxisNames.R) && Client.IsGameBuild && !reloadCalled)
			{
				reloadButtonPressedTime += Time.deltaTime;
				Item itemInHands2 = Inventory.ItemInHands;
				if (reloadButtonPressedTime > specialMenuTreshold && itemInHands2 != null && animHelper.CanSpecial)
				{
					reloadCalled = true;
					reloadButtonPressedTime = 0f;
					itemInHands2.Special();
				}
			}
			if ((ZeroGravity.UI.InputManager.GetButtonUp(ZeroGravity.UI.InputManager.AxisNames.R) || (LookingAtItem != null && ZeroGravity.UI.InputManager.GetButtonDown(ZeroGravity.UI.InputManager.AxisNames.F))) && Client.IsGameBuild && !animHelper.GetParameterBool(AnimatorHelper.Parameter.Reloading))
			{
				Item itemInHands3 = Inventory.ItemInHands;
				if (itemInHands3 != null && (ZeroGravity.UI.InputManager.GetButtonUp(ZeroGravity.UI.InputManager.AxisNames.R) || (ZeroGravity.UI.InputManager.GetButtonDown(ZeroGravity.UI.InputManager.AxisNames.F) && itemInHands3.CanReloadOnInteract(LookingAtItem))))
				{
					if (LookingAtItem != null)
					{
						itemInHands3.Reload(LookingAtItem);
					}
					else if (itemInHands3 is Weapon)
					{
						(itemInHands3 as Weapon).ReloadFromInventory();
					}
					LookingAtItem = null;
				}
			}
			if (ZeroGravity.UI.InputManager.GetButton(ZeroGravity.UI.InputManager.AxisNames.Mouse1) && Client.IsGameBuild && animHelper.CanSwitchState)
			{
				Item itemInHands4 = Inventory.ItemInHands;
				if (LockedToTrigger == null)
				{
					if (itemInHands4 != null)
					{
						if (itemInHands4.HasActiveStance && currentStance == PlayerStance.Passive && animHelper.doneSwitchingState)
						{
							currentStance = PlayerStance.Active;
							AnimatorHelper animatorHelper = animHelper;
							PlayerStance? playerStance = currentStance;
							animatorHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, playerStance);
							FpsController.SetStateSpeedMultiplier(itemInHands4.ActiveSpeedMultiplier);
						}
						if (ItemTypeRange.IsWeapon(itemInHands4.Type))
						{
							if (animHelper.CanShoot && currentStance != PlayerStance.Passive)
							{
								itemInHands4.PrimaryFunction();
							}
						}
						else
						{
							itemInHands4.PrimaryFunction();
						}
					}
				}
				else if (LockedToTrigger.TriggerType == SceneTriggerType.Turret)
				{
					(LockedToTrigger as SceneTriggerTurret).GetComponent<Turret>().Shoot();
				}
			}
			if (ZeroGravity.UI.InputManager.GetButtonUp(ZeroGravity.UI.InputManager.AxisNames.Mouse1) && Client.IsGameBuild)
			{
				Item itemInHands5 = Inventory.ItemInHands;
				if (itemInHands5 != null)
				{
					itemInHands5.PrimaryReleased();
				}
			}
			if (ZeroGravity.UI.InputManager.GetButtonUp(ZeroGravity.UI.InputManager.AxisNames.B) && Client.IsGameBuild)
			{
				Item itemInHands6 = Inventory.ItemInHands;
				if (itemInHands6 != null && itemInHands6 is Weapon)
				{
					(itemInHands6 as Weapon).IncrementMod();
				}
			}
			if (!animHelper.GetParameterBool(AnimatorHelper.Parameter.Reloading) && animHelper.CanSwitchState && ZeroGravity.UI.InputManager.GetButtonDown(ZeroGravity.UI.InputManager.AxisNames.Z))
			{
				Item itemInHands7 = Inventory.ItemInHands;
				if (itemInHands7 != null && itemInHands7.HasActiveStance)
				{
					currentStance = ((currentStance != PlayerStance.Passive) ? PlayerStance.Passive : PlayerStance.Active);
					FpsController.SetStateSpeedMultiplier((currentStance != PlayerStance.Passive) ? itemInHands7.ActiveSpeedMultiplier : itemInHands7.PassiveSpeedMultiplier);
					AnimatorHelper animatorHelper2 = animHelper;
					PlayerStance? playerStance = currentStance;
					animatorHelper2.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, playerStance);
					if (!FpsController.MainCamera.fieldOfView.IsEpsilonEqual(Client.DefaultCameraFov))
					{
						ChangeCamerasFov(Client.DefaultCameraFov);
					}
					if (itemInHands7 is Weapon)
					{
						(itemInHands7 as Weapon).IsSpecialStance = false;
					}
				}
			}
			if (ZeroGravity.UI.InputManager.GetButton(ZeroGravity.UI.InputManager.AxisNames.LeftShift) && characterController.CanLockToPoint && characterController.IsZeroG)
			{
				if (!characterController.IsLockedToPoint)
				{
					characterController.grabSlowEnabled = true;
				}
			}
			else if (ZeroGravity.UI.InputManager.GetButtonUp(ZeroGravity.UI.InputManager.AxisNames.LeftShift) && characterController.IsLockedToPoint)
			{
				characterController.ResetPlayerLock();
			}
			if (IsLockedToTrigger)
			{
				if (ZeroGravity.UI.InputManager.GetButtonDown(ZeroGravity.UI.InputManager.AxisNames.LeftAlt) && FpsController.MainCamera.fieldOfView != Client.DefaultCameraFov)
				{
					ChangeCamerasFov(Client.DefaultCameraFov);
				}
				else if (ZeroGravity.UI.InputManager.GetButtonUp(ZeroGravity.UI.InputManager.AxisNames.LeftAlt) && FpsController.MainCamera.fieldOfView != CurrentPanelFov && CurrentPanelFov > 10f)
				{
					ChangeCamerasFov(CurrentPanelFov);
				}
				else if (ZeroGravity.UI.InputManager.GetButtonDown(ZeroGravity.UI.InputManager.AxisNames.Mouse2) && ZeroGravity.UI.InputManager.GetButton(ZeroGravity.UI.InputManager.AxisNames.LeftAlt))
				{
					ChangeCamerasFov(cameraFovZoomMinValue);
				}
				else if (ZeroGravity.UI.InputManager.GetButtonUp(ZeroGravity.UI.InputManager.AxisNames.Mouse2) && ZeroGravity.UI.InputManager.GetButton(ZeroGravity.UI.InputManager.AxisNames.LeftAlt))
				{
					ChangeCamerasFov(Client.DefaultCameraFov);
				}
			}
			else if (ZeroGravity.UI.InputManager.GetButtonDown(ZeroGravity.UI.InputManager.AxisNames.Mouse2) && Client.IsGameBuild && CurrentStance != PlayerStance.Special)
			{
				cameraFovZoomCounter = Time.time;
			}
			else if (ZeroGravity.UI.InputManager.GetButton(ZeroGravity.UI.InputManager.AxisNames.Mouse2) && Client.IsGameBuild && CurrentStance != PlayerStance.Special)
			{
				if (Time.time - cameraFovZoomCounter > cameraFovZoomTreshold)
				{
					ChangeCamerasFov(cameraFovZoomMinValue);
				}
			}
			else if (ZeroGravity.UI.InputManager.GetButtonUp(ZeroGravity.UI.InputManager.AxisNames.Mouse2) && Client.IsGameBuild && !cameraFovLerpValue.IsNotEpsilonZero())
			{
				Item itemInHands8 = Inventory.ItemInHands;
				if (itemInHands8 != null)
				{
					itemInHands8.SecondaryFunction();
				}
			}
			else if (ZeroGravity.UI.InputManager.GetButtonUp(ZeroGravity.UI.InputManager.AxisNames.Mouse2) && cameraFovLerpValue.IsNotEpsilonZero() && currentStance != PlayerStance.Special)
			{
				ChangeCamerasFov(Client.DefaultCameraFov);
			}
			if (inventoryUI != null && ((Inventory != null) ? Inventory.Outfit : null) != null && animHelper.CanDrop)
			{
				if (ZeroGravity.UI.InputManager.GetButtonDown(ZeroGravity.UI.InputManager.AxisNames.Alpha1))
				{
					StartCoroutine(QuickSwitchItem(InventorySlot.Group.Primary));
				}
				else if (ZeroGravity.UI.InputManager.GetButtonDown(ZeroGravity.UI.InputManager.AxisNames.Alpha2))
				{
					StartCoroutine(QuickSwitchItem(InventorySlot.Group.Secondary));
				}
				else if (ZeroGravity.UI.InputManager.GetButtonDown(ZeroGravity.UI.InputManager.AxisNames.Alpha3))
				{
					StartCoroutine(QuickSwitchItem(null, new List<ItemType>
					{
						ItemType.APGrenade,
						ItemType.EMPGrenade
					}));
				}
				else if (ZeroGravity.UI.InputManager.GetButtonDown(ZeroGravity.UI.InputManager.AxisNames.Alpha4))
				{
					StartCoroutine(QuickSwitchItem(null, new List<ItemType>
					{
						ItemType.AltairMedpackBig,
						ItemType.AltairMedpackSmall,
						ItemType.AltairResourceContainer
					}));
				}
			}
			if (LockedToTrigger == null || (LockedToTrigger.TriggerType != SceneTriggerType.ShipControl && LockedToTrigger.TriggerType != SceneTriggerType.Turret && LockedToTrigger.TriggerType != SceneTriggerType.DockingPanel) || !(Parent is Ship))
			{
				return;
			}
			Ship ship = Parent as Ship;
			float axis = ZeroGravity.UI.InputManager.GetAxis(ZeroGravity.UI.InputManager.AxisNames.Lean);
			float axisRaw = ZeroGravity.UI.InputManager.GetAxisRaw(ZeroGravity.UI.InputManager.AxisNames.Forward);
			float axisRaw2 = ZeroGravity.UI.InputManager.GetAxisRaw(ZeroGravity.UI.InputManager.AxisNames.Right);
			float num = 0f;
			float num2 = 0f;
			if (!FpsController.IsFreeLook)
			{
				if (FpsController.MouseUpAxis.IsNotEpsilonZero())
				{
					num = Mathf.Clamp((float)(Client.Instance.InvertMouseWhileDriving ? 1 : (-1)) * TeamUtility.IO.InputManager.GetAxis("LookVertical"), -1f, 1f);
				}
				if (FpsController.MouseRightAxis.IsNotEpsilonZero())
				{
					num2 = TeamUtility.IO.InputManager.GetAxis("LookHorizontal");
				}
			}
			if (LockedToTrigger.TriggerType == SceneTriggerType.Turret)
			{
				(LockedToTrigger as SceneTriggerTurret).GetComponent<Turret>().SetNewRotation(num, num2, ZeroGravity.UI.InputManager.GetButton(ZeroGravity.UI.InputManager.AxisNames.Mouse2));
				return;
			}
			bool flag = false;
			if (ship.Engine != null && ship.Engine.Status == SystemStatus.OnLine && (LockedToTrigger == null || LockedToTrigger.TriggerType != SceneTriggerType.DockingPanel))
			{
				bool flag2 = false;
				bool button = ZeroGravity.UI.InputManager.GetButton(ZeroGravity.UI.InputManager.AxisNames.NumPlus);
				bool button2 = ZeroGravity.UI.InputManager.GetButton(ZeroGravity.UI.InputManager.AxisNames.NumMinus);
				if (button || button2 || axisRaw.IsNotEpsilonZero())
				{
					float num3;
					if (button)
					{
						num3 = 1f;
						cruiseControl = true;
					}
					else if (button2)
					{
						num3 = -1f;
						cruiseControl = true;
					}
					else
					{
						num3 = Mathf.Sign(axisRaw);
						cruiseControl = false;
					}
					if ((float) Mathf.Sign(ship.EngineThrustPercentage) != num3 && !cruiseControl)
					{
						ship.EngineThrustPercentage = 0f;
					}
					flag2 = !ship.EngineOnLine;
					changeEngineThrustTime += Time.deltaTime * 10f;
					if (changeEngineThrustTime > 0.1f)
					{
						float num4 = Mathf.Clamp(ship.EngineThrustPercentage + 0.01f * num3, -1f, 1f);
						changeEngineThrustTime -= 0.1f;
						if ((float) Mathf.Sign(num4) != prevEngineThrustDirection && prevEngineThrustDirection != 0f && cruiseControl)
						{
							num4 = 0f;
							changeEngineThrustTime = -10f;
						}
						flag2 |= ship.EngineThrustPercentage != num4;
						ship.EngineThrustPercentage = num4;
						if (num4 != float.Epsilon)
						{
							prevEngineThrustDirection = Mathf.Sign(num4);
						}
					}
				}
				else if (!cruiseControl)
				{
					changeEngineThrustTime = 0f;
					flag2 = ship.EngineOnLine || ship.EngineThrustPercentage != axisRaw;
					ship.EngineThrustPercentage = 0f;
				}
				if (flag2)
				{
					float? engineThrustPercentage = ship.EngineThrustPercentage;
					ship.ChangeStats(null, null, null, engineThrustPercentage);
				}
			}
			else if (axisRaw.IsNotEpsilonZero())
			{
				shipThrust = Mathf.Sign(axisRaw) * ThrustForward;
				flag = true;
			}
			if (ship.Engine != null && ZeroGravity.UI.InputManager.GetButtonDown(ZeroGravity.UI.InputManager.AxisNames.Enter))
			{
				if (ship.Engine.IsSwitchedOn())
				{
					ship.Engine.SwitchOff();
				}
				else
				{
					ship.Engine.SwitchOn();
				}
			}
			if (axisRaw2.IsNotEpsilonZero())
			{
				shipThrust += Mathf.Sign(axisRaw2) * ThrustRight;
				flag = true;
			}
			if (ZeroGravity.UI.InputManager.GetButton(ZeroGravity.UI.InputManager.AxisNames.Space) && !ZeroGravity.UI.InputManager.GetButton(ZeroGravity.UI.InputManager.AxisNames.LeftCtrl))
			{
				shipThrust += ThrustUp;
				flag = true;
			}
			if (ZeroGravity.UI.InputManager.GetButton(ZeroGravity.UI.InputManager.AxisNames.LeftCtrl) && !ZeroGravity.UI.InputManager.GetButton(ZeroGravity.UI.InputManager.AxisNames.Space))
			{
				shipThrust += -ThrustUp;
				flag = true;
			}
			if (flag && Parent is Ship)
			{
				if (ship.RCS.CanThrust())
				{
					shipThrustStrength = Mathf.Clamp01(shipThrustStrength + Time.smoothDeltaTime * Client.Instance.RCS_THRUST_SENSITIVITY);
				}
				else
				{
					shipThrustStrength = 0f;
				}
			}
			else
			{
				shipThrustStrength = 0f;
			}
			if (ZeroGravity.UI.InputManager.GetButton(ZeroGravity.UI.InputManager.AxisNames.LeftShift))
			{
				Vector3? autoStabilize = Vector3.one;
				ship.ChangeStats(null, null, autoStabilize);
			}
			else if (ZeroGravity.UI.InputManager.GetButtonUp(ZeroGravity.UI.InputManager.AxisNames.LeftShift))
			{
				Vector3? autoStabilize = Vector3.zero;
				ship.ChangeStats(null, null, autoStabilize);
			}
			if (ZeroGravity.UI.InputManager.GetButtonDown(ZeroGravity.UI.InputManager.AxisNames.M))
			{
				Client.Instance.InGamePanels.Pilot.StartTargetStabilization();
			}
			if (ZeroGravity.UI.InputManager.GetKeyDown(KeyCode.KeypadMinus) || ZeroGravity.UI.InputManager.GetKeyDown(KeyCode.KeypadPlus))
			{
				changeEngineThrust = true;
			}
			else if (ZeroGravity.UI.InputManager.GetKeyUp(KeyCode.KeypadMinus) && ZeroGravity.UI.InputManager.GetKeyUp(KeyCode.KeypadPlus))
			{
				changeEngineThrust = false;
				changeEngineThrustTime = 0f;
			}
			if (changeEngineThrust)
			{
				if (ZeroGravity.UI.InputManager.GetKey(KeyCode.KeypadMinus))
				{
					changeEngineThrustTime += Time.deltaTime * 5f;
					if (changeEngineThrustTime > 0.1f)
					{
						bool flag3 = ship.EngineThrustPercentage > float.Epsilon;
						ship.EngineThrustPercentage = Mathf.Clamp(ship.EngineThrustPercentage - 0.01f, -1f, 1f);
						if (flag3 && ship.EngineThrustPercentage <= 0f)
						{
							ship.EngineThrustPercentage = 0f;
							changeEngineThrust = false;
						}
						changeEngineThrustTime -= 0.1f;
						float? engineThrustPercentage = ship.EngineThrustPercentage;
						ship.ChangeStats(null, null, null, engineThrustPercentage);
					}
				}
				else if (ZeroGravity.UI.InputManager.GetKey(KeyCode.KeypadPlus))
				{
					changeEngineThrustTime += Time.deltaTime * 5f;
					if (changeEngineThrustTime > 0.1f)
					{
						bool flag4 = ship.EngineThrustPercentage < -1.401298E-45f;
						ship.EngineThrustPercentage = Mathf.Clamp(ship.EngineThrustPercentage + 0.01f, -1f, 1f);
						if (flag4 && ship.EngineThrustPercentage >= 0f)
						{
							ship.EngineThrustPercentage = 0f;
							changeEngineThrust = false;
						}
						changeEngineThrustTime -= 0.1f;
						float? engineThrustPercentage = ship.EngineThrustPercentage;
						ship.ChangeStats(null, null, null, engineThrustPercentage);
					}
				}
			}
			if (FpsController.IsFreeLook)
			{
				return;
			}
			bool flag5 = false;
			if (axis.IsNotEpsilonZero())
			{
				shipRotation.z = 0f - Mathf.Clamp(axis, -1f, 1f);
				flag5 = true;
			}
			if (num.IsNotEpsilonZero())
			{
				shipRotation.x = Mathf.Clamp(num, -1f, 1f);
				flag5 = true;
			}
			if (num2.IsNotEpsilonZero())
			{
				shipRotation.y = Mathf.Clamp(num2, -1f, 1f);
				flag5 = true;
			}
			if (flag5 && Parent is Ship)
			{
				if (ship.RCS.CanRotate())
				{
					shipRotationStrength = Mathf.Clamp01(shipRotationStrength + Time.smoothDeltaTime * Client.Instance.RCS_ROTATION_SENSITIVITY);
				}
				else
				{
					shipRotationStrength = 0f;
				}
			}
			else
			{
				shipRotationStrength = 0f;
			}
		}

		private void CancelInteract()
		{
			if (LockedToTrigger != null)
			{
				BaseSceneTrigger lockedToTrigger = LockedToTrigger;
				bool isDrivingShip = IsDrivingShip;
				LockedToTrigger.CancelInteract(this);
				if (lockedToTrigger != null && CancelInteractExecuter != null && !isDrivingShip)
				{
					SceneTriggerExecuter cancelInteractExecuter = CancelInteractExecuter;
					CancelInteractExecuter = null;
					cancelInteractExecuter.CancelInteract();
				}
			}
			else if (CancelInteractExecuter != null)
			{
				SceneTriggerExecuter cancelInteractExecuter2 = CancelInteractExecuter;
				CancelInteractExecuter = null;
				cancelInteractExecuter2.CancelInteract();
			}
		}

		private IEnumerator QuickSwitchItem(InventorySlot.Group? group, List<ItemType> types = null)
		{
			if (QuickSwitchItemRunning)
			{
				yield break;
			}
			QuickSwitchItemRunning = true;
			Item handsItem = Inventory.HandsSlot.Item;
			float t;
			if (group.HasValue)
			{
				InventorySlot slot = Inventory.Outfit.InventorySlots.FirstOrDefault((KeyValuePair<short, InventorySlot> m) => m.Value.SlotGroup == group.Value).Value;
				if (slot != null)
				{
					if (slot.Item != null)
					{
						if (handsItem != null)
						{
							InventorySlot freeSlot2 = Inventory.FindEmptyOutfitSlot(handsItem);
							if (freeSlot2 == null)
							{
								QuickSwitchItemRunning = false;
								yield break;
							}
							handsItem.RequestAttach(freeSlot2);
							t = Time.realtimeSinceStartup;
							yield return new WaitUntil(() => Inventory.HandsSlot.Item == null && (double)Time.realtimeSinceStartup > (double)t + 0.3);
							if (Inventory.HandsSlot.Item != null)
							{
								QuickSwitchItemRunning = false;
								yield break;
							}
						}
						slot.Item.RequestAttach(Inventory.HandsSlot);
						t = Time.realtimeSinceStartup;
						yield return new WaitUntil(() => Inventory.HandsSlot.Item != null && (double)Time.realtimeSinceStartup > (double)t + 0.3);
						if (Inventory.HandsSlot.Item is Weapon && CurrentStance == PlayerStance.Passive)
						{
							ChangeStance(PlayerStance.Active, 1f);
						}
						else if (CurrentStance != PlayerStance.Passive)
						{
							ChangeStance(PlayerStance.Passive, 1f);
						}
					}
					else if (handsItem != null && slot.CanFitItem(handsItem))
					{
						handsItem.RequestAttach(slot);
					}
				}
			}
			else if (types != null)
			{
				List<Item> items = (from m in Inventory.GetAllSlots().Values
					where m.Item != null && types.Contains(m.Item.Type) && (!(m.Item is ICargo) || (m.Item as ICargo).Compartments.Sum((ICargoCompartment n) => n.Capacity - n.AvailableCapacity) > 0f)
					select m.Item).ToList();
				List<ItemType> itemTypes = (from m in items.Select((Item m) => m.Type).Distinct()
					orderby types.IndexOf(m)
					select m).ToList();
				if (items.Count > 0)
				{
					Item nextItem2 = null;
					if (handsItem != null)
					{
						int idx = itemTypes.IndexOf(handsItem.Type) + 1;
						if (idx >= itemTypes.Count)
						{
							idx = 0;
						}
						nextItem2 = items.FirstOrDefault((Item m) => m.Type == itemTypes[idx]);
					}
					else
					{
						nextItem2 = items.FirstOrDefault((Item m) => m.Type == itemTypes[0]);
					}
					if (handsItem != null)
					{
						InventorySlot freeSlot = Inventory.FindEmptyOutfitSlot(handsItem);
						if (freeSlot == null)
						{
							QuickSwitchItemRunning = false;
							yield break;
						}
						if (CurrentStance != PlayerStance.Passive)
						{
							ChangeStance(PlayerStance.Passive, 1f);
						}
						ChangeStance(PlayerStance.Passive, 10f);
						handsItem.RequestAttach(freeSlot);
						t = Time.realtimeSinceStartup;
						yield return new WaitUntil(() => Inventory.HandsSlot.Item == null && (double)Time.realtimeSinceStartup > (double)t + 0.3);
						if (Inventory.HandsSlot.Item != null)
						{
							QuickSwitchItemRunning = false;
							yield break;
						}
					}
					if (nextItem2 != null && (handsItem == null || nextItem2.Type != handsItem.Type))
					{
						nextItem2.RequestAttach(Inventory.HandsSlot);
					}
				}
			}
			QuickSwitchItemRunning = false;
		}

		public SaveFileAuxData GetSaveFileAuxData()
		{
			Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();
			SaveFileAuxData saveFileAuxData = new SaveFileAuxData();
			saveFileAuxData.CelestialBody = ((!(Parent is ArtificialBody)) ? null : (Parent as ArtificialBody).Orbit.Parent.CelestialBody.Name);
			saveFileAuxData.ParentSceneID = ((!(Parent is SpaceObjectVessel)) ? GameScenes.SceneID.None : (Parent as SpaceObjectVessel).SceneID);
			saveFileAuxData.LockedToTrigger = ((!(LockedToTrigger != null)) ? null : LockedToTrigger.TriggerType.ToString());
			saveFileAuxData.Screenshot = tex.EncodeToJPG();
			saveFileAuxData.ClientVersion = new Regex("[^0-9.]").Replace(Application.version, string.Empty);
			saveFileAuxData.ClientHash = Client.CombinedHash;
			return saveFileAuxData;
		}

		public bool ActivatePlayer(PlayerSpawnResponse s)
		{
			if (Client.Instance.MainMenuSceneController != null)
			{
				Client.Instance.MainMenuSceneController.Disable();
			}
			SceneSpawnPoint sceneSpawnPoint = null;
			if (s.SpawnPointID > 0)
			{
				if (Parent is Ship)
				{
					sceneSpawnPoint = (Parent as Ship).GetStructureObject<SceneSpawnPoint>(s.SpawnPointID);
				}
				if (sceneSpawnPoint != null)
				{
					base.transform.position = sceneSpawnPoint.transform.position;
					base.transform.rotation = sceneSpawnPoint.transform.rotation;
				}
				else
				{
					Dbg.Error("Cannot find spawn point");
				}
			}
			else
			{
				base.transform.localPosition = s.CharacterTransform.LocalPosition.ToVector3();
				base.transform.localRotation = s.CharacterTransform.LocalRotation.ToQuaternion();
			}
			Health = s.Health;
			IsAdmin = s.IsAdmin;
			healthEffect.Health = Health;
			AkSoundEngine.SetRTPCValue(SoundManager.instance.Health, Health);
			equipSpawnDynamicObjects = s.DynamicObjects;
			base.gameObject.SetActive(value: true);
			rigidBody.isKinematic = false;
			FpsController.ResetVelocity();
			FpsController.ToggleMovement(true);
			AnimatorHelper animatorHelper = animHelper;
			bool? isZeroG = FpsController.IsZeroG;
			animatorHelper.SetParameter(null, null, isZeroG);
			if (sceneSpawnPoint != null && sceneSpawnPoint.Executer != null)
			{
				InIteractLayer = true;
				sceneSpawnPoint.Executer.SetExecuterDetails(new SceneTriggerExecuterDetails
				{
					PlayerThatActivated = Instance.GUID,
					InSceneID = sceneSpawnPoint.Executer.InSceneID,
					IsImmediate = true,
					IsFail = false,
					CurrentStateID = sceneSpawnPoint.Executer.CurrentStateID,
					NewStateID = sceneSpawnPoint.Executer.GetStateID(sceneSpawnPoint.ExecuterState)
				}, isInstant: false, null, checkCurrentState: false);
			}
			return true;
		}

		/// <summary>
		/// 	Spawns a player owned by the local machine into the world.
		/// </summary>
		public static MyPlayer SpawnMyPlayer(LogInResponse l)
		{
			long iD = l.ID;
			Client.Instance.NetworkController.SenderID = iD;
			GameObject characterObject = UnityEngine.Object.Instantiate(Resources.Load("Models/Units/Characters/FirstPersonCharacter")) as GameObject;
			GenderSettings genderSettings = characterObject.GetComponent<GenderSettings>();
			GenderSettings.GenderItem genderItem = null;
			foreach (GenderSettings.GenderItem setting in genderSettings.settings)
			{
				if (setting.Gender != l.Data.Gender)
				{
					GameObject.Destroy(setting.Outfit.gameObject);
				}
				else
				{
					genderItem = setting;
				}
			}
			if (genderItem == null)
			{
				Dbg.Error("AAAAAAAAAAAAAA");
				return null;
			}
			InventoryCharacterPreview.instance.ChangeGender(l.Data.Gender);
			MyPlayer myPlayer = characterObject.GetComponent<MyPlayer>();
			characterObject.name = "MyCharacter";

			GameObject headObject = UnityEngine.Object.Instantiate(Resources.Load("Models/Units/Characters/Heads/" + l.Data.Gender.ToString() + "/Head" + l.Data.HeadType)) as GameObject;
			headObject.transform.parent = characterObject.transform;
			headObject.transform.localPosition = new Vector3(0f, -1.34f, 0f);
			headObject.transform.localRotation = Quaternion.identity;
			headObject.transform.localScale = Vector3.one;

			AnimatorHelper animatorHelper = genderItem.Outfit.GetComponent<AnimatorHelper>();
			myPlayer.FpsController.animatorHelper = animatorHelper;
			myPlayer.animHelper = animatorHelper;
			myPlayer.FpsController.CameraController.animatorHelper = animatorHelper;

			myPlayer.HeadSkin = headObject.GetComponent<SkinnedMeshRenderer>();
			myPlayer.FpsController.HeadCameraParent = genderItem.HeadCameraParent;
			myPlayer.FpsController.ragdollChestRigidbody = myPlayer.FpsController.animatorHelper.GetBone(AnimatorHelper.HumanBones.Spine2).GetComponent<Rigidbody>();
			myPlayer.ragdollComponent = genderItem.Outfit.GetComponent<RagdollHelper>();
			myPlayer.Outfit = genderItem.Outfit;
			myPlayer.HeadSkin.rootBone = myPlayer.FpsController.animatorHelper.GetBone(AnimatorHelper.HumanBones.Spine2);
			myPlayer.ReferenceHead.rootBone = myPlayer.FpsController.animatorHelper.GetBone(AnimatorHelper.HumanBones.Spine2);
			myPlayer.ArmSkins.Clear();
			myPlayer.ArmSkins = genderItem.ArmSkins;
			myPlayer.UpdateReferenceHead();
			myPlayer.FpsController.CameraController.spineTransform = myPlayer.FpsController.animatorHelper.GetBone(AnimatorHelper.HumanBones.Spine2);
			myPlayer.RefreshOutfitData();
			myPlayer.GUID = iD;
			myPlayer.PlayerName = l.Data.Name;
			myPlayer.SunCameraRoot = Client.Instance.SunCameraRootTransform;
			myPlayer.SunCamera = Client.Instance.SunCameraTransform;
			myPlayer.PlanetsCameraRoot = Client.Instance.PlanetsCameraRootTransform;
			myPlayer.PlanetsCamera = Client.Instance.PlanetsCameraTransform;
			myPlayer.ShipSunLight = Client.Instance.ShipSunLightTransform;
			myPlayer.shipExteriorSunLight = Client.Instance.ShipSunLightTransform.GetComponent<Light>();
			myPlayer.gameObject.SetActive(value: false);
			myPlayer.rigidBody.isKinematic = true;
			AkSoundEngine.SetRTPCValue(SoundManager.instance.InGameVolume, 0f);
			Client.Instance.NetworkController.SendToGameServer(new ConsoleMessage
			{
				Text = "god"
			});
			Client.Instance.CanvasManager.DefaultInteractionTipSeen = false;
			return myPlayer;
		}

		public void SendPlayerRoomMessage()
		{
			if (Client.IsGameBuild)
			{
				PlayerRoomMessage playerRoomMessage = new PlayerRoomMessage();
				if (base.CurrentRoomTrigger != null)
				{
					playerRoomMessage.ID = new VesselObjectID(base.CurrentRoomTrigger.ParentVessel.GUID, base.CurrentRoomTrigger.InSceneID);
					playerRoomMessage.IsOutsideRoom = base.CurrentRoomTrigger.DisablePlayerInsideOccluder;
				}
				else
				{
					playerRoomMessage.ID = null;
				}
				Client.Instance.NetworkController.SendToGameServer(playerRoomMessage);
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (Client.IsGameBuild)
			{
				Client.Instance.NetworkController.EventSystem.RemoveListener(typeof(PlayerShootingMessage), PlayerShootingMessageListener);
				Client.Instance.NetworkController.EventSystem.RemoveListener(typeof(PlayerStatsMessage), PlayerStatMessageListener);
				Client.Instance.NetworkController.EventSystem.RemoveListener(typeof(TextChatMessage), TextChatMessageListener);
				Client.Instance.NetworkController.EventSystem.RemoveListener(EventSystem.InternalEventType.EquipAnimationEnd, EquipAnimationEndListener);
				Client.Instance.NetworkController.EventSystem.RemoveListener(typeof(LockToTriggerMessage), LockToTriggerMessageListener);
				Client.Instance.NetworkController.EventSystem.RemoveListener(typeof(QuestStatsMessage), QuestStatsMessageListener);
				Client.Instance.NetworkController.EventSystem.RemoveListener(typeof(UpdateBlueprintsMessage), UpdateBlueprintsMessageListener);
			}
		}

		private void OnGUI()
		{
			string text = "";
			if (Parent == null)
			{
				text = "null";
			}
			else if (Parent is SpaceObjectVessel)
			{
				text = (Parent as SpaceObjectVessel).CustomName;
			}
			else
			{
				text = Parent.gameObject.name;
			}
			if (!ShowGUIElements)
			{
				return;
			}
			if (FpsController.IsJetpackOn)
			{
				GUI.Label(new Rect(15f, 75f, 120f, 30f), "Fuel: ");
				GUI.HorizontalSlider(new Rect(45f, 81f, 350f, 30f), FpsController.JetpackFuel, 0f, 10f);
			}
			GUI.Label(new Rect(15f, 225f, 320f, 30f), "Occlusion status: " + ((!ZeroOcclusion.UseOcclusion) ? "Inactive" : "Active"));
			GUI.Label(new Rect(Screen.width - 250, 250f, 320f, 30f), "Latency: " + Client.Instance.LatencyMs + " ms");
			GUI.Box(new Rect(Screen.width - 460, 280f, 420f, 340f), Client.Instance.GetNetworkDataLogs());
			if (ShotDebugList.Count > 0)
			{
				GUI.Label(new Rect(Screen.width / 2, Screen.height / 2, 120f, 30f), "Shotz fired: ");
				int num = 1;
				foreach (string shotDebug in ShotDebugList)
				{
					GUI.Label(new Rect(Screen.width / 2, Screen.height / 2 + (120 - 30 * num), 350f, 30f), shotDebug);
					num++;
				}
			}
			Item itemInHands = Inventory.ItemInHands;
			if (itemInHands is HandDrill)
			{
				HandDrill handDrill = itemInHands as HandDrill;
				if (handDrill.Canister != null)
				{
					GUI.Label(new Rect(250f, 220f, 250f, 30f), "Canister resources:");
					foreach (CargoResourceData resource in handDrill.Canister.GetCompartment().Resources)
					{
						GUI.Label(new Rect(250f, 250 + 30 * (int)resource.ResourceType, 250f, 30f), string.Concat("Resource", resource.ResourceType, " :", resource.Quantity));
					}
					GUI.Label(new Rect(250f, 190f, 250f, 30f), "ResDbg: ");
					foreach (KeyValuePair<ResourceType, float> item in ResDbg)
					{
						GUI.Label(new Rect(250 + 150 * (int)item.Key, 250f, 250f, 30f), string.Concat("Resource", item.Key, " :", item.Value));
					}
				}
				if (handDrill.Battery != null)
				{
					GUI.Label(new Rect(250f, 175f, 250f, 30f), "Battery:" + handDrill.Battery.CurrentPower);
				}
			}
			GUI.color = Color.red;
			GUI.Label(new Rect(15f, 15f, 250f, 30f), "Health: " + Health);
			if (Parent != null && Parent is SpaceObjectVessel)
			{
				GUI.Label(new Rect(250f, 15f, 200f, 40f), "Parent GUID: " + Parent.GUID + ", Health: " + (Parent as SpaceObjectVessel).Health + " (" + FormatHelper.Percentage((Parent as SpaceObjectVessel).Health / (Parent as SpaceObjectVessel).MaxHealth) + ")");
			}
			else
			{
				GUI.Label(new Rect(250f, 15f, 200f, 30f), "NO PARENT");
			}
			if (Parent is SpaceObjectVessel)
			{
				GUI.Label(new Rect(750f, 15f, 1000f, 30f), "Rad.sig.: " + (Parent as SpaceObjectVessel).MainVessel.RadarSignature);
			}
			GUI.color = Color.green;
			if (LockedToTrigger != null)
			{
				if (LockedToTrigger.TriggerType == SceneTriggerType.ShipControl)
				{
					GUI.Label(new Rect(15f, 55f, 250f, 30f), "Press '" + ZeroGravity.UI.InputManager.GetAxisKeyName(ZeroGravity.UI.InputManager.AxisNames.Tab) + "' to stop driving ship");
				}
				else if (LockedToTrigger.TriggerType == SceneTriggerType.NavigationPanel)
				{
					GUI.Label(new Rect(15f, 55f, 250f, 30f), "Press '" + ZeroGravity.UI.InputManager.GetAxisKeyName(ZeroGravity.UI.InputManager.AxisNames.Tab) + "' to stop interactin with panel");
				}
			}
			else if (LookingAtTrigger != null)
			{
				if (LookingAtTrigger.TriggerType == SceneTriggerType.ShipControl)
				{
					GUI.Label(new Rect(15f, 55f, 250f, 30f), "Press '" + ZeroGravity.UI.InputManager.GetAxisKeyName(ZeroGravity.UI.InputManager.AxisNames.F) + "' to drive ship");
				}
				else if (LookingAtTrigger.TriggerType == SceneTriggerType.LightSwitch)
				{
					GUI.Label(new Rect(15f, 55f, 250f, 30f), "Press '" + ZeroGravity.UI.InputManager.GetAxisKeyName(ZeroGravity.UI.InputManager.AxisNames.F) + "' to switch light");
				}
				else if (LookingAtTrigger.TriggerType == SceneTriggerType.NavigationPanel)
				{
					GUI.Label(new Rect(15f, 55f, 250f, 30f), "Press '" + ZeroGravity.UI.InputManager.GetAxisKeyName(ZeroGravity.UI.InputManager.AxisNames.F) + "' interact with panel");
				}
				else if (LookingAtTrigger.TriggerType == SceneTriggerType.Door)
				{
					GUI.Label(new Rect(15f, 55f, 250f, 30f), "Press '" + ZeroGravity.UI.InputManager.GetAxisKeyName(ZeroGravity.UI.InputManager.AxisNames.F) + "' interact");
				}
				else if (LookingAtTrigger.TriggerType == SceneTriggerType.Turret)
				{
					GUI.Label(new Rect(15f, 55f, 250f, 30f), "Press '" + ZeroGravity.UI.InputManager.GetAxisKeyName(ZeroGravity.UI.InputManager.AxisNames.F) + "' to use turret");
				}
			}
			else if (LookingAtItem != null)
			{
				GUI.Label(new Rect(15f, 55f, 250f, 30f), "Press '" + ZeroGravity.UI.InputManager.GetAxisKeyName(ZeroGravity.UI.InputManager.AxisNames.F) + "' to pick up item");
			}
			if (base.CurrentRoomTrigger != null)
			{
				GUI.color = Color.magenta;
				GUI.Label(new Rect(15f, 95f, 250f, 30f), "Press J to toggle gravity");
			}
			GUI.color = Color.green;
			if (base.CurrentRoomTrigger != null)
			{
				GUI.Label(new Rect(Screen.width - 200, 15f, 200f, 30f), $"Air quality: {base.CurrentRoomTrigger.AirQuality * 100f:0.##}%");
				GUI.Label(new Rect(Screen.width - 200, 30f, 200f, 30f), $"Air pressure: {base.CurrentRoomTrigger.AirPressure:0.##} bar");
			}
			if (Parent != null && Client.IsGameBuild && Parent is Ship)
			{
				Ship ship = Parent as Ship;
				GUILayout.BeginArea(new Rect(Screen.width - 200, 55f, 450f, 300f));
				GUILayout.Label($"Velocity: {ship.Velocity.Magnitude:0.00}");
				GUILayout.Label($"Rotation: {ship.AngularVelocity.x:0.00}, {ship.AngularVelocity.y:0.00}, {ship.AngularVelocity.z:0.00}");
				GUILayout.Label(string.Format("Engine: {0}, Perc: {1}", (!ship.EngineOnLine) ? "Off" : "On", (int)(ship.EngineThrustPercentage * 100f) + "%"));
				GUILayout.EndArea();
				if (ship.resDebug != null)
				{
					GUI.Label(default(Rect), "Debug resources: ");
					for (int i = 0; i < ship.resDebug.Length; i++)
					{
						GUI.Label(new Rect(650 + 120 * i, 80f, 120f, 30f), i + ": " + ship.resDebug[i]);
					}
				}
				if (showSystemsDetails == 1)
				{
					GUI.color = Color.cyan;
					GUIStyle gUIStyle = new GUIStyle();
					gUIStyle.alignment = TextAnchor.MiddleRight;
					gUIStyle.normal.textColor = Color.cyan;
					GUI.Label(new Rect(Screen.width - 450, 70f, 385f, 30f), "SUBSYSTEMS", gUIStyle);
					int num2 = 0;
					SubSystem[] componentsInChildren = ship.GeometryRoot.GetComponentsInChildren<SubSystem>();
					foreach (SubSystem subSystem in componentsInChildren)
					{
						GUI.Label(new Rect(Screen.width - 450, 95 + 20 * num2++, 385f, 30f), subSystem.name + " (" + subSystem.Status.ToString() + ")", gUIStyle);
					}
				}
				else if (showSystemsDetails == 2)
				{
					GUI.color = Color.cyan;
					GUIStyle gUIStyle2 = new GUIStyle();
					gUIStyle2.alignment = TextAnchor.MiddleRight;
					gUIStyle2.normal.textColor = Color.cyan;
					GUI.Label(new Rect(Screen.width - 350, 70f, 385f, 30f), "GENERATORS");
					int num3 = 0;
					Generator[] componentsInChildren2 = ship.GeometryRoot.GetComponentsInChildren<Generator>();
					foreach (Generator generator in componentsInChildren2)
					{
						GUI.Label(new Rect(Screen.width - 450, 95 + 20 * num3++, 385f, 30f), $"{generator.name}: {generator.Status}, {generator.SecondaryStatus}, {generator.Output}/{generator.NominalOutput}, {((!(generator.NominalOutput > 0f)) ? 0f : (generator.Output / generator.NominalOutput * 100f)):0.#}%", gUIStyle2);
					}
				}
				else if (showSystemsDetails == 3)
				{
					GUI.color = Color.cyan;
					GUI.Label(new Rect(Screen.width - 350, 70f, 385f, 30f), "RESOURCE CONTAINERS");
					int num4 = 0;
					ResourceContainer[] componentsInChildren3 = ship.GeometryRoot.GetComponentsInChildren<ResourceContainer>();
					foreach (ResourceContainer resourceContainer in componentsInChildren3)
					{
						GUI.Label(new Rect(Screen.width - 350, 95 + 20 * num4++, 385f, 30f), resourceContainer.name + $" ({resourceContainer.DistributionSystemType.ToString()}): {resourceContainer.Quantity:0.####} / {resourceContainer.Capacity:0.####}" + ", " + ((!resourceContainer.IsInUse) ? "NOT IN USE" : "IN USE"));
					}
				}
				else if (showSystemsDetails == 4)
				{
					GUI.color = Color.cyan;
					GUI.Label(new Rect(Screen.width - 350, 70f, 385f, 30f), "AIR VOLUMES");
					int num5 = 0;
					SceneTriggerRoom[] componentsInChildren4 = ship.GeometryRoot.GetComponentsInChildren<SceneTriggerRoom>();
					foreach (SceneTriggerRoom sceneTriggerRoom in componentsInChildren4)
					{
						GUI.Label(new Rect(Screen.width - 350, 95 + 20 * num5++, 385f, 30f), sceneTriggerRoom.name + $" - quality: {sceneTriggerRoom.InterpolatedAirQuality * 100f:0.##}%, pressure: {sceneTriggerRoom.InterpolatedAirPressure:0.##} bar");
					}
				}
			}
			if (InLadderTrigger && !FpsController.IsOnLadder && !FpsController.IsZeroG)
			{
				GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 15, 300f, 30f), "Press '" + ZeroGravity.UI.InputManager.GetAxisKeyName(ZeroGravity.UI.InputManager.AxisNames.F) + "' to use");
			}
		}

		private IEnumerator VesselChangeCoutdown()
		{
			yield return new WaitForSeconds(0.5f);
			allowVesselChange = true;
			if (vesselChangeQueue != null)
			{
				if (vesselChangeIsEnter && Parent != vesselChangeQueue && base.CurrentRoomTrigger != null)
				{
					EnterVessel(vesselChangeQueue);
				}
				else if (!vesselChangeIsEnter && Parent is SpaceObjectVessel && Parent == vesselChangeQueue && base.CurrentRoomTrigger == null)
				{
					ExitVessel(forceExit: false);
				}
				vesselChangeQueue = null;
			}
		}

		public override void EnterVessel(SpaceObjectVessel vessel)
		{
			if (vessel == null || !Client.IsGameBuild)
			{
				return;
			}
			if (!allowVesselChange)
			{
				vesselChangeQueue = vessel;
				vesselChangeIsEnter = true;
				return;
			}
			SpaceObjectVessel spaceObjectVessel = ((!(Parent is SpaceObjectVessel) || !(Parent as SpaceObjectVessel).IsDocked) ? (Parent as SpaceObjectVessel) : (Parent as SpaceObjectVessel).DockedToMainVessel);
			SpaceObjectVessel spaceObjectVessel2 = ((!vessel.IsDocked) ? vessel : vessel.DockedToMainVessel);
			Vector3D vector3D = ((!(spaceObjectVessel != null)) ? Parent.Position : spaceObjectVessel.Position);
			Vector3D position = spaceObjectVessel2.Position;
			if (Parent is Pivot)
			{
				Client.Instance.SolarSystem.RemoveArtificialBody(Parent as Pivot);
				UnityEngine.Object.Destroy(Parent.gameObject);
				if (vessel.IsMainVessel)
				{
					Client.LogCustomEvent("enter_vessel", new Dictionary<string, object> { { "name", vessel.Name } });
				}
				else
				{
					Client.LogCustomEvent("enter_vessel", new Dictionary<string, object>
					{
						{ "name", vessel.Name },
						{
							"main_vessel_name",
							vessel.MainVessel.Name
						}
					});
				}
				SceneQuestTrigger.CheckInChildren(vessel.MainVessel.gameObject, SceneQuestTriggerEvent.EnterStation);
			}
			else if (spaceObjectVessel != null && spaceObjectVessel != spaceObjectVessel2 && spaceObjectVessel.transform.parent != Client.Instance.ShipExteriorRoot.transform)
			{
				spaceObjectVessel.transform.parent = Client.Instance.ShipExteriorRoot.transform;
				spaceObjectVessel.SetTargetPositionAndRotation(null, spaceObjectVessel.Forward, spaceObjectVessel.Up, instant: true);
				rigidBody.velocity = Vector3.zero;
			}
			SceneQuestTrigger.CheckInChildren(vessel.GeometryRoot, SceneQuestTriggerEvent.EnterVessel);
			Parent = vessel;
			if (CurrentHelmet != null && Parent is Ship)
			{
				CurrentHelmet.HudUI.Radar.CanRadarWork = false;
				CurrentHelmet.HudUI.Radar.ToggleTargeting(val: false);
			}
			if (spaceObjectVessel != null && spaceObjectVessel != spaceObjectVessel2)
			{
				spaceObjectVessel.ToggleOptimization(optimizationEnabled: true);
				foreach (SpaceObjectVessel allDockedVessel in spaceObjectVessel.AllDockedVessels)
				{
					allDockedVessel.ToggleOptimization(optimizationEnabled: true);
				}
			}
			spaceObjectVessel2.ToggleOptimization(optimizationEnabled: false);
			foreach (SpaceObjectVessel allDockedVessel2 in spaceObjectVessel2.AllDockedVessels)
			{
				allDockedVessel2.ToggleOptimization(optimizationEnabled: false);
			}
			GameObject gameObject = null;
			GameObject gameObject2 = null;
			if (cameraLerpHelper < 1f && !cameraLerpLocal && cameraLerpPosFrom.HasValue && cameraLerpRotFrom.HasValue)
			{
				gameObject = new GameObject();
				gameObject.transform.SetParent(vessel.transform);
				gameObject.transform.position = cameraLerpPosFrom.Value;
				gameObject.transform.rotation = cameraLerpRotFrom.Value;
				gameObject2 = new GameObject();
				gameObject2.transform.SetParent(vessel.transform);
				gameObject2.transform.position = cameraLerpPosTo.Value;
				gameObject2.transform.rotation = cameraLerpRotTo.Value;
			}
			spaceObjectVessel2.transform.SetParent(null);
			spaceObjectVessel2.SetTargetPositionAndRotation(Vector3.zero, spaceObjectVessel2.Forward, spaceObjectVessel2.Up, instant: true);
			spaceObjectVessel2.transform.Reset();
			if (gameObject != null && gameObject2 != null)
			{
				gameObject.transform.SetParent(null);
				gameObject2.transform.SetParent(null);
				cameraLerpPosFrom = gameObject.transform.position;
				cameraLerpRotFrom = gameObject.transform.rotation;
				cameraLerpPosTo = gameObject2.transform.position;
				cameraLerpRotTo = gameObject2.transform.rotation;
			}
			if (VesselChangeHelperRb == null)
			{
				GameObject gameObject3 = new GameObject("VesselChangeHelper");
				gameObject3.transform.parent = null;
				gameObject3.transform.Reset();
				VesselChangeHelperRb = gameObject3.AddComponent<Rigidbody>();
				VesselChangeHelperRb.mass = 1f;
				VesselChangeHelperRb.drag = 0f;
				VesselChangeHelperRb.angularDrag = 0f;
				VesselChangeHelperRb.useGravity = false;
				VesselChangeHelperRb.isKinematic = true;
			}
			VesselChangeHelperRb.gameObject.SetActive(value: true);
			VesselChangeHelperRb.transform.position = Vector3.zero;
			VesselChangeHelperRb.transform.rotation = Quaternion.identity;
			VesselChangeHelperRb.isKinematic = false;
			VesselChangeHelperRb.angularVelocity = vessel.MainVessel.AngularVelocity * ((float) Mathf.PI / 180f);
			Vector3 relativePointVelocity = VesselChangeHelperRb.GetRelativePointVelocity(base.transform.position - vessel.MainVessel.transform.position);
			rigidBody.velocity = Quaternion.LookRotation(vessel.MainVessel.Forward, vessel.MainVessel.Up).Inverse() * rigidBody.velocity - relativePointVelocity;
			VesselChangeHelperRb.gameObject.SetActive(value: false);
			UnityEngine.Object.Destroy(gameObject);
			UnityEngine.Object.Destroy(gameObject2);
			foreach (ArtificialBody artificialBody in Client.Instance.SolarSystem.ArtificialBodies)
			{
				if (!artificialBody.IsMainObject && (!(artificialBody is SpaceObjectVessel) || (artificialBody as SpaceObjectVessel).DockedToMainVessel == null))
				{
					artificialBody.ModifyPositionAndRotation((vector3D - position).ToVector3());
				}
				artificialBody.UpdateArtificialBodyPosition(updateChildren: false);
			}
			Client.Instance.SolarSystem.CenterPlanets();
			UpdateCameraPositions();
			Client.Instance.CubemapRenderer.RenderCubemapForReflectionProbe();
			Client.Instance.CubemapRenderer.RenderCubemapForDockingPort();
			if (spaceObjectVessel != null)
			{
				ZeroOcclusion.CheckOcclusionFor(spaceObjectVessel, onlyCheckDistance: false);
				foreach (SpaceObjectVessel allDockedVessel3 in spaceObjectVessel.AllDockedVessels)
				{
					ZeroOcclusion.CheckOcclusionFor(allDockedVessel3, onlyCheckDistance: false);
				}
			}
			if (spaceObjectVessel2 != null)
			{
				ZeroOcclusion.CheckOcclusionFor(spaceObjectVessel2, onlyCheckDistance: false);
				foreach (SpaceObjectVessel allDockedVessel4 in spaceObjectVessel2.AllDockedVessels)
				{
					ZeroOcclusion.CheckOcclusionFor(allDockedVessel4, onlyCheckDistance: false);
				}
			}
			allowVesselChange = false;
			StartCoroutine(VesselChangeCoutdown());
		}

		public override void ExitVessel(bool forceExit)
		{
			if (!Client.IsGameBuild)
			{
				return;
			}
			if (!allowVesselChange)
			{
				if (Parent is SpaceObjectVessel)
				{
					vesselChangeQueue = Parent as SpaceObjectVessel;
					vesselChangeIsEnter = false;
				}
				return;
			}
			if (Parent is SpaceObjectVessel)
			{
				SpaceObjectVessel spaceObjectVessel = Parent as SpaceObjectVessel;
				if (spaceObjectVessel.IsMainVessel)
				{
					Client.LogCustomEvent("exit_vessel", new Dictionary<string, object> { { "name", spaceObjectVessel.Name } });
				}
				else
				{
					Client.LogCustomEvent("exit_vessel", new Dictionary<string, object>
					{
						{ "name", spaceObjectVessel.Name },
						{
							"main_vessel_name",
							spaceObjectVessel.MainVessel.Name
						}
					});
				}
				SceneQuestTrigger.CheckInChildren(spaceObjectVessel.GeometryRoot, SceneQuestTriggerEvent.ExitVessel);
				SceneQuestTrigger.CheckInChildren(spaceObjectVessel.MainVessel.gameObject, SceneQuestTriggerEvent.ExitStation);
			}
			SpaceObjectVessel spaceObjectVessel2 = Parent as SpaceObjectVessel;
			SpaceObjectVessel mainVessel = spaceObjectVessel2.MainVessel;
			if (spaceObjectVessel2 is Ship)
			{
				Ship ship = Parent as Ship;
				if (CurrentHelmet != null)
				{
					CurrentHelmet.HudUI.Radar.CanRadarWork = true;
					CurrentHelmet.HudUI.Radar.ToggleTargeting(val: true);
				}
				Instance.FpsController.CameraController.cameraShakeController.Stop();
			}
			mainVessel.transform.parent = Client.Instance.ShipExteriorRoot.transform;
			mainVessel.SetTargetPositionAndRotation(null, mainVessel.Forward, mainVessel.Up, instant: true);
			Client.Instance.ShipExteriorRoot.transform.rotation = Quaternion.identity;
			if (VesselChangeHelperRb == null)
			{
				GameObject gameObject = new GameObject("VesselChangeHelper");
				gameObject.transform.parent = null;
				gameObject.transform.Reset();
				VesselChangeHelperRb = gameObject.AddComponent<Rigidbody>();
				VesselChangeHelperRb.mass = 1f;
				VesselChangeHelperRb.drag = 0f;
				VesselChangeHelperRb.angularDrag = 0f;
				VesselChangeHelperRb.useGravity = false;
				VesselChangeHelperRb.isKinematic = true;
			}
			VesselChangeHelperRb.gameObject.SetActive(value: true);
			VesselChangeHelperRb.transform.position = Vector3.zero;
			VesselChangeHelperRb.transform.rotation = Quaternion.identity;
			VesselChangeHelperRb.isKinematic = false;
			VesselChangeHelperRb.angularVelocity = mainVessel.AngularVelocity * ((float) Mathf.PI / 180f);
			Vector3 relativePointVelocity = VesselChangeHelperRb.GetRelativePointVelocity(base.transform.position - mainVessel.transform.position);
			rigidBody.velocity = mainVessel.transform.rotation * rigidBody.velocity + relativePointVelocity;
			VesselChangeHelperRb.gameObject.SetActive(value: false);
			Parent = Pivot.Create(SpaceObjectType.PlayerPivot, base.GUID, mainVessel, isMainObject: true);
			(Parent as Pivot).Orbit.CopyDataFrom(mainVessel.Orbit, Client.Instance.SolarSystem.CurrentTime, exactCopy: true);
			Parent.SetTargetPositionAndRotation(Vector3.zero, Quaternion.identity, instant: true);
			Client.Instance.SolarSystem.CenterPlanets();
			UpdateCameraPositions();
			Client.Instance.CubemapRenderer.RenderCubemapForReflectionProbe();
			Client.Instance.CubemapRenderer.RenderCubemapForDockingPort();
			foreach (SpaceObjectVessel item in Client.Instance.SolarSystem.ArtificialBodies.Where((ArtificialBody m) => m is SpaceObjectVessel))
			{
				item.ToggleOptimization(optimizationEnabled: true);
				item.UpdateArtificialBodyPosition(updateChildren: false);
			}
			if (isRagdolled)
			{
				ToggleRagdoll(false);
			}
			allowVesselChange = false;
			StartCoroutine(VesselChangeCoutdown());
			if (!(mainVessel != null))
			{
				return;
			}
			ZeroOcclusion.CheckOcclusionFor(mainVessel, onlyCheckDistance: false);
			foreach (SpaceObjectVessel allDockedVessel in mainVessel.AllDockedVessels)
			{
				ZeroOcclusion.CheckOcclusionFor(allDockedVessel, onlyCheckDistance: false);
			}
		}

		public void ProcessMovementMessage(CharacterMovementMessage msg)
		{
			if (msg.GUID != base.GUID || msg == null || !msg.PivotReset || !(Parent is Pivot) || PivotReset)
			{
				return;
			}
			Pivot pivot = Parent as Pivot;
			Vector3 vector = msg.PivotPositionCorrection.ToVector3();
			base.transform.position -= vector;
			rigidBody.velocity -= msg.PivotVelocityCorrection.ToVector3();
			FpsController.CenterOfMassRigidbody.detectCollisions = false;
			rigidBody.detectCollisions = false;
			foreach (ArtificialBody artificialBody in Client.Instance.SolarSystem.ArtificialBodies)
			{
				if (artificialBody != Parent && (!(artificialBody is SpaceObjectVessel) || !(artificialBody as SpaceObjectVessel).IsDocked))
				{
					artificialBody.SetTargetPositionAndRotation(artificialBody.transform.localPosition - vector, null, instant: true);
				}
			}
			FpsController.CenterOfMassRigidbody.detectCollisions = true;
			rigidBody.detectCollisions = true;
			PivotReset = true;
		}

		public void OnApplicationFocus(bool focus)
		{
		}

		private void LockToTrigger(BaseSceneTrigger trigger)
		{
			LockedToTrigger = trigger;
			if (FpsController.CurrentJetpack != null)
			{
				wasJetpackOn = FpsController.CurrentJetpack.IsActive;
				FpsController.CurrentJetpack.IsActive = false;
			}
		}

		public void DetachFromPanel()
		{
			Client.Instance.InGamePanels.Detach();
			FpsController.ResetVelocity();
			FpsController.ToggleAttached(false);
			FpsController.ToggleCameraMovement(true);
			ChangeCamerasFov(Client.DefaultCameraFov);
			FpsController.ToggleMovement(!SittingOnPilotSeat);
			if (SittingOnPilotSeat)
			{
				FpsController.ToggleCameraAttachToHeadBone(true);
			}
			FpsController.ToggleAutoFreeLook(SittingOnPilotSeat);
			LockedToTrigger = null;
			Client.Instance.ToggleCursor(false);
			Client.Instance.InputModule.ToggleCustomCursorPosition(val: true);
			Client.Instance.CanvasManager.OverlayCanvasIsOn = false;
			ResetMyRoomTrigger();
			if (FpsController.CurrentJetpack != null)
			{
				FpsController.CurrentJetpack.IsActive = wasJetpackOn;
			}
		}

		public void ResetMyRoomTrigger()
		{
			if (MyRoomTrigger.gameObject.activeInHierarchy)
			{
				MyRoomTrigger.transform.localPosition = new Vector3(0f, -0.3f, 0f);
				EnableTransitionTrigger();
			}
		}

		public void AttachToPanel(BaseSceneTrigger trigger, bool useCursor = true)
		{
			Client.Instance.CanvasManager.HelmetOverlayModel.SetAxis(0f, 0f, 0f);
			Client.Instance.CanvasManager.HelmetOverlayModel.SetMovement(0f, 0f);
			Client.Instance.InGamePanels.Interact();
			FpsController.ResetVelocity();
			FpsController.ToggleAttached(true);
			FpsController.ToggleMovement(false);
			FpsController.ToggleCameraMovement(false);
			LockToTrigger(trigger);
			Client.Instance.ToggleCursor(useCursor);
			Client.Instance.InputModule.ToggleCustomCursorPosition(!useCursor);
			Client.Instance.CanvasManager.OverlayCanvasIsOn = true;
			if (!(Parent is SpaceObjectVessel) && Client.IsGameBuild)
			{
				SpaceObjectVessel spaceObjectVessel = trigger.GetComponentInParent<GeometryRoot>().MainObject as SpaceObjectVessel;
				if (spaceObjectVessel != null)
				{
					SetMyRoomTrigger(spaceObjectVessel);
				}
			}
		}

		public void SetMyRoomTrigger(SpaceObjectVessel vessel)
		{
			MyRoomTrigger.ParentVessel = vessel;
			MyRoomTrigger.transform.localPosition = Vector3.zero;
		}

		public void ToggleRagdoll(bool? enabled = null)
		{
			if (enabled.HasValue)
			{
				isRagdolled = enabled.Value;
			}
			else
			{
				isRagdolled = !isRagdolled;
			}
			if (isRagdolled)
			{
				wasInStance = animHelper.GetParameterBool(AnimatorHelper.Parameter.InStance);
				animHelper.SetParameter(null, null, null, null, null, null, null, null, false);
				isRagdollFinished = false;
				FpsController.ToggleMovement(false);
				Vector3 velocity = rigidBody.velocity;
				FpsController.ToggleColliders(isEnabled: false);
				rigidBody.isKinematic = true;
				ragdollComponent.ToggleRagdoll(enabled: true, this, velocity);
				FpsController.ResetVelocity();
				FpsController.ToggleCameraAttachToHeadBone(true);
				return;
			}
			FpsController.ReparentCenterOfMass(isInChest: false);
			ragdollComponent.ToggleRagdoll(enabled: false, null);
			Transform bone = animHelper.GetBone(AnimatorHelper.HumanBones.Hips);
			RaycastHit hitInfo;
			if (FpsController.IsZeroG)
			{
				FpsController.transform.position = bone.position;
			}
			else if (Physics.Raycast(bone.position, base.GravityDirection, out hitInfo, 2f, Client.DefaultLayerMask))
			{
				FpsController.transform.position = hitInfo.point - base.GravityDirection * 1.34f;
			}
			Quaternion rotation = FpsController.transform.rotation;
			if (Vector3.Dot(bone.up, base.GravityDirection) >= 0f)
			{
				FpsController.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(bone.right, -base.GravityDirection), -base.GravityDirection);
			}
			else
			{
				FpsController.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(-bone.right, -base.GravityDirection), -base.GravityDirection);
			}
			ragdollComponent.AddRootRotation(rotation * FpsController.transform.rotation.Inverse());
			ragdollComponent.RestorePositions();
			AnimatorHelper animatorHelper = animHelper;
			float? getUpType = FpsController.CheckGetUpRoom();
			animatorHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, getUpType);
			FpsController.ToggleColliders(isEnabled: true);
			rigidBody.isKinematic = false;
			FpsController.ReparentCenterOfMass(isInChest: true);
		}

		public void RagdollFinished()
		{
			isRagdollFinished = true;
			FpsController.ToggleCameraAttachToHeadBone(false);
			FpsController.LerpCameraBack(3f);
			FpsController.ToggleMovement(true);
			FpsController.ToggleCameraMovement(true);
			AnimatorHelper animatorHelper = animHelper;
			bool? inStance = wasInStance;
			animatorHelper.SetParameter(null, null, null, null, null, null, null, null, inStance);
			wasInStance = false;
		}

		public void ChangeCamerasFov(float fovVal)
		{
			if (!(fovVal <= 0f))
			{
				cameraFovLerpFrom = fovVal;
				cameraFovLerpTo = FpsController.MainCamera.fieldOfView;
				cameraFovLerpValue = 1f;
				Client.Instance.CanvasManager.CanvasUI.HelmetHud.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
				if (fovVal == Client.DefaultCameraFov)
				{
					Client.Instance.CanvasManager.CanvasUI.HelmetHud.transform.localScale = new Vector3(1f, 1f, 1f);
				}
			}
		}

		public void ResetCameraFov()
		{
			if (!FpsController.MainCamera.fieldOfView.IsEpsilonEqual(Client.DefaultCameraFov))
			{
				ChangeCamerasFov(Client.DefaultCameraFov);
			}
		}

		private void LerpCameraFov()
		{
			cameraFovLerpValue -= Time.deltaTime * cameraFovLerpStrength;
			if (cameraFovLerpValue <= 0.001f)
			{
				cameraFovLerpValue = 0f;
			}
			float fieldOfView = Mathf.Lerp(cameraFovLerpFrom, cameraFovLerpTo, cameraFovLerpValue);
			if (Client.IsGameBuild)
			{
				Client.Instance.SunCameraTransform.GetComponent<Camera>().fieldOfView = fieldOfView;
				Client.Instance.PlanetsCameraTransform.GetComponent<Camera>().fieldOfView = fieldOfView;
			}
			FpsController.MainCamera.fieldOfView = fieldOfView;
			FpsController.FarCamera.fieldOfView = fieldOfView;
			FpsController.NearCamera.fieldOfView = fieldOfView;
		}

		public void CheckDistanceFromVessel()
		{
		}

		public void ChangeLockedToTrigger(BaseSceneTrigger trigger)
		{
			LockedToTrigger = trigger;
		}

		private void UpdateReferenceHead()
		{
			Transform[] array = new Transform[ReferenceHead.bones.Length];
			Transform bone = animHelper.GetBone(AnimatorHelper.HumanBones.Spine2);
			for (int i = 0; i < 6; i++)
			{
				array[i] = bone.FindChildByName(ReferenceHead.bones[i].name);
			}
			ReferenceHead.bones = array;
		}

		private void RefreshHeadBones()
		{
			Transform[] array = new Transform[ReferenceHead.bones.Length];
			Transform bone = animHelper.GetBone(AnimatorHelper.HumanBones.Spine2);
			for (int i = 0; i < HeadSkin.bones.Length; i++)
			{
				array[i] = bone.FindChildByName(ReferenceHead.bones[i].name);
			}
			HeadSkin.bones = array;
		}

		public void RefreshOutfitData()
		{
			animHelper.RebindAndReload();
			ragdollComponent.RefreshRagdollVariables();
			RefreshHeadBones();
			FpsController.RefreshOutfitData(Outfit);
		}

		private Dictionary<byte, RagdollItemData> GetRagdollData()
		{
			Dictionary<byte, RagdollItemData> dictionary = new Dictionary<byte, RagdollItemData>();
			foreach (KeyValuePair<AnimatorHelper.HumanBones, Transform> bone in animHelper.GetBones())
			{
				dictionary.Add((byte)bone.Key, new RagdollItemData
				{
					Position = bone.Value.localPosition.ToArray(),
					LocalRotation = bone.Value.localRotation.ToArray()
				});
			}
			return dictionary;
		}

		public void OnTriggerEnter(Collider coli)
		{
			OcSector component = coli.GetComponent<OcSector>();
			if (component != null)
			{
				if (currOcSector == null)
				{
					EventSystem.Invoke(new EventSystem.InternalEventData(EventSystem.InternalEventType.OcExteriorStatus, false));
					component.ToggleVisibility(true);
				}
				component.ToggleNeighbours(isVisible: true);
				currOcSector = component;
			}
		}

		public void OnTriggerExit(Collider coli)
		{
			OcSector component = coli.GetComponent<OcSector>();
			if (component != null)
			{
				if (component == currOcSector)
				{
					currOcSector = null;
					EventSystem.Invoke(new EventSystem.InternalEventData(EventSystem.InternalEventType.OcExteriorStatus, true));
				}
				else
				{
					component.ToggleNeighbours(isVisible: false, currOcSector);
				}
			}
		}

		public void AnimInteraction_LockEnter()
		{
			InIteractLayer = true;
			InLockState = true;
			if (Client.Instance.CanvasManager.IsPlayerOverviewOpen)
			{
				Client.Instance.CanvasManager.PlayerOverview.Toggle(val: false);
			}
			if (OnLockStart != null)
			{
				OnLockStart();
			}
		}

		public void AnimInteraction_LockExit()
		{
			InLockState = false;
			if (OnLockComplete != null)
			{
				OnLockComplete();
			}
		}

		public void AnimInteraction_InteractEnter()
		{
			InIteractLayer = true;
			InInteractState = true;
			if (Client.Instance.CanvasManager.IsPlayerOverviewOpen)
			{
				Client.Instance.CanvasManager.PlayerOverview.Toggle(val: false);
			}
			if (OnIteractStart != null)
			{
				OnIteractStart();
			}
		}

		public void AnimInteraction_InteractExit()
		{
			InInteractState = false;
			if (OnIteractComplete != null)
			{
				OnIteractComplete();
			}
		}

		public void AnimInteraction_NoneEnter()
		{
			InIteractLayer = false;
		}

		public void AnimInteraction_NoneExit()
		{
		}

		public void ResetLookingAtItem()
		{
			LookingAtItem = null;
		}

		public void ToggleMeshRendereres(bool enableMesh)
		{
			if (!Client.IsGameBuild)
			{
				return;
			}
			MeshRenderersEnabled = enableMesh;
			SkinnedMeshRenderer[] componentsInChildren = Outfit.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
			{
				if (!ArmSkins.Contains(skinnedMeshRenderer) && (CurrentOutfit == null || !CurrentOutfit.ArmSkins.Contains(skinnedMeshRenderer)))
				{
					skinnedMeshRenderer.shadowCastingMode = (enableMesh ? ShadowCastingMode.On : ShadowCastingMode.ShadowsOnly);
				}
			}
		}

		public override void DockedVesselParentChanged(SpaceObjectVessel vessel)
		{
			SpaceObjectVessel spaceObjectVessel = Parent as SpaceObjectVessel;
			Parent = vessel;
			SceneQuestTrigger.CheckInChildren(vessel.GeometryRoot, SceneQuestTriggerEvent.EnterVessel);
			if (spaceObjectVessel != null)
			{
				ZeroOcclusion.CheckOcclusionFor(spaceObjectVessel, onlyCheckDistance: false);
				spaceObjectVessel.ToggleOptimization(optimizationEnabled: true);
			}
			if (vessel != null)
			{
				ZeroOcclusion.CheckOcclusionFor(vessel, onlyCheckDistance: false);
				vessel.ToggleOptimization(optimizationEnabled: false);
			}
		}

		public override void OnGravityChanged(Vector3 oldGravity)
		{
			FpsController.SetGravity(base.Gravity);
			InventoryCharacterPreview.instance.ChangeGravity(FpsController.IsZeroG);
		}

		public override void RoomChanged(SceneTriggerRoom prevRoomTrigger)
		{
			base.RoomChanged(prevRoomTrigger);
			SendPlayerRoomMessage();
		}

		public void Suicide()
		{
			Client.Instance.NetworkController.SendToGameServer(new SuicideRequest());
		}

		public void CheckCameraShake()
		{
			if (!(Parent is Ship))
			{
				FpsController.CameraController.cameraShakeController.Stop();
				return;
			}
			Ship ship = Parent as Ship;
			if (ship.IsWarpOnline || ship.AllDockedVessels.Find((SpaceObjectVessel m) => m as Ship != null && (m as Ship).IsWarpOnline) != null)
			{
				FpsController.CameraController.cameraShakeController.ShakeCamera(CameraShake.ShakeType.Warp, infiniteShake: true);
			}
			else if ((ship.Engine != null && ship.Engine.Status == SystemStatus.OnLine) || ship.AllDockedVessels.Find((SpaceObjectVessel m) => m as Ship != null && (m as Ship).Engine != null && (m as Ship).Engine.Status == SystemStatus.OnLine) != null)
			{
				FpsController.CameraController.cameraShakeController.TargetMainMultiplier = ship.Engine.OperationRate;
				FpsController.CameraController.cameraShakeController.ShakeCamera(CameraShake.ShakeType.Engine, infiniteShake: true);
			}
			else
			{
				FpsController.CameraController.cameraShakeController.Stop();
			}
		}

		public void StartExitCryoChamberCountdown(SceneTriggerExecuter cryoExecuter, string stateName)
		{
			StartCoroutine(WaitFromCryoChamberExit(cryoExecuter, stateName));
		}

		private IEnumerator WaitFromCryoChamberExit(SceneTriggerExecuter cryoExecuter, string stateName)
		{
			yield return new WaitForSeconds(12.12f);
			if (InIteractLayer)
			{
				cryoExecuter.ChangeStateImmediate(stateName);
				cryoExecuter.SetExecuterDetails(new SceneTriggerExecuterDetails
				{
					PlayerThatActivated = Instance.GUID,
					InSceneID = cryoExecuter.InSceneID,
					IsImmediate = true,
					IsFail = false,
					CurrentStateID = cryoExecuter.CurrentStateID,
					NewStateID = cryoExecuter.GetStateID(stateName)
				}, isInstant: false, null, checkCurrentState: false);
				cryoExecuter.CharacterUnlock();
			}
		}

		public float? GetDistance(ISoundOccludable otherObject, out bool throughBulkhead)
		{
			throughBulkhead = false;
			if (otherObject is SpaceObjectTransferable)
			{
				return GetDistance(otherObject as SpaceObjectTransferable, out throughBulkhead);
			}
			if (otherObject is VesselSystem)
			{
				return GetDistance(otherObject as VesselSystem, out throughBulkhead);
			}
			if (otherObject is SceneDoor)
			{
				return GetDistance(otherObject as SceneDoor, out throughBulkhead);
			}
			if (otherObject == null)
			{
				return GetDistance(Vector3.zero, out throughBulkhead);
			}
			return null;
		}

		public float? GetDistance(SpaceObjectTransferable otherObject, out bool throughBulkhead)
		{
			if (otherObject.Parent is Player)
			{
				return GetDistance(otherObject.Parent.transform.position, (otherObject.Parent as Player).CurrentRoomTrigger, out throughBulkhead);
			}
			return GetDistance(otherObject.transform.position, otherObject.CurrentRoomTrigger, out throughBulkhead);
		}

		public float? GetDistance(VesselSystem vesselComponent, out bool throughBulkhead)
		{
			return GetDistance(vesselComponent.transform.position, vesselComponent.Room, out throughBulkhead);
		}

		public float? GetDistance(SceneDoor door, out bool throughBulkhead)
		{
			return GetDistance(door.DoorPassageTrigger.bounds.center, (!(door.Room1 != null)) ? door.Room2 : door.Room1, out throughBulkhead, door);
		}

		public float? GetDistance(Vector3 position, out bool throughBulkhead)
		{
			throughBulkhead = false;
			Dbg.Error("*******************NE BI TREBALO OVO DA SE ZOVE NIKAADDAAAAA");
			return null;
		}

		public float? GetDistance(Vector3 position, SceneTriggerRoom room, out bool throughBulkhead, SceneDoor ignoreDoor = null)
		{
			throughBulkhead = false;
			if (!IsInsideSpaceObject || room == null || base.CurrentRoomTrigger == null)
			{
				return null;
			}
			if (base.CurrentRoomTrigger == room || (ignoreDoor != null && ignoreDoor.Room1 == room) || (ignoreDoor != null && ignoreDoor.Room2 == room))
			{
				return (base.transform.position - position).magnitude;
			}
			if (base.CurrentRoomTrigger.CompoundRoomID == room.CompoundRoomID || (ignoreDoor != null && ignoreDoor.Room1 != null && ignoreDoor.Room1.CompoundRoomID == base.CurrentRoomTrigger.CompoundRoomID) || (ignoreDoor != null && ignoreDoor.Room2 != null && ignoreDoor.Room2.CompoundRoomID == base.CurrentRoomTrigger.CompoundRoomID))
			{
				SceneTriggerRoom currentRoomTrigger = base.CurrentRoomTrigger;
				List<SceneTriggerRoom> list = FindPath(currentRoomTrigger, room, null, ignoreDoor);
				if (list != null)
				{
					List<Vector3> list2 = new List<Vector3>();
					SceneTriggerRoom sceneTriggerRoom = list[0];
					list.RemoveAt(0);
					foreach (SceneTriggerRoom item in list)
					{
						foreach (SceneDoor item2 in sceneTriggerRoom.Doors.FindAll((SceneDoor m) => m.IsOpen || !m.IsSealable))
						{
							if ((item2.Room1 == sceneTriggerRoom && item2.Room2 == item) || (item2.Room2 == sceneTriggerRoom && item2.Room1 == item))
							{
								list2.Add(item2.DoorPassageTrigger.bounds.center);
								break;
							}
						}
						if (sceneTriggerRoom.ParentVessel != item.ParentVessel)
						{
							foreach (SceneDoor item3 in item.Doors.FindAll((SceneDoor m) => m.IsOpen || !m.IsSealable))
							{
								if ((item3.Room1 == sceneTriggerRoom && item3.Room2 == item) || (item3.Room2 == sceneTriggerRoom && item3.Room1 == item))
								{
									list2.Add(item3.DoorPassageTrigger.bounds.center);
									break;
								}
							}
						}
						sceneTriggerRoom = item;
					}
					list2.Add(position);
					float num = 0f;
					Vector3 vector = base.transform.position;
					foreach (Vector3 item4 in list2)
					{
						num += (vector - item4).magnitude;
						vector = item4;
					}
					return num;
				}
			}
			throughBulkhead = true;
			return (base.transform.position - position).magnitude;
		}

		private List<SceneTriggerRoom> FindPath(SceneTriggerRoom current, SceneTriggerRoom target, HashSet<SceneTriggerRoom> traversedRooms = null, SceneDoor ignoreDoor = null)
		{
			if (traversedRooms == null)
			{
				traversedRooms = new HashSet<SceneTriggerRoom>();
			}
			if (current == target)
			{
				List<SceneTriggerRoom> list = new List<SceneTriggerRoom>();
				list.Add(target);
				return list;
			}
			if (current == null || !traversedRooms.Add(current))
			{
				return null;
			}
			List<SceneDoor> list2 = current.Doors.FindAll((SceneDoor m) => m.IsOpen || !m.IsSealable || m == ignoreDoor);
			foreach (SceneDoor item in list2)
			{
				List<SceneTriggerRoom> list3 = FindPath((!(item.Room1 == current)) ? item.Room1 : item.Room2, target, traversedRooms);
				if (list3 != null)
				{
					list3.Insert(0, current);
					return list3;
				}
			}
			return null;
		}

		public void EnableTransitionTrigger()
		{
			TransitionTrigger.Enabled = true;
		}

		public void DisableTransitionTrigger()
		{
			TransitionTrigger.Enabled = false;
		}

		public bool IsInVesselHierarchy(SpaceObjectVessel vessel)
		{
			if (vessel == null || !(Parent is SpaceObjectVessel))
			{
				return false;
			}
			return Parent == vessel || vessel.AllVessels.Contains(Parent as SpaceObjectVessel);
		}

		public void HighlightAttachPoints(ItemType? item = null, GenericItemSubType? generic = null, MachineryPartType? part = null, int? partTier = null)
		{
			BaseSceneAttachPoint baseSceneAttachPoint = null;
			Collider[] array = Physics.OverlapSphere(base.transform.position, highlightAttachPointsRange, highlightAttachPointMask, QueryTriggerInteraction.Collide);
			foreach (Collider collider in array)
			{
				baseSceneAttachPoint = collider.GetComponentInParent<BaseSceneAttachPoint>();
				if (baseSceneAttachPoint != null && baseSceneAttachPoint.Collider == collider && (!item.HasValue || baseSceneAttachPoint.CanAttachItemType(item.Value, generic, part, partTier)))
				{
					if (!item.HasValue)
					{
						Client.Instance.CanvasManager.HighlightSlotMaterial.mainTexture = Client.Instance.CanvasManager.HighlightSlotNormal;
					}
					else
					{
						Client.Instance.CanvasManager.HighlightSlotMaterial.mainTexture = Client.Instance.CanvasManager.HighlightSlotItemHere;
					}
				}
			}
		}

		public void HideHiglightedAttachPoints()
		{
			foreach (GameObject higlightedAttachPoint in higlightedAttachPoints)
			{
				higlightedAttachPoint.SetActive(value: false);
			}
			higlightedAttachPoints.Clear();
		}

		public void MeleeAttack()
		{
			SpaceObject spaceObject = ((!(Instance.Parent is SpaceObjectVessel)) ? Instance.Parent : (Instance.Parent as SpaceObjectVessel).MainVessel);
			ShotData shotData = new ShotData();
			shotData.Position = (Quaternion.LookRotation(spaceObject.Forward, spaceObject.Up) * Instance.FpsController.MainCamera.transform.position).ToArray();
			shotData.Orientation = (Quaternion.LookRotation(spaceObject.Forward, spaceObject.Up) * Instance.FpsController.MainCamera.transform.forward.normalized).ToArray();
			shotData.parentGUID = spaceObject.GUID;
			shotData.parentType = spaceObject.Type;
			shotData.Range = MeleeRange;
			shotData.IsMeleeAttack = true;
			ShotData shotData2 = shotData;
			Attack(shotData2, base.CurrentActiveItem, 0f, 0f);
		}

		private IEnumerator DebrisFieldCheckCoroutine()
		{
			while (true)
			{
				if (PlayerReady && Parent != null && Parent is ArtificialBody)
				{
					DebrisField inDebrisField = null;
					Vector3D orbitVelocity = Vector3D.Zero;
					foreach (DebrisField debrisField in Client.Instance.DebrisFields)
					{
						if (debrisField.CheckObject(Parent as ArtificialBody, out orbitVelocity))
						{
							inDebrisField = debrisField;
							Client.Instance.DebrisEffect.UpdateEffect(playStop: true, Client.Instance.ShipExteriorRoot.transform.rotation * orbitVelocity.ToVector3(), debrisField.FragmentsDensity, debrisField.FragmentsVelocity);
							break;
						}
						Client.Instance.DebrisEffect.UpdateEffect(playStop: false, Vector3.zero, -1f, -1f);
					}
					DebrisField inDebrisField2 = InDebrisField;
					InDebrisField = inDebrisField;
					DebrisFieldVelocityDirection = orbitVelocity;
					if (inDebrisField2 != InDebrisField)
					{
						Client.Instance.CanvasManager.CanvasUI.HelmetHud.WarningsUpdate();
					}
				}
				yield return new WaitForSeconds(1f);
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			Corpse componentInParent = collision.collider.GetComponentInParent<Corpse>();
			if (componentInParent != null)
			{
				componentInParent.PushedByMyPlayer(collision.relativeVelocity);
			}
		}

		private void LockToTriggerMessageListener(NetworkData data)
		{
			LockToTriggerMessage ltm = data as LockToTriggerMessage;
			if (ltm.TriggerID == null || (!(LockedToTrigger == null) && LockedToTrigger.GetID().Equals(ltm.TriggerID)))
			{
				return;
			}
			if (LookingAtTrigger == null || !LookingAtTrigger.GetID().Equals(ltm.TriggerID))
			{
				SpaceObjectVessel vessel = Client.Instance.GetVessel(ltm.TriggerID.VesselGUID);
				if (vessel != null)
				{
					LookingAtTrigger = vessel.GeometryRoot.GetComponentsInChildren<BaseSceneTrigger>().FirstOrDefault((BaseSceneTrigger m) => m.GetID().InSceneID == ltm.TriggerID.InSceneID);
				}
			}
			if (LookingAtTrigger != null)
			{
				LookingAtTrigger.Interact(this);
			}
		}

		private void QuestStatsMessageListener(NetworkData data)
		{
			QuestStatsMessage questStatsMessage = data as QuestStatsMessage;
			SetQuestDetails(questStatsMessage.QuestDetails);
		}

		private void UpdateBlueprintsMessageListener(NetworkData data)
		{
			UpdateBlueprintsMessage updateBlueprintsMessage = data as UpdateBlueprintsMessage;
			if (updateBlueprintsMessage.Blueprints != null)
			{
				Blueprints = updateBlueprintsMessage.Blueprints;
			}
		}

		public void SetQuestDetails(QuestDetails details, bool showNotifications = true, bool playCutScenes = true)
		{
			Quest quest = Client.Instance.Quests.FirstOrDefault((Quest m) => m.ID == details.ID);
			if (quest == null)
			{
				return;
			}
			quest.SetDetails(details, showNotifications, playCutScenes);
			if (quest.Status != 0)
			{
				QuestUI questUI = Client.Instance.CanvasManager.PlayerOverview.Quests.AllQuests.FirstOrDefault((QuestUI m) => m.Quest == quest);
				if (questUI == null)
				{
					questUI = Client.Instance.CanvasManager.PlayerOverview.Quests.CreateQuestUI(quest);
				}
				questUI.RefreshQuestUI();
			}
			if (Client.Instance.CanvasManager.PlayerOverview.Quests.gameObject.activeInHierarchy)
			{
				Client.Instance.CanvasManager.PlayerOverview.Quests.RefreshSelectedQuest();
			}
		}

		public void CheckEquipmentAchievement()
		{
			if (!(CurrentOutfit == null) && !(CurrentHelmet == null) && !(FpsController.CurrentJetpack == null) && !IsAdmin && CurrentOutfit.Type == ItemType.SoePressurisedSuit && CurrentHelmet.Type == ItemType.SoePressurisedHelmet && FpsController.CurrentJetpack.Type == ItemType.SoePressurisedJetpack)
			{
				SteamStats.SetAchievement(SteamAchievementID.collection_full_soe_outfit);
			}
		}

		public void ResetStatistics()
		{
			Serializer.ResetStatistics();
			SentPacketStatistics = string.Empty;
			ReceivedPacketStatistics = string.Empty;
		}
	}
}
