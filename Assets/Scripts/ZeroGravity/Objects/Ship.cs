using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenHellion.Networking;
using OpenHellion.ProviderSystem;
using ThreeEyedGames;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Effects;
using ZeroGravity.LevelDesign;
using ZeroGravity.Math;
using ZeroGravity.Network;
using ZeroGravity.ShipComponents;
using ZeroGravity.UI;

namespace ZeroGravity.Objects
{
	public class Ship : SpaceObjectVessel
	{
		public delegate void DockCompletedDelegate(bool isInitialize);

		public Rigidbody Rigidbody;

		private bool shipStatsChanged;

		private ShipStatsMessage shipStatsMsg;

		public Dictionary<DistributionSystemType, float> AvailableResourceCapacities = new Dictionary<DistributionSystemType, float>();

		public Dictionary<DistributionSystemType, float> AvailableResourceQuantities = new Dictionary<DistributionSystemType, float>();

		public Ship TargetObject;

		private Transform stardustCameraTrans;

		private Stardust stardustObj;

		public float EngineThrustPercentage;

		public double EndWarpTime;

		public Vector3 AutoStabilize = Vector3.zero;

		public short lvl;

		public short[] index;

		public float[] resDebug;

		public Vector3 CurrRcsMoveThrust;

		public Vector3 CurrRcsRotationThrust;

		public bool gatherAtmos;

		public bool IsRotationStabilized;

		public NavigationPanel NavPanel;

		private EngineThrusters engineThrusters;

		private WarpEffect warpEffect;

		private WarpStartEffect warpStartEffect;

		public Task WarpStartEffectTask;

		private WarpEndEffect warpEndEffect;

		public Task WarpEndEffectTask;

		private WarpInductorExecuter warpInductorExecuter;

		private RCSThrusters rcsThrusters;

		private RefuelingStationUI refuelingStation;

		private bool listenersConnected;

		private List<Material> temperatureMaterials = new List<Material>();

		private Vector3D rotPredictionForward = Vector3D.Forward;

		private Vector3D rotPredictionUp = Vector3D.Up;

		private double rotationPredictionLastTime;

		private bool _useRCS;

		private bool _rcsOn;

		private Queue<VesselObjects> shipObjectsLoadingQueue;

		private bool rcsShipPlaying;

		private float previousThrustSoundTime;

		private bool sceneLoadStarted;

		public DockCompletedDelegate OnDockCompleted;

		public DockCompletedDelegate OnUndockCompleted;

		public DockCompletedDelegate OnUndockStarted;

		private SceneDockingPortDetails currDockDetails;

		private float lerpTimer;

		public override SpaceObjectType Type => SpaceObjectType.Ship;

		public long CourseWaitingActivation { get; private set; }

		public Vector3 CourseStartDirection { get; private set; }

		public double CourseStartTime { get; private set; }

		public double CourseEndTime { get; private set; }

		public SecuritySystem SecuritySystem { get; private set; }

		public bool IsThrusting
		{
			get
			{
				if (shipStatsMsg != null && (shipStatsMsg.Thrust != null || shipStatsMsg.Rotation != null))
				{
					return true;
				}
				return false;
			}
		}

		public bool RCSIsOn
		{
			get
			{
				return _useRCS;
			}
			set
			{
				if (value != _rcsOn)
				{
					_useRCS = value;
					_rcsOn = value;
				}
			}
		}

		public void ToggleGatheringAtmosphere(bool? isGathering = null)
		{
			if (isGathering.HasValue)
			{
				gatherAtmos = isGathering.HasValue;
			}
			else
			{
				gatherAtmos = !gatherAtmos;
			}
			if (shipStatsMsg == null)
			{
				shipStatsMsg = new ShipStatsMessage();
			}
			shipStatsMsg.GatherAtmosphere = gatherAtmos;
			shipStatsChanged = true;
		}

		private void CreateNewStatsMessage()
		{
			shipStatsMsg = new ShipStatsMessage
			{
				GUID = GUID,
				VesselObjects = new VesselObjects
				{
					SubSystems = new List<SubSystemDetails>(),
					Generators = new List<GeneratorDetails>(),
					RoomTriggers = new List<RoomDetails>(),
					Doors = new List<DoorDetails>(),
					SceneTriggerExecuters = new List<SceneTriggerExecuterDetails>(),
					DockingPorts = new List<SceneDockingPortDetails>(),
					AttachPoints = new List<AttachPointDetails>(),
					SpawnPoints = new List<SpawnPointStats>()
				}
			};
			shipStatsChanged = false;
		}

		public override void ChangeStats(Vector3? thrust = null, Vector3? rotation = null, Vector3? autoStabilize = null, float? engineThrustPercentage = null, SubSystemDetails subSystem = null, GeneratorDetails generator = null, RoomDetails roomTrigger = null, DoorDetails door = null, SceneTriggerExecuterDetails sceneTriggerExecuter = null, SceneDockingPortDetails dockingPort = null, AttachPointDetails attachPoint = null, long? stabilizationTarget = null, SpawnPointStats spawnPoint = null, float? selfDestructTime = null, string emblemId = null)
		{
			if (shipStatsMsg == null)
			{
				CreateNewStatsMessage();
			}
			if (thrust.HasValue && thrust.Value.IsNotEpsilonZero())
			{
				if (shipStatsMsg.Thrust != null)
				{
					shipStatsMsg.Thrust = (shipStatsMsg.Thrust.ToVector3() + thrust.Value).ToArray();
				}
				else
				{
					shipStatsMsg.Thrust = thrust.Value.ToArray();
				}
				shipStatsChanged = true;
			}
			if (rotation.HasValue && rotation.Value.IsNotEpsilonZero())
			{
				if (shipStatsMsg.Rotation != null)
				{
					shipStatsMsg.Rotation = (shipStatsMsg.Rotation.ToVector3() + base.transform.rotation * rotation.Value).ToArray();
				}
				else
				{
					shipStatsMsg.Rotation = (base.transform.rotation * rotation.Value).ToArray();
				}
				shipStatsChanged = true;
			}
			if (autoStabilize.HasValue)
			{
				shipStatsMsg.AutoStabilize = autoStabilize.Value.ToArray();
				shipStatsChanged = true;
			}
			if (engineThrustPercentage.HasValue)
			{
				shipStatsMsg.EngineThrustPercentage = engineThrustPercentage.Value;
				shipStatsChanged = true;
			}
			if (subSystem != null)
			{
				shipStatsMsg.VesselObjects.SubSystems.Add(subSystem);
				shipStatsChanged = true;
			}
			if (generator != null)
			{
				shipStatsMsg.VesselObjects.Generators.Add(generator);
				shipStatsChanged = true;
			}
			if (roomTrigger != null)
			{
				shipStatsMsg.VesselObjects.RoomTriggers.Add(roomTrigger);
				shipStatsChanged = true;
			}
			if (door != null)
			{
				shipStatsMsg.VesselObjects.Doors.Add(door);
				shipStatsChanged = true;
			}
			if (sceneTriggerExecuter != null)
			{
				shipStatsMsg.VesselObjects.SceneTriggerExecuters.Add(sceneTriggerExecuter);
				shipStatsChanged = true;
			}
			if (dockingPort != null)
			{
				shipStatsMsg.VesselObjects.DockingPorts.Add(dockingPort);
				shipStatsChanged = true;
			}
			if (attachPoint != null)
			{
				shipStatsMsg.VesselObjects.AttachPoints.Add(attachPoint);
				shipStatsChanged = true;
			}
			if (stabilizationTarget.HasValue)
			{
				shipStatsMsg.TargetStabilizationGUID = stabilizationTarget.Value;
				shipStatsChanged = true;
			}
			if (spawnPoint != null)
			{
				shipStatsMsg.VesselObjects.SpawnPoints.Add(spawnPoint);
				shipStatsChanged = true;
			}
			if (selfDestructTime.HasValue)
			{
				shipStatsMsg.SelfDestructTime = selfDestructTime;
				shipStatsChanged = true;
			}
			if (emblemId != null)
			{
				shipStatsMsg.VesselObjects.EmblemId = emblemId;
				shipStatsChanged = true;
			}
		}

		public override void DestroyGeometry()
		{
			base.DestroyGeometry();
			if (base.IsMainVessel)
			{
				foreach (SpaceObjectVessel allDockedVessel in AllDockedVessels)
				{
					allDockedVessel.DestroyGeometry();
				}
			}
			base.IsDummyObject = true;
			sceneLoadStarted = false;
			base.SceneObjectsLoaded = false;
		}

		public static Ship Create(long guid, VesselData data, ObjectTransform trans, bool isMainObject)
		{
			Ship ship = ArtificialBody.Create(SpaceObjectType.Ship, guid, trans, isMainObject) as Ship;
			if (data != null)
			{
				ship.VesselData = data;
			}
			ship.IsDummyObject = true;
			ship.SceneObjectsLoaded = false;
			Client.Instance.Map.InitializeMapObject(ship);
			return ship;
		}

		private void Start()
		{
			ConnectMessageListeners();
			this.InvokeRepeating(ProximityCanvasCheck, 1f, MathHelper.RandomRange(1f, 1.5f));
		}

		private void FixedUpdate()
		{
			if (AutoStabilize.IsNotEpsilonZero() && base.AngularVelocity != Vector3.zero)
			{
				Vector3? autoStabilize = AutoStabilize;
				ChangeStats(null, null, autoStabilize);
			}
			else if (AutoStabilize.IsNotEpsilonZero() && base.AngularVelocity == Vector3.zero)
			{
				Vector3? autoStabilize = Vector3.one;
				ChangeStats(null, null, autoStabilize);
				AutoStabilize = Vector3.zero;
			}
			if (MyPlayer.Instance.Parent == this && MyPlayer.Instance.IsDrivingShip && shipStatsChanged && (shipStatsMsg.Thrust != null || shipStatsMsg.Rotation != null))
			{
				ShipStatsMessage shipStatsMessage = new ShipStatsMessage();
				shipStatsMessage.GUID = base.GUID;
				shipStatsMessage.ThrustStats = new RcsThrustStats();
				if (shipStatsMsg.Thrust != null)
				{
					Vector3D vector3D = shipStatsMsg.Thrust.ToVector3D();
					if (!vector3D.IsEpsilonEqual(Vector3D.Zero, 0.0001))
					{
						if (vector3D.SqrMagnitude > 1.0)
						{
							vector3D = vector3D.Normalized;
						}
						vector3D = vector3D * ((!(RCS != null)) ? 0f : (RCS.Acceleration * RCS.MaxOperationRate)) * Time.fixedDeltaTime;
						Orbit.RelativePosition += vector3D * Time.fixedDeltaTime;
						Orbit.RelativeVelocity += vector3D;
						Orbit.InitFromCurrentStateVectors(Client.Instance.SolarSystem.CurrentTime);
						shipStatsMessage.ThrustStats.MoveTrust = vector3D.ToFloatArray();
					}
				}
				if (shipStatsMsg.Rotation != null)
				{
					Vector3D vector3D2 = shipStatsMsg.Rotation.ToVector3D();
					if (!vector3D2.IsEpsilonEqual(Vector3D.Zero, 0.0001))
					{
						if (vector3D2.SqrMagnitude > 1.0)
						{
							vector3D2 = vector3D2.Normalized;
						}
						vector3D2 = vector3D2 * ((!(RCS != null)) ? 0f : (RCS.RotationAcceleration * RCS.MaxOperationRate)) * Time.fixedDeltaTime;
						RotationVec += vector3D2;
						shipStatsMessage.ThrustStats.RotationTrust = vector3D2.ToFloatArray();
					}
				}
				ShipStatsMessageListener(shipStatsMessage);
			}
			if (shipStatsChanged)
			{
				NetworkController.Instance.SendToGameServer(shipStatsMsg);
				CreateNewStatsMessage();
			}
			SmoothRotation(Time.fixedDeltaTime);
		}

		public void SetRotationPredictionValues(Vector3D forward, Vector3D up)
		{
			rotPredictionForward = forward;
			rotPredictionUp = up;
			rotationPredictionLastTime = -1.0;
		}

		public Vector3D DampenRotationPrediction(double timeDelta, bool dampen, double stabilizationMultiplier = 1.0)
		{
			double num = (double)((!(RCS != null)) ? 0f : (RCS.RotationStabilization * RCS.MaxOperationRate)) * stabilizationMultiplier * timeDelta;
			Vector3D rotationVec = RotationVec;
			if (dampen)
			{
				if (RotationVec.X > 0.0)
				{
					RotationVec.X = MathHelper.Clamp(RotationVec.X - num, 0.0, RotationVec.X);
				}
				else
				{
					RotationVec.X = MathHelper.Clamp(RotationVec.X + num, RotationVec.X, 0.0);
				}
				if (RotationVec.Y > 0.0)
				{
					RotationVec.Y = MathHelper.Clamp(RotationVec.Y - num, 0.0, RotationVec.Y);
				}
				else
				{
					RotationVec.Y = MathHelper.Clamp(RotationVec.Y + num, RotationVec.Y, 0.0);
				}
				if (RotationVec.Z > 0.0)
				{
					RotationVec.Z = MathHelper.Clamp(RotationVec.Z - num, 0.0, RotationVec.Z);
				}
				else
				{
					RotationVec.Z = MathHelper.Clamp(RotationVec.Z + num, RotationVec.Z, 0.0);
				}
			}
			return RotationVec - rotationVec;
		}

		private void ApplyRotationPrediction()
		{
			if (rotationPredictionLastTime < 0.0)
			{
				rotationPredictionLastTime = 0.0;
				return;
			}
			if (RotationVec.Y.IsNotEpsilonZeroD(1E-05))
			{
				rotPredictionForward = QuaternionD.AngleAxis(RotationVec.Y * (double)Time.deltaTime, rotPredictionUp) * rotPredictionForward;
			}
			if (RotationVec.X.IsNotEpsilonZeroD(1E-05))
			{
				Vector3D vector3D = Vector3D.Cross(-rotPredictionForward, rotPredictionUp);
				rotPredictionForward = QuaternionD.AngleAxis(RotationVec.X * (double)Time.deltaTime, vector3D) * rotPredictionForward;
				rotPredictionUp = Vector3D.Cross(rotPredictionForward, vector3D);
			}
			if (RotationVec.Z.IsNotEpsilonZeroD(1E-05))
			{
				rotPredictionUp = QuaternionD.AngleAxis(RotationVec.Z * (double)Time.deltaTime, rotPredictionForward) * rotPredictionUp;
			}
			if ((double)Time.realtimeSinceStartup - rotationPredictionLastTime > 0.05000000074505806)
			{
				rotationPredictionLastTime = Time.realtimeSinceStartup;
				SetTargetPositionAndRotation(null, rotPredictionForward.ToVector3(), rotPredictionUp.ToVector3());
			}
		}

		private void Update()
		{
			if (MyPlayer.Instance.Parent != null)
			{
				UpdatePositionAndRotation(!base.IsMainObject && DockedToMainVessel == null);
			}
			if (ActivateGeometry)
			{
				if (RootObject == null || RootObject.activeInHierarchy)
				{
					ActivateGeometry = false;
				}
				else
				{
					RootObject.SetActive(value: true);
				}
			}
		}

		private void OnDestroy()
		{
			this.CancelInvoke(ActivateDamagePoints);
			DisconnectMessageListeners();
			Client.Instance.Map.RemoveMapObject(this);
			Client.Instance.SolarSystem.RemoveArtificialBody(this);
			SceneHelper.RemoveCubemapProbes(base.gameObject);
			Client.Instance.ActiveVessels.Remove(base.GUID);
		}

		public void ConnectMessageListeners()
		{
			if (!listenersConnected)
			{
				EventSystem.AddListener(typeof(ShipStatsMessage), ShipStatsMessageListener);
				EventSystem.AddListener(typeof(InitializeSpaceObjectMessage), InitializeSpaceObjectsMessageListener);
				EventSystem.AddListener(typeof(ManeuverCourseResponse), ManeuverCourseResponseListener);
				EventSystem.AddListener(typeof(VesselSecurityResponse), VesselSecurityResponseListener);
				EventSystem.AddListener(typeof(NameTagMessage), NameTagMessageListener);
				EventSystem.AddListener(typeof(VesselRequestResponse), VesselRequestResponseListener);
				EventSystem.AddListener(typeof(DestroyVesselMessage), DestroyVesselMessageListener);
			}
		}

		public void DisconnectMessageListeners()
		{
			EventSystem.RemoveListener(typeof(ShipStatsMessage), ShipStatsMessageListener);
			EventSystem.RemoveListener(typeof(InitializeSpaceObjectMessage), InitializeSpaceObjectsMessageListener);
			EventSystem.RemoveListener(typeof(ManeuverCourseResponse), ManeuverCourseResponseListener);
			EventSystem.RemoveListener(typeof(VesselSecurityResponse), VesselSecurityResponseListener);
			EventSystem.RemoveListener(typeof(NameTagMessage), NameTagMessageListener);
			EventSystem.RemoveListener(typeof(VesselRequestResponse), VesselRequestResponseListener);
			EventSystem.RemoveListener(typeof(DestroyVesselMessage), DestroyVesselMessageListener);
		}

		private void DestroyVesselMessageListener(NetworkData data)
		{
			DestroyVesselMessage destroyVesselMessage = data as DestroyVesselMessage;
			if (destroyVesselMessage.GUID == base.GUID && GeometryPlaceholder != null && DestructionEffects != null)
			{
				DestructionEffects.transform.parent = GeometryPlaceholder.transform;
				DestructionEffects.transform.Reset();
				DestructionEffects.transform.parent = Client.Instance.ShipExteriorRoot.transform;
				DestructionEffects.gameObject.SetActive(value: true);
				DestructionEffects.enabled = true;
			}
		}

		private void VesselRequestResponseListener(NetworkData data)
		{
			VesselRequestResponse vesselRequestResponse = data as VesselRequestResponse;
			if (vesselRequestResponse.GUID == base.GUID)
			{
				if (vesselRequestResponse.Message == RescueShipMessages.ShipCalled)
				{
					Client.Instance.CanvasManager.CanvasUI.Notification(Localization.GetLocalizedField("RescueShipWillArriveIn") + " " + FormatHelper.PeriodFormat(vesselRequestResponse.Time), CanvasUI.NotificationType.Alert);
				}
				if (vesselRequestResponse.Message == RescueShipMessages.ShipEnRoute)
				{
					Client.Instance.CanvasManager.CanvasUI.Notification(Localization.GetLocalizedField("RescueShipEnRoute") + " " + FormatHelper.PeriodFormat(vesselRequestResponse.Time), CanvasUI.NotificationType.Alert);
				}
				if (vesselRequestResponse.Message == RescueShipMessages.AnotherShipInRange)
				{
					Client.Instance.CanvasManager.CanvasUI.Notification(Localization.GetLocalizedField("AnotherShipInRange"), CanvasUI.NotificationType.Alert);
				}
				if (vesselRequestResponse.Message == RescueShipMessages.ShipArrived)
				{
					Client.Instance.CanvasManager.CanvasUI.Notification(Localization.GetLocalizedField("RescueShipArrived"), CanvasUI.NotificationType.Alert);
				}
			}
		}

		private void InitializeSpaceObjectsMessageListener(NetworkData data)
		{
			InitializeSpaceObjectMessage initializeSpaceObjectMessage = data as InitializeSpaceObjectMessage;
			if (initializeSpaceObjectMessage.GUID == base.GUID)
			{
				InitializeShipAuxData initializeShipAuxData = initializeSpaceObjectMessage.AuxData as InitializeShipAuxData;
				UpdateShipObjects(initializeShipAuxData.VesselObjects, isInitialize: true);
				UpdateDynamicObjects(initializeSpaceObjectMessage.DynamicObjects);
				UpdateCharacters(initializeSpaceObjectMessage.Characters);
				UpdateCorpses(initializeSpaceObjectMessage.Corpses);
			}
		}

		public void SendCancelManeuverCourseRequest()
		{
			ManeuverCourseRequest maneuverCourseRequestData = Client.Instance.Map.GetManeuverCourseRequestData(lockCourse: true);
			if (maneuverCourseRequestData == null)
			{
				maneuverCourseRequestData.ShipGUID = base.GUID;
				maneuverCourseRequestData.Activate = false;
				NetworkController.Instance.SendToGameServer(maneuverCourseRequestData);
			}
		}

		public void SendManeuverCourseRequest()
		{
			ManeuverCourseRequest maneuverCourseRequestData = Client.Instance.Map.GetManeuverCourseRequestData(lockCourse: true);
			if (maneuverCourseRequestData != null)
			{
				maneuverCourseRequestData.ShipGUID = base.GUID;
				NetworkController.Instance.SendToGameServer(maneuverCourseRequestData);
			}
		}

		public void SendManeuverCourseActivationRequest()
		{
			if (CourseWaitingActivation > 0)
			{
				NetworkController.Instance.SendToGameServer(new ManeuverCourseRequest
				{
					CourseGUID = CourseWaitingActivation,
					ShipGUID = base.GUID,
					Activate = true
				});
			}
		}

		private void ManeuverCourseResponseListener(NetworkData data)
		{
			ManeuverCourseResponse maneuverCourseResponse = data as ManeuverCourseResponse;
			if (maneuverCourseResponse.VesselGUID != base.GUID)
			{
				return;
			}
			if (maneuverCourseResponse.IsValid)
			{
				if (maneuverCourseResponse.StartDirection != null && maneuverCourseResponse.StartTime.HasValue)
				{
					CourseWaitingActivation = maneuverCourseResponse.CourseGUID;
					CourseStartDirection = maneuverCourseResponse.StartDirection.ToVector3();
					CourseStartTime = maneuverCourseResponse.StartTime.Value;
					CourseEndTime = maneuverCourseResponse.EndTime.Value;
					if (Client.Instance.Map.WarpManeuver != null)
					{
						Client.Instance.Map.WarpManeuver.Initialized = true;
					}
					if (warpInductorExecuter != null)
					{
						warpInductorExecuter.ToggleInductor(isActive: true, isInstant: false);
					}
				}
				if (!maneuverCourseResponse.IsActivated.HasValue || maneuverCourseResponse.IsActivated.Value)
				{
				}
				if (maneuverCourseResponse.StaringSoon.HasValue && maneuverCourseResponse.StaringSoon.Value)
				{
					base.IsWarpOnline = true;
					if (maneuverCourseResponse.EndTime.HasValue)
					{
						EndWarpTime = maneuverCourseResponse.EndTime.Value;
					}
					if (MyPlayer.Instance.IsInVesselHierarchy(this))
					{
						MyPlayer.Instance.CheckCameraShake();
					}
					else if (warpStartEffect != null)
					{
						WarpStartEffectTask = new Task(delegate
						{
							if (!base.IsDummyObject)
							{
								ActivateWarpStartEffect();
								WarpStartEffectTask = null;
							}
						});
					}
					if (warpEffect != null && MyPlayer.Instance.IsInVesselHierarchy(this))
					{
						warpEffect.SetActive(value: true);
					}

					foreach (SoundEffect warpSound in FTLEngine.WarpSounds)
					{
						warpSound.Play(0, dontPlayIfPlaying: true);
					}
				}
			}
			else
			{
				CancelManeuver();
				if (maneuverCourseResponse.IsFinished.HasValue && maneuverCourseResponse.IsFinished.Value && !MyPlayer.Instance.IsInVesselHierarchy(this))
				{
					WarpEndEffectTask = new Task(delegate
					{
						if (!base.IsDummyObject)
						{
							ActivateWarpEndEffect();
							WarpEndEffectTask = null;
						}
					});
				}
			}
			ProviderManager.UpdateStatus();
		}

		public void ActivateWarpStartEffect()
		{
			if (warpStartEffect != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(warpStartEffect.gameObject, Client.Instance.ShipExteriorRoot.transform);
				gameObject.transform.position = warpStartEffect.transform.position;
				gameObject.transform.localScale = warpStartEffect.transform.localScale;
				gameObject.gameObject.Activate(value: true);
			}
		}

		public void ActivateWarpEndEffect()
		{
			if (warpEndEffect != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(warpEndEffect.gameObject, Client.Instance.ShipExteriorRoot.transform);
				gameObject.transform.position = warpEndEffect.transform.position;
				gameObject.transform.localScale = warpEndEffect.transform.localScale;
				gameObject.gameObject.Activate(value: true);
			}
		}

		public void CancelManeuver()
		{
			CourseWaitingActivation = 0L;
			base.IsWarpOnline = false;
			if (MyPlayer.Instance.IsInVesselHierarchy(this))
			{
				foreach (SoundEffect warpSound in FTLEngine.WarpSounds)
				{
					warpSound.Play(1);
				}
				if (MyPlayer.Instance.LockedToTrigger is SceneTriggerNavigationPanel)
				{
					Client.Instance.InGamePanels.Navigation.CancelWarp();
				}
				Client.Instance.Map.RemoveManeuverCourse();
			}
			if (MyPlayer.Instance.IsInVesselHierarchy(this))
			{
				MyPlayer.Instance.CheckCameraShake();
			}
			if (warpEffect != null)
			{
				warpEffect.SetActive(value: false);
			}
			if (warpInductorExecuter != null)
			{
				warpInductorExecuter.ToggleInductor(isActive: false, isInstant: false);
			}
		}

		private void VesselSecurityResponseListener(NetworkData data)
		{
			VesselSecurityResponse vesselSecurityResponse = data as VesselSecurityResponse;
			if (vesselSecurityResponse.VesselGUID == base.GUID && !(SecuritySystem == null))
			{
				SecuritySystem.ParseSecurityData(vesselSecurityResponse.Data);
				if (MyPlayer.Instance.LockedToTrigger is SceneTriggerPowerSupplyPanel)
				{
					(MyPlayer.Instance.LockedToTrigger as SceneTriggerPowerSupplyPanel).MyPowerSupply.RefreshPowerSupply();
				}
				if (MyPlayer.Instance.LockedToTrigger is SceneTriggerLifeSupportPanel)
				{
					(MyPlayer.Instance.LockedToTrigger as SceneTriggerLifeSupportPanel).MyLifeSupport.RefreshLifeSupport();
				}
			}
		}

		private void NameTagMessageListener(NetworkData data)
		{
			NameTagMessage nameTagMessage = data as NameTagMessage;
			if (nameTagMessage.ID.VesselGUID != base.GUID)
			{
				return;
			}
			try
			{
				NameTags[nameTagMessage.ID.InSceneID].SetNameTagText(nameTagMessage.NameTagText);
			}
			catch
			{
			}
		}

		private void UpdateShipObjects(VesselObjects shipObjects, bool isInitialize = false)
		{
			if (shipObjects == null || !base.gameObject.activeInHierarchy)
			{
				return;
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			if (shipObjects.MiscStatuses != null)
			{
				if (shipObjects.MiscStatuses.CourseInProgress != null)
				{
					base.IsWarpOnline = true;
					EndWarpTime = shipObjects.MiscStatuses.CourseInProgress.EndSolarSystemTime;
				}
				if (engineThrusters != null)
				{
					engineThrusters.OnOff = base.EngineOnLine;
				}
				if (warpEffect != null)
				{
					warpEffect.SetActive(base.IsWarpOnline && MyPlayer.Instance.IsInVesselHierarchy(this));
				}
				if (warpInductorExecuter != null)
				{
					warpInductorExecuter.ToggleInductor(base.IsWarpOnline, isInstant: true);
				}
				MyPlayer.Instance.CheckCameraShake();
			}
			if (shipObjects.SecurityData != null && SecuritySystem != null)
			{
				SecuritySystem.ParseSecurityData(shipObjects.SecurityData);
			}
			if (shipObjects.SubSystems != null)
			{
				foreach (SubSystemDetails subSystem in shipObjects.SubSystems)
				{
					try
					{
						SubSystem structureObject = GetStructureObject<SubSystem>(subSystem.InSceneID);
						structureObject.SetDetails(subSystem);
						if (MyPlayer.Instance.Parent == this && structureObject is VesselBaseSystem && MyPlayer.Instance.ShipControlMode == ShipControlMode.Navigation && structureObject.Status == SystemStatus.Offline)
						{
							MyPlayer.Instance.ShipControlMode = ShipControlMode.Piloting;
						}
						if ((structureObject is IPowerConsumer || structureObject is VesselBaseSystem) && MyPlayer.Instance.LockedToTrigger is SceneTriggerPowerSupplyPanel)
						{
							flag2 = true;
						}
						if (structureObject is SubSystemRefinery)
						{
							if (MyPlayer.Instance.LockedToTrigger is SceneTriggerCargoPanel)
							{
								flag3 = true;
							}
						}
						else if (structureObject is SubSystemFabricator)
						{
							if (MyPlayer.Instance.LockedToTrigger is SceneTriggerCargoPanel)
							{
								flag3 = true;
							}
						}
						else if (structureObject is SubSystemEngine)
						{
							if (MyPlayer.Instance.IsInVesselHierarchy(this))
							{
								MyPlayer.Instance.CheckCameraShake();
							}
							if (engineThrusters != null)
							{
								engineThrusters.OnOff = structureObject.Status == SystemStatus.Online;
							}
						}
						else if (structureObject is SubSystemFTL && warpEffect != null && isInitialize)
						{
							warpEffect.SetActive(structureObject.Status == SystemStatus.Online && MyPlayer.Instance.IsInVesselHierarchy(this), instant: true);
						}
						if (structureObject.Room != null)
						{
							structureObject.Room.ExecuteBehaviourScripts();
						}
					}
					catch (Exception ex)
					{
						Dbg.Error("SubSystemDetails exception", base.GUID, base.SceneID.ToString(), subSystem.InSceneID, ex.Message, ex.StackTrace);
					}
				}
			}
			if (shipObjects.NameTags != null)
			{
				foreach (NameTagData nameTag in shipObjects.NameTags)
				{
					try
					{
						SceneNameTag structureObject2 = GetStructureObject<SceneNameTag>(nameTag.InSceneID);
						structureObject2.SetNameTagText(nameTag.NameTagText);
					}
					catch (Exception ex2)
					{
						Dbg.Error("NameTagData exception", base.GUID, base.SceneID.ToString(), nameTag.InSceneID, ex2.Message, ex2.StackTrace);
					}
				}
			}
			if (shipObjects.Generators != null)
			{
				foreach (GeneratorDetails generator in shipObjects.Generators)
				{
					try
					{
						Generator structureObject3 = GetStructureObject<Generator>(generator.InSceneID);
						structureObject3.SetDetails(generator);
						if (structureObject3 is GeneratorAir || structureObject3 is GeneratorScrubbedAir)
						{
							flag = true;
							flag2 = true;
						}
						if (MyPlayer.Instance.LockedToTrigger is SceneTriggerPowerSupplyPanel)
						{
							(MyPlayer.Instance.LockedToTrigger as SceneTriggerPowerSupplyPanel).MyPowerSupply.UpdateGenerator(structureObject3);
							(MyPlayer.Instance.LockedToTrigger as SceneTriggerPowerSupplyPanel).MyPowerSupply.UpdateCapacitor(structureObject3);
						}
						if (MyPlayer.Instance.LockedToTrigger is SceneTriggerLifeSupportPanel)
						{
							(MyPlayer.Instance.LockedToTrigger as SceneTriggerLifeSupportPanel).MyLifeSupport.UpdateGenerator(structureObject3);
						}
					}
					catch (Exception ex3)
					{
						Dbg.Error("GeneratorDetails exception", base.GUID, base.SceneID.ToString(), generator.InSceneID, ex3.Message, ex3.StackTrace);
					}
				}
			}
			if (shipObjects.RoomTriggers != null)
			{
				bool flag4 = false;
				foreach (RoomDetails roomTrigger in shipObjects.RoomTriggers)
				{
					try
					{
						SceneTriggerRoom structureObject4 = GetStructureObject<SceneTriggerRoom>(roomTrigger.InSceneID);
						if (structureObject4.AirPressure < 1f && roomTrigger.AirPressure == 1f)
						{
							SceneQuestTrigger.OnTrigger(structureObject4.gameObject, SceneQuestTriggerEvent.Pressurize);
						}
						else if (structureObject4.AirPressure > 0f && roomTrigger.AirPressure == 0f)
						{
							SceneQuestTrigger.OnTrigger(structureObject4.gameObject, SceneQuestTriggerEvent.Depressurize);
						}
						short compoundRoomID = structureObject4.CompoundRoomID;
						if (structureObject4.UseGravity != roomTrigger.UseGravity)
						{
							structureObject4.UseGravity = roomTrigger.UseGravity;
							MyPlayer.Instance.CheckRoomTrigger(structureObject4);
							DynamicObject[] componentsInChildren = TransferableObjectsRoot.GetComponentsInChildren<DynamicObject>();
							foreach (DynamicObject dynamicObject in componentsInChildren)
							{
								dynamicObject.CheckRoomTrigger(structureObject4);
							}
							Corpse[] componentsInChildren2 = TransferableObjectsRoot.GetComponentsInChildren<Corpse>();
							foreach (Corpse corpse in componentsInChildren2)
							{
								corpse.CheckRoomTrigger(structureObject4);
							}
						}
						structureObject4.AirFiltering = roomTrigger.AirFiltering;
						structureObject4.AirPressure = roomTrigger.AirPressure;
						structureObject4.AirQuality = roomTrigger.AirQuality;
						structureObject4.AirPressureChangeRate = roomTrigger.AirPressureChangeRate;
						structureObject4.AirQualityChangeRate = roomTrigger.AirQualityChangeRate;
						structureObject4.PressurizationStatus = roomTrigger.PressurizationStatus;
						structureObject4.CompoundRoomID = roomTrigger.CompoundRoomID;
						structureObject4.Fire = roomTrigger.Fire;
						structureObject4.Breach = roomTrigger.Breach;
						bool flag5 = structureObject4.GravityMalfunction != roomTrigger.GravityMalfunction && MyPlayer.Instance.CurrentRoomTrigger == structureObject4;
						structureObject4.GravityMalfunction = roomTrigger.GravityMalfunction;
						structureObject4.ExecuteBehaviourScripts();
						if (flag5)
						{
							MyPlayer.Instance.CheckRoomTrigger(null);
						}
						if (MyPlayer.Instance.IsLockedToTrigger && MyPlayer.Instance.LockedToTrigger is SceneTriggerLifeSupportPanel)
						{
							(MyPlayer.Instance.LockedToTrigger as SceneTriggerLifeSupportPanel).MyLifeSupport.UpdateRoom(structureObject4);
						}
						if (MyPlayer.Instance.CurrentRoomTrigger != null && ((MyPlayer.Instance.CurrentRoomTrigger.CompoundRoomID == compoundRoomID && compoundRoomID != roomTrigger.CompoundRoomID) || MyPlayer.Instance.CurrentRoomTrigger.CompoundRoomID == structureObject4.CompoundRoomID))
						{
							flag4 = true;
						}
					}
					catch (Exception ex4)
					{
						Dbg.Error("RoomDetails exception", base.GUID, base.SceneID.ToString(), roomTrigger.InSceneID, ex4.Message, ex4.StackTrace);
					}
				}
				if (flag4)
				{
					Client.Instance.CanvasManager.CanvasUI.HelmetHud.WarningsUpdate();
				}
			}
			if (shipObjects.ResourceContainers != null)
			{
				foreach (ResourceContainerDetails resourceContainer in shipObjects.ResourceContainers)
				{
					try
					{
						ResourceContainer structureObject5 = GetStructureObject<ResourceContainer>(resourceContainer.InSceneID);
						float quantity = structureObject5.Quantity;
						structureObject5.SetDetails(resourceContainer);
						if (MyPlayer.Instance.IsLockedToTrigger && MyPlayer.Instance.LockedToTrigger is SceneTriggerCargoPanel)
						{
							(MyPlayer.Instance.LockedToTrigger as SceneTriggerCargoPanel).CargoPanel.RefreshSystemObject(structureObject5);
							if (structureObject5.Quantity > quantity)
							{
								SceneQuestTrigger.OnTrigger(structureObject5.gameObject, SceneQuestTriggerEvent.IncreaseQuantity);
							}
							else if (structureObject5.Quantity < quantity)
							{
								SceneQuestTrigger.OnTrigger(structureObject5.gameObject, SceneQuestTriggerEvent.DecreaseQuantity);
							}
						}
						if (refuelingStation != null)
						{
							refuelingStation.UpdateResourceContainer(structureObject5);
						}
						if (MyPlayer.Instance.IsLockedToTrigger && MyPlayer.Instance.LockedToTrigger is SceneTriggerPowerSupplyPanel)
						{
							(MyPlayer.Instance.LockedToTrigger as SceneTriggerPowerSupplyPanel).MyPowerSupply.UpdateGenerator(structureObject5);
						}
					}
					catch (Exception ex5)
					{
						Dbg.Error("ResourceContainerDetails exception", base.GUID, base.SceneID.ToString(), resourceContainer.InSceneID, ex5.Message, ex5.StackTrace);
					}
				}
			}
			if (shipObjects.Doors != null)
			{
				foreach (DoorDetails door in shipObjects.Doors)
				{
					try
					{
						SceneDoor structureObject6 = GetStructureObject<SceneDoor>(door.InSceneID);
						structureObject6.SetDoorDetails(door);
						structureObject6.UpdateDoorUI();
					}
					catch (Exception ex6)
					{
						Dbg.Error("DoorDetails exception", base.GUID, base.SceneID.ToString(), door.InSceneID, ex6.Message, ex6.StackTrace);
					}
				}
			}
			if (shipObjects.SceneTriggerExecuters != null)
			{
				foreach (SceneTriggerExecuterDetails sceneTriggerExecuter in shipObjects.SceneTriggerExecuters)
				{
					try
					{
						SceneTriggerExecuter structureObject7 = GetStructureObject<SceneTriggerExecuter>(sceneTriggerExecuter.InSceneID);
						structureObject7.SetExecuterDetails(sceneTriggerExecuter, isInitialize);
					}
					catch (Exception ex7)
					{
						Dbg.Error("SceneTriggerExecuterDetails exception", base.GUID, base.SceneID.ToString(), sceneTriggerExecuter.InSceneID, ex7.Message, ex7.StackTrace);
					}
				}
			}
			if (shipObjects.DockingPorts != null)
			{
				foreach (SceneDockingPortDetails dockingPort in shipObjects.DockingPorts)
				{
					try
					{
						SceneDockingPort structureObject8 = GetStructureObject<SceneDockingPort>(dockingPort.ID.InSceneID);
						if (MyPlayer.Instance.LockedToTrigger is SceneTriggerDockingPanel || MyPlayer.Instance.LockedToTrigger is SceneTriggerShipControl2)
						{
							if (structureObject8.DockedToPort == null && dockingPort.DockedToID != null)
							{
								SceneQuestTrigger.OnTrigger(structureObject8.gameObject, SceneQuestTriggerEvent.Dock);
							}
							else if (structureObject8.DockedToPort != null && dockingPort.DockedToID == null)
							{
								SceneQuestTrigger.OnTrigger(structureObject8.gameObject, SceneQuestTriggerEvent.Undock);
							}
						}
						structureObject8.SetDetails(dockingPort, isInitialize);
						if (MyPlayer.Instance.LockedToTrigger is SceneTriggerPowerSupplyPanel)
						{
							(MyPlayer.Instance.LockedToTrigger as SceneTriggerPowerSupplyPanel).MyPowerSupply.RefreshPowerSupply();
						}
						if (MyPlayer.Instance.LockedToTrigger is SceneTriggerCargoPanel)
						{
							(MyPlayer.Instance.LockedToTrigger as SceneTriggerCargoPanel).CargoPanel.RefreshCargoPanel();
						}
						if (MyPlayer.Instance.LockedToTrigger is SceneTriggerLifeSupportPanel)
						{
							(MyPlayer.Instance.LockedToTrigger as SceneTriggerLifeSupportPanel).MyLifeSupport.RefreshLifeSupport();
						}
						if (MyPlayer.Instance.LockedToTrigger is SceneTriggerAirlockPanel)
						{
							(MyPlayer.Instance.LockedToTrigger as SceneTriggerAirlockPanel).AirlockUI.GetVesselAndAirTanks();
						}
					}
					catch (Exception ex)
					{
						Dbg.Error("SceneDockingPortDetails exception", base.GUID, base.SceneID.ToString(), dockingPort.ID, ex.Message, ex.StackTrace);
					}
				}
			}
			if (shipObjects.CargoBay != null)
			{
				try
				{
					CargoBay.SetDetails(shipObjects.CargoBay);
					if (MyPlayer.Instance.IsLockedToTrigger && MyPlayer.Instance.LockedToTrigger is SceneTriggerCargoPanel)
					{
						CargoPanel cargoPanel = (MyPlayer.Instance.LockedToTrigger as SceneTriggerCargoPanel).CargoPanel;
						if (cargoPanel.CurrentlySelectedCargoBay == CargoBay)
						{
							cargoPanel.RefreshMainCargoResources();
						}
						cargoPanel.UpdateVesselObjects(CargoBay.ParentVessel);
					}
				}
				catch (Exception ex)
				{
					Dbg.Error("CargoBayDetails exception", base.GUID, base.SceneID.ToString(), shipObjects.CargoBay.InSceneID, ex.Message, ex.StackTrace);
				}
			}
			if (shipObjects.SpawnWithChance != null)
			{
			}
			if (shipObjects.SpawnPoints != null)
			{
				foreach (SpawnPointStats spawnPoint in shipObjects.SpawnPoints)
				{
					try
					{
						SceneSpawnPoint structureObject9 = GetStructureObject<SceneSpawnPoint>(spawnPoint.InSceneID);
						if (MyPlayer.Instance.LockedToTrigger is SceneTriggerCryoPod)
						{
							long? playerGUID = spawnPoint.PlayerGUID;
							if (playerGUID.GetValueOrDefault() == MyPlayer.Instance.GUID && playerGUID.HasValue && spawnPoint.NewState == SpawnPointState.Authorized)
							{
								SceneQuestTrigger.OnTrigger(structureObject9.gameObject, SceneQuestTriggerEvent.AssignOnCryoPod);
							}
						}
						structureObject9.SetStats(spawnPoint);
					}
					catch (Exception ex)
					{
						Dbg.Error("SpawnPointStats exception", base.GUID, base.SceneID.ToString(), spawnPoint.InSceneID, ex.Message, ex.StackTrace);
					}
				}
			}
			if (shipObjects.RepairPoints != null)
			{
				foreach (VesselRepairPointDetails repairPoint in shipObjects.RepairPoints)
				{
					try
					{
						VesselRepairPoint structureObject10 = GetStructureObject<VesselRepairPoint>(repairPoint.InSceneID);
						structureObject10.SetDetails(repairPoint);
					}
					catch (Exception ex)
					{
						Dbg.Error("VesselRepairPoint exception", base.GUID, base.SceneID.ToString(), repairPoint.InSceneID, ex.Message, ex.StackTrace);
					}
				}
			}
			if (shipObjects.EmblemId != null)
			{
				try
				{
					foreach (SceneVesselEmblem item in Emblems.Where((SceneVesselEmblem m) => m != null))
					{
						item.SetEmblem(shipObjects.EmblemId);
					}
					if (SecuritySystem != null)
					{
						SecuritySystem.UpdateUI();
					}
				}
				catch (Exception ex)
				{
					Dbg.Error("Emblem exception", base.GUID, base.SceneID.ToString(), ex.Message, ex.StackTrace);
				}
			}
			if (flag && MyPlayer.Instance.LockedToTrigger is SceneTriggerLifeSupportPanel)
			{
				(MyPlayer.Instance.LockedToTrigger as SceneTriggerLifeSupportPanel).MyLifeSupport.GetPowerStatus();
			}
			if (flag2 && MyPlayer.Instance.LockedToTrigger is SceneTriggerPowerSupplyPanel)
			{
				(MyPlayer.Instance.LockedToTrigger as SceneTriggerPowerSupplyPanel).MyPowerSupply.UpdateVesselConsumers(this);
			}
			if (flag3 && MyPlayer.Instance.LockedToTrigger is SceneTriggerCargoPanel)
			{
				if ((MyPlayer.Instance.LockedToTrigger as SceneTriggerCargoPanel).CargoPanel.RefineryActive.activeInHierarchy)
				{
					(MyPlayer.Instance.LockedToTrigger as SceneTriggerCargoPanel).CargoPanel.UpdateRefineryResources();
				}
				else if ((MyPlayer.Instance.LockedToTrigger as SceneTriggerCargoPanel).CargoPanel.CraftingActive.activeInHierarchy)
				{
					(MyPlayer.Instance.LockedToTrigger as SceneTriggerCargoPanel).CargoPanel.UpdateCraftingResources();
				}
			}
		}

		private void ShipStatsMessageListener(NetworkData data)
		{
			ShipStatsMessage shipStatsMessage = data as ShipStatsMessage;
			if (shipStatsMessage.GUID != base.GUID)
			{
				return;
			}
			if (shipStatsMessage.ThrustStats != null)
			{
				CurrRcsMoveThrust = ((shipStatsMessage.ThrustStats.MoveTrust == null) ? Vector3.zero : shipStatsMessage.ThrustStats.MoveTrust.ToVector3());
				CurrRcsRotationThrust = ((shipStatsMessage.ThrustStats.RotationTrust == null) ? Vector3.zero : shipStatsMessage.ThrustStats.RotationTrust.ToVector3());
				bool flag = CurrRcsMoveThrust.magnitude > 0f || CurrRcsRotationThrust.magnitude > 0f;
				Vector3 moveVector = Quaternion.LookRotation(Forward, Up).Inverse() * CurrRcsMoveThrust;
				if (rcsThrusters != null)
				{
					if (flag)
					{
						rcsThrusters.SetMoveVector(moveVector);
						rcsThrusters.SetRotateVector(CurrRcsRotationThrust);
						rcsThrusters.UpdateThrusters();
					}
					else
					{
						rcsThrusters.SetMoveVector(Vector3.zero);
						rcsThrusters.SetRotateVector(Vector3.zero);
						rcsThrusters.UpdateThrusters();
					}
				}
			}
			if (shipStatsMessage.lvl.HasValue)
			{
				lvl = shipStatsMessage.lvl.Value;
			}
			if (shipStatsMessage.index != null && shipStatsMessage.index.Length == 2)
			{
				index = shipStatsMessage.index;
			}
			if (shipStatsMessage.debugRes != null)
			{
				resDebug = shipStatsMessage.debugRes;
			}
			if (shipStatsMessage.VesselObjects != null)
			{
				if (base.SceneObjectsLoaded)
				{
					UpdateShipObjects(shipStatsMessage.VesselObjects);
				}
				else
				{
					if (shipObjectsLoadingQueue == null)
					{
						shipObjectsLoadingQueue = new Queue<VesselObjects>();
					}
					shipObjectsLoadingQueue.Enqueue(shipStatsMessage.VesselObjects);
				}
			}
			if (shipStatsMessage.Temperature.HasValue)
			{
				Temperature = shipStatsMessage.Temperature.Value;
			}

			DoorEnviormentPanel[] doorEnviormentPanels = null;
			if (shipStatsMessage.Health.HasValue || shipStatsMessage.Armor.HasValue)
			{
				doorEnviormentPanels = GeometryRoot.GetComponentsInChildren<DoorEnviormentPanel>();
			}

			if (shipStatsMessage.Health.HasValue)
			{
				Health = shipStatsMessage.Health.Value;
				foreach (DoorEnviormentPanel doorEnviormentPanel in doorEnviormentPanels)
				{
					doorEnviormentPanel.DoorEnviormentUpdateUI();
				}
				foreach (VesselHealthDecal vesselHealthDecal in GeometryRoot.GetComponentsInChildren<VesselHealthDecal>(includeInactive: true))
				{
					vesselHealthDecal.UpdateDecals();
				}
				float num = ((!base.IsDummyObject) ? GetDamagePointsFrequency() : 0f);
				bool flag2 = this.IsInvoking(ActivateDamagePoints);
				if (num > float.Epsilon && !flag2)
				{
					ActivateDamagePoints();
				}
				else if (num <= float.Epsilon && flag2)
				{
					this.CancelInvoke(ActivateDamagePoints);
				}
				if (MyPlayer.Instance.LockedToTrigger is SceneTriggerLifeSupportPanel)
				{
					(MyPlayer.Instance.LockedToTrigger as SceneTriggerLifeSupportPanel).MyLifeSupport.UpdateConnectedVesselsHealth();
				}
			}
			if (shipStatsMessage.Armor.HasValue)
			{
				Armor = shipStatsMessage.Armor.Value;
				foreach (DoorEnviormentPanel doorEnviormentPanel2 in doorEnviormentPanels)
				{
					doorEnviormentPanel2.DoorEnviormentUpdateUI();
				}
				foreach (VesselArmorDecal vesselArmorDecal in GeometryRoot.GetComponentsInChildren<VesselArmorDecal>())
				{
					vesselArmorDecal.UpdateDecals();
				}
				if (MyPlayer.Instance.LockedToTrigger is SceneTriggerLifeSupportPanel)
				{
					(MyPlayer.Instance.LockedToTrigger as SceneTriggerLifeSupportPanel).MyLifeSupport.UpdateConnectedVesselsHealth();
				}
			}
			if (shipStatsMessage.SelfDestructTime.HasValue)
			{
				if (shipStatsMessage.SelfDestructTime.Value >= 0f)
				{
					SelfDestructTimer = shipStatsMessage.SelfDestructTime.Value;
				}
				else
				{
					SelfDestructTimer = null;
				}
				DoorEnviormentPanel[] componentsInChildren5 = GeometryRoot.GetComponentsInChildren<DoorEnviormentPanel>();
				foreach (DoorEnviormentPanel doorEnviormentPanel3 in componentsInChildren5)
				{
					doorEnviormentPanel3.DoorEnviormentUpdateUI();
				}
				if (SecuritySystem != null)
				{
					SecuritySystem.UpdateSelfDestructTimer();
				}
			}
		}

		private float GetDamagePointsFrequency()
		{
			return (DamagePointEffectFrequency == null) ? 0f : DamagePointEffectFrequency.Evaluate(Health / base.MaxHealth);
		}

		public override void ParseSpawnData(SpawnObjectResponseData data)
		{
			base.ParseSpawnData(data);
			SpawnShipResponseData spawnShipResponseData = data as SpawnShipResponseData;
			VesselData = spawnShipResponseData.Data;
			if (SecuritySystem != null)
			{
				SecuritySystem.UpdateUI();
			}
			if (spawnShipResponseData.DockedVessels != null)
			{
				DummyDockedVessels = spawnShipResponseData.DockedVessels;
			}
			if (!sceneLoadStarted && !base.SceneObjectsLoaded && !base.IsDummyObject)
			{
				sceneLoadStarted = true;
				base.transform.parent = Client.Instance.ShipExteriorRoot.transform;
				base.gameObject.SetActive(value: true);
				StartCoroutine(LoadAllShipScenesCoroutine(spawnShipResponseData));
			}
		}

		private IEnumerator LoadAllShipScenesCoroutine(SpawnShipResponseData res)
		{
			yield return StartCoroutine(LoadShipScenesCoroutine(isMainShip: false, res.VesselObjects));
			if (res.DockedVessels != null && res.DockedVessels.Count > 0)
			{
				foreach (DockedVesselData dockVess in res.DockedVessels)
				{
					Ship childShip = Client.Instance.GetVessel(dockVess.GUID) as Ship;
					childShip.IsDummyObject = false;
					sceneLoadStarted = true;
					childShip.VesselData = dockVess.Data;
					yield return StartCoroutine(childShip.LoadShipScenesCoroutine(isMainShip: false, dockVess.VesselObjects));
					childShip.transform.parent = ConnectedObjectsRoot.transform;
				}
			}
			if (res.VesselObjects.DockingPorts != null)
			{
				SceneDockingPort[] componentsInChildren = GeometryRoot.GetComponentsInChildren<SceneDockingPort>(includeInactive: true);
				foreach (SceneDockingPort dp in componentsInChildren)
				{
					SceneDockingPortDetails sceneDockingPortDetails = res.VesselObjects.DockingPorts.Find((SceneDockingPortDetails m) => m.ID.InSceneID == dp.InSceneID);
					if (sceneDockingPortDetails != null)
					{
						dp.SetDetails(sceneDockingPortDetails, isInitialize: true);
					}
				}
			}
			DockingControlsDisabled = res.DockingControlsDisabled;
			SecurityPanelsLocked = res.SecurityPanelsLocked;
			if (res.DockedVessels != null && res.DockedVessels.Count > 0)
			{
				foreach (DockedVesselData dockedVessel in res.DockedVessels)
				{
					Ship ship = Client.Instance.GetVessel(dockedVessel.GUID) as Ship;
					ship.DockingControlsDisabled = dockedVessel.DockingControlsDisabled;
					ship.SecurityPanelsLocked = dockedVessel.SecurityPanelsLocked;
					if (dockedVessel.VesselObjects.DockingPorts == null)
					{
						continue;
					}
					SceneDockingPort[] componentsInChildren2 = ship.GeometryRoot.GetComponentsInChildren<SceneDockingPort>(includeInactive: true);
					foreach (SceneDockingPort dport in componentsInChildren2)
					{
						SceneDockingPortDetails sceneDockingPortDetails2 = dockedVessel.VesselObjects.DockingPorts.Find((SceneDockingPortDetails m) => m.ID.InSceneID == dport.InSceneID);
						if (sceneDockingPortDetails2 != null)
						{
							dport.SetDetails(sceneDockingPortDetails2, isInitialize: true);
						}
					}
				}
			}
			if (SecuritySystem != null && res.Data != null)
			{
				SceneNameTag[] shipNameTags = SecuritySystem.ShipNameTags;
				foreach (SceneNameTag sceneNameTag in shipNameTags)
				{
					sceneNameTag.SetNameTagText(res.Data.VesselName);
				}
			}
		}

		private IEnumerator LoadStructureScenes(GameScenes.SceneID sceneID, Transform rootTransform, VesselObjects shipObjects)
		{
			yield return StartCoroutine(Client.Instance.SceneLoader.LoadSceneCoroutine(SceneLoader.SceneType.Structure, (long)sceneID));
			GameObject sceneRoot = Client.Instance.SceneLoader.GetLoadedScene(SceneLoader.SceneType.Structure, (long)sceneID);
			sceneRoot.transform.SetParent(rootTransform);
			sceneRoot.transform.localRotation = Quaternion.identity;
			RootObject = sceneRoot;
			if (GeometryRoot != null)
			{
				DestructionEffects = GeometryRoot.GetComponentInChildren<VesselDestructionEffects>(includeInactive: true);
				if (DestructionEffects != null)
				{
					DestructionEffects.gameObject.SetActive(value: false);
				}
			}
			if (TargetRotation.HasValue)
			{
				SetTargetPositionAndRotation(null, TargetRotation.Value, instant: true);
			}
			sceneRoot.GetComponentInParent<SpaceObjectVessel>().ActivateGeometry = true;
			Client.Instance.ActiveVessels[base.GUID] = this;
			sceneRoot.SetActive(value: true);
			sceneRoot.SetActive(value: true);
			StructureScene sscene = sceneRoot.GetComponent<StructureScene>();
			Mass = sscene.Mass * 1000f;
			List<StructureSceneConnection> sceneConnections = new List<StructureSceneConnection>(sceneRoot.GetComponentsInChildren<StructureSceneConnection>());
			List<long> connectedConnections = new List<long>();
			sceneRoot.transform.localPosition = Vector3.zero;
			SceneHelper.FillAttachPoints(this, sceneRoot, AttachPoints, shipObjects?.AttachPoints);
			SceneHelper.FillSubSystems(sceneRoot, SubSystems, shipObjects?.SubSystems);
			SceneHelper.FillGenerators(sceneRoot, Generators, shipObjects?.Generators);
			SceneHelper.FillRoomTriggers(this, sceneRoot, RoomTriggers, shipObjects?.RoomTriggers);
			SceneHelper.FillResourceContainers(sceneRoot, ResourceContainers, shipObjects?.ResourceContainers);
			SceneHelper.FillDoors(this, sceneRoot, Doors, shipObjects?.Doors);
			SceneHelper.FillSceneTriggerExecuters(this, sceneRoot, SceneTriggerExecuters, shipObjects?.SceneTriggerExecuters);
			SceneHelper.FillSpawnWithChanceData(sceneRoot, shipObjects.SpawnWithChance);
			SceneHelper.FillSceneDockingPorts(this, sceneRoot, DockingPorts, shipObjects?.DockingPorts);
			SceneHelper.FillSpawnPoints(this, sceneRoot, SpawnPoints, shipObjects?.SpawnPoints);
			SceneHelper.FillCubemapProbes(sceneRoot);
			SceneHelper.FillNameTags(this, sceneRoot, NameTags, shipObjects?.NameTags);
			SceneHelper.FillRepairPoints(this, sceneRoot, RepairPoints, shipObjects?.RepairPoints);
			SceneHelper.CheckTags(sceneRoot, (VesselData == null) ? string.Empty : VesselData.Tag);
			SceneHelper.FillEmblems(sceneRoot, this);
			SceneHelper.FillDamagePoints(sceneRoot, this);
			VesselBaseSystem = sceneRoot.GetComponentInChildren<VesselBaseSystem>();
			RCS = sceneRoot.GetComponentInChildren<SubSystemRCS>();
			Engine = sceneRoot.GetComponentInChildren<SubSystemEngine>();
			FTLEngine = sceneRoot.GetComponentInChildren<SubSystemFTL>();
			Capacitor = sceneRoot.GetComponentInChildren<GeneratorCapacitor>();
			CargoBay = sceneRoot.GetComponentInChildren<SceneCargoBay>();
			NavPanel = sceneRoot.GetComponentInChildren<NavigationPanel>(includeInactive: true);
			base.RadarSystem = sceneRoot.GetComponentInChildren<SubSystemRadar>();
			SecuritySystem = sceneRoot.GetComponentInChildren<SecuritySystem>(includeInactive: true);
			base.MaxHealth = sscene.MaxHealth;
			DamagePointEffectFrequency = sscene.DamageEffectsFrequency;
			foreach (StructureSceneConnection item in sceneConnections)
			{
				bool showEnabled = connectedConnections.Contains(item.InSceneID);
				item.ToggleObjects(showEnabled);
			}
		}

		public void ActivateDamagePoints()
		{
			float damagePointsFrequency = GetDamagePointsFrequency();
			if (damagePointsFrequency <= float.Epsilon)
			{
				return;
			}
			int num = MathHelper.Clamp((int)damagePointsFrequency, 1, int.MaxValue);
			int num2 = 0;
			foreach (DamagePointData item in DamagePoints.OrderBy((DamagePointData m) => MathHelper.RandomNextDouble()))
			{
				Vector3 vector = item.ParentTransform.position + item.Position;
				Collider[] source = Physics.OverlapSphere(vector, 0.05f);
				bool flag = source.FirstOrDefault((Collider m) => m.GetComponentInParent<SceneTriggerRoom>()) != null;
				bool flag2 = MyPlayer.Instance.IsInVesselHierarchy(base.MainVessel);
				if ((!item.UseOcclusion || flag2 == flag) && (vector - MyPlayer.Instance.transform.position).magnitude <= item.VisibilityThreshold)
				{
					Transform transform = item.ParentTransform.Find("DamagePointTemp");
					if (transform == null)
					{
						GameObject gameObject = new GameObject("DamagePointTemp");
						gameObject.Activate(value: false);
						gameObject.transform.parent = item.ParentTransform;
						gameObject.transform.Reset(resetScale: true);
						transform = gameObject.transform;
					}
					GameObject gameObject2 = UnityEngine.Object.Instantiate(item.Effects.OrderBy((GameObject m) => MathHelper.RandomNextDouble()).FirstOrDefault(), transform);
					gameObject2.transform.localPosition = item.Position;
					gameObject2.transform.localRotation = item.Rotation;
					gameObject2.transform.localScale = item.Scale;
					if (!flag)
					{
						SoundEffect componentInChildren = gameObject2.GetComponentInChildren<SoundEffect>(includeInactive: true);
						if (componentInChildren != null)
						{
							UnityEngine.Object.Destroy(componentInChildren);
						}
					}
					gameObject2.transform.parent = item.ParentTransform;
				}
				if (++num2 >= num)
				{
					break;
				}
			}
			this.Invoke(ActivateDamagePoints, MathHelper.RandomRange(0f, MathHelper.Clamp(1f / damagePointsFrequency, 1f, 10f)));
		}

		public IEnumerator LoadShipScenesCoroutine(bool isMainShip, VesselObjects shipObjects)
		{
			CreateArtificalRigidbody();
			if (isMainShip)
			{
				yield return StartCoroutine(LoadStructureScenes(VesselData.SceneID, GeometryRoot.transform, shipObjects));
				UpdateShipObjects(shipObjects, isInitialize: true);
			}
			else
			{
				Client.Instance.CanvasManager.ToggleBusyLoading(isActive: true);
				yield return StartCoroutine(LoadStructureScenes(VesselData.SceneID, GeometryRoot.transform, shipObjects));
				UpdateShipObjects(shipObjects, isInitialize: true);
				Client.Instance.CanvasManager.ToggleBusyLoading(isActive: false);
			}
			OnSceneLoaded();
			if (shipObjects != null && shipObjects.SecurityData != null && SecuritySystem != null)
			{
				SecuritySystem.ParseSecurityData(shipObjects.SecurityData);
			}
			if (!VesselData.CollidersCenterOffset.ToVector3().IsEpsilonEqual(Vector3.zero))
			{
				GeometryPlaceholder.transform.localPosition = -VesselData.CollidersCenterOffset.ToVector3();
				UpdateArtificialBodyPosition(updateChildren: false);
			}
			if (!isMainShip)
			{
				ToggleOptimization(optimizationEnabled: true);
			}
			if (shipObjectsLoadingQueue != null)
			{
				while (shipObjectsLoadingQueue.Count > 0)
				{
					UpdateShipObjects(shipObjectsLoadingQueue.Dequeue());
				}
			}
			base.IsDummyObject = false;
			base.SceneObjectsLoaded = true;
			ZeroOcclusion.CheckOcclusionFor(this, onlyCheckDistance: false);
		}

		private void DockUndockCompleted(bool isDock, bool isInitialize)
		{
			AllowOtherCharacterMovement();
			if (currDockDetails == null)
			{
				return;
			}
			if (OnDockCompleted != null && isDock)
			{
				OnDockCompleted(isInitialize);
				OnDockCompleted = null;
			}
			else if (OnUndockCompleted != null && !isDock)
			{
				OnUndockCompleted(isInitialize);
				OnUndockCompleted = null;
			}
			if (isDock)
			{
				if (currDockDetails.ExecutersMerge != null && currDockDetails.ExecutersMerge.Count > 0)
				{
					Ship ship = ((currDockDetails.ExecutersMerge[0].ParentTriggerID.VesselGUID != base.GUID) ? (Client.Instance.GetVessel(currDockDetails.ExecutersMerge[0].ParentTriggerID.VesselGUID) as Ship) : this);
					Ship ship2 = ((currDockDetails.ExecutersMerge[0].ChildTriggerID.VesselGUID != base.GUID) ? (Client.Instance.GetVessel(currDockDetails.ExecutersMerge[0].ChildTriggerID.VesselGUID) as Ship) : this);
					if (ship != null && ship.SceneObjectsLoaded && ship2 != null && ship2.SceneObjectsLoaded)
					{
						SceneTriggerExecuter sceneTriggerExecuter = null;
						SceneTriggerExecuter sceneTriggerExecuter2 = null;
						foreach (ExecuterMergeDetails item in currDockDetails.ExecutersMerge)
						{
							sceneTriggerExecuter = ship.GetStructureObject<SceneTriggerExecuter>(item.ParentTriggerID.InSceneID);
							sceneTriggerExecuter2 = ship2.GetStructureObject<SceneTriggerExecuter>(item.ChildTriggerID.InSceneID);
							if (sceneTriggerExecuter != null && sceneTriggerExecuter2 != null)
							{
								sceneTriggerExecuter.SetChild(sceneTriggerExecuter2, isInitialize);
							}
						}
					}
				}
			}
			else if (currDockDetails.ExecutersMerge != null && currDockDetails.ExecutersMerge.Count > 0)
			{
				Ship ship3 = ((currDockDetails.ExecutersMerge[0].ParentTriggerID.VesselGUID != base.GUID) ? (Client.Instance.GetVessel(currDockDetails.ExecutersMerge[0].ParentTriggerID.VesselGUID) as Ship) : this);
				if (ship3 != null && ship3.SceneObjectsLoaded)
				{
					SceneTriggerExecuter sceneTriggerExecuter3 = null;
					foreach (ExecuterMergeDetails item2 in currDockDetails.ExecutersMerge)
					{
						sceneTriggerExecuter3 = ship3.GetStructureObject<SceneTriggerExecuter>(item2.ParentTriggerID.InSceneID);
						if (sceneTriggerExecuter3 != null)
						{
							if (sceneTriggerExecuter3.ParentExecuter != null)
							{
								sceneTriggerExecuter3.ParentExecuter.SetChild(null, isInitialize);
							}
							else if (sceneTriggerExecuter3.ChildExecuter != null)
							{
								sceneTriggerExecuter3.SetChild(null, isInitialize);
							}
						}
					}
				}
			}
			if (currDockDetails.PairedDoors != null && currDockDetails.PairedDoors.Count > 0)
			{
				foreach (PairedDoorsDetails pairedDoor in currDockDetails.PairedDoors)
				{
					if (pairedDoor.DoorID == null)
					{
						continue;
					}
					SceneDoor sceneDoor = null;
					SceneTriggerRoom sceneTriggerRoom = null;
					SpaceObjectVessel vessel = Client.Instance.GetVessel(pairedDoor.DoorID.VesselGUID);
					if (vessel == null)
					{
						continue;
					}
					SceneDoor structureObject = vessel.GetStructureObject<SceneDoor>(pairedDoor.DoorID.InSceneID);
					if (structureObject == null || structureObject == null)
					{
						continue;
					}
					if (pairedDoor.PairedDoorID != null)
					{
						SpaceObjectVessel vessel2 = Client.Instance.GetVessel(pairedDoor.PairedDoorID.VesselGUID);
						if (vessel2 != null)
						{
							sceneDoor = vessel2.GetStructureObject<SceneDoor>(pairedDoor.PairedDoorID.InSceneID);
							if (sceneDoor != null)
							{
								if (sceneDoor.Room1 != null && sceneDoor.Room1.ParentVessel == vessel2)
								{
									sceneTriggerRoom = sceneDoor.Room1;
								}
								else if (sceneDoor.Room2 != null && sceneDoor.Room2.ParentVessel == vessel2)
								{
									sceneTriggerRoom = sceneDoor.Room2;
								}
							}
						}
					}
					if (pairedDoor.PairedDoorID == null || pairedDoor.DoorID == null)
					{
						continue;
					}
					Ship ship4 = ((pairedDoor.PairedDoorID.VesselGUID != base.GUID) ? (Client.Instance.GetVessel(pairedDoor.PairedDoorID.VesselGUID) as Ship) : this);
					Ship ship5 = ((pairedDoor.DoorID.VesselGUID != base.GUID) ? (Client.Instance.GetVessel(pairedDoor.DoorID.VesselGUID) as Ship) : this);
					if (!isDock)
					{
						continue;
					}
					try
					{
						SceneDoor structureObject2 = ship4.GetStructureObject<SceneDoor>(pairedDoor.PairedDoorID.InSceneID);
						SceneDoor structureObject3 = ship5.GetStructureObject<SceneDoor>(pairedDoor.DoorID.InSceneID);
						float num = Vector3.Distance(structureObject2.DoorPassageTrigger.transform.position, structureObject3.DoorPassageTrigger.transform.position);
						if (structureObject2.DockingDoorPatch != null)
						{
							UnityEngine.Object.Destroy(structureObject2.DockingDoorPatch);
						}
						if (structureObject3.DockingDoorPatch != null)
						{
							UnityEngine.Object.Destroy(structureObject3.DockingDoorPatch);
						}
						structureObject2.DockingDoorPatch = UnityEngine.Object.Instantiate(structureObject2.DoorPassageTrigger.gameObject, (structureObject2.Room1 != null) ? structureObject2.Room1.transform : ((!(structureObject2.Room2 != null)) ? null : structureObject2.Room2.transform));
						structureObject2.DockingDoorPatch.transform.position = structureObject2.DoorPassageTrigger.transform.position;
						structureObject2.DockingDoorPatch.transform.rotation = structureObject2.DoorPassageTrigger.transform.rotation;
						GameObject dockingDoorPatch = structureObject2.DockingDoorPatch;
						dockingDoorPatch.name = dockingDoorPatch.name + structureObject2.InSceneID + "_patch";
						BoxCollider component = structureObject2.DockingDoorPatch.GetComponent<BoxCollider>();
						component.size = new Vector3(component.size.x, component.size.y, (num + component.size.z) / 2f);
						component.center = new Vector3(component.center.x, component.center.y, component.center.z + num / 4f);
						component.enabled = true;
						structureObject2.DockingDoorPatch.AddComponent<SceneTriggerRoomSegment>().BaseRoom = ((structureObject2.Room1 != null) ? structureObject2.Room1 : ((!(structureObject2.Room2 != null)) ? null : structureObject2.Room2));
						structureObject2.DockingDoorPatch.tag = "Ignore";
						structureObject3.DockingDoorPatch = UnityEngine.Object.Instantiate(structureObject3.DoorPassageTrigger.gameObject, (structureObject3.Room1 != null) ? structureObject3.Room1.transform : ((!(structureObject3.Room2 != null)) ? null : structureObject3.Room2.transform));
						structureObject3.DockingDoorPatch.transform.position = structureObject3.DoorPassageTrigger.transform.position;
						structureObject3.DockingDoorPatch.transform.rotation = structureObject3.DoorPassageTrigger.transform.rotation;
						GameObject dockingDoorPatch2 = structureObject3.DockingDoorPatch;
						dockingDoorPatch2.name = dockingDoorPatch2.name + structureObject3.InSceneID + "_patch";
						component = structureObject3.DockingDoorPatch.GetComponent<BoxCollider>();
						component.size = new Vector3(component.size.x, component.size.y, (num + component.size.z) / 2f);
						component.center = new Vector3(component.center.x, component.center.y, component.center.z + num / 4f);
						component.enabled = true;
						structureObject3.DockingDoorPatch.AddComponent<SceneTriggerRoomSegment>().BaseRoom = ((structureObject3.Room1 != null) ? structureObject3.Room1 : ((!(structureObject3.Room2 != null)) ? null : structureObject3.Room2));
						structureObject3.DockingDoorPatch.tag = "Ignore";
					}
					catch
					{
					}
				}
			}
			if (!isDock)
			{
				SceneDockingPort structureObject4 = Client.Instance.GetVessel(currDockDetails.ID.VesselGUID).GetStructureObject<SceneDockingPort>(currDockDetails.ID.InSceneID);
				SceneDockingPort structureObject5 = Client.Instance.GetVessel(currDockDetails.DockedToID.VesselGUID).GetStructureObject<SceneDockingPort>(currDockDetails.DockedToID.InSceneID);
				if (structureObject4 != null)
				{
					foreach (SceneDoor door in structureObject4.Doors)
					{
						if (door.DockingDoorPatch != null)
						{
							UnityEngine.Object.Destroy(door.DockingDoorPatch);
						}
					}
				}
				if (structureObject5 != null)
				{
					foreach (SceneDoor door2 in structureObject5.Doors)
					{
						if (door2.DockingDoorPatch != null)
						{
							UnityEngine.Object.Destroy(door2.DockingDoorPatch);
						}
					}
				}
			}
			MyPlayer.Instance.EnableTransitionTrigger();
			if (isDock)
			{
				SetRcsThrustersCenterOfMass(this, resetIsOn: true);
			}
			else
			{
				Ship ship6 = ((currDockDetails.ID.VesselGUID != base.GUID) ? (Client.Instance.GetVessel(currDockDetails.ID.VesselGUID) as Ship) : this);
				Ship ship7 = ((currDockDetails.DockedToID.VesselGUID != base.GUID) ? (Client.Instance.GetVessel(currDockDetails.DockedToID.VesselGUID) as Ship) : this);
				if (ship6 != null)
				{
					SetRcsThrustersCenterOfMass(ship6, resetIsOn: true);
				}
				if (ship7 != null)
				{
					SetRcsThrustersCenterOfMass(ship7, resetIsOn: true);
				}
			}
			currDockDetails = null;
			if (MyPlayer.Instance.ShipControlMode == ShipControlMode.Docking)
			{
				Client.Instance.InGamePanels.Docking.UpdateDockingPorts();
			}
		}

		private static void SetRcsThrustersCenterOfMass(Ship sh, bool resetIsOn)
		{
			Ship ship = sh;
			if (sh.DockedToMainVessel != null)
			{
				ship = sh.DockedToMainVessel as Ship;
			}
			if (!(ship != null))
			{
				return;
			}
			if (ship.rcsThrusters != null)
			{
				ship.rcsThrusters.CenterOfMass = ship.transform;
			}
			foreach (Ship allDockedVessel in ship.AllDockedVessels)
			{
				if (allDockedVessel != null && allDockedVessel.rcsThrusters != null)
				{
					allDockedVessel.rcsThrusters.CenterOfMass = ship.transform;
				}
			}
		}

		public void DockToShip(SceneDockingPort myPort, Ship dockToShip, SceneDockingPort dockToPort, SceneDockingPortDetails details, bool isInitialize)
		{
			currDockDetails = details;
			SpaceObjectVessel spaceObjectVessel = ((!dockToShip.IsDocked) ? dockToShip : dockToShip.DockedToMainVessel);
			spaceObjectVessel.RecreateDockedVesselsTree();
			if (MyPlayer.Instance.Parent is Ship && (MyPlayer.Instance.Parent == DockedToMainVessel || DockedToMainVessel.AllDockedVessels.Contains(MyPlayer.Instance.Parent as SpaceObjectVessel)))
			{
				MyPlayer.Instance.DisableTransitionTrigger();
			}
			if (details.RelativePositionUpdate != null && details.RelativeRotationUpdate != null)
			{
				foreach (KeyValuePair<long, float[]> item in details.RelativePositionUpdate)
				{
					SpaceObjectVessel vessel = Client.Instance.GetVessel(item.Key);
					vessel.RelativePosition = item.Value.ToVector3();
					vessel.RelativeRotation = details.RelativeRotationUpdate[item.Key].ToQuaternion();
				}
			}
			if (isInitialize && details.RelativePosition != null && details.RelativeRotation != null)
			{
				RelativePosition = details.RelativePosition.ToVector3();
				RelativeRotation = details.RelativeRotation.ToQuaternion();
			}
			base.transform.parent = dockToPort.transform;
			foreach (SpaceObjectVessel allDockedVessel in DockedToMainVessel.AllDockedVessels)
			{
				if (allDockedVessel != this && allDockedVessel.transform.parent != allDockedVessel.DockedToVessel.ConnectedObjectsRoot.transform)
				{
					allDockedVessel.transform.parent = allDockedVessel.DockedToVessel.ConnectedObjectsRoot.transform;
				}
			}
			Ship ship = DockedToMainVessel as Ship;
			Vector3 vector = ship.VesselData.CollidersCenterOffset.ToVector3() - details.CollidersCenterOffset.ToVector3();
			Vector3 vector2 = Quaternion.LookRotation(ship.Forward, ship.Up) * vector;
			ship.ModifyPositionAndRotation(-vector2);
			ship.VesselData.CollidersCenterOffset = details.CollidersCenterOffset;
			ship.GeometryPlaceholder.transform.localPosition = -ship.VesselData.CollidersCenterOffset.ToVector3();
			ship.ConnectedObjectsRoot.transform.localPosition = -ship.VesselData.CollidersCenterOffset.ToVector3();
			base.transform.position += GeometryPlaceholder.transform.localPosition;
			GeometryPlaceholder.transform.localPosition = Vector3.zero;
			ship.UpdateArtificialBodyPosition(updateChildren: false);
			foreach (SpaceObjectVessel allDockedVessel2 in DockedToMainVessel.AllDockedVessels)
			{
				Ship ship2 = allDockedVessel2 as Ship;
				ship2.GeometryPlaceholder.transform.localPosition = Vector3.zero;
				if (allDockedVessel2 != this)
				{
					ship2.transform.localPosition = ship2.RelativePosition;
					ship2.transform.localRotation = ship2.RelativeRotation;
					ship2.UpdateArtificialBodyPosition(updateChildren: false);
				}
				ship2.ConnectedObjectsRoot.transform.Reset();
			}
			base.transform.parent = dockToShip.ConnectedObjectsRoot.transform;
			DockedToMainVessel.Orbit.RelativePosition -= vector2.ToVector3D();
			DockedToMainVessel.Orbit.InitFromCurrentStateVectors(Client.Instance.SolarSystem.CurrentTime);
			if (MyPlayer.Instance.Parent is Ship && (MyPlayer.Instance.Parent == DockedToMainVessel || DockedToMainVessel.AllDockedVessels.Contains(MyPlayer.Instance.Parent as SpaceObjectVessel)))
			{
				Vector3 localPosition = DockedToMainVessel.transform.localPosition;
				DockedToMainVessel.transform.SetParent(null);
				DockedToMainVessel.SetTargetPositionAndRotation(Vector3.zero, DockedToMainVessel.Forward, DockedToMainVessel.Up, instant: true);
				DockedToMainVessel.transform.Reset();
				MyPlayer.Instance.SendDockUndockMsg = true;
				foreach (ArtificialBody artificialBody in Client.Instance.SolarSystem.ArtificialBodies)
				{
					if (artificialBody != Parent && artificialBody != DockedToMainVessel && (!(artificialBody is SpaceObjectVessel) || (artificialBody as SpaceObjectVessel).DockedToMainVessel == null))
					{
						artificialBody.ModifyPositionAndRotation(-localPosition);
					}
				}
				Client.Instance.SolarSystem.CenterPlanets();
				MyPlayer.Instance.UpdateCameraPositions();
			}
			if (!isInitialize)
			{
				StartCoroutine(LerpDock(RelativePosition, RelativeRotation));
			}
			else
			{
				if (MyPlayer.Instance.Parent is Ship && (MyPlayer.Instance.Parent == DockedToMainVessel || DockedToMainVessel.AllDockedVessels.Contains(MyPlayer.Instance.Parent as SpaceObjectVessel)))
				{
					Ship ship3 = MyPlayer.Instance.Parent as Ship;
					base.transform.localPosition = RelativePosition;
					base.transform.localRotation = RelativeRotation;
					SetTargetPositionAndRotation(base.transform.localPosition, base.transform.forward, base.transform.up, instant: true);
					MyPlayer.Instance.SendDockUndockMsg = true;
				}
				else
				{
					base.transform.localPosition = RelativePosition;
					base.transform.localRotation = RelativeRotation;
					SetTargetPositionAndRotation(base.transform.localPosition, base.transform.forward, base.transform.up, instant: true);
				}
				UpdateArtificialBodyPosition(updateChildren: false);
				DockUndockCompleted(isDock: true, isInitialize: true);
				ZeroOcclusion.CheckOcclusionFor(base.MainVessel, onlyCheckDistance: false);
				foreach (SpaceObjectVessel allDockedVessel3 in base.MainVessel.AllDockedVessels)
				{
					ZeroOcclusion.CheckOcclusionFor(allDockedVessel3, onlyCheckDistance: false);
				}
				if (base.IsDocked && DockedToMainVessel.IsSubscribedTo)
				{
					Subscribe();
				}
			}
			UpdateMapObjects(dockToShip);
			if (RCS != null)
			{
				RCS.Status = SystemStatus.Offline;
			}
			if (dockToShip.RCS != null)
			{
				dockToShip.RCS.Status = SystemStatus.Offline;
			}
		}

		private IEnumerator LerpDock(Vector3 targetPos, Quaternion targetRot)
		{
			Vector3 startingPosition = base.transform.localPosition;
			Quaternion startingRotation = base.transform.localRotation;
			lerpTimer = 0f;
			while (lerpTimer < 1f)
			{
				if (MyPlayer.Instance.Parent is Ship && (MyPlayer.Instance.Parent == DockedToMainVessel || DockedToMainVessel.AllDockedVessels.Contains(MyPlayer.Instance.Parent as SpaceObjectVessel)))
				{
					base.transform.localPosition = Vector3.Lerp(startingPosition, targetPos, Mathf.SmoothStep(0f, 1f, lerpTimer));
					base.transform.localRotation = Quaternion.Lerp(startingRotation, targetRot, Mathf.SmoothStep(0f, 1f, lerpTimer));
					SetTargetPositionAndRotation(base.transform.localPosition, base.transform.forward, base.transform.up, instant: true);
				}
				else
				{
					base.transform.localPosition = Vector3.Lerp(startingPosition, targetPos, Mathf.SmoothStep(0f, 1f, lerpTimer));
					base.transform.localRotation = Quaternion.Lerp(startingRotation, targetRot, Mathf.SmoothStep(0f, 1f, lerpTimer));
					SetTargetPositionAndRotation(base.transform.localPosition, base.transform.forward, base.transform.up, instant: true);
				}
				UpdateArtificialBodyPosition(updateChildren: false);
				lerpTimer += Time.deltaTime * 0.5f;
				yield return new WaitForEndOfFrame();
			}
			if (MyPlayer.Instance.Parent is Ship && (MyPlayer.Instance.Parent == DockedToMainVessel || DockedToMainVessel.AllDockedVessels.Contains(MyPlayer.Instance.Parent as SpaceObjectVessel)))
			{
				base.transform.localPosition = RelativePosition;
				base.transform.localRotation = RelativeRotation;
			}
			else
			{
				base.transform.localPosition = RelativePosition;
				base.transform.localRotation = RelativeRotation;
			}
			DockUndockCompleted(isDock: true, isInitialize: false);
		}

		public void UndockFromShip(SceneDockingPort myPort, Ship dockedToShip, SceneDockingPort dockedToPort, SceneDockingPortDetails details)
		{
			if (myPort == null || dockedToPort == null || !dockedToShip)
			{
				return;
			}
			currDockDetails = details;
			myPort.LeverPulse = false;
			dockedToPort.LeverPulse = false;
			if (OnUndockStarted != null)
			{
				OnUndockStarted(isInitialize: false);
				OnUndockStarted = null;
			}
			DockedVessels.Remove(dockedToShip);
			dockedToShip.DockedVessels.Remove(this);
			DockedToVessel = null;
			dockedToShip.DockedToVessel = null;
			SpaceObjectVessel spaceObjectVessel = ((!(DockedToMainVessel != null)) ? dockedToShip.DockedToMainVessel : DockedToMainVessel);
			Vector3 position = spaceObjectVessel.transform.position;
			Quaternion rotation = spaceObjectVessel.transform.rotation;
			Vector3 vector = (spaceObjectVessel as Ship).VesselData.CollidersCenterOffset.ToVector3();
			Quaternion value = Quaternion.LookRotation(spaceObjectVessel.Forward, spaceObjectVessel.Up);
			spaceObjectVessel.ResetDockedToVessel();
			SpaceObjectVessel vessel = Client.Instance.GetVessel(details.VesselOrbit.GUID.Value);
			SpaceObjectVessel vessel2 = Client.Instance.GetVessel(details.VesselOrbitOther.GUID.Value);
			vessel.RecreateDockedVesselsTree();
			vessel2.RecreateDockedVesselsTree();
			vessel.transform.SetParent(Client.Instance.ShipExteriorRoot.transform);
			vessel2.transform.SetParent(Client.Instance.ShipExteriorRoot.transform);
			Vector3 vector2 = Vector3.zero;
			Vector3 vector3 = Vector3.zero;
			if (details.RelativePositionUpdate != null && details.RelativeRotationUpdate != null)
			{
				foreach (KeyValuePair<long, float[]> item in details.RelativePositionUpdate)
				{
					SpaceObjectVessel vessel3 = Client.Instance.GetVessel(item.Key);
					if (item.Key == vessel.GUID)
					{
						Quaternion value2 = details.RelativeRotationUpdate[item.Key].ToQuaternion();
						vessel3.RelativePosition = Vector3.zero;
						vessel3.RelativeRotation = Quaternion.identity;
						vessel3.SetTargetPositionAndRotation(null, value2, instant: true);
						vector2 = details.RelativePositionUpdate[item.Key].ToVector3();
					}
					else if (item.Key == vessel2.GUID)
					{
						Quaternion value3 = details.RelativeRotationUpdate[item.Key].ToQuaternion();
						vessel3.RelativePosition = Vector3.zero;
						vessel3.RelativeRotation = Quaternion.identity;
						vessel3.SetTargetPositionAndRotation(null, value3, instant: true);
						vector3 = details.RelativePositionUpdate[item.Key].ToVector3();
					}
					else
					{
						vessel3.RelativePosition = item.Value.ToVector3();
						vessel3.RelativeRotation = details.RelativeRotationUpdate[item.Key].ToQuaternion();
					}
				}
			}
			foreach (SpaceObjectVessel allDockedVessel in vessel.AllDockedVessels)
			{
				if (allDockedVessel != this && allDockedVessel.transform.parent != allDockedVessel.DockedToVessel.ConnectedObjectsRoot.transform)
				{
					allDockedVessel.transform.parent = allDockedVessel.DockedToVessel.ConnectedObjectsRoot.transform;
				}
			}
			foreach (SpaceObjectVessel allDockedVessel2 in vessel2.AllDockedVessels)
			{
				if (allDockedVessel2 != this && allDockedVessel2.transform.parent != allDockedVessel2.DockedToVessel.ConnectedObjectsRoot.transform)
				{
					allDockedVessel2.transform.parent = allDockedVessel2.DockedToVessel.ConnectedObjectsRoot.transform;
				}
			}
			vessel.VesselData.CollidersCenterOffset = details.CollidersCenterOffset.ToArray();
			vessel.GeometryPlaceholder.transform.localPosition = -vessel.VesselData.CollidersCenterOffset.ToVector3();
			vessel.ConnectedObjectsRoot.transform.localPosition = -vessel.VesselData.CollidersCenterOffset.ToVector3();
			vessel.Orbit.ParseNetworkData(details.VesselOrbit, resetOrbit: true);
			vessel.UpdateArtificialBodyPosition(updateChildren: false);
			foreach (SpaceObjectVessel allDockedVessel3 in vessel.AllDockedVessels)
			{
				Ship ship = allDockedVessel3 as Ship;
				ship.GeometryPlaceholder.transform.localPosition = Vector3.zero;
				ship.transform.localPosition = ship.RelativePosition;
				ship.transform.localRotation = ship.RelativeRotation;
				ship.ConnectedObjectsRoot.transform.Reset();
				ship.UpdateArtificialBodyPosition(updateChildren: false);
			}
			vessel2.VesselData.CollidersCenterOffset = details.CollidersCenterOffsetOther.ToArray();
			vessel2.GeometryPlaceholder.transform.localPosition = -vessel2.VesselData.CollidersCenterOffset.ToVector3();
			vessel2.ConnectedObjectsRoot.transform.localPosition = -vessel2.VesselData.CollidersCenterOffset.ToVector3();
			vessel2.Orbit.ParseNetworkData(details.VesselOrbitOther, resetOrbit: true);
			vessel2.UpdateArtificialBodyPosition(updateChildren: false);
			foreach (SpaceObjectVessel allDockedVessel4 in vessel2.AllDockedVessels)
			{
				Ship ship2 = allDockedVessel4 as Ship;
				ship2.GeometryPlaceholder.transform.localPosition = Vector3.zero;
				ship2.transform.localPosition = ship2.RelativePosition;
				ship2.transform.localRotation = ship2.RelativeRotation;
				ship2.ConnectedObjectsRoot.transform.Reset();
				ship2.UpdateArtificialBodyPosition(updateChildren: false);
			}
			vessel.transform.position = position + rotation * (-vector + vector2 + value.Inverse() * Quaternion.LookRotation(vessel.Forward, vessel.Up) * vessel.VesselData.CollidersCenterOffset.ToVector3());
			vessel2.transform.position = position + rotation * (-vector + vector3 + value.Inverse() * Quaternion.LookRotation(vessel2.Forward, vessel2.Up) * vessel2.VesselData.CollidersCenterOffset.ToVector3());
			vessel.SetTargetPositionAndRotation(vessel.transform.localPosition, null, instant: true);
			vessel2.SetTargetPositionAndRotation(vessel2.transform.localPosition, null, instant: true);
			if (MyPlayer.Instance.Parent is Ship)
			{
				Ship ship3 = MyPlayer.Instance.Parent as Ship;
				if (ship3 == vessel || vessel.AllDockedVessels.Contains(ship3))
				{
					Vector3 localPosition = vessel.transform.localPosition;
					vessel.transform.SetParent(null);
					vessel.SetTargetPositionAndRotation(Vector3.zero, vessel.Forward, vessel.Up, instant: true);
					vessel.transform.Reset();
					MyPlayer.Instance.SendDockUndockMsg = true;
					foreach (ArtificialBody artificialBody in Client.Instance.SolarSystem.ArtificialBodies)
					{
						if (artificialBody != Parent && artificialBody != vessel && (!(artificialBody is SpaceObjectVessel) || (artificialBody as SpaceObjectVessel).DockedToMainVessel == null))
						{
							artificialBody.ModifyPositionAndRotation(-localPosition);
						}
					}
				}
				else if (ship3 == vessel2 || vessel2.AllDockedVessels.Contains(ship3))
				{
					Vector3 localPosition2 = vessel2.transform.localPosition;
					vessel2.transform.SetParent(null);
					vessel2.SetTargetPositionAndRotation(Vector3.zero, vessel2.Forward, vessel2.Up, instant: true);
					vessel2.transform.Reset();
					MyPlayer.Instance.SendDockUndockMsg = true;
					foreach (ArtificialBody artificialBody2 in Client.Instance.SolarSystem.ArtificialBodies)
					{
						if (artificialBody2 != Parent && artificialBody2 != vessel2 && (!(artificialBody2 is SpaceObjectVessel) || (artificialBody2 as SpaceObjectVessel).DockedToMainVessel == null))
						{
							artificialBody2.ModifyPositionAndRotation(-localPosition2);
						}
					}
				}
			}
			Client.Instance.SolarSystem.CenterPlanets();
			MyPlayer.Instance.UpdateCameraPositions();
			DockUndockCompleted(isDock: false, isInitialize: false);
			ZeroOcclusion.CheckOcclusionFor(base.MainVessel, onlyCheckDistance: false);
			foreach (SpaceObjectVessel allDockedVessel5 in base.MainVessel.AllDockedVessels)
			{
				ZeroOcclusion.CheckOcclusionFor(allDockedVessel5, onlyCheckDistance: false);
			}
			ZeroOcclusion.CheckOcclusionFor(dockedToShip.MainVessel, onlyCheckDistance: false);
			foreach (SpaceObjectVessel allDockedVessel6 in dockedToShip.MainVessel.AllDockedVessels)
			{
				ZeroOcclusion.CheckOcclusionFor(allDockedVessel6, onlyCheckDistance: false);
			}
			UpdateMapObjects(dockedToShip);
		}

		private void UpdateMapObjects(Ship dockedToShip)
		{
			if (Client.Instance.Map.AllMapObjects.TryGetValue(this, out var value) && value != null)
			{
				value.gameObject.SetActive(base.IsMainVessel);
			}
			if (Client.Instance.Map.AllMapObjects.TryGetValue(dockedToShip, out var value2) && value2 != null)
			{
				value2.gameObject.SetActive(dockedToShip.IsMainVessel);
			}
		}

		public override void OnSceneLoaded()
		{
			base.OnSceneLoaded();
			ConnectMessageListeners();
			engineThrusters = GeometryRoot.GetComponentInChildren<EngineThrusters>(includeInactive: true);
			if (engineThrusters != null)
			{
				engineThrusters.OnOff = base.EngineOnLine;
			}
			warpEffect = GeometryRoot.GetComponentInChildren<WarpEffect>(includeInactive: true);
			if (warpEffect != null)
			{
				warpEffect.SetActive(base.IsWarpOnline, instant: true);
			}
			warpInductorExecuter = GeometryRoot.GetComponentInChildren<WarpInductorExecuter>(includeInactive: true);
			if (warpInductorExecuter != null)
			{
				warpInductorExecuter.ToggleInductor(isActive: false, isInstant: false);
			}
			warpStartEffect = GeometryRoot.GetComponentInChildren<WarpStartEffect>(includeInactive: true);
			if (warpStartEffect != null)
			{
				warpStartEffect.gameObject.Activate(value: false);
			}
			warpEndEffect = GeometryRoot.GetComponentInChildren<WarpEndEffect>(includeInactive: true);
			if (warpEndEffect != null)
			{
				warpEndEffect.gameObject.Activate(value: false);
			}
			if (SecuritySystem != null)
			{
				SecuritySystem.UpdateUI();
			}
			refuelingStation = GeometryRoot.GetComponentInChildren<RefuelingStationUI>(includeInactive: true);
			rcsThrusters = GeometryRoot.GetComponentInChildren<RCSThrusters>(includeInactive: true);
			if (rcsThrusters != null && !base.IsDocked)
			{
				rcsThrusters.CenterOfMass = base.transform;
			}
			OptimizationColliders = new List<Collider>();
			DontOptimizeColliders = new List<Collider>();
			Collider[] componentsInChildren = GeometryRoot.GetComponentsInChildren<Collider>();
			foreach (Collider collider in componentsInChildren)
			{
				if (collider.enabled && !collider.CompareTag("Ignore") && !(collider.GetComponent<SceneDockingTrigger>() != null) && (!(collider.GetComponentInParent<BaseSceneAttachPoint>() != null) || !(collider.GetComponentInParent<BaseSceneAttachPoint>().Collider == collider)))
				{
					if (!collider.CompareTag("DontOptimize"))
					{
						OptimizationColliders.Add(collider);
					}
					else
					{
						DontOptimizeColliders.Add(collider);
					}
				}
			}
			ZeroOcclusion.AddOccludersFrom(this);
			if (VesselData == null || !(VesselData.CreationSolarSystemTime + 30.0 > Client.Instance.SolarSystem.CurrentTime) || !SceneHelper.CompareTags(VesselData.Tag, "_RescueVessel") || MyPlayer.Instance.IsInVesselHierarchy(this) || !(warpEndEffect != null))
			{
				return;
			}
			WarpEndEffectTask = new Task(delegate
			{
				if (!base.IsDummyObject)
				{
					ActivateWarpEndEffect();
					WarpEndEffectTask = null;
				}
			});
		}

		public override void OnStabilizationChanged(bool isStabilized)
		{
			if (!base.IsMainVessel)
			{
				return;
			}
			foreach (SpaceObjectVessel allDockedVessel in base.MainVessel.AllDockedVessels)
			{
				allDockedVessel.OnStabilizationChanged(isStabilized);
			}
		}

		public override bool IsPlayerAuthorized(Player pl)
		{
			return IsPlayerAuthorized(pl, new HashSet<SpaceObjectVessel>(), new HashSet<SecuritySystem>());
		}

		public override bool IsPlayerAuthorizedOrNoSecurity(Player pl)
		{
			HashSet<SecuritySystem> hashSet = new HashSet<SecuritySystem>();
			return IsPlayerAuthorized(pl, new HashSet<SpaceObjectVessel>(), hashSet) || hashSet.Count == 0;
		}

		public override bool IsPlayerAuthorizedOrFreeSecurity(Player pl)
		{
			HashSet<SecuritySystem> hashSet = new HashSet<SecuritySystem>();
			return IsPlayerAuthorized(pl, new HashSet<SpaceObjectVessel>(), hashSet) || hashSet.Count == 0 || hashSet.Count((SecuritySystem m) => m.AuthorizedPlayers.Count == 0) == hashSet.Count;
		}

		private bool IsPlayerAuthorized(Player pl, HashSet<SpaceObjectVessel> traversedVessels, HashSet<SecuritySystem> securitySystems)
		{
			if (!traversedVessels.Add(this))
			{
				return false;
			}
			if (SecuritySystem != null)
			{
				securitySystems.Add(SecuritySystem);
			}

			// Player is authorised on this ship.
			bool flag = SecuritySystem != null && SecuritySystem.AuthorizedPlayers.Find((AuthorizedPerson m) => m.PlayerId == pl.PlayerId) != null;
			if (GameScenes.Ranges.IsShip(base.SceneID) || flag)
			{
				return flag;
			}

			// Player is authorised in docked vessel.
			if (DockedToVessel != null && DockedToVessel is Ship && !GameScenes.Ranges.IsShip(DockedToVessel.SceneID) && (DockedToVessel as Ship).IsPlayerAuthorized(pl, traversedVessels, securitySystems))
			{
				return true;
			}
			foreach (SpaceObjectVessel dockedVessel in DockedVessels)
			{
				if (dockedVessel is Ship && !GameScenes.Ranges.IsShip(dockedVessel.SceneID) && (dockedVessel as Ship).IsPlayerAuthorized(pl, traversedVessels, securitySystems))
				{
					return true;
				}
			}
			return false;
		}

		public void ProximityCanvasCheck()
		{
			if (!base.IsDummyObject)
			{
				Canvas[] componentsInChildren = GetComponentsInChildren<Canvas>(includeInactive: true);
				foreach (Canvas canvas in componentsInChildren)
				{
					float magnitude = (MyPlayer.Instance.transform.position - canvas.transform.position).magnitude;
					canvas.gameObject.Activate(magnitude < MyPlayer.Instance.HideCanvasDistance);
				}
				DeferredDecal[] componentsInChildren2 = GetComponentsInChildren<DeferredDecal>(includeInactive: true);
				foreach (DeferredDecal deferredDecal in componentsInChildren2)
				{
					float magnitude2 = (MyPlayer.Instance.transform.position - deferredDecal.transform.position).magnitude;
					deferredDecal.enabled = magnitude2 < MyPlayer.Instance.HideCanvasDistance;
				}
				Decalicious[] componentsInChildren3 = GetComponentsInChildren<Decalicious>(includeInactive: true);
				foreach (Decalicious decalicious in componentsInChildren3)
				{
					float magnitude3 = (MyPlayer.Instance.transform.position - decalicious.transform.position).magnitude;
					decalicious.enabled = magnitude3 < MyPlayer.Instance.HideCanvasDistance;
				}
			}
		}
	}
}
