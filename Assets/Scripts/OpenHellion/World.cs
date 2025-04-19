// World.cs
//
// Copyright (C) 2024, OpenHellion contributors
//
// SPDX-License-Identifier: GPL-3.0-or-later
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenHellion.Net;
using OpenHellion.Social;
using OpenHellion.Social.RichPresence;
using OpenHellion.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using ZeroGravity;
using ZeroGravity.Data;
using ZeroGravity.Effects;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;
using Cysharp.Threading.Tasks;

namespace OpenHellion
{
	/// <summary>
	/// 	This is the main component of the game. Is the whole game manager when in world scene. It handles everything from saving to managing parts of the GUI.<br/>
	/// </summary>
	/// <seealso cref="MyPlayer"/>
	[RequireComponent(typeof(SolarSystem))]
	public class World : MonoBehaviour
	{
		[Title("Config")]
		public float RCS_THRUST_SENSITIVITY = 0.5f;

		public float RCS_ROTATION_SENSITIVITY = 5f;

		public static double CELESTIAL_BODY_RADIUS_MULTIPLIER = 1.0;

		public static float DROP_THRESHOLD = 0.2f;

		public static float DROP_MIN_FORCE = 0f;

		public static float DROP_MAX_FORCE = 8f;

		public static float DROP_MAX_TIME = 3f;

		public static float VESSEL_ROTATION_LERP_VALUE = 0.9f;

		public static bool VESSEL_ROTATION_LERP_UNCLAMPED = false;

		public static float VESSEL_TRANSLATION_LERP_VALUE = 0.8f;

		public static bool VESSEL_TRANSLATION_LERP_UNCLAMPED = false;

		public List<DebrisField> DebrisFields = new List<DebrisField>();

		public Texture2D DefaultCursor;

		[Title("Object references")]
		private SolarSystem _solarSystem;

		public GameObject SolarSystemRoot;

		public GameObject ShipExteriorRoot;

		public Transform SunCameraRootTransform;

		public Transform SunCameraTransform;

		public Transform PlanetsCameraRootTransform;

		public Transform PlanetsCameraTransform;

		public Transform ShipSunLightTransform;

		public Transform PlanetsRootTransform;

		public Transform PlanetsSunLightTransform;

		public RenderToCubeMap CubemapRenderer;

		public Map Map;

		public DebrisFieldEffect DebrisEffect;

		public EffectPrefabs EffectPrefabs;

		public QuestCollectionObject QuestCollection;

		private volatile bool _logoutRequestSent;

		[NonSerialized] public readonly Dictionary<long, OtherPlayer> Players = new Dictionary<long, OtherPlayer>();

		[NonSerialized]
		public readonly Dictionary<long, DynamicObject> DynamicObjects = new Dictionary<long, DynamicObject>();

		[NonSerialized] public readonly Dictionary<long, Corpse> Corpses = new Dictionary<long, Corpse>();

		[NonSerialized]
		public readonly Dictionary<long, SpaceObjectVessel> ActiveVessels = new Dictionary<long, SpaceObjectVessel>();

		[NonSerialized] public List<ItemIngredientsData> ItemsIngredients;

		[NonSerialized] public List<Quest> Quests = new List<Quest>();

		// TODO: Move to correct class.
		[NonSerialized] public bool OffSpeedHelper = true;

		private bool _openMainSceneStarted;

		[NonSerialized] public readonly Dictionary<long, CharacterInteractionState> CharacterInteractionStatesQueue =
			new Dictionary<long, CharacterInteractionState>();

		[NonSerialized] public DateTime? ServerRestartTime;

		// Can't these two be done with the underlying network transport?
		private float _lastLatencyMessageTime = -1f;
		private volatile int _latencyMs;

		public static volatile int MainThreadID;

		private Task _restoreMapDetailsTask;

		public double ExposureRange;

		private float[] _vesselExposureValues;

		private float[] _playerExposureValues;

		[NonSerialized] public Action LoadingFinishedDelegate;

		[NonSerialized] public bool IsChatOpened;

		public InWorldPanels InWorldPanels;

		public InGameGUI InGameGUI;

		public static int DefaultLayerMask => 1 << LayerMask.NameToLayer("Default");

		public SolarSystem SolarSystem => _solarSystem;

		public int LatencyMs
		{
			get
			{
				if (_lastLatencyMessageTime < 0f)
				{
					return 0;
				}

				float num = Time.realtimeSinceStartup - _lastLatencyMessageTime;
				if (_latencyMs < 0 || num > 5f)
				{
					return (int)(num * 1000f);
				}

				return _latencyMs;
			}
		}


		private void Awake()
		{
			RCS_THRUST_SENSITIVITY = Properties.GetProperty("rcs_thrust_sensitivity", RCS_THRUST_SENSITIVITY);
			RCS_ROTATION_SENSITIVITY = Properties.GetProperty("rcs_rotation_sensitivity", RCS_ROTATION_SENSITIVITY);
			CELESTIAL_BODY_RADIUS_MULTIPLIER =
				Properties.GetProperty("celestial_body_radius_multiplier", CELESTIAL_BODY_RADIUS_MULTIPLIER);
			DROP_THRESHOLD = Properties.GetProperty("drop_threshold", DROP_THRESHOLD);
			DROP_MIN_FORCE = Properties.GetProperty("drop_min_force", DROP_MIN_FORCE);
			DROP_MAX_FORCE = Properties.GetProperty("drop_max_force", DROP_MAX_FORCE);
			DROP_MAX_TIME = Properties.GetProperty("drop_max_time", DROP_MAX_TIME);
			VESSEL_ROTATION_LERP_VALUE =
				Properties.GetProperty("vessel_rotation_lerp_value", VESSEL_ROTATION_LERP_VALUE);
			VESSEL_ROTATION_LERP_UNCLAMPED =
				Properties.GetProperty("vessel_rotation_lerp_unclamped", VESSEL_ROTATION_LERP_UNCLAMPED);
			VESSEL_TRANSLATION_LERP_VALUE =
				Properties.GetProperty("vessel_translation_lerp_value", VESSEL_TRANSLATION_LERP_VALUE);
			VESSEL_TRANSLATION_LERP_UNCLAMPED = Properties.GetProperty("vessel_translation_lerp_unclamped",
				VESSEL_TRANSLATION_LERP_UNCLAMPED);

			StaticData.LoadData();
			Application.runInBackground = true;
			MainThreadID = Thread.CurrentThread.ManagedThreadId;
			_openMainSceneStarted = false;
			_solarSystem = GetComponent<SolarSystem>();

			Texture[] emblems = Resources.LoadAll<Texture>("Emblems");
			SceneVesselEmblem.Textures = emblems.ToDictionary(x => x.name, y => y);

			Globals.Instance.OnHellionQuit = () =>
			{
				OnDestroy();
			};
		}

		private void Start()
		{
			InWorldPanels.LocalizePanels();

			EventSystem.AddListener(typeof(KillPlayerMessage), KillPlayerMessageListener);
			EventSystem.AddListener(typeof(LogOutResponse), LogOutResponseListener);
			EventSystem.AddListener(typeof(DestroyObjectMessage), DestroyObjectMessageListener);
			EventSystem.AddListener(typeof(SpawnObjectsResponse), SpawnObjectsResponseListener);
			EventSystem.AddListener(typeof(MovementMessage), MovementMessageListener);
			EventSystem.AddListener(typeof(DynamicObjectsInfoMessage), DynamicObjectsInfoMessageListener);
			EventSystem.AddListener(typeof(PlayersOnServerResponse), PlayersOnServerResponseListener);
			EventSystem.AddListener(typeof(ConsoleMessage), ConsoleMessageListener);
			EventSystem.AddListener(typeof(ShipCollisionMessage), ShipCollisionMessageListener);
			EventSystem.AddListener(typeof(UpdateVesselDataMessage), UpdateVesselDataMessageListener);

			Settings.LoadSettings(Settings.SettingsType.Game);
		}

		private void DynamicObjectsInfoMessageListener(NetworkData data)
		{
			DynamicObjectsInfoMessage dynamicObjectsInfoMessage = data as DynamicObjectsInfoMessage;
			Debug.Assert(dynamicObjectsInfoMessage != null, nameof(dynamicObjectsInfoMessage) + " != null");
			foreach (DynamicObjectInfo info in dynamicObjectsInfoMessage.Infos)
			{
				DynamicObject dynamicObject = GetObject(info.GUID, SpaceObjectType.DynamicObject) as DynamicObject;
				if (dynamicObject != null && dynamicObject.Item != null)
				{
					dynamicObject.Item.ProcesStatsData(info.Stats);
				}
			}
		}

		private void SpawnObjectsResponseListener(NetworkData data)
		{
			SpawnObjectsResponse spawnObjectsData = data as SpawnObjectsResponse;
			foreach (SpawnObjectResponseData objectToSpawn in spawnObjectsData.Data)
			{
				if (objectToSpawn.Type is SpaceObjectType.Ship or SpaceObjectType.Asteroid)
				{
					SpaceObjectVessel spaceObjectVessel = GetObject(objectToSpawn.GUID, objectToSpawn.Type) as SpaceObjectVessel;
					spaceObjectVessel.ParseSpawnData(objectToSpawn);
					if (spaceObjectVessel.IsMainVessel && spaceObjectVessel.Orbit.Parent != null)
					{
						Map.InitialiseMapObject(spaceObjectVessel);
					}
				}
				else if (objectToSpawn.Type is SpaceObjectType.DynamicObject && GetDynamicObject(objectToSpawn.GUID) is null)
				{
					DynamicObject.SpawnDynamicObject(objectToSpawn);
				}
				else if (objectToSpawn.Type is SpaceObjectType.Corpse && GetCorpse(objectToSpawn.GUID) is null)
				{
					Corpse.SpawnCorpse(objectToSpawn);
				}
				else if (objectToSpawn.Type is SpaceObjectType.Player && GetPlayer(objectToSpawn.GUID) is null)
				{
					OtherPlayer.SpawnPlayer(objectToSpawn);
				}
				else
				{
					Debug.LogWarningFormat("Tried to spawn unimplemented space object with type {0}.", objectToSpawn.Type);
				}
			}

			if (_restoreMapDetailsTask != null)
			{
				_restoreMapDetailsTask.RunSynchronously();
				_restoreMapDetailsTask = null;
			}
		}

		private void ShipCollisionMessageListener(NetworkData data)
		{
			ShipCollisionMessage shipCollisionMessage = data as ShipCollisionMessage;
			if (GetVessel(shipCollisionMessage.ShipOne) != MyPlayer.Instance.Parent &&
			    (shipCollisionMessage.ShipTwo == -1 ||
			     GetVessel(shipCollisionMessage.ShipTwo) != MyPlayer.Instance.Parent) ||
			    shipCollisionMessage.CollisionVelocity <= float.Epsilon)
			{
				return;
			}

			MyPlayer.Instance.FpsController.CameraController.cameraShakeController.CamShake(0.8f, 0.3f, 15f, 15f,
				useSparks: true);
			VesselHealthSounds[] healthSoundsArray = GetVessel(shipCollisionMessage.ShipOne).GeometryRoot
				.GetComponentsInChildren<VesselHealthSounds>();
			foreach (VesselHealthSounds vesselHealthSounds in healthSoundsArray)
			{
				vesselHealthSounds.PlaySounds();
			}

			if (shipCollisionMessage.ShipTwo != -1)
			{
				VesselHealthSounds[] vesselHealthSoundsArray = GetVessel(shipCollisionMessage.ShipTwo).GeometryRoot
					.GetComponentsInChildren<VesselHealthSounds>();
				foreach (VesselHealthSounds vesselHealthSounds in vesselHealthSoundsArray)
				{
					vesselHealthSounds.PlaySounds();
				}
			}
		}

		private void LogOutResponseListener(NetworkData data)
		{
			LogOutResponse logOutResponse = data as LogOutResponse;
			if (logOutResponse is null || logOutResponse.Status == NetworkData.MessageStatus.Failure)
			{
				Debug.LogError("Failed to log out properly");
			}

			ReturnToMainMenu();
		}

		private void DestroyObjectMessageListener(NetworkData data)
		{
			DestroyObjectMessage destroyObjectMessage = data as DestroyObjectMessage;
			SpaceObject obj = GetObject(destroyObjectMessage.ID, destroyObjectMessage.ObjectType);
			if (obj != null && obj.Type != SpaceObjectType.PlayerPivot &&
			    obj.Type != SpaceObjectType.DynamicObjectPivot && obj.Type != SpaceObjectType.CorpsePivot)
			{
				obj.DestroyGeometry();
				if (obj is DynamicObject && (obj as DynamicObject).Item != null &&
				    (obj as DynamicObject).Item.AttachPoint != null)
				{
					(obj as DynamicObject).Item.AttachPoint.DetachItem((obj as DynamicObject).Item);
				}

				if (MyPlayer.Instance != null && MyPlayer.Instance.CurrentActiveItem != null &&
				    MyPlayer.Instance.CurrentActiveItem.GUID == obj.Guid)
				{
					MyPlayer.Instance.Inventory.RemoveItemFromHands(resetStance: true);
				}

				Destroy(obj.gameObject);
			}
		}

		// Starts logging out.
		public void LogOut()
		{
			GlobalGUI.ShowLoadingScreen(GlobalGUI.LoadingScreenType.Loading);

			if (!_logoutRequestSent)
			{
				_logoutRequestSent = true;
				NetworkController.Send(new LogOutRequest());
			}
		}

		/// <summary>
		/// 	Closes the current game, returns to main menu, and disconnects from the server.
		/// </summary>
		public void ReturnToMainMenu()
		{
			if (!_openMainSceneStarted)
			{
				Debug.Log("Returning to main menu...");
				_openMainSceneStarted = true;
				Globals.ToggleCursor(true);
				if (MyPlayer.Instance != null)
				{
					Destroy(MyPlayer.Instance.gameObject);
				}
				NetworkController.Disconnect();
				SceneManager.LoadScene("MainMenuScene", LoadSceneMode.Single);
				GlobalGUI.CloseLoadingScreen();
			}
		}

		public void ConnectionFailedListener()
		{
			Debug.LogError("Connection to server failed.");
			ReturnToMainMenu();
		}

		private void OnDestroy()
		{
			EventSystem.RemoveListener(typeof(KillPlayerMessage), KillPlayerMessageListener);
			EventSystem.RemoveListener(typeof(LogOutResponse), LogOutResponseListener);
			EventSystem.RemoveListener(typeof(DestroyObjectMessage), DestroyObjectMessageListener);
			EventSystem.RemoveListener(typeof(MovementMessage), MovementMessageListener);
			EventSystem.RemoveListener(typeof(PlayersOnServerResponse), PlayersOnServerResponseListener);
			EventSystem.RemoveListener(typeof(ShipCollisionMessage), ShipCollisionMessageListener);
			EventSystem.RemoveListener(typeof(UpdateVesselDataMessage), UpdateVesselDataMessageListener);
		}

		private void KillPlayerMessageListener(NetworkData data)
		{
			KillPlayerMessage killPlayerMessage = data as KillPlayerMessage;
			if (killPlayerMessage.GUID != MyPlayer.Instance.Guid)
			{
				return;
			}

			MyPlayer.Instance.IsAlive = false;

			InGameGUI.ToggleDeadMsg(val: true);
			if (killPlayerMessage.CauseOfDeath == HurtType.Shipwreck && killPlayerMessage.VesselDamageType != 0)
			{
				InGameGUI.DeadMsgText.text = killPlayerMessage.VesselDamageType.ToLocalizedString().ToUpper();
			}
			else
			{
				InGameGUI.DeadMsgText.text = killPlayerMessage.CauseOfDeath.ToLocalizedString().ToUpper();
			}
		}

		public void AddPlayer(long guid, OtherPlayer pl)
		{
			Players[guid] = pl;
			RefreshMovementIntervals();
		}

		private void RefreshMovementIntervals()
		{
			MyPlayer.SendMovementInterval = Players.Count <= 0 ? 1f : 0.1f;
			DynamicObject.SendMovementInterval = Players.Count <= 0 ? 1f : 0.1f;
			Corpse.SendMovementInterval = Players.Count <= 0 ? 1f : 0.1f;
		}

		public void RemovePlayer(long guid)
		{
			Players.Remove(guid);
			RefreshMovementIntervals();
		}

		public OtherPlayer GetPlayer(long guid)
		{
			if (Players.TryGetValue(guid, out var player))
			{
				return player;
			}

			if (MyPlayer.Instance != null && MyPlayer.Instance.Parent != null &&
			    MyPlayer.Instance.Parent is SpaceObjectVessel)
			{
				OtherPlayer[] componentsInChildren = (MyPlayer.Instance.Parent as SpaceObjectVessel).MainVessel
					.GetComponentsInChildren<OtherPlayer>();
				foreach (OtherPlayer otherPlayer in componentsInChildren)
				{
					if (otherPlayer.Guid == guid)
					{
						AddPlayer(otherPlayer.Guid, otherPlayer);
						return otherPlayer;
					}
				}
			}

			OtherPlayer[] componentsInChildren2 = ShipExteriorRoot.GetComponentsInChildren<OtherPlayer>();
			foreach (OtherPlayer otherPlayer2 in componentsInChildren2)
			{
				if (otherPlayer2.Guid == guid)
				{
					AddPlayer(otherPlayer2.Guid, otherPlayer2);
					return otherPlayer2;
				}
			}

			return null;
		}

		public void AddDynamicObject(long guid, DynamicObject obj)
		{
			DynamicObjects[guid] = obj;
		}

		public void RemoveDynamicObject(long guid)
		{
			DynamicObjects.Remove(guid);
		}

		public DynamicObject GetDynamicObject(long guid)
		{
			if (DynamicObjects.TryGetValue(guid, out var value))
			{
				return value;
			}

			if (MyPlayer.Instance != null && MyPlayer.Instance.Parent != null)
			{
				DynamicObject[] componentsInChildren =
					MyPlayer.Instance.Parent.GetComponentsInChildren<DynamicObject>();
				foreach (DynamicObject dynamicObject in componentsInChildren)
				{
					if (dynamicObject.Guid == guid)
					{
						AddDynamicObject(dynamicObject.Guid, dynamicObject);
						return dynamicObject;
					}
				}
			}

			DynamicObject[] componentsInChildren2 = ShipExteriorRoot.GetComponentsInChildren<DynamicObject>();
			foreach (DynamicObject dynamicObject2 in componentsInChildren2)
			{
				if (dynamicObject2.Guid == guid)
				{
					AddDynamicObject(dynamicObject2.Guid, dynamicObject2);
					return dynamicObject2;
				}
			}

			return null;
		}

		public void AddCorpse(long guid, Corpse obj)
		{
			Corpses[guid] = obj;
		}

		public void RemoveCorpse(long guid)
		{
			Corpses.Remove(guid);
		}

		public Corpse GetCorpse(long guid)
		{
			if (Corpses.TryGetValue(guid, out var corpse))
			{
				return corpse;
			}

			if (MyPlayer.Instance != null && MyPlayer.Instance.Parent != null)
			{
				Corpse[] corpsesInGame = MyPlayer.Instance.Parent.GetComponentsInChildren<Corpse>();
				foreach (Corpse corpseInGame in corpsesInGame)
				{
					if (corpseInGame.Guid == guid)
					{
						AddCorpse(corpseInGame.Guid, corpseInGame);
						return corpseInGame;
					}
				}
			}

			Corpse[] componentsInChildren2 = ShipExteriorRoot.GetComponentsInChildren<Corpse>();
			foreach (Corpse corpse2 in componentsInChildren2)
			{
				if (corpse2.Guid == guid)
				{
					AddCorpse(corpse2.Guid, corpse2);
					return corpse2;
				}
			}

			return null;
		}

		public SpaceObjectVessel GetVessel(long guid)
		{
			if (ActiveVessels.TryGetValue(guid, out var value))
			{
				return value;
			}

			value = SolarSystem.ArtificialBodyReferences.FirstOrDefault((ArtificialBody m) => m.Guid == guid) as SpaceObjectVessel;
			if (value != null)
			{
				return value;
			}

			SpaceObjectVessel[] componentsInChildren = ShipExteriorRoot.GetComponentsInChildren<SpaceObjectVessel>();
			foreach (SpaceObjectVessel spaceObjectVessel in componentsInChildren)
			{
				if (spaceObjectVessel.Guid == guid)
				{
					return spaceObjectVessel;
				}
			}

			Debug.LogWarning("Could not find space object vessel in world with guid: " + guid);

			return null;
		}

		public void SendVesselRequest(SpaceObjectVessel obj, float time, GameScenes.SceneId sceneID, string tag)
		{
			NetworkController.Send(new VesselRequest
			{
				GUID = obj.Guid,
				Time = time,
				RescueShipSceneID = sceneID,
				RescueShipTag = tag
			});
		}

		public void SendDistressCall(ArtificialBody body, bool isDistressActive)
		{
			NetworkController.Send(new DistressCallRequest
			{
				GUID = body.Guid,
				IsDistressActive = isDistressActive
			});
		}

		private void PlayersOnServerResponseListener(NetworkData data)
		{
			try
			{
				PlayersOnServerResponse playersOnServerResponse = data as PlayersOnServerResponse;
				if (playersOnServerResponse.SpawnPointID != null)
				{
					GetVessel(playersOnServerResponse.SpawnPointID.VesselGUID)
						.GetStructureObject<SceneSpawnPoint>(playersOnServerResponse.SpawnPointID.InSceneID)
						.ParsePlayersOnServerResponse(playersOnServerResponse).Forget();
				}
				else if (playersOnServerResponse.SecuritySystemID != null)
				{
					(GetVessel(playersOnServerResponse.SecuritySystemID.VesselGUID) as Ship).SecuritySystem
						.ParsePlayersOnServerResponse(playersOnServerResponse);
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		private void ConsoleMessageListener(NetworkData data)
		{
			try
			{
				ConsoleMessage consoleMessage = data as ConsoleMessage;
				InGameGUI.Console.CreateTextElement(consoleMessage.Text, Colors.Orange);
				if (consoleMessage.Text == "God mode: ON")
				{
					InGameGUI.Console.GodMode.isOn = true;
				}
				else if (consoleMessage.Text == "God mode: OFF")
				{
					InGameGUI.Console.GodMode.isOn = false;
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		private void LateUpdate()
		{
			if (MyPlayer.Instance == null || !MyPlayer.Instance.PlayerReady) return;
			SolarSystem.UpdatePositions();
			SolarSystem.CenterPlanets();
		}

		// Also spawns artificial bodies for some reason.
		// Caution: Executed very often.
		private void MovementMessageListener(NetworkData data)
		{
			if (MyPlayer.Instance == null)
			{
				return;
			}

			SpaceObjectVessel spaceObjectVessel = null;
			float nearestVessel = 0f;
			MovementMessage movementMessage = data as MovementMessage;
			var artificialBodiesCopy = SolarSystem.ArtificialBodyReferences.ToList();
			if (movementMessage.Transforms is { Count: > 0 })
			{
				foreach (ObjectTransform objectTransform in movementMessage.Transforms)
				{
					bool isArtificialBodyNew = false;
					ArtificialBody artificialBody = SolarSystem.GetArtificialBody(objectTransform.GUID);
					if (artificialBody != null)
					{
						artificialBodiesCopy.Remove(artificialBody);
					}

					switch (artificialBody)
					{
						case null when objectTransform.GUID == MyPlayer.Instance.Guid:
							continue;
						case null when objectTransform.Orbit is not null || objectTransform.Realtime is not null ||
						               objectTransform.StabilizeToTargetGUID is > 0:
							artificialBody = ArtificialBody.CreateDummy(objectTransform);
							isArtificialBodyNew = true;
							break;
					}

					if (artificialBody == null)
					{
						Debug.LogError("Could not create dummy artificial body.");
						continue;
					}

					if (objectTransform.StabilizeToTargetGUID is > 0)
					{
						artificialBody.StabilizeToTarget(objectTransform.StabilizeToTargetGUID.Value,
							objectTransform.StabilizeToTargetRelPosition.ToVector3D());
					}
					else if (objectTransform.Orbit != null)
					{
						CelestialBody celestialBody = artificialBody.Orbit.Parent.CelestialBody;
						artificialBody.DisableStabilization();
						if (artificialBody is Ship ship && ship.WarpStartEffectTask != null)
						{
							ship.WarpStartEffectTask.RunSynchronously();
						}

						artificialBody.Orbit.ParseNetworkData(this, objectTransform.Orbit);
						artificialBody.UpdateOrbitPosition(SolarSystem.CurrentTime, resetTime: true);
						UpdateMapObject(artificialBody, celestialBody);
					}
					else if (objectTransform.Realtime != null)
					{
						artificialBody.DisableStabilization();
						artificialBody.Orbit.ParseNetworkData(this, objectTransform.Realtime);
					}

					bool flag2 = (artificialBody.Maneuver != null && objectTransform.Maneuver == null) ||
					             (artificialBody.Maneuver == null && objectTransform.Maneuver != null);
					if (artificialBody.Maneuver != null && objectTransform.Maneuver == null)
					{
						artificialBody.ManeuverExited = true;
					}

					artificialBody.Maneuver = objectTransform.Maneuver;
					if (artificialBody.Maneuver != null)
					{
						artificialBody.Orbit.ParseNetworkData(this, objectTransform.Maneuver);
					}

					if (flag2 && MyPlayer.Instance.Parent is Ship &&
					    (MyPlayer.Instance.Parent as Ship).RadarSystem != null)
					{
						(MyPlayer.Instance.Parent as Ship).RadarSystem.PassiveScanObject(artificialBody);
						SolarSystem.ArtificialBodiesVisibilityModified();
					}

					if (objectTransform.Forward != null && objectTransform.Up != null)
					{
						artificialBody.SetTargetPositionAndRotation(null, objectTransform.Forward.ToVector3(),
							objectTransform.Up.ToVector3(), isArtificialBodyNew || !artificialBody.IsInVisibilityRange,
							movementMessage.SolarSystemTime);
						artificialBody.AngularVelocity = objectTransform.AngularVelocity.ToVector3();
						if (objectTransform.RotationVec != null)
						{
							artificialBody.RotationVec = objectTransform.RotationVec.ToVector3D();
						}
					}

					if (objectTransform.CharactersMovement != null)
					{
						foreach (CharacterMovementMessage item in objectTransform.CharactersMovement)
						{
							if (item == null)
							{
								continue;
							}

							if (item.GUID == MyPlayer.Instance.Guid)
							{
								MyPlayer.Instance.ProcessMovementMessage(item);
								continue;
							}

							OtherPlayer player = GetPlayer(item.GUID);
							if (player != null)
							{
								player.ProcessMovementMessage(item);
							}
						}
					}

					if (objectTransform.DynamicObjectsMovement != null)
					{
						foreach (DynamicObjectMovementMessage item2 in objectTransform.DynamicObjectsMovement)
						{
							if (item2 != null)
							{
								DynamicObject dynamicObject = GetDynamicObject(item2.GUID);
								if (dynamicObject != null)
								{
									dynamicObject.ProcessDynamicObectMovementMessage(item2);
								}
							}
						}
					}

					if (objectTransform.CorpsesMovement != null)
					{
						foreach (CorpseMovementMessage item3 in objectTransform.CorpsesMovement)
						{
							if (item3 != null)
							{
								Corpse corpse = GetCorpse(item3.GUID);
								if (corpse != null)
								{
									corpse.ProcessMoveCorpseObectMessage(item3);
								}
							}
						}
					}

					float num2 =
						(artificialBody.transform.position - MyPlayer.Instance.transform.position).sqrMagnitude -
						(float)(artificialBody.Radius * artificialBody.Radius);
					if (artificialBody is SpaceObjectVessel body && (spaceObjectVessel == null || nearestVessel > num2))
					{
						spaceObjectVessel = body;
						nearestVessel = num2;
					}
				}
			}

			if (artificialBodiesCopy.Count > 0)
			{
				foreach (ArtificialBody body in artificialBodiesCopy)
				{
					float bodyDistance = (body.transform.position - MyPlayer.Instance.transform.position).sqrMagnitude -
					             (float)(body.Radius * body.Radius);
					if (body is SpaceObjectVessel { IsDebrisFragment: false } vessel &&
					    (spaceObjectVessel == null || (nearestVessel > bodyDistance && (spaceObjectVessel.FTLEngine == null ||
					                                                  spaceObjectVessel.FTLEngine.Status !=
					                                                  SystemStatus.Online ||
					                                                  (spaceObjectVessel.Velocity -
					                                                   MyPlayer.Instance.Parent.Velocity).SqrMagnitude <
					                                                  900.0))))
					{
						spaceObjectVessel = vessel;
						nearestVessel = bodyDistance;
					}
				}
			}

			MyPlayer.Instance.NearestVessel = spaceObjectVessel;
			MyPlayer.Instance.NearestVesselSqDistance = nearestVessel;
		}

		private void UpdateMapObject(ArtificialBody ab, CelestialBody oldParent)
		{
			if (ab is not SpaceObjectVessel vessel) return;

			MapObject mapObject;
			if (!Map.AllMapObjects.TryGetValue(vessel, out mapObject))
			{
				Map.InitialiseMapObject(vessel);
				Map.AllMapObjects.TryGetValue(vessel, out mapObject);
			}

			if (mapObject == null)
			{
				return;
			}

			if (oldParent != vessel.Orbit.Parent.CelestialBody)
			{
				if (Map.gameObject.activeInHierarchy)
				{
					Map.UpdateParent(mapObject.MainObject);
				}

				if (MyPlayer.Instance != null && this == MyPlayer.Instance.Parent)
				{
					RichPresenceManager.UpdateStatus();
				}
			}

			if (MyPlayer.Instance.LockedToTrigger is SceneTriggerNavigationPanel ||
			    MyPlayer.Instance.ShipControlMode == ShipControlMode.Navigation)
			{
				mapObject.SetOrbit();
			}
		}

		public SpaceObject GetObject(long guid, SpaceObjectType objectType)
		{
			switch (objectType)
			{
				case SpaceObjectType.Player:
					if (guid == MyPlayer.Instance.Guid)
					{
						return MyPlayer.Instance;
					}

					return GetPlayer(guid);
				case SpaceObjectType.DynamicObject:
					return GetDynamicObject(guid);
				case SpaceObjectType.Corpse:
					return GetCorpse(guid);
				case SpaceObjectType.PlayerPivot:
				case SpaceObjectType.DynamicObjectPivot:
				case SpaceObjectType.CorpsePivot:
				{
					return SolarSystem.GetArtificialBody(guid) as Pivot;
				}
				case SpaceObjectType.Ship:
				{
					return SolarSystem.GetArtificialBody(guid) as Ship;
				}
				case SpaceObjectType.Asteroid:
				{
					return SolarSystem.GetArtificialBody(guid) as Asteroid;
				}
			}

			throw new NotImplementedException();
		}

		public void MovePanelCursor(Transform trans, float panelWidth, float panelHeight)
		{
			float x = Mathf.Clamp(
				trans.localPosition.x + Mouse.current.delta.x.ReadValue() * Globals.Instance.MouseSpeedOnPanels, 0f,
				panelWidth);
			float y = Mathf.Clamp(
				trans.localPosition.y + Mouse.current.delta.y.ReadValue() * Globals.Instance.MouseSpeedOnPanels *
				(!Settings.SettingsData.ControlsSettings.InvertMouse ? 1 : (-1)), 0f, panelHeight);
			trans.localPosition = new Vector3(x, y, trans.localPosition.z);
		}

		public void DeleteCharacterRequest(ServerData gs)
		{
			GlobalGUI.ShowConfirmMessageBox(Localization.DeleteCharacter, Localization.AreYouSureDeleteCharacter,
				Localization.Yes, Localization.No, async delegate
				{
					DeleteCharacterRequest deleteCharacterRequest = new DeleteCharacterRequest
					{
						ServerId = gs.Id,
						PlayerId = await NakamaClient.GetUserId()
					};

					await NetworkController.SendTcp(deleteCharacterRequest, gs.IpAddress, gs.StatusPort, false, true);
				});
		}

		public async UniTaskVoid LatencyTestMessage()
		{
			_lastLatencyMessageTime = Time.realtimeSinceStartup;

			int latency = await NetworkController.LatencyTest(MainMenuGUI.LastConnectedServer.IpAddress, MainMenuGUI.LastConnectedServer.StatusPort);
			_latencyMs = latency;

			if (MyPlayer.Instance.IsAlive)
			{
				if (LatencyMs > 120 && LatencyMs < 150)
				{
					InGameGUI.Latency.color = Colors.SlotGray;
					InGameGUI.Latency.gameObject.Activate(value: true);
				}
				else if (LatencyMs >= 150)
				{
					InGameGUI.Latency.color = Colors.PowerRed;
					InGameGUI.Latency.gameObject.Activate(value: true);
				}
				else
				{
					InGameGUI.Latency.gameObject.Activate(value: false);
				}
			}
			else
			{
				InGameGUI.Latency.gameObject.Activate(value: false);
			}

			Invoke(nameof(LatencyTestMessage), 1f);
		}

		public float GetVesselExposureDamage(double distance)
		{
			if (_vesselExposureValues == null)
			{
				return 1f;
			}

			return _vesselExposureValues[(int)(Mathf.Clamp01((float)(distance / ExposureRange)) * 99f)];
		}

		public float GetPlayerExposureDamage(double distance)
		{
			if (_playerExposureValues == null)
			{
				return 0f;
			}

			return _playerExposureValues[(int)(Mathf.Clamp01((float)(distance / ExposureRange)) * 99f)];
		}

		private void UpdateVesselDataMessageListener(NetworkData data)
		{
			UpdateVesselDataMessage updateVesselDataMessage = data as UpdateVesselDataMessage;
			if (updateVesselDataMessage is null || updateVesselDataMessage.VesselsDataUpdate is null)
			{
				return;
			}

			foreach (VesselDataUpdate item in updateVesselDataMessage.VesselsDataUpdate)
			{
				SpaceObjectVessel spaceObjectVessel = SolarSystem.GetArtificialBody(item.GUID) as SpaceObjectVessel;
				if (spaceObjectVessel != null && spaceObjectVessel.VesselData is not null)
				{
					if (item.VesselName is not null)
					{
						spaceObjectVessel.VesselData.VesselName = item.VesselName;
					}

					if (item.VesselRegistration is not null)
					{
						spaceObjectVessel.VesselData.VesselRegistration = item.VesselRegistration;
					}

					if (item.RadarSignature.HasValue)
					{
						spaceObjectVessel.VesselData.RadarSignature = item.RadarSignature.Value;
					}

					if (item.IsAlwaysVisible.HasValue)
					{
						spaceObjectVessel.VesselData.IsAlwaysVisible = item.IsAlwaysVisible.Value;
					}

					if (item.IsDistressSignalActive.HasValue)
					{
						spaceObjectVessel.VesselData.IsDistressSignalActive = item.IsDistressSignalActive.Value;
					}
				}
			}
		}

		/// <summary>
		/// 	Listen for connection drops and attempt to reconnect.
		/// </summary>
		public void ReconnectAutoListener()
		{
			GameStarter gameStarter = GameStarter.Create();
			gameStarter.FindServerAndConnect(true).Forget();
		}

		public async UniTask<bool> OnLogin(LogInResponse logInResponse, VesselObjectID invitedToServerSpawnPointId = null)
		{
			SolarSystem.Set(GameObject.Find("/SolarSystemRoot/SunRoot").transform,
				GameObject.Find("/SolarSystemRoot/PlanetsRoot").transform, logInResponse.ServerTime);
			SolarSystem.LoadDataFromResources();
			await MyPlayer.SpawnMyPlayer(this, logInResponse);

			foreach (DebrisFieldDetails debrisField in logInResponse.DebrisFields)
			{
				DebrisFields.Add(new DebrisField(this, debrisField));
				Map.InitialiseMapObject(new DebrisField(this, debrisField));
			}

			ItemsIngredients = logInResponse.ItemsIngredients;
			Quests = logInResponse.Quests.Select((QuestData m) => new Quest(m, QuestCollection)).ToList();
			SpaceObjectVessel.VesselDecayRateMultiplier = logInResponse.VesselDecayRateMultiplier;
			ExposureRange = logInResponse.ExposureRange;
			_vesselExposureValues = logInResponse.VesselExposureValues;
			_playerExposureValues = logInResponse.PlayerExposureValues;

			PlayerSpawnRequest playerSpawnRequest;

			if (logInResponse.IsAlive)
			{
				playerSpawnRequest = new PlayerSpawnRequest
				{
					SpawnPointParentId = 0L
				};
			}
			else if (invitedToServerSpawnPointId != null)
			{
				playerSpawnRequest = new PlayerSpawnRequest
				{
					SpawnPointParentId = invitedToServerSpawnPointId.VesselGUID
				};
				invitedToServerSpawnPointId = null;
			}
			else
			{
				/*MainMenuGUI.SendSpawnRequest(new SpawnPointDetails
				{
					SpawnSetupType = SpawnSetupType.Start1,
					IsPartOfCrew = false,
					PlayersOnShip = new List<string>()
				});*/

				playerSpawnRequest = new PlayerSpawnRequest
				{
					SpawnSetupType = SpawnSetupType.Start1,
					SpawnPointParentId = 0L
				};

				//MainMenuGUI.ShowSpawnPointSelection(logInResponse.SpawnPointsList, logInResponse.CanContinue);
			}

			try
			{
				var spawnResponse = await NetworkController.SendReceiveAsync(playerSpawnRequest) as PlayerSpawnResponse;

				if (spawnResponse.Status != NetworkData.MessageStatus.Success)
				{
					GlobalGUI.ShowErrorMessage(Localization.SpawnErrorTitle, Localization.SpawnErrorMessage);
					Debug.LogWarning("Spawn response returned with failure.");
					MainMenuGUI.CanChooseSpawn = true;
					ReturnToMainMenu();
					return false;
				}

				LoadingFinishedDelegate = new Action(() =>
				{
					Debug.Log("Loading finished task completed.");
					LoadingFinishedDelegate = null;
					MyPlayer.Instance.ActivatePlayer(spawnResponse);
				});

				Debug.Log("Started loading world.");
				SolarSystemRoot.SetActive(true);
				if (spawnResponse.HomeGUID.HasValue)
				{
					MyPlayer.Instance.HomeStationGUID = spawnResponse.HomeGUID.Value;
				}

				Globals.SceneLoader.LoadScenesWithIDs(spawnResponse.Scenes);

				if (spawnResponse.ParentType == SpaceObjectType.Ship)
				{
					Ship ship = Ship.Create(spawnResponse.MainVesselID, spawnResponse.VesselData, spawnResponse.ParentTransform, isMainObject: true);
					ship.gameObject.SetActive(true);
					if (spawnResponse.DockedVessels is { Count: > 0 })
					{
						foreach (DockedVesselData dockedVessel in spawnResponse.DockedVessels)
						{
							Ship ship2 = Ship.Create(dockedVessel.GUID, dockedVessel.Data, spawnResponse.ParentTransform,
								isMainObject: true);
							ship2.gameObject.SetActive(true);
							ship2.DockedToMainVessel = ship;
						}
					}

					MyPlayer.Instance.Parent = GetVessel(spawnResponse.ParentID);
					Debug.Log("Starting main scene load of type ship.");
					await LoadMainSceneShip(spawnResponse, ship, spawnResponse.VesselObjects);
				}
				else if (spawnResponse.ParentType == SpaceObjectType.Asteroid)
				{
					Asteroid asteroid = Asteroid.Create(spawnResponse.ParentTransform, spawnResponse.VesselData, isMainObject: true);
					asteroid.gameObject.SetActive(true);
					MyPlayer.Instance.Parent = asteroid;
					Debug.Log("Starting main scene load of type asteroid.");
					await LoadMainSceneAstroid(spawnResponse, asteroid);
				}
				else if (spawnResponse.ParentType == SpaceObjectType.PlayerPivot)
				{
					Pivot parent = Pivot.Create(SpaceObjectType.PlayerPivot, spawnResponse.ParentTransform, isMainObject: true);
					MyPlayer.Instance.Parent = parent;
				}
				else
				{
					ReturnToMainMenu();
					Debug.LogErrorFormat("Unknown player parent {0}, with id {1}.", spawnResponse.ParentType, spawnResponse.ParentID);
					GlobalGUI.ShowErrorMessage(Localization.SpawnErrorTitle, Localization.SpawnErrorMessage);
					MainMenuGUI.CanChooseSpawn = true;
					return false;
				}

				if (spawnResponse.TimeUntilServerRestart.HasValue)
				{
					ServerRestartTime = DateTime.UtcNow.AddSeconds(spawnResponse.TimeUntilServerRestart.Value);
				}
				else
				{
					ServerRestartTime = null;
				}

				if (spawnResponse.Quests != null)
				{
					foreach (QuestDetails quest in spawnResponse.Quests)
					{
						MyPlayer.Instance.SetQuestDetails(quest, showNotifications: false, playCutScenes: false);
					}
				}

				if (spawnResponse.Blueprints != null)
				{
					MyPlayer.Instance.Blueprints = spawnResponse.Blueprints;
				}

				if (spawnResponse.NavMapDetails != null)
				{
					_restoreMapDetailsTask = new Task(() =>
					{
						foreach (UnknownMapObjectDetails det in spawnResponse.NavMapDetails.Unknown)
						{
							SpaceObjectVessel spaceObjectVessel = null;
							if (det.SpawnRuleID != 0)
							{
								spaceObjectVessel = SolarSystem.ArtificialBodyReferences.FirstOrDefault((ArtificialBody m) =>
										m is SpaceObjectVessel && (m as SpaceObjectVessel).IsMainVessel &&
										(m as SpaceObjectVessel).VesselData != null &&
										(m as SpaceObjectVessel).VesselData.SpawnRuleID == det.SpawnRuleID) as
									SpaceObjectVessel;
							}

							if (spaceObjectVessel == null)
							{
								spaceObjectVessel = GetVessel(det.GUID);
							}

							if (spaceObjectVessel != null)
							{
								spaceObjectVessel.RadarVisibilityType = RadarVisibilityType.Unknown;
								spaceObjectVessel.LastKnownMapOrbit = new OrbitParameters();
								spaceObjectVessel.LastKnownMapOrbit.ParseNetworkData(this, det.LastKnownOrbit);
								if (spaceObjectVessel.VesselData != null && spaceObjectVessel.VesselData.SpawnRuleID != 0)
								{
									Map.UnknownVisibilityOrbits[spaceObjectVessel.VesselData.SpawnRuleID] =
										spaceObjectVessel.LastKnownMapOrbit;
								}
							}
						}

						SolarSystem.ArtificialBodiesVisibilityModified();
					});
				}

				return true;
			}
			catch (TimeoutException)
			{
				GlobalGUI.ShowErrorMessage(Localization.SpawnErrorTitle, Localization.ConnectionTimedOut);
				Debug.Log("Connection timed out when logging in.");
				MainMenuGUI.CanChooseSpawn = true;
				ReturnToMainMenu();
				return false;
			}
		}

		private async UniTask LoadMainSceneAstroid(PlayerSpawnResponse spawnResponse, Asteroid asteroid)
		{
			await asteroid.LoadAsync(spawnResponse.MiningPoints);
		}

		private async UniTask LoadMainSceneShip(PlayerSpawnResponse spawnResponse, Ship sh, VesselObjects shipObjects)
		{
			if (sh != null)
			{
				await sh.LoadAsync(isMainShip: true, shipObjects);
				if (spawnResponse.DockedVessels is { Count: > 0 })
				{
					foreach (DockedVesselData dockedVessel in spawnResponse.DockedVessels)
					{
						Ship childShip = GetVessel(dockedVessel.GUID) as Ship;
						await childShip.LoadAsync(isMainShip: true, dockedVessel.VesselObjects);
						childShip.transform.parent = sh.ConnectedObjectsRoot.transform;
					}
				}
			}

			if (shipObjects.DockingPorts != null)
			{
				SceneDockingPort[] componentsInChildren =
					sh.GeometryRoot.GetComponentsInChildren<SceneDockingPort>(includeInactive: true);
				foreach (SceneDockingPort dp in componentsInChildren)
				{
					SceneDockingPortDetails sceneDockingPortDetails =
						shipObjects.DockingPorts.Find((SceneDockingPortDetails m) => m.ID.InSceneID == dp.InSceneID);
					if (sceneDockingPortDetails != null)
					{
						dp.SetDetails(sceneDockingPortDetails, isInitialize: true);
					}
				}
			}

			if (spawnResponse.DockedVessels is { Count: > 0 })
			{
				foreach (DockedVesselData dockedVessel in spawnResponse.DockedVessels)
				{
					if (dockedVessel.VesselObjects.DockingPorts == null)
					{
						continue;
					}

					Ship ship = GetVessel(dockedVessel.GUID) as Ship;
					SceneDockingPort[] componentsInChildren2 =
						ship.GeometryRoot.GetComponentsInChildren<SceneDockingPort>(includeInactive: true);
					foreach (SceneDockingPort dport in componentsInChildren2)
					{
						SceneDockingPortDetails sceneDockingPortDetails2 =
							dockedVessel.VesselObjects.DockingPorts.Find((SceneDockingPortDetails m) =>
								m.ID.InSceneID == dport.InSceneID);
						if (sceneDockingPortDetails2 != null)
						{
							dport.SetDetails(sceneDockingPortDetails2, isInitialize: true);
						}
					}
				}
			}

			DynamicObject[] componentsInChildren3 = sh.TransferableObjectsRoot.GetComponentsInChildren<DynamicObject>();
			foreach (DynamicObject dynamicObject in componentsInChildren3)
			{
				dynamicObject.ToggleEnabled(isEnabled: true, toggleColliders: true);
				dynamicObject.CheckRoomTrigger(null);
			}

			Corpse[] componentsInChildren4 = sh.TransferableObjectsRoot.GetComponentsInChildren<Corpse>();
			foreach (Corpse corpse in componentsInChildren4)
			{
				corpse.CheckRoomTrigger(null);
			}
		}

		public void OnDisconnectedFromServer()
		{
			Debug.Log("Client disconnected from server.");

			if (!_logoutRequestSent)
			{
				MainMenuGUI.WasDisconnectUncontrolled = true;
			}

			ReturnToMainMenu();
		}
	}
}
