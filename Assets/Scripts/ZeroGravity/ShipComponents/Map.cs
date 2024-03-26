using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenHellion;
using OpenHellion.Net;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using ZeroGravity.Data;
using ZeroGravity.Math;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.UI;

namespace ZeroGravity.ShipComponents
{
	public class Map : MonoBehaviour
	{
		[NonSerialized] public NavigationPanel NavPanel;

		public Dictionary<IMapMainObject, MapObject> AllMapObjects = new Dictionary<IMapMainObject, MapObject>();

		[Title("Prefabs")] public GameObject MapObjectCelestial;

		public GameObject MapObjectAsteroid;

		public GameObject MapObjectShip;

		public GameObject MapObjectDebrisField;

		public GameObject MapObjectCustomOrbit;

		public GameObject MapObjectWarpEnd;

		[FormerlySerializedAs("MapObjectWarpStart")]
		public GameObject MapObjectWarpStartNear;

		public GameObject MapObjectWarpStartFar;

		public GameObject MapObjectFuzzuScan;

		public GameObject MapManeuverCourse;

		public GameObject ScanEffectPrefab;

		private GameObject _mapScanEffect;

		public Transform MapObjectsRoot;

		public Camera MapCamera;

		[NonSerialized] public MapObject MyShip;

		[NonSerialized] public MapObject Home;

		private MapObject _sun;

		[Title("Zoom")] public AnimationCurve PanCurve;

		public AnimationCurve ZoomCurve;

		public double Scale = 1.0;

		public double zoom = 1.0;

		public float zoomStep = 0.1f;

		public float ClosestSunScale = 10f;

		public bool Focusing;

		public Vector3D Focus;

		public float TransitionDuration = 0.5f;

		public float ZoomLerpSpeed = 1f;

		private float _transitionTimer;

		public MapObject FocusObject;

		private MapObject _oldFocusObject;

		public Collider IndicatorObject;

		public MapObject SelectedObject;

		public MapObject DraggingObject;

		public ManeuverCourse DraggingManeuver;

		public bool Dragging;

		public float MinZoom = 2f;

		public float MaxZoom = 70000f;

		[SerializeField] private float planetMaxZoom = 1f;

		[Title("ROTATE")] public GameObject CameraPitch;

		[SerializeField] private float pitch;

		public GameObject CameraYawn;

		[SerializeField] private float yaw;

		public float minPitch;

		public float maxPitch;

		[Range(1f, 10f)] public float RotationStrenght = 10f;

		public float RotationSmoothness = 10f;

		public List<MapObjectCustomOrbit> AllCustomOrbits = new List<MapObjectCustomOrbit>();

		public ManeuverCourse WarpManeuver;

		private float _doubleClickTimer = 0.5f;

		public Light Sunlight;

		private Ray _ray;

		[HideInInspector]
		public Dictionary<long, OrbitParameters> UnknownVisibilityOrbits = new Dictionary<long, OrbitParameters>();

		[NonSerialized] public HashSet<long> KnownSpawnRuleIDs = new HashSet<long>();

		public bool AllObjectsVisible;

		public bool IsInitializing = true;

		[NonSerialized] public List<MapObject> VesselsGroup = new List<MapObject>();

		[NonSerialized] public List<MapObject> SelectedVesselsGroup = new List<MapObject>();

		[RequiredMember]
		[SerializeField] private World _world;

		private void Start()
		{
			_mapScanEffect = Instantiate(ScanEffectPrefab, MapObjectsRoot);
			_mapScanEffect.name = "ScanEffect";
			_mapScanEffect.SetActive(value: false);
			_mapScanEffect.SetLayerRecursively("Map");
		}

		private void Update()
		{
			if (_doubleClickTimer != 0.5f)
			{
				_doubleClickTimer -= Time.deltaTime;
			}

			if (_doubleClickTimer < 0f)
			{
				_doubleClickTimer = 0.5f;
			}

			if (Focusing)
			{
				Scale = zoom;
			}
			else
			{
				Scale = MathHelper.Lerp(Scale, zoom, Time.deltaTime * ZoomLerpSpeed);
			}

			if (!Focusing && FocusObject != null)
			{
				Focus = FocusObject.TruePosition;
			}

			RaycastHit raycastHit = default;
			_ray = MapCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
			LayerMask layerMask = 8192;
			Debug.DrawRay(_ray.origin, _ray.direction * 100f, Color.magenta);
			if (!Focusing && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
			{
				RaycastHit[] source = (from m in Physics.RaycastAll(_ray, float.PositiveInfinity, layerMask)
					where m.collider.GetComponentInParent<MapObject>() != null
					select m).ToArray();
				VesselsGroup = (from m in source.Select((Func<RaycastHit, MapObject>)((RaycastHit m) =>
						m.collider.GetComponentInParent<MapObjectVessel>()))
					where m?.MainObject != null
					orderby (m.TruePosition - MyPlayer.Instance.Parent.Position).SqrMagnitude
					select m).ToList();
				raycastHit = (from m in source
					orderby m.collider.GetComponentInParent<MapObjectFixedPosition>() != null, (m.collider
							.GetComponentInParent<MapObject>().TruePosition - MyPlayer.Instance.Parent.Position)
						.SqrMagnitude
					select m).FirstOrDefault();
				if (raycastHit.collider != null)
				{
					if (IndicatorObject == null)
					{
						IndicatorObject = raycastHit.collider;
						OnHover(IndicatorObject);
					}
					else if (IndicatorObject != raycastHit.collider && !Dragging)
					{
						OnUnhover(IndicatorObject);
						IndicatorObject = raycastHit.collider;
						OnHover(IndicatorObject);
					}

					if (Mouse.current.leftButton.wasPressedThisFrame &&
					    !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
					{
						OnClick(IndicatorObject);
						Dragging = true;
						if (_doubleClickTimer < 0.5f)
						{
							OnDoubleClick(IndicatorObject);
							OnRelease(IndicatorObject);
							Dragging = false;
						}

						_doubleClickTimer -= Time.deltaTime;
					}

					if (Mouse.current.rightButton.wasPressedThisFrame &&
					    !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
					{
						OnRightClick(IndicatorObject);
					}

					if (Mouse.current.leftButton.wasReleasedThisFrame)
					{
						OnRelease(IndicatorObject);
						Dragging = false;
					}
				}
				else
				{
					if (IndicatorObject != null && !Dragging)
					{
						OnUnhover(IndicatorObject);
						IndicatorObject = null;
					}

					if (Mouse.current.leftButton.wasPressedThisFrame &&
					    !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
					{
						OnClick(null);
					}

					if (Mouse.current.leftButton.wasReleasedThisFrame)
					{
						if (IndicatorObject != null)
						{
							OnUnhover(IndicatorObject);
							OnRelease(IndicatorObject);
						}

						Dragging = false;
					}
				}

				if (DraggingObject != null && DraggingObject.IsDragging)
				{
					DraggingObject.Dragging(_ray);
				}

				if (DraggingManeuver != null && DraggingManeuver.IsDragging)
				{
					DraggingManeuver.Dragging(_ray);
				}
			}

			// Map zoom.
			if (Mouse.current.scroll.y.ReadValue().IsNotEpsilonZero() &&
			    !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
			{
				float axis = Mouse.current.scroll.y.ReadValue();
				if (axis > 0f)
				{
					zoom *= 1f + zoomStep;
				}
				else if (axis < 0f)
				{
					zoom /= 1f + zoomStep;
				}

				zoom = MathHelper.Clamp(zoom, MinZoom, Mathf.Clamp(planetMaxZoom, 0f, MaxZoom));
			}

			// Map controls.
			if (Mouse.current.rightButton.isPressed &&
			    !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
			{
				float horizontalMouse = Mouse.current.delta.x.ReadValue() * 0.1f;
				float verticalMouse = Mouse.current.delta.y.ReadValue() * 0.1f;
				if (horizontalMouse.IsNotEpsilonZero() || verticalMouse.IsNotEpsilonZero())
				{
					if (Keyboard.current.leftCtrlKey.isPressed)
					{
						if (SelectedObject is MapObjectShip)
						{
							MapObjectShip mapObjectShip = SelectedObject as MapObjectShip;
							if (mapObjectShip.ScanningCone != null &&
							    mapObjectShip.ScanningCone.activeInHierarchy == true)
							{
								float scanningConePitch = (NavPanel.PitchAngle - verticalMouse + 360f) % 360f;
								float scanningConeYaw = (NavPanel.YawAngle + horizontalMouse + 360f) % 360f;
								NavPanel.SetScanningConePitch(scanningConePitch);
								NavPanel.SetScanningConeYaw(scanningConeYaw);
							}
						}
					}
					else
					{
						pitch += verticalMouse * RotationStrenght;
						yaw += horizontalMouse * RotationStrenght;
						pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
					}
				}
			}

			// Handle rotation.
			RotateCamera();

			if (_mapScanEffect.activeInHierarchy)
			{
				_mapScanEffect.transform.position = MyShip.Position.transform.position;
				_mapScanEffect.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f) * ((float)Scale / 1500f);
			}

			Sunlight.transform.forward = -_sun.Position.position;
		}

		public void OnHover(Collider collider)
		{
			string empty = string.Empty;
			MapIndicator component = collider.GetComponent<MapIndicator>();
			if (component != null)
			{
				component.SetIndicator(MapIndicator.IndicatorAction.hover);
				empty = component.name;
			}

			MapObject componentInParent = collider.GetComponentInParent<MapObject>();
			if (componentInParent != null)
			{
				if (componentInParent is MapObjectCelestial)
				{
					(componentInParent as MapObjectCelestial).ShowGravityInfluence();
				}

				empty = componentInParent.name;
				if (VesselsGroup.Count == 0)
				{
					NavPanel.ShowHoverInfo(new List<MapObject> { componentInParent });
				}
				else
				{
					NavPanel.ShowHoverInfo(VesselsGroup);
				}
			}
		}

		public void OnUnhover(Collider collider)
		{
			string empty = string.Empty;
			MapIndicator component = collider.GetComponent<MapIndicator>();
			if (component != null)
			{
				component.SetIndicator(MapIndicator.IndicatorAction.unhover);
				empty = component.name;
			}

			MapObject componentInParent = collider.GetComponentInParent<MapObject>();
			if (componentInParent != null)
			{
				if (componentInParent is MapObjectCelestial && componentInParent != SelectedObject)
				{
					(componentInParent as MapObjectCelestial).HideGravityInfluence();
				}

				empty = componentInParent.name;
			}

			NavPanel.HideHoverInfo();
		}

		public void OnClick(Collider collider)
		{
			if (collider == null)
			{
				UnselectMapObject(SelectedObject);
				SelectMapObject(null);
				return;
			}

			MapIndicator component = collider.GetComponent<MapIndicator>();
			if (component != null)
			{
				component.SetIndicator(MapIndicator.IndicatorAction.click);
			}

			MapObject componentInParent = collider.GetComponentInParent<MapObject>();
			if (componentInParent != null && componentInParent.CanBeDragged && !Dragging)
			{
				DraggingObject = componentInParent;
				DraggingObject.StartDragging(IndicatorObject.gameObject, _ray);
			}

			ManeuverCourse componentInParent2 = collider.GetComponentInParent<ManeuverCourse>();
			if (componentInParent2 != null && !Dragging)
			{
				DraggingManeuver = componentInParent2;
				DraggingManeuver.StartDragging(IndicatorObject.gameObject, _ray);
			}
		}

		public void OnRelease(Collider collider)
		{
			MapIndicator component = collider.GetComponent<MapIndicator>();
			if (component != null)
			{
				component.SetIndicator(MapIndicator.IndicatorAction.release);
			}

			MapObject componentInParent = collider.GetComponentInParent<MapObject>();
			if (componentInParent != null)
			{
				SelectMapObject(componentInParent);
			}

			if (DraggingObject != null && DraggingObject.IsDragging)
			{
				DraggingObject.StopDragging();
				DraggingObject = null;
			}

			if (DraggingManeuver != null && DraggingManeuver.IsDragging)
			{
				DraggingManeuver.StopDragging();
				DraggingManeuver = null;
			}
		}

		public void OnDoubleClick(Collider collider)
		{
			MapObject mapObject = collider.GetComponentInParent<MapObject>();
			if (!(mapObject != null))
			{
				return;
			}

			if (mapObject is MapObjectCustomOrbit)
			{
				FocusToObject(AllMapObjects.FirstOrDefault((KeyValuePair<IMapMainObject, MapObject> m) =>
					m.Key == (mapObject as MapObjectCustomOrbit).Orbit.Parent.CelestialBody).Value);
			}
			else
			{
				FocusToObject(mapObject);
			}
		}

		public void OnRightClick(Collider collider)
		{
			if (collider == null)
			{
				return;
			}

			MapObjectShip mapObjectShip = SelectedObject as MapObjectShip;
			if (collider != null && mapObjectShip?.ScanningCone?.activeInHierarchy == true)
			{
				MapObject componentInParent = collider.GetComponentInParent<MapObject>();
				if (componentInParent != null)
				{
					Quaternion quaternion = Quaternion.FromToRotation(Vector3.forward,
						(componentInParent.TruePosition - mapObjectShip.MainObject.Position).ToVector3());
					NavPanel.SetScanningConePitch(quaternion.eulerAngles.x);
					NavPanel.SetScanningConeYaw(quaternion.eulerAngles.y);
				}
			}
		}

		public void SelectMapObject(MapObject obj)
		{
			if (obj != null)
			{
				obj.UpdateVisibility();
			}

			if (SelectedObject != null)
			{
				UnselectMapObject(SelectedObject);
			}

			SelectedObject = obj;
			if (VesselsGroup.Contains(SelectedObject))
			{
				SelectedVesselsGroup = VesselsGroup;
			}

			if (VesselsGroup.Count > 1 && VesselsGroup.Contains(obj))
			{
				NavPanel.ActivateOther(3);
			}
			else
			{
				NavPanel.ActivateOther(0);
			}

			if (SelectedObject is MapObjectCelestial)
			{
				(SelectedObject as MapObjectCelestial).ShowGravityInfluence();
			}

			ShowAllChildObjects(SelectedObject);
		}

		public void UnselectMapObject(MapObject obj)
		{
			if (obj is MapObjectCelestial && IndicatorObject != obj)
			{
				(obj as MapObjectCelestial).HideGravityInfluence();
			}
		}

		public void OnInteract(Ship MyParentShip)
		{
			NavPanel = _world.InWorldPanels.Navigation;
			NavPanel.gameObject.SetActive(value: true);
			gameObject.SetActive(value: true);
			MapCamera.enabled = true;
			MyPlayer.Instance.FpsController.MainCamera.enabled = false;
			MyPlayer.Instance.FpsController.NearCamera.enabled = false;
			MyPlayer.Instance.FpsController.FarCamera.enabled = false;
			MyPlayer.Instance.PlanetsCameraRoot.gameObject.SetActive(value: false);
			MyPlayer.Instance.SunCameraRoot.gameObject.SetActive(value: false);
			_world.InGameGUI.QuestIndicators.AddMarkersOnMap();
			if (MyParentShip != null && AllMapObjects.TryGetValue(MyParentShip, out var value))
			{
				MyShip = value;
			}

			if (FocusObject == null)
			{
				FocusObject = MyShip;
			}

			foreach (MapObject value2 in AllMapObjects.Values)
			{
				value2.UpdateVisibility();
			}

			planetMaxZoom = 8.45228E+09f / FocusObject.Radius * ClosestSunScale;
			SpaceObjectVessel homeVessel = _world.GetVessel(MyPlayer.Instance.HomeStationGUID);
			if (homeVessel != null)
			{
				Home = AllMapObjects.FirstOrDefault((KeyValuePair<IMapMainObject, MapObject> m) =>
					m.Key.Guid == homeVessel.Guid || m.Key.Guid == homeVessel.MainVessel.Guid).Value;
			}

			ShowAllChildObjects(FocusObject);
			NavPanel.OnInteract(MyParentShip);
			SelectMapObject(FocusObject);
			_oldFocusObject = SelectedObject;
			Globals.ToggleCursor(true);
			_world.InGameGUI.OverlayCanvasIsOn = true;
			if (FocusObject == MyShip)
			{
				zoom = 8452280000.0 / FocusObject.MainObject.ParentCelesitalBody.Radius * ClosestSunScale / 2.0;
			}

			if (MyParentShip.RadarSystem != null)
			{
				MyParentShip.RadarSystem.PassiveScan();
			}
		}

		public void OnDetach()
		{
			gameObject.SetActive(value: false);
			MapCamera.enabled = false;
			MyPlayer.Instance.FpsController.MainCamera.enabled = true;
			MyPlayer.Instance.FpsController.NearCamera.enabled = true;
			MyPlayer.Instance.FpsController.FarCamera.enabled = true;
			MyPlayer.Instance.PlanetsCameraRoot.gameObject.SetActive(value: true);
			MyPlayer.Instance.SunCameraRoot.gameObject.SetActive(value: true);
			NavPanel.OnDetach();
			Globals.ToggleCursor(false);
			_world.InGameGUI.OverlayCanvasIsOn = false;
			_world.InGameGUI.QuestIndicators.RemoveMarkersOnMap();
		}

		public void InitialiseMapObject(IMapMainObject obj, CelestialBodyData cbd = null)
		{
			if (AllMapObjects.ContainsKey(obj))
			{
				UpdateObjectData(obj);
			}
			else if (obj is CelestialBody celestialBody)
			{
				GameObject celestialBodyObject = Instantiate(MapObjectCelestial, MapObjectsRoot);
				celestialBodyObject.name = celestialBody.Name;
				MapObjectCelestial mapObjectCelestial = celestialBodyObject.GetComponent<MapObjectCelestial>();
				mapObjectCelestial.OnCreate(_world, celestialBody);
				mapObjectCelestial.CelestialBodyData = cbd;
				if (celestialBody.Guid == 1)
				{
					_sun = mapObjectCelestial;
				}
				mapObjectCelestial.UpdateVisibility();

				UpdateCelestialParent(celestialBody);
				AllMapObjects[celestialBody] = mapObjectCelestial;
			}
			else if (obj is Ship ship)
			{
				if (!ship.IsDebrisFragment)
				{
					GameObject shipObject = Instantiate(MapObjectShip, MapObjectsRoot);
					shipObject.name = ship.CustomName;
					MapObjectShip mapObjectShip = shipObject.GetComponent<MapObjectShip>();
					mapObjectShip.OnCreate(_world, ship);
					if (ship.Guid == MyPlayer.Instance.HomeStationGUID)
					{
						Home = mapObjectShip;
					}

					mapObjectShip.gameObject.SetActive(ship.IsMainVessel);
					if (obj.Orbit.Parent == null)
					{
						Debug.LogWarning("Ship orbit has no parent.");
					}

					mapObjectShip.UpdateObject();
					mapObjectShip.UpdateVisibility();

					UpdateParent(ship);
					AllMapObjects[ship] = mapObjectShip;
				}
			}
			else if (obj is Asteroid asteroid)
			{
				GameObject asteroidObject = Instantiate(MapObjectAsteroid, MapObjectsRoot);
				asteroidObject.name = asteroid.CustomName;
				MapObjectAsteroid mapObjectAsteroid = asteroidObject.GetComponent<MapObjectAsteroid>();
				mapObjectAsteroid.OnCreate(_world, asteroid);
				mapObjectAsteroid.UpdateObject();
				mapObjectAsteroid.UpdateVisibility();

				UpdateParent(asteroid);
				AllMapObjects[asteroid] = mapObjectAsteroid;
			}
			else if (obj is DebrisField debrisField)
			{
				GameObject debrisFieldObject = Instantiate(MapObjectDebrisField, MapObjectsRoot);
				debrisFieldObject.name = debrisField.Name;
				MapObjectDebrisField mapObjectDebrisField = debrisFieldObject.GetComponent<MapObjectDebrisField>();
				mapObjectDebrisField.OnCreate(_world, debrisField);
				mapObjectDebrisField.UpdateVisibility();

				UpdateParent(debrisField);
				AllMapObjects[debrisField] = mapObjectDebrisField;
			}
		}

		public void InitializeMapObjectEndWarp(Vector3D position, double minMaxScale)
		{
			GameObject fixedPositionObject = Instantiate(MapObjectWarpEnd, MapObjectsRoot);
			MapObjectFixedPosition mapObjectFixedPosition = fixedPositionObject.GetComponent<MapObjectFixedPosition>();
			mapObjectFixedPosition.FixedPosition = position;
			mapObjectFixedPosition.MinMaxScale = minMaxScale;
			mapObjectFixedPosition.Name = Localization.WarpSignature.ToUpper();
			mapObjectFixedPosition.Description = Localization.WarpSignatureDescription;
			fixedPositionObject.Activate(value: true);
		}

		public void InitializeMapObjectFuzzyScan(Vector3D position, double scale, SpaceObjectVessel vessel)
		{
			GameObject fuzzyScanObject = Instantiate(MapObjectFuzzuScan, MapObjectsRoot);
			MapObjectFuzzyScan mapObjectFuzzyScan = fuzzyScanObject.GetComponent<MapObjectFuzzyScan>();
			mapObjectFuzzyScan.FixedPosition = position;
			mapObjectFuzzyScan.MinMaxScale = (scale - mapObjectFuzzyScan.MinScale) /
			                                 (mapObjectFuzzyScan.MaxScale - mapObjectFuzzyScan.MinScale);
			mapObjectFuzzyScan.Name = Localization.UnidentifiedObject.ToUpper();
			mapObjectFuzzyScan.Description = Localization.UnidentifiedObjectDescription;
			mapObjectFuzzyScan.Vessels.Add(vessel);
			fuzzyScanObject.Activate(value: true);
		}

		public void InitializeMapObjectStartWarpNear(Vector3D position, Quaternion rotation)
		{
			GameObject warpStartObject = Instantiate(MapObjectWarpStartNear, MapObjectsRoot);
			MapObjectWarpStart mapObjectWarpStart = warpStartObject.GetComponent<MapObjectWarpStart>();
			mapObjectWarpStart.WarpStartPosition = position;
			mapObjectWarpStart.WarpConeRotation = rotation;
			mapObjectWarpStart.Name = Localization.WarpSignature.ToUpper();
			warpStartObject.Activate(value: true);
		}

		public void InitializeMapObjectStartWarpFar(Vector3D position, double minMaxScale)
		{
			GameObject fixedPositionObject = Instantiate(MapObjectWarpStartFar, MapObjectsRoot);
			MapObjectFixedPosition mapObjectFixedPosition = fixedPositionObject.GetComponent<MapObjectFixedPosition>();
			mapObjectFixedPosition.FixedPosition = position;
			mapObjectFixedPosition.MinMaxScale = minMaxScale;
			mapObjectFixedPosition.Name = Localization.WarpSignature.ToUpper();
			mapObjectFixedPosition.Description = Localization.WarpSignatureDescription;
			fixedPositionObject.Activate(value: true);
		}

		public void UpdateObjectData(IMapMainObject mapObject)
		{
			if (mapObject is CelestialBody)
			{
				UpdateCelestialParent(mapObject);
				return;
			}

			AllMapObjects.TryGetValue(mapObject, out var value);
			if (mapObject is Ship ship)
			{
				if (value != null)
				{
					value.name = ship.CustomName;
				}

				UpdateParent(ship);
			}
			else
			{
				Asteroid asteroid = mapObject as Asteroid;
				if (value != null)
				{
					value.name = asteroid.CustomName;
				}

				UpdateParent(mapObject);
			}
		}

		public void UpdateCelestialParent(IMapMainObject obj)
		{
			if (obj.Guid != 1 && obj.Orbit.Parent.CelestialBody != null &&
			    AllMapObjects.ContainsKey(obj.Orbit.Parent.CelestialBody) &&
			    AllMapObjects.TryGetValue(obj, out var value) &&
			    AllMapObjects.TryGetValue(obj.Orbit.Parent.CelestialBody, out var value2))
			{
				value.transform.SetParent((value2 as MapObjectCelestial).CelestialObjects.transform);
				value.transform.localPosition = Vector3.zero;
				value.Visual.localPosition = Vector3.zero;
			}
		}

		public void UpdateParent(IMapMainObject obj)
		{
			if (obj.Orbit.Parent.CelestialBody != null && AllMapObjects.ContainsKey(obj.Orbit.Parent.CelestialBody) &&
			    AllMapObjects.TryGetValue(obj, out var value) &&
			    AllMapObjects.TryGetValue(obj.Orbit.Parent.CelestialBody, out var value2))
			{
				value.transform.SetParent((value2 as MapObjectCelestial).ChildObjects.transform);
				value.transform.localPosition = Vector3.zero;
				value.Visual.localPosition = Vector3.zero;
			}
		}

		public void RemoveMapObject(IMapMainObject obj)
		{
			if (AllMapObjects.TryGetValue(obj, out var value) && value != null)
			{
				Destroy(value.gameObject);
			}

			AllMapObjects.Remove(obj);
		}

		private IEnumerator UpdateFocus()
		{
			planetMaxZoom = 8.45228E+09f / FocusObject.Radius * ClosestSunScale;
			if (_oldFocusObject == FocusObject)
			{
				Focus = FocusObject.TruePosition;
			}
			else if (FocusObject != _oldFocusObject && !Focusing)
			{
				Focusing = true;
				Vector3D startPosition = Vector3D.Zero;
				float startZoom = (float)zoom;
				if (_oldFocusObject != null)
				{
					startPosition = _oldFocusObject.TruePosition;
				}

				while (_transitionTimer < TransitionDuration)
				{
					if (zoom > planetMaxZoom)
					{
						zoom = startZoom * Mathf.Pow(planetMaxZoom / startZoom,
							ZoomCurve.Evaluate(_transitionTimer / TransitionDuration));
					}

					Focus = Vector3D.Lerp(startPosition, FocusObject.TruePosition,
						PanCurve.Evaluate(_transitionTimer / TransitionDuration));
					_transitionTimer += Time.deltaTime;
					yield return null;
				}

				_transitionTimer = 0f;
				if (startZoom > planetMaxZoom)
				{
					zoom = planetMaxZoom;
				}

				_oldFocusObject = FocusObject;
				Focus = FocusObject.TruePosition;
				Focusing = false;
			}

			Focusing = false;
		}

		private void ShowAllChildObjects(MapObject obj)
		{
			if (obj == _sun)
			{
				(obj as MapObjectCelestial).ChildObjects.gameObject.SetActive(value: true);
				MapObjectCelestial[] componentsInChildren = (obj as MapObjectCelestial).CelestialObjects
					.GetComponentsInChildren<MapObjectCelestial>();
				foreach (MapObjectCelestial mapObjectCelestial in componentsInChildren)
				{
					mapObjectCelestial.ChildObjects.gameObject.SetActive(value: false);
				}
			}
			else
			{
				if (obj == null || obj.Orbit == null || obj.Orbit.Parent.CelestialBody == null)
				{
					return;
				}

				if (obj.Orbit.Parent.CelestialBody == _sun.MainObject)
				{
					ToggleAllChildObjects(obj, show: true);
					return;
				}

				MapObject value = AllMapObjects.FirstOrDefault((KeyValuePair<IMapMainObject, MapObject> m) =>
					m.Key == obj.Orbit.Parent.CelestialBody).Value;
				if (value.Orbit.Parent != null && value.Orbit.Parent.CelestialBody == _sun.MainObject)
				{
					ToggleAllChildObjects(value, show: true);
				}
				else
				{
					ShowAllChildObjects(value);
				}
			}
		}

		private void ToggleAllChildObjects(MapObject obj, bool show)
		{
			if (obj is MapObjectCelestial)
			{
				MapObjectCelestial mapObjectCelestial = obj as MapObjectCelestial;
				mapObjectCelestial.ChildObjects.gameObject.SetActive(show);
				MapObjectCelestial[] componentsInChildren =
					mapObjectCelestial.CelestialObjects.GetComponentsInChildren<MapObjectCelestial>();
				foreach (MapObjectCelestial mapObjectCelestial2 in componentsInChildren)
				{
					mapObjectCelestial2.ChildObjects.gameObject.SetActive(show);
				}
			}
			else
			{
				(_sun as MapObjectCelestial).ChildObjects.gameObject.SetActive(value: true);
			}
		}

		private void RotateCamera()
		{
			if (CameraPitch.transform.localRotation.IsEpsilonEqual(Quaternion.Euler(0f - pitch, 0f, 0f), 1E-05f))
			{
				CameraPitch.transform.localRotation = Quaternion.Euler(0f - pitch, 0f, 0f);
			}
			else
			{
				CameraPitch.transform.localRotation = Quaternion.Lerp(CameraPitch.transform.localRotation,
					Quaternion.Euler(0f - pitch, 0f, 0f), Time.deltaTime * RotationSmoothness);
			}

			if (CameraYawn.transform.localRotation.IsEpsilonEqual(Quaternion.Euler(0f, yaw, 0f), 1E-05f))
			{
				CameraYawn.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
			}
			else
			{
				CameraYawn.transform.localRotation = Quaternion.Lerp(CameraYawn.transform.localRotation,
					Quaternion.Euler(0f, yaw, 0f), Time.deltaTime * RotationSmoothness);
			}
		}

		public void FocusToObject(MapObject obj)
		{
			if (!Focusing)
			{
				obj.UpdateVisibility();
				FocusObject = obj;
				StartCoroutine(UpdateFocus());
			}
		}

		public void FocusToParentVessel()
		{
			FocusObject = MyShip;
			SelectedObject = MyShip;
			StartCoroutine(UpdateFocus());
			MyShip.UpdateVisibility();
		}

		public void FocusToHome()
		{
			if (Home != null)
			{
				SelectedObject = Home;
				FocusObject = Home;
				StartCoroutine(UpdateFocus());
			}
		}

		public void CreateCustomOrbit(bool useParentNameInName = true, string orbitName = "CustomOrbit")
		{
			if (SelectedObject is MapObjectCelestial)
			{
				GameObject customOrbitObject = Instantiate(MapObjectCustomOrbit, MapObjectsRoot);
				if (useParentNameInName)
				{
					customOrbitObject.name = orbitName + " around " + SelectedObject.name;
				}
				else
				{
					customOrbitObject.name = orbitName;
				}

				MapObjectCustomOrbit newCustomOrbit = customOrbitObject.GetComponent<MapObjectCustomOrbit>();
				AllCustomOrbits.Add(newCustomOrbit);
				newCustomOrbit.transform.SetParent((SelectedObject as MapObjectCelestial).ChildObjects.transform);
				newCustomOrbit.transform.localPosition = Vector3.zero;
				newCustomOrbit.Visual.localPosition = Vector3.zero;
				newCustomOrbit.CreateOrbit(SelectedObject);
				newCustomOrbit.CreateVisual();
				SelectMapObject(newCustomOrbit);
				FocusToObject(AllMapObjects.FirstOrDefault((KeyValuePair<IMapMainObject, MapObject> m) =>
					m.Key == newCustomOrbit.Orbit.Parent.CelestialBody).Value);
				SaveMapDetails();
			}
		}

		public void RemoveCustomOrbit()
		{
			if (SelectedObject is MapObjectCustomOrbit)
			{
				AllCustomOrbits.Remove(SelectedObject as MapObjectCustomOrbit);
				SelectedObject.gameObject.DestroyAll();
				SelectedObject = null;
				SaveMapDetails();
			}
		}

		public void CreateManeuverCourse()
		{
			if (SelectedObject != null && SelectedObject != MyShip && !(SelectedObject is MapObjectCelestial))
			{
				RemoveManeuverCourse();
				GameObject gameObject = Instantiate(MapManeuverCourse, MapObjectsRoot);
				gameObject.SetLayerRecursively("Map");
				gameObject.name = "Warp Maneuver";
				WarpManeuver = gameObject.GetComponent<ManeuverCourse>();
				WarpManeuver.MyShipOrbit = MyShip.Orbit;
				WarpManeuver.EndOrbit = SelectedObject.Orbit;
				WarpManeuver.TargetVessel = SelectedObject.MainObject as SpaceObjectVessel;
				WarpManeuver.Initialize();
			}
		}

		public void RemoveManeuverCourse()
		{
			if (WarpManeuver is not null)
			{
				Destroy(WarpManeuver.gameObject);
				WarpManeuver = null;
			}
		}

		public void InitializeManeuverCourse()
		{
			(MyShip.MainObject as Ship).SendManeuverCourseRequest();
		}

		public ManeuverCourseRequest GetManeuverCourseRequestData(bool lockCourse)
		{
			if (WarpManeuver == null)
			{
				return null;
			}

			ManeuverCourseRequest maneuverCourseRequest = new ManeuverCourseRequest();
			maneuverCourseRequest.CourseGUID = WarpManeuver.GUID;
			maneuverCourseRequest.CourseItems = new List<CourseItemData>();
			CourseItemData courseItemData = new CourseItemData();
			courseItemData.GUID = WarpManeuver.Transfer.GUID;
			courseItemData.Type = WarpManeuver.Transfer.Type;
			courseItemData.WarpIndex = WarpManeuver.Transfer.WarpIndex;
			courseItemData.WarpCells = WarpManeuver.Transfer.WarpCells;
			courseItemData.EndOrbitAngle = WarpManeuver.Transfer.EndOrbitAngle;
			courseItemData.StartOrbitAngle = WarpManeuver.Transfer.StartOrbitAngle;
			courseItemData.StartSolarSystemTime = WarpManeuver.Transfer.StartSolarSystemTime;
			courseItemData.EndSolarSystemTime = WarpManeuver.Transfer.EndSolarSystemTime;
			courseItemData.TravelTime = ((WarpManeuver.Transfer.Type != ManeuverType.Transfer)
				? 0f
				: WarpManeuver.Transfer.TravelTime);
			CourseItemData courseItemData2 = courseItemData;
			courseItemData2.StartOrbit = new OrbitData();
			courseItemData2.EndOrbit = new OrbitData();
			WarpManeuver.StartOrbit.FillOrbitData(ref courseItemData2.StartOrbit);
			WarpManeuver.EndOrbit.FillOrbitData(ref courseItemData2.EndOrbit, WarpManeuver.TargetVessel);
			maneuverCourseRequest.CourseItems.Add(courseItemData2);
			return maneuverCourseRequest;
		}

		private void OnEnable()
		{
			_world.InGameGUI.HelmetHud.gameObject.Activate(value: false);
		}

		private void OnDisable()
		{
			_world.InGameGUI.HelmetHud.gameObject.Activate(value: true);
		}

		public void ShowScanningEffect()
		{
			if (MyShip is MapObjectShip { ScanningEffectCone: not null } ship)
			{
				ship.ScanningEffectCone.transform.rotation = ship.ScanningCone.transform.rotation;
				ship.ScanningEffectCone.transform.localScale =
					ship.ScanningCone.transform.localScale *
					ship.ScanningEffectConeScaleMultiplier;
				ship.ScanningEffectCone.Activate(value: true);
			}
			else
			{
				Debug.LogWarning("Tried to show scanning effect, but scanning effect cone is null.");
			}
		}

		public void HideScanningEffect()
		{
			if (MyShip is MapObjectShip { ScanningEffectCone: not null } ship)
			{
				ship.ScanningEffectCone.Activate(value: false);
			}
			else
			{
				Debug.LogError("Tried to hide scanning effect, but scanning effect cone is null.");
			}
		}

		public void SaveMapDetails()
		{
			NavigationMapDetails navigationMapDetails = new NavigationMapDetails();
			navigationMapDetails.Unknown = (from m in AllMapObjects
				where m.Key is SpaceObjectVessel && m.Value.RadarVisibilityType == RadarVisibilityType.Unknown
				select new UnknownMapObjectDetails
				{
					GUID = m.Key.Guid,
					SpawnRuleID = (m.Key as SpaceObjectVessel).VesselData.SpawnRuleID,
					LastKnownOrbit =
						(m.Key as SpaceObjectVessel).LastKnownMapOrbit.GetOrbitData(m.Key as SpaceObjectVessel)
				}).ToList();
			NetworkController.SendToGameServer(new NavigationMapDetailsMessage
			{
				NavMapDetails = navigationMapDetails
			});
		}
	}
}
