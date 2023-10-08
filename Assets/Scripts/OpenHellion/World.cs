// World.cs
//
// Copyright (C) 2023, OpenHellion contributors
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
using OpenHellion.Net.Message;
using OpenHellion.Social;
using OpenHellion.Social.RichPresence;
using OpenHellion.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using ZeroGravity;
using ZeroGravity.Data;
using ZeroGravity.Effects;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;

namespace OpenHellion
{
	/// <summary>
	/// 	This is the main component of the game. It handles everything from saving to managing parts of the GUI.<br/>
	/// 	Avoid referencing this class if you're able. Make classes as self-contained as possible.
	/// </summary>
	/// <seealso cref="MyPlayer"/>
	public class World : MonoBehaviour
	{
		[Title("Config")] public bool ExperimentalBuild;

		[Multiline(2)] public string ExperimentalText;

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

		[Title("Object references")] public GameObject ExperimentalGameObject;

		public SolarSystem SolarSystem;

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

		public SoundEffect AmbientSounds;

		public DebrisFieldEffect DebrisEffect;

		public EffectPrefabs EffectPrefabs;

		public QuestCollectionObject QuestCollection;

		[NonSerialized] public volatile bool LogoutRequestSent;

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

		[NonSerialized] public Task LoadingFinishedTask;

		[NonSerialized] public bool IsChatOpened;

		public InWorldPanels InWorldPanels;

		public InGameGUI InGameGUI;

		public NakamaClient Nakama;

		public SceneLoader SceneLoader { get; private set; }

		public static int DefaultLayerMask => 1 << LayerMask.NameToLayer("Default");

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

			SceneLoader = GameObject.Find("/SceneLoader").GetComponent<SceneLoader>();

			Texture[] emblems = Resources.LoadAll<Texture>("Emblems");
			SceneVesselEmblem.Textures = emblems.ToDictionary(x => x.name, y => y);

			Globals.Instance.OnHellionQuit = () =>
			{
				LogOut();
				OnDestroy();
			};
		}

		private void Start()
		{
			if (ExperimentalGameObject != null)
			{
				if (ExperimentalBuild)
				{
					ExperimentalGameObject.SetActive(value: true);
					ExperimentalGameObject.GetComponentInChildren<Text>().text =
						ExperimentalText.Trim() + " " + Application.version;
					RichPresenceManager.SetAchievement(AchievementID.other_testing_squad_member);
				}
				else
				{
					ExperimentalGameObject.SetActive(value: false);
				}
			}

			InWorldPanels.LocalizePanels();

			EventSystem.AddListener(typeof(KillPlayerMessage), KillPlayerMessageListener);
			EventSystem.AddListener(typeof(LogOutResponse), LogOutResponseListener);
			EventSystem.AddListener(typeof(DestroyObjectMessage), DestroyObjectMessageListener);
			EventSystem.AddListener(typeof(SpawnObjectsResponse), SpawnObjectsReponseListener);
			EventSystem.AddListener(typeof(MovementMessage), MovementMessageListener);
			EventSystem.AddListener(typeof(DynamicObjectsInfoMessage), DynamicObjectsInfoMessageListener);
			EventSystem.AddListener(typeof(PlayersOnServerResponse), PlayersOnServerResponseListener);
			EventSystem.AddListener(typeof(ConsoleMessage), ConsoleMessageListener);
			EventSystem.AddListener(typeof(ShipCollisionMessage), ShipCollisionMessageListener);
			EventSystem.AddListener(typeof(UpdateVesselDataMessage), UpdateVesselDataMessageListener);
			EventSystem.AddListener(EventSystem.InternalEventType.OpenMainScreen, OpenMainScreenListener);
			EventSystem.AddListener(EventSystem.InternalEventType.ReconnectAuto, ReconnectAutoListener);
			EventSystem.AddListener(EventSystem.InternalEventType.ConnectionFailed, ConnectionFailedListener);
			EventSystem.AddListener(typeof(PlayerSpawnResponse), PlayerSpawnResponseListener);

			if (Settings.Instance != null)
			{
				Settings.Instance.LoadSettings(Settings.SettingsType.Game);
			}
		}

		private void DynamicObjectsInfoMessageListener(NetworkData data)
		{
			DynamicObjectsInfoMessage dynamicObjectsInfoMessage = data as DynamicObjectsInfoMessage;
			Debug.Assert(dynamicObjectsInfoMessage != null, nameof(dynamicObjectsInfoMessage) + " != null");
			foreach (DynamicObjectInfo info in dynamicObjectsInfoMessage.Infos)
			{
				DynamicObject dynamicObject = GetObject(info.GUID, SpaceObjectType.DynamicObject) as DynamicObject;
				if (dynamicObject is not null && dynamicObject.Item is not null)
				{
					dynamicObject.Item.ProcesStatsData(info.Stats);
				}
			}
		}

		private void SpawnObjectsReponseListener(NetworkData data)
		{
			SpawnObjectsResponse spawnObjectsResponse = data as SpawnObjectsResponse;
			foreach (SpawnObjectResponseData datum in spawnObjectsResponse.Data)
			{
				try
				{
					if (datum.Type == SpaceObjectType.Ship || datum.Type == SpaceObjectType.Asteroid)
					{
						SpaceObjectVessel spaceObjectVessel = GetObject(datum.GUID, datum.Type) as SpaceObjectVessel;
						spaceObjectVessel.ParseSpawnData(datum);
						if (spaceObjectVessel.IsMainVessel && spaceObjectVessel.Orbit.Parent != null)
						{
							Map.InitializeMapObject(spaceObjectVessel);
						}
					}
					else if (datum.Type == SpaceObjectType.DynamicObject && GetDynamicObject(datum.GUID) is null)
					{
						DynamicObject.SpawnDynamicObject(datum);
					}
					else if (datum.Type == SpaceObjectType.Corpse && GetCorpse(datum.GUID) is null)
					{
						Corpse.SpawnCorpse(datum);
					}
					else if (datum.Type == SpaceObjectType.Player && GetPlayer(datum.GUID) is null)
					{
						OtherPlayer.SpawnPlayer(datum);
					}
				}
				catch (Exception ex)
				{
					Dbg.Error(ex.Message, ex.StackTrace);
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
			if (!(GetVessel(shipCollisionMessage.ShipOne) == MyPlayer.Instance.Parent) &&
			    (shipCollisionMessage.ShipTwo == -1 ||
			     !(GetVessel(shipCollisionMessage.ShipTwo) == MyPlayer.Instance.Parent)) ||
			    !(shipCollisionMessage.CollisionVelocity > float.Epsilon))
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

		public void OpenMainScreenListener(EventSystem.InternalEventData data)
		{
			if (!LogoutRequestSent)
			{
				MainMenuGUI.HasDisconnected = true;
			}
			else
			{
				OpenMainScreen();
			}
		}

		private void SendLogoutRequest()
		{
			if (!LogoutRequestSent)
			{
				LogoutRequestSent = true;
				NetworkController.Instance.SendToGameServer(new LogOutRequest());
			}
		}

		private void LogOutResponseListener(NetworkData data)
		{
			LogOutResponse logOutResponse = data as LogOutResponse;
			if (logOutResponse is null || logOutResponse.Response == ResponseResult.Error)
			{
				Dbg.Error("Failed to log out properly");
			}

			NetworkController.Instance.Disconnect();
			OpenMainScreen();
		}

		private void DestroyObjectMessageListener(NetworkData data)
		{
			DestroyObjectMessage destroyObjectMessage = data as DestroyObjectMessage;
			SpaceObject obj = GetObject(destroyObjectMessage.ID, destroyObjectMessage.ObjectType);
			if (obj is not null && obj.Type != SpaceObjectType.PlayerPivot &&
			    obj.Type != SpaceObjectType.DynamicObjectPivot && obj.Type != SpaceObjectType.CorpsePivot)
			{
				obj.DestroyGeometry();
				if (obj is DynamicObject && (obj as DynamicObject).Item is not null &&
				    (obj as DynamicObject).Item.AttachPoint is not null)
				{
					(obj as DynamicObject).Item.AttachPoint.DetachItem((obj as DynamicObject).Item);
				}

				if (MyPlayer.Instance is not null && MyPlayer.Instance.CurrentActiveItem is not null &&
				    MyPlayer.Instance.CurrentActiveItem.GUID == obj.GUID)
				{
					MyPlayer.Instance.Inventory.RemoveItemFromHands(resetStance: true);
				}

				Destroy(obj.gameObject);
			}
		}

		public void LogOut()
		{
			InWorldPanels.gameObject.SetActive(false);
			InGameGUI.gameObject.SetActive(false);
			GlobalGUI.ShowLoadingScreen(GlobalGUI.LoadingScreenType.Loading);
			SendLogoutRequest();
		}

		/// <summary>
		/// 	Closes the current game and returns to main menu.
		/// </summary>
		public void OpenMainScreen()
		{
			if (!_openMainSceneStarted)
			{
				_openMainSceneStarted = true;
				InWorldPanels.Detach();
				Globals.ToggleCursor(true);
				if (MyPlayer.Instance is not null)
				{
					Destroy(MyPlayer.Instance.gameObject);
				}

				SceneManager.LoadScene("MainMenuScene", LoadSceneMode.Single);
			}
		}

		public void ConnectionFailedListener(EventSystem.InternalEventData data)
		{
			Dbg.Error("Connection to server failed.", data.Objects);
			OpenMainScreen();
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
			EventSystem.RemoveListener(EventSystem.InternalEventType.OpenMainScreen, OpenMainScreenListener);
			EventSystem.RemoveListener(EventSystem.InternalEventType.ReconnectAuto, ReconnectAutoListener);
			EventSystem.RemoveListener(EventSystem.InternalEventType.ConnectionFailed, ConnectionFailedListener);
			EventSystem.RemoveListener(typeof(PlayerSpawnResponse), PlayerSpawnResponseListener);
			Localization.RevertToDefault();
		}

		public void OnApplicationFocus(bool focusStatus)
		{
			if (MyPlayer.Instance != null)
			{
				if (MyPlayer.Instance.InLockState && !MyPlayer.Instance.IsLockedToTrigger &&
				    MyPlayer.Instance.Parent is SpaceObjectVessel &&
				    (MyPlayer.Instance.Parent as SpaceObjectVessel).SpawnPoints.Values.FirstOrDefault(
					    (SceneSpawnPoint m) => m.PlayerGUID == MyPlayer.Instance.GUID) != null)
				{
					MyPlayer.Instance.FpsController.CameraController.ToggleFreeLook(isActive: true);
				}

				MyPlayer.Instance.HideHiglightedAttachPoints();
			}
		}

		private void KillPlayerMessageListener(NetworkData data)
		{
			KillPlayerMessage killPlayerMessage = data as KillPlayerMessage;
			if (killPlayerMessage.GUID != MyPlayer.Instance.GUID)
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

			if (MyPlayer.Instance is not null && MyPlayer.Instance.Parent is not null &&
			    MyPlayer.Instance.Parent is SpaceObjectVessel)
			{
				OtherPlayer[] componentsInChildren = (MyPlayer.Instance.Parent as SpaceObjectVessel).MainVessel
					.GetComponentsInChildren<OtherPlayer>();
				foreach (OtherPlayer otherPlayer in componentsInChildren)
				{
					if (otherPlayer.GUID == guid)
					{
						AddPlayer(otherPlayer.GUID, otherPlayer);
						return otherPlayer;
					}
				}
			}

			OtherPlayer[] componentsInChildren2 = ShipExteriorRoot.GetComponentsInChildren<OtherPlayer>();
			foreach (OtherPlayer otherPlayer2 in componentsInChildren2)
			{
				if (otherPlayer2.GUID == guid)
				{
					AddPlayer(otherPlayer2.GUID, otherPlayer2);
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
					if (dynamicObject.GUID == guid)
					{
						AddDynamicObject(dynamicObject.GUID, dynamicObject);
						return dynamicObject;
					}
				}
			}

			DynamicObject[] componentsInChildren2 = ShipExteriorRoot.GetComponentsInChildren<DynamicObject>();
			foreach (DynamicObject dynamicObject2 in componentsInChildren2)
			{
				if (dynamicObject2.GUID == guid)
				{
					AddDynamicObject(dynamicObject2.GUID, dynamicObject2);
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
			if (Corpses.ContainsKey(guid))
			{
				return Corpses[guid];
			}

			if (MyPlayer.Instance is not null && MyPlayer.Instance.Parent is not null)
			{
				Corpse[] componentsInChildren = MyPlayer.Instance.Parent.GetComponentsInChildren<Corpse>();
				foreach (Corpse corpse in componentsInChildren)
				{
					if (corpse.GUID == guid)
					{
						AddCorpse(corpse.GUID, corpse);
						return corpse;
					}
				}
			}

			Corpse[] componentsInChildren2 = ShipExteriorRoot.GetComponentsInChildren<Corpse>();
			foreach (Corpse corpse2 in componentsInChildren2)
			{
				if (corpse2.GUID == guid)
				{
					AddCorpse(corpse2.GUID, corpse2);
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

			value =
				SolarSystem.ArtificialBodies.FirstOrDefault((ArtificialBody m) => m.GUID == guid) as SpaceObjectVessel;
			if (value is not null)
			{
				return value;
			}

			SpaceObjectVessel[] componentsInChildren = ShipExteriorRoot.GetComponentsInChildren<SpaceObjectVessel>();
			foreach (SpaceObjectVessel spaceObjectVessel in componentsInChildren)
			{
				if (spaceObjectVessel.GUID == guid)
				{
					return spaceObjectVessel;
				}
			}

			return null;
		}

		public void SendVesselRequest(SpaceObjectVessel obj, float time, GameScenes.SceneID sceneID, string tag)
		{
			NetworkController.Instance.SendToGameServer(new VesselRequest
			{
				GUID = obj.GUID,
				Time = time,
				RescueShipSceneID = sceneID,
				RescueShipTag = tag
			});
		}

		public void SendDistressCall(ArtificialBody body, bool isDistressActive)
		{
			NetworkController.Instance.SendToGameServer(new DistressCallRequest
			{
				GUID = body.GUID,
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
						.ParsePlayersOnServerResponse(playersOnServerResponse);
				}
				else if (playersOnServerResponse.SecuritySystemID != null)
				{
					(GetVessel(playersOnServerResponse.SecuritySystemID.VesselGUID) as Ship).SecuritySystem
						.ParsePlayersOnServerResponse(playersOnServerResponse);
				}
			}
			catch (Exception ex)
			{
				Dbg.Error("PlayersOnServerResponseListener", ex.Message, ex.StackTrace);
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
				Dbg.Error("ConsoleMessageListener", ex.Message, ex.StackTrace);
			}
		}

		private void LateUpdate()
		{
			if (MyPlayer.Instance is not null && MyPlayer.Instance.PlayerReady)
			{
				SolarSystem.UpdatePositions(Time.deltaTime);
				SolarSystem.CenterPlanets();
			}
		}

		private void MovementMessageListener(NetworkData data)
		{
			if (MyPlayer.Instance is null)
			{
				return;
			}

			SpaceObjectVessel spaceObjectVessel = null;
			float num = 0f;
			MovementMessage movementMessage = data as MovementMessage;
			var artificialBodies = SolarSystem.ArtificialBodies;
			if (movementMessage.Transforms != null && movementMessage.Transforms.Count > 0)
			{
				foreach (ObjectTransform objectTransform in movementMessage.Transforms)
				{
					bool flag = false;
					ArtificialBody artificialBody = SolarSystem.GetArtificialBody(objectTransform.GUID);
					if (artificialBody is not null)
					{
						artificialBodies.Remove(artificialBody);
					}

					switch (artificialBody)
					{
						case null when objectTransform.GUID == MyPlayer.Instance.GUID:
							continue;
						case null when (objectTransform.Orbit != null || objectTransform.Realtime != null ||
						                (objectTransform.StabilizeToTargetGUID.HasValue &&
						                 objectTransform.StabilizeToTargetGUID.Value > 0)):
							artificialBody = ArtificialBody.CreateArtificialBody(objectTransform);
							flag = true;
							break;
					}

					if (artificialBody is not null || objectTransform.Type == SpaceObjectType.DynamicObject ||
					    objectTransform.Type != SpaceObjectType.PlayerPivot)
					{
					}

					if (artificialBody is null)
					{
						continue;
					}

					if (objectTransform.StabilizeToTargetGUID.HasValue &&
					    objectTransform.StabilizeToTargetGUID.Value > 0)
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
					    (MyPlayer.Instance.Parent as Ship).RadarSystem is not null)
					{
						(MyPlayer.Instance.Parent as Ship).RadarSystem.PassiveScanObject(artificialBody);
						SolarSystem.ArtificialBodiesVisiblityModified();
					}

					if (objectTransform.Forward != null && objectTransform.Up != null)
					{
						artificialBody.SetTargetPositionAndRotation(null, objectTransform.Forward.ToVector3(),
							objectTransform.Up.ToVector3(), flag || !artificialBody.IsInVisibilityRange,
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

							if (item.GUID == MyPlayer.Instance.GUID)
							{
								MyPlayer.Instance.ProcessMovementMessage(item);
								continue;
							}

							OtherPlayer player = GetPlayer(item.GUID);
							if (player is not null)
							{
								player.ProcessMovementMessage(item);
							}
						}
					}

					if (objectTransform.DynamicObjectsMovement != null)
					{
						foreach (DynamicObectMovementMessage item2 in objectTransform.DynamicObjectsMovement)
						{
							if (item2 != null)
							{
								DynamicObject dynamicObject = GetDynamicObject(item2.GUID);
								if (dynamicObject is not null)
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
								if (corpse is not null)
								{
									corpse.ProcessMoveCorpseObectMessage(item3);
								}
							}
						}
					}

					float num2 =
						(artificialBody.transform.position - MyPlayer.Instance.transform.position).sqrMagnitude -
						(float)(artificialBody.Radius * artificialBody.Radius);
					if (artificialBody is SpaceObjectVessel && (spaceObjectVessel is null || num > num2))
					{
						spaceObjectVessel = artificialBody as SpaceObjectVessel;
						num = num2;
					}
				}
			}

			if (artificialBodies.Count > 0)
			{
				foreach (ArtificialBody body in artificialBodies)
				{
					float num3 = (body.transform.position - MyPlayer.Instance.transform.position).sqrMagnitude -
					             (float)(body.Radius * body.Radius);
					if (body is SpaceObjectVessel && !(body as SpaceObjectVessel).IsDebrisFragment &&
					    (spaceObjectVessel == null || (num > num3 && (spaceObjectVessel.FTLEngine == null ||
					                                                  spaceObjectVessel.FTLEngine.Status !=
					                                                  SystemStatus.Online ||
					                                                  (spaceObjectVessel.Velocity -
					                                                   MyPlayer.Instance.Parent.Velocity).SqrMagnitude <
					                                                  900.0))))
					{
						spaceObjectVessel = body as SpaceObjectVessel;
						num = num3;
					}
				}
			}

			MyPlayer.Instance.NearestVessel = spaceObjectVessel;
			MyPlayer.Instance.NearestVesselSqDistance = num;
		}

		private void UpdateMapObject(ArtificialBody ab, CelestialBody oldParent)
		{
			if (ab is not SpaceObjectVessel vessel) return;

			MapObject mapObject;
			if (!Map.AllMapObjects.TryGetValue(vessel, out mapObject))
			{
				Map.InitializeMapObject(vessel);
				Map.AllMapObjects.TryGetValue(vessel, out mapObject);
			}

			if (mapObject is null)
			{
				return;
			}

			if (oldParent != vessel.Orbit.Parent.CelestialBody)
			{
				if (Map.gameObject.activeInHierarchy)
				{
					Map.UpdateParent(mapObject.MainObject);
				}

				if (MyPlayer.Instance is not null && this == MyPlayer.Instance.Parent)
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
					if (guid == MyPlayer.Instance.GUID)
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
					ArtificialBody corpse = SolarSystem.GetArtificialBody(guid);
					if (corpse is not null)
					{
						return corpse as Pivot;
					}

					break;
				}
				case SpaceObjectType.Ship:
				{
					ArtificialBody ship = SolarSystem.GetArtificialBody(guid);
					if (ship is not null)
					{
						return ship as Ship;
					}

					break;
				}
				case SpaceObjectType.Asteroid:
				{
					ArtificialBody asteroid = SolarSystem.GetArtificialBody(guid);
					if (asteroid is not null)
					{
						return asteroid as Asteroid;
					}

					break;
				}
			}

			return null;
		}

		public void MovePanelCursor(Transform trans, float panelWidth, float panelHeight)
		{
			float x = Mathf.Clamp(
				trans.localPosition.x + Mouse.current.delta.x.ReadValue() * Globals.Instance.MouseSpeedOnPanels, 0f,
				panelWidth);
			float y = Mathf.Clamp(
				trans.localPosition.y + Mouse.current.delta.y.ReadValue() * Globals.Instance.MouseSpeedOnPanels *
				(!Settings.Instance.SettingsData.ControlsSettings.InvertMouse ? 1 : (-1)), 0f, panelHeight);
			trans.localPosition = new Vector3(x, y, trans.localPosition.z);
		}

		public void DeleteCharacterRequest(ServerData gs)
		{
			GlobalGUI.ShowConfirmMessageBox(Localization.DeleteCharacter, Localization.AreYouSureDeleteCharacter,
				Localization.Yes, Localization.No, delegate
				{
					DeleteCharacterRequest deleteCharacterRequest = new DeleteCharacterRequest
					{
						ServerId = gs.Id,
						PlayerId = Nakama.NakamaIdCached
					};

					NetworkController.SendTcp(deleteCharacterRequest, gs.IpAddress, gs.StatusPort, false, true)
						.RunSynchronously();
				});
		}

		public void LatencyTestMessage()
		{
			_lastLatencyMessageTime = Time.realtimeSinceStartup;

			//int latency = await NetworkController.LatencyTest(LastConnectedServer.IpAddress, (int) LastConnectedServer.StatusPort);
			//_latencyMs = latency;

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
				if (spaceObjectVessel is not null && spaceObjectVessel.VesselData is not null)
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
		public void ReconnectAutoListener(EventSystem.InternalEventData data)
		{
			try
			{
				Reconnect();
			}
			catch (Exception ex)
			{
				Dbg.Error("Reconnecting after connection drop failed with exception: ", ex);
				OpenMainScreen();
			}
		}

		/// <summary>
		/// 	Reconnect after we have been disconnected.
		/// </summary>
		public void Reconnect()
		{
			// TODO: Call GameStarter.
		}

		public void OnLogin(LogInResponse logInResponse, ref VesselObjectID invitedToServerSpawnPointId)
		{
			SolarSystem.Set(GameObject.Find("/SolarSystemRoot/SunRoot").transform,
				GameObject.Find("/SolarSystemRoot/PlanetsRoot").transform, logInResponse.ServerTime);
			SolarSystem.LoadDataFromResources();
			MyPlayer.SpawnMyPlayer(logInResponse);

			if (logInResponse.IsAlive)
			{
				PlayerSpawnRequest playerSpawnRequest = new PlayerSpawnRequest
				{
					SpawPointParentID = 0L
				};
				NetworkController.Instance.SendToGameServer(playerSpawnRequest);
			}
			else if (invitedToServerSpawnPointId != null)
			{
				PlayerSpawnRequest playerSpawnRequest2 = new PlayerSpawnRequest
				{
					SpawPointParentID = invitedToServerSpawnPointId.VesselGUID
				};
				NetworkController.Instance.SendToGameServer(playerSpawnRequest2);
				invitedToServerSpawnPointId = null;
			}
			else
			{
				MainMenuGUI.SendSpawnRequest(new SpawnPointDetails
				{
					SpawnSetupType = SpawnSetupType.Start1,
					IsPartOfCrew = false,
					PlayersOnShip = new List<string>()
				});

				//MainMenuGUI.ShowSpawnPointSelection(logInResponse.SpawnPointsList, logInResponse.CanContinue);
			}

			foreach (DebrisFieldDetails debrisField in logInResponse.DebrisFields)
			{
				DebrisFields.Add(new DebrisField(this, debrisField));
				Map.InitializeMapObject(new DebrisField(this, debrisField));
			}

			ItemsIngredients = logInResponse.ItemsIngredients;
			Quests = logInResponse.Quests.Select((QuestData m) => new Quest(m, QuestCollection)).ToList();
			SpaceObjectVessel.VesselDecayRateMultiplier = logInResponse.VesselDecayRateMultiplier;
			ExposureRange = logInResponse.ExposureRange;
			_vesselExposureValues = logInResponse.VesselExposureValues;
			_playerExposureValues = logInResponse.PlayerExposureValues;
		}

		private void PlayerSpawnResponseListener(NetworkData data)
		{
			PlayerSpawnResponse s = data as PlayerSpawnResponse;
			Debug.Assert(s != null);
			if (s.Response == ResponseResult.Success)
			{
				GlobalGUI.ShowLoadingScreen(GlobalGUI.LoadingScreenType.Loading);
				SolarSystemRoot.SetActive(value: true);
				if (s.HomeGUID.HasValue)
				{
					MyPlayer.Instance.HomeStationGUID = s.HomeGUID.Value;
				}

				SceneLoader.LoadScenesWithIDs(s.Scenes);
				if (s.ParentType == SpaceObjectType.Ship)
				{
					Ship ship = Ship.Create(s.MainVesselID, s.VesselData, s.ParentTransform, isMainObject: true);
					ship.gameObject.SetActive(value: true);
					if (s.DockedVessels != null && s.DockedVessels.Count > 0)
					{
						foreach (DockedVesselData dockedVessel in s.DockedVessels)
						{
							Ship ship2 = Ship.Create(dockedVessel.GUID, dockedVessel.Data, s.ParentTransform,
								isMainObject: true);
							ship2.gameObject.SetActive(value: true);
							ship2.DockedToMainVessel = ship;
						}
					}

					MyPlayer.Instance.Parent = GetVessel(s.ParentID);
					Dbg.Log("Starting main scene load, Ship");
					StartCoroutine(LoadMainScenesCoroutine(s, ship, s.VesselObjects));
				}
				else if (s.ParentType == SpaceObjectType.Asteroid)
				{
					Asteroid asteroid = Asteroid.Create(s.ParentTransform, s.VesselData, isMainObject: true);
					asteroid.gameObject.SetActive(value: true);
					MyPlayer.Instance.Parent = asteroid;
					Dbg.Log("Starting main scene load, Asteroid");
					StartCoroutine(LoadMainScenesCoroutine(s, asteroid));
				}
				else if (s.ParentType == SpaceObjectType.PlayerPivot)
				{
					Pivot parent = Pivot.Create(SpaceObjectType.PlayerPivot, s.ParentTransform, isMainObject: true);
					MyPlayer.Instance.Parent = parent;
					LoadingFinishedTask = new Task(delegate { OnLoadingComplete(s); });
				}
				else
				{
					SceneManager.LoadScene(1);
					Dbg.Error("Unknown player parent", s.ParentType, s.ParentID);
					GlobalGUI.ShowMessageBox(Localization.SpawnErrorTitle, Localization.SpawnErrorMessage);
					MainMenuGUI.CanChooseSpawn = true;
				}

				if (s.TimeUntilServerRestart.HasValue)
				{
					ServerRestartTime = DateTime.UtcNow.AddSeconds(s.TimeUntilServerRestart.Value);
				}
				else
				{
					ServerRestartTime = null;
				}

				if (s.Quests != null)
				{
					foreach (QuestDetails quest in s.Quests)
					{
						MyPlayer.Instance.SetQuestDetails(quest, showNotifications: false, playCutScenes: false);
					}
				}

				if (s.Blueprints != null)
				{
					MyPlayer.Instance.Blueprints = s.Blueprints;
				}

				if (s.NavMapDetails == null)
				{
					return;
				}

				_restoreMapDetailsTask = new Task(delegate
				{
					foreach (UnknownMapObjectDetails det in s.NavMapDetails.Unknown)
					{
						SpaceObjectVessel spaceObjectVessel = null;
						if (det.SpawnRuleID != 0)
						{
							spaceObjectVessel = SolarSystem.ArtificialBodies.FirstOrDefault((ArtificialBody m) =>
									m is SpaceObjectVessel && (m as SpaceObjectVessel).IsMainVessel &&
									(m as SpaceObjectVessel).VesselData != null &&
									(m as SpaceObjectVessel).VesselData.SpawnRuleID == det.SpawnRuleID) as
								SpaceObjectVessel;
						}

						if (spaceObjectVessel is null)
						{
							spaceObjectVessel = GetVessel(det.GUID);
						}

						if (spaceObjectVessel is not null)
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

					SolarSystem.ArtificialBodiesVisiblityModified();
				});
			}
			else
			{
				GlobalGUI.ShowMessageBox(Localization.SpawnErrorTitle, Localization.SpawnErrorMessage);
				MainMenuGUI.CanChooseSpawn = true;
			}
		}

		private IEnumerator LoadMainScenesCoroutine(PlayerSpawnResponse s, Asteroid ast)
		{
			yield return StartCoroutine(ast.LoadScenesCoroutine(s.MiningPoints));
			LoadingFinishedTask = new Task(delegate { OnLoadingComplete(s); });
		}

		private IEnumerator LoadMainScenesCoroutine(PlayerSpawnResponse s, Ship sh, VesselObjects shipObjects)
		{
			if (sh is not null)
			{
				yield return StartCoroutine(sh.LoadShipScenesCoroutine(isMainShip: true, shipObjects));
				if (s.DockedVessels != null && s.DockedVessels.Count > 0)
				{
					foreach (DockedVesselData dockVess in s.DockedVessels)
					{
						Ship childShip = GetVessel(dockVess.GUID) as Ship;
						yield return StartCoroutine(
							childShip.LoadShipScenesCoroutine(isMainShip: true, dockVess.VesselObjects));
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

			if (s.DockedVessels != null && s.DockedVessels.Count > 0)
			{
				foreach (DockedVesselData dockedVessel in s.DockedVessels)
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

			LoadingFinishedTask = new Task(delegate { OnLoadingComplete(s); });
		}

		private void OnLoadingComplete(PlayerSpawnResponse s)
		{
			LoadingFinishedTask = null;
			MyPlayer.Instance.ActivatePlayer(s);
		}
	}
}
