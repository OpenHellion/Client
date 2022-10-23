using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
		[HideInInspector]
		public NavigationPanel Panel;

		public Dictionary<IMapMainObject, MapObject> AllMapObjects = new Dictionary<IMapMainObject, MapObject>();

		[Header("PREFABS")]
		public GameObject MapObjectCelestial;

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

		public GameObject MapScanEffect;

		public Transform MapObjectsRoot;

		public Camera MapCamera;

		public MapObject MyShip;

		public MapObject Home;

		public MapObject Sun;

		[Header("ZOOM")]
		public AnimationCurve PanCurve;

		public AnimationCurve ZoomCurve;

		public double Scale = 1.0;

		public double zoom = 1.0;

		public float zoomStep = 0.1f;

		public float ClosestSunScale = 10f;

		public bool Focusing;

		public Vector3D Focus;

		public float TransitionDuration = 0.5f;

		public float ZoomLerpSpeed = 1f;

		private float TransitionTimer;

		public MapObject FocusObject;

		private MapObject OldFocusObject;

		public Collider IndicatorObject;

		public MapObject SelectedObject;

		public MapObject DraggingObject;

		public ManeuverCourse DraggingManeuver;

		public bool Dragging;

		public float MinZoom = 2f;

		public float MaxZoom = 70000f;

		[SerializeField]
		private float planetMaxZoom = 1f;

		[Header("ROTATE")]
		public GameObject CameraPitch;

		[SerializeField]
		private float pitch;

		public GameObject CameraYawn;

		[SerializeField]
		private float yaw;

		public float minPitch;

		public float maxPitch;

		[Range(1f, 10f)]
		public float RotationStrenght = 10f;

		public float RotationSmoothness = 10f;

		private List<MapObject> objectsToUpdate = new List<MapObject>();

		public List<MapObjectCustomOrbit> AllCustomOrbits = new List<MapObjectCustomOrbit>();

		public ManeuverCourse WarpManeuver;

		public bool HaveManeuver;

		private float doubleClickTimer = 0.5f;

		public Light Sunlight;

		private Ray ray;

		public Dictionary<long, OrbitParameters> UnknownVisibilityOrbits = new Dictionary<long, OrbitParameters>();

		[NonSerialized]
		public HashSet<long> KnownSpawnRuleIDs = new HashSet<long>();

		public bool AllObjectsVisible;

		public bool IsInitializing = true;

		[NonSerialized]
		public List<MapObject> VesselsGroup = new List<MapObject>();

		[NonSerialized]
		public List<MapObject> SelectedVesselsGroup = new List<MapObject>();

		private void Start()
		{
			MapScanEffect = UnityEngine.Object.Instantiate(ScanEffectPrefab, MapObjectsRoot);
			MapScanEffect.name = "ScanEffect";
			MapScanEffect.SetActive(value: false);
			MapScanEffect.SetLayerRecursively("Map");
		}

		private void Update()
		{
			if (doubleClickTimer != 0.5f)
			{
				doubleClickTimer -= Time.deltaTime;
			}
			if (doubleClickTimer < 0f)
			{
				doubleClickTimer = 0.5f;
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
			RaycastHit raycastHit = default(RaycastHit);
			ray = MapCamera.ScreenPointToRay(Input.mousePosition);
			LayerMask layerMask = 8192;
			Debug.DrawRay(ray.origin, ray.direction * 100f, Color.magenta);
			if (!Focusing && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
			{
				RaycastHit[] source = (from m in Physics.RaycastAll(ray, float.PositiveInfinity, layerMask)
					where m.collider.GetComponentInParent<MapObject>() != null
					select m).ToArray();
				VesselsGroup = (from m in ((IEnumerable<RaycastHit>)source).Select((Func<RaycastHit, MapObject>)((RaycastHit m) => m.collider.GetComponentInParent<MapObjectVessel>()))
					where m?.MainObject != null
					orderby (m.TruePosition - MyPlayer.Instance.Parent.Position).SqrMagnitude
					select m).ToList();
				raycastHit = (from m in source
					orderby m.collider.GetComponentInParent<MapObjectFixedPosition>() != null, (m.collider.GetComponentInParent<MapObject>().TruePosition - MyPlayer.Instance.Parent.Position).SqrMagnitude
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
					if (InputManager.GetButtonDown(InputManager.AxisNames.Mouse1) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
					{
						OnClick(IndicatorObject);
						Dragging = true;
						if (doubleClickTimer < 0.5f)
						{
							OnDoubleClick(IndicatorObject);
							OnRelease(IndicatorObject);
							Dragging = false;
						}
						doubleClickTimer -= Time.deltaTime;
					}
					if (InputManager.GetButtonDown(InputManager.AxisNames.Mouse2) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
					{
						OnRightClick(IndicatorObject);
					}
					if (InputManager.GetButtonUp(InputManager.AxisNames.Mouse1))
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
					if (InputManager.GetButtonDown(InputManager.AxisNames.Mouse1) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
					{
						OnClick(null);
					}
					if (InputManager.GetButtonUp(InputManager.AxisNames.Mouse1))
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
					DraggingObject.Dragging(ray);
				}
				if (DraggingManeuver != null && DraggingManeuver.IsDragging)
				{
					DraggingManeuver.Dragging(ray);
				}
			}
			if (InputManager.GetAxis(InputManager.AxisNames.MouseWheel).IsNotEpsilonZero() && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
			{
				float axis = InputManager.GetAxis(InputManager.AxisNames.MouseWheel);
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
			if (InputManager.GetButton(InputManager.AxisNames.Mouse2) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
			{
				float axis2 = InputManager.GetAxis(InputManager.AxisNames.LookHorizontal);
				float axis3 = InputManager.GetAxis(InputManager.AxisNames.LookVertical);
				if (axis2.IsNotEpsilonZero() || axis3.IsNotEpsilonZero())
				{
					if (InputManager.GetKey(KeyCode.LeftControl))
					{
						MapObjectShip mapObjectShip = SelectedObject as MapObjectShip;
						if (mapObjectShip?.ScanningCone?.activeInHierarchy == true)
						{
							float scanningConePitch = (Client.Instance.Map.Panel.PitchAngle - axis3 + 360f) % 360f;
							float scanningConeYaw = (Client.Instance.Map.Panel.YawAngle + axis2 + 360f) % 360f;
							mapObjectShip.Map.Panel.SetScanningConePitch(scanningConePitch);
							mapObjectShip.Map.Panel.SetScanningConeYaw(scanningConeYaw);
						}
					}
					else
					{
						pitch += axis3 * RotationStrenght;
						yaw += axis2 * RotationStrenght;
						pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
					}
				}
			}
			RotateCamera();
			if (MapScanEffect.activeInHierarchy)
			{
				MapScanEffect.transform.position = MyShip.Position.transform.position;
				MapScanEffect.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f) * ((float)Scale / 1500f);
			}
			Sunlight.transform.forward = -Sun.Position.position;
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
					Panel.ShowHoverInfo(new List<MapObject> { componentInParent });
				}
				else
				{
					Panel.ShowHoverInfo(VesselsGroup);
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
			Panel.HideHoverInfo();
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
				DraggingObject.StartDragging(IndicatorObject.gameObject, ray);
			}
			ManeuverCourse componentInParent2 = collider.GetComponentInParent<ManeuverCourse>();
			if (componentInParent2 != null && !Dragging)
			{
				DraggingManeuver = componentInParent2;
				DraggingManeuver.StartDragging(IndicatorObject.gameObject, ray);
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
				FocusToObject(AllMapObjects.FirstOrDefault((KeyValuePair<IMapMainObject, MapObject> m) => m.Key == (mapObject as MapObjectCustomOrbit).Orbit.Parent.CelestialBody).Value);
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
					Quaternion quaternion = Quaternion.FromToRotation(Vector3.forward, (componentInParent.TruePosition - mapObjectShip.MainObject.Position).ToVector3());
					mapObjectShip.Map.Panel.SetScanningConePitch(quaternion.eulerAngles.x);
					mapObjectShip.Map.Panel.SetScanningConeYaw(quaternion.eulerAngles.y);
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
				Panel.ActivateOther(3);
			}
			else
			{
				Panel.ActivateOther(0);
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
			Panel = Client.Instance.InGamePanels.Navigation;
			Panel.gameObject.SetActive(value: true);
			base.gameObject.SetActive(value: true);
			MapCamera.enabled = true;
			MyPlayer.Instance.FpsController.MainCamera.enabled = false;
			MyPlayer.Instance.FpsController.NearCamera.enabled = false;
			MyPlayer.Instance.FpsController.FarCamera.enabled = false;
			MyPlayer.Instance.PlanetsCameraRoot.gameObject.SetActive(value: false);
			MyPlayer.Instance.SunCameraRoot.gameObject.SetActive(value: false);
			Client.Instance.CanvasManager.CanvasUI.QuestIndicators.AddMarkersOnMap();
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
			SpaceObjectVessel homeVessel = Client.Instance.GetVessel(MyPlayer.Instance.HomeStationGUID);
			if (homeVessel != null)
			{
				Home = AllMapObjects.FirstOrDefault((KeyValuePair<IMapMainObject, MapObject> m) => m.Key.GUID == homeVessel.GUID || m.Key.GUID == homeVessel.MainVessel.GUID).Value;
			}
			ShowAllChildObjects(FocusObject);
			Panel.OnInteract(MyParentShip);
			SelectMapObject(FocusObject);
			OldFocusObject = SelectedObject;
			Client.Instance.ToggleCursor(true);
			Client.Instance.CanvasManager.OverlayCanvasIsOn = true;
			Client.Instance.InputModule.ToggleCustomCursorPosition(val: false);
			if (FocusObject == MyShip)
			{
				zoom = 8452280000.0 / FocusObject.MainObject.ParentCelesitalBody.Radius * (double)ClosestSunScale / 2.0;
			}
			if (MyParentShip.RadarSystem != null)
			{
				MyParentShip.RadarSystem.PassiveScan();
			}
		}

		public void OnDetach()
		{
			base.gameObject.SetActive(value: false);
			MapCamera.enabled = false;
			MyPlayer.Instance.FpsController.MainCamera.enabled = true;
			MyPlayer.Instance.FpsController.NearCamera.enabled = true;
			MyPlayer.Instance.FpsController.FarCamera.enabled = true;
			MyPlayer.Instance.PlanetsCameraRoot.gameObject.SetActive(value: true);
			MyPlayer.Instance.SunCameraRoot.gameObject.SetActive(value: true);
			Panel.OnDetach();
			Client.Instance.ToggleCursor(false);
			Client.Instance.CanvasManager.OverlayCanvasIsOn = false;
			Client.Instance.InputModule.ToggleCustomCursorPosition(val: true);
			Client.Instance.CanvasManager.CanvasUI.QuestIndicators.RemoveMarkersOnMap();
		}

		public void InitializeMapObject(IMapMainObject obj, CelestialBodyData cbd = null)
		{
			if (AllMapObjects.ContainsKey(obj))
			{
				UpdateObjectData(obj);
			}
			else if (obj is CelestialBody)
			{
				CelestialBody celestialBody = obj as CelestialBody;
				GameObject gameObject = UnityEngine.Object.Instantiate(MapObjectCelestial, MapObjectsRoot);
				gameObject.name = celestialBody.Name;
				MapObjectCelestial component = gameObject.GetComponent<MapObjectCelestial>();
				component.MainObject = obj;
				component.cbd = cbd;
				AllMapObjects[obj] = component;
				UpdateCelestialParent(obj);
				if (celestialBody.GUID == 1)
				{
					Sun = component;
				}
				component.UpdateVisibility();
			}
			else if (obj is Ship)
			{
				Ship ship = obj as Ship;
				if (!ship.IsDebrisFragment)
				{
					GameObject gameObject2 = UnityEngine.Object.Instantiate(MapObjectShip, MapObjectsRoot);
					gameObject2.name = ship.CustomName;
					MapObjectShip component2 = gameObject2.GetComponent<MapObjectShip>();
					component2.MainObject = obj;
					AllMapObjects[obj] = component2;
					UpdateParent(obj);
					if (ship.GUID == MyPlayer.Instance.HomeStationGUID)
					{
						Home = component2;
					}
					component2.gameObject.SetActive(ship.IsMainVessel);
					if (obj.Orbit.Parent == null)
					{
					}
					component2.UpdateObject();
					component2.UpdateVisibility();
				}
			}
			else if (obj is Asteroid)
			{
				Asteroid asteroid = obj as Asteroid;
				GameObject gameObject3 = UnityEngine.Object.Instantiate(MapObjectAsteroid, MapObjectsRoot);
				gameObject3.name = asteroid.CustomName;
				MapObjectAsteroid component3 = gameObject3.GetComponent<MapObjectAsteroid>();
				component3.MainObject = obj;
				AllMapObjects[obj] = component3;
				UpdateParent(obj);
				component3.UpdateObject();
				component3.UpdateVisibility();
			}
			else if (obj is DebrisField)
			{
				DebrisField debrisField = obj as DebrisField;
				GameObject gameObject4 = UnityEngine.Object.Instantiate(MapObjectDebrisField, MapObjectsRoot);
				gameObject4.name = debrisField.Name;
				MapObjectDebrisField component4 = gameObject4.GetComponent<MapObjectDebrisField>();
				component4.MainObject = obj;
				AllMapObjects[obj] = component4;
				UpdateParent(obj);
				component4.UpdateVisibility();
			}
		}

		public void InitializeMapObjectEndWarp(Vector3D position, double minMaxScale)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(MapObjectWarpEnd, MapObjectsRoot);
			MapObjectFixedPosition component = gameObject.GetComponent<MapObjectFixedPosition>();
			component.FixedPosition = position;
			component.MinMaxScale = minMaxScale;
			component.Name = Localization.WarpSignature.ToUpper();
			component.Description = Localization.WarpSignatureDescription;
			gameObject.Activate(value: true);
		}

		public void InitializeMapObjectFuzzyScan(Vector3D position, double scale, SpaceObjectVessel vessel)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(MapObjectFuzzuScan, MapObjectsRoot);
			MapObjectFuzzyScan component = gameObject.GetComponent<MapObjectFuzzyScan>();
			component.FixedPosition = position;
			component.MinMaxScale = (scale - component.MinScale) / (component.MaxScale - component.MinScale);
			component.Name = Localization.UnidentifiedObject.ToUpper();
			component.Description = Localization.UnidentifiedObjectDescription;
			component.Vessels.Add(vessel);
			gameObject.Activate(value: true);
		}

		public void InitializeMapObjectStartWarpNear(Vector3D position, Quaternion rotation)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(MapObjectWarpStartNear, MapObjectsRoot);
			MapObjectWarpStart component = gameObject.GetComponent<MapObjectWarpStart>();
			component.WarpStartPosition = position;
			component.WarpConeRotation = rotation;
			component.Name = Localization.WarpSignature.ToUpper();
			gameObject.Activate(value: true);
		}

		public void InitializeMapObjectStartWarpFar(Vector3D position, double minMaxScale)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(MapObjectWarpStartFar, MapObjectsRoot);
			MapObjectFixedPosition component = gameObject.GetComponent<MapObjectFixedPosition>();
			component.FixedPosition = position;
			component.MinMaxScale = minMaxScale;
			component.Name = Localization.WarpSignature.ToUpper();
			component.Description = Localization.WarpSignatureDescription;
			gameObject.Activate(value: true);
		}

		public void UpdateObjectData(IMapMainObject obj)
		{
			if (obj is CelestialBody)
			{
				CelestialBody celestialBody = obj as CelestialBody;
				UpdateCelestialParent(obj);
				return;
			}
			AllMapObjects.TryGetValue(obj, out var value);
			if (obj is Ship)
			{
				Ship ship = obj as Ship;
				if (value != null)
				{
					value.name = ship.CustomName;
				}
				UpdateParent(obj);
			}
			else
			{
				Asteroid asteroid = obj as Asteroid;
				if (value != null)
				{
					value.name = asteroid.CustomName;
				}
				UpdateParent(obj);
			}
		}

		public void UpdateCelestialParent(IMapMainObject obj)
		{
			if (obj.GUID != 1 && obj.Orbit.Parent.CelestialBody != null && AllMapObjects.ContainsKey(obj.Orbit.Parent.CelestialBody) && AllMapObjects.TryGetValue(obj, out var value) && AllMapObjects.TryGetValue(obj.Orbit.Parent.CelestialBody, out var value2))
			{
				value.transform.SetParent((value2 as MapObjectCelestial).CelestialObjects.transform);
				value.transform.localPosition = Vector3.zero;
				value.Visual.localPosition = Vector3.zero;
			}
		}

		public void UpdateParent(IMapMainObject obj)
		{
			if (obj.Orbit.Parent.CelestialBody != null && AllMapObjects.ContainsKey(obj.Orbit.Parent.CelestialBody) && AllMapObjects.TryGetValue(obj, out var value) && AllMapObjects.TryGetValue(obj.Orbit.Parent.CelestialBody, out var value2))
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
				UnityEngine.Object.Destroy(value.gameObject);
			}
			AllMapObjects.Remove(obj);
		}

		public IEnumerator UpdateFocus()
		{
			planetMaxZoom = 8.45228E+09f / FocusObject.Radius * ClosestSunScale;
			if (OldFocusObject == FocusObject)
			{
				Focus = FocusObject.TruePosition;
			}
			else if (FocusObject != OldFocusObject && !Focusing)
			{
				Focusing = true;
				Vector3D startPosition = Vector3D.Zero;
				float startZoom = (float)zoom;
				if (OldFocusObject != null)
				{
					startPosition = OldFocusObject.TruePosition;
				}
				while (TransitionTimer < TransitionDuration)
				{
					if (zoom > (double)planetMaxZoom)
					{
						zoom = startZoom * Mathf.Pow(planetMaxZoom / startZoom, ZoomCurve.Evaluate(TransitionTimer / TransitionDuration));
					}
					Focus = Vector3D.Lerp(startPosition, FocusObject.TruePosition, PanCurve.Evaluate(TransitionTimer / TransitionDuration));
					TransitionTimer += Time.deltaTime;
					yield return null;
				}
				TransitionTimer = 0f;
				if (startZoom > planetMaxZoom)
				{
					zoom = planetMaxZoom;
				}
				OldFocusObject = FocusObject;
				Focus = FocusObject.TruePosition;
				Focusing = false;
			}
			Focusing = false;
		}

		private void ShowAllChildObjects(MapObject obj)
		{
			if (obj == Sun)
			{
				(obj as MapObjectCelestial).ChildObjects.gameObject.SetActive(value: true);
				MapObjectCelestial[] componentsInChildren = (obj as MapObjectCelestial).CelestialObjects.GetComponentsInChildren<MapObjectCelestial>();
				foreach (MapObjectCelestial mapObjectCelestial in componentsInChildren)
				{
					mapObjectCelestial.ChildObjects.gameObject.SetActive(value: false);
				}
			}
			else
			{
				if (obj?.Orbit?.Parent.CelestialBody == null)
				{
					return;
				}
				if (obj.Orbit.Parent.CelestialBody == Sun.MainObject)
				{
					ToggleAllChildObjects(obj, show: true);
					return;
				}
				MapObject value = AllMapObjects.FirstOrDefault((KeyValuePair<IMapMainObject, MapObject> m) => m.Key == obj.Orbit.Parent.CelestialBody).Value;
				if (value.Orbit.Parent != null && value.Orbit.Parent.CelestialBody == Sun.MainObject)
				{
					ToggleAllChildObjects(value, show: true);
				}
				else
				{
					ShowAllChildObjects(value);
				}
			}
		}

		private void HideAllChildObjects(MapObject obj)
		{
			if (!(obj == Sun) && obj.Orbit.Parent.CelestialBody != null)
			{
				MapObject value = AllMapObjects.FirstOrDefault((KeyValuePair<IMapMainObject, MapObject> m) => m.Key == obj.Orbit.Parent.CelestialBody).Value;
				if (value.MainObject == Sun.MainObject)
				{
					ToggleAllChildObjects(value, show: false);
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
				mapObjectCelestial = obj as MapObjectCelestial;
				mapObjectCelestial.ChildObjects.gameObject.SetActive(show);
				MapObjectCelestial[] componentsInChildren = mapObjectCelestial.CelestialObjects.GetComponentsInChildren<MapObjectCelestial>();
				foreach (MapObjectCelestial mapObjectCelestial2 in componentsInChildren)
				{
					mapObjectCelestial2.ChildObjects.gameObject.SetActive(show);
				}
			}
			else
			{
				(Sun as MapObjectCelestial).ChildObjects.gameObject.SetActive(value: true);
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
				CameraPitch.transform.localRotation = Quaternion.Lerp(CameraPitch.transform.localRotation, Quaternion.Euler(0f - pitch, 0f, 0f), Time.deltaTime * RotationSmoothness);
			}
			if (CameraYawn.transform.localRotation.IsEpsilonEqual(Quaternion.Euler(0f, yaw, 0f), 1E-05f))
			{
				CameraYawn.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
			}
			else
			{
				CameraYawn.transform.localRotation = Quaternion.Lerp(CameraYawn.transform.localRotation, Quaternion.Euler(0f, yaw, 0f), Time.deltaTime * RotationSmoothness);
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

		public void CreateCustomOrbit(bool useParentNameInName = true, string name = "CustomOrbit")
		{
			if (SelectedObject is MapObjectCelestial)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(MapObjectCustomOrbit, MapObjectsRoot);
				if (useParentNameInName)
				{
					gameObject.name = name + " around " + SelectedObject.name;
				}
				else
				{
					gameObject.name = name;
				}
				MapObjectCustomOrbit newCustomOrbit = gameObject.GetComponent<MapObjectCustomOrbit>();
				AllCustomOrbits.Add(newCustomOrbit);
				newCustomOrbit.transform.SetParent((SelectedObject as MapObjectCelestial).ChildObjects.transform);
				newCustomOrbit.transform.localPosition = Vector3.zero;
				newCustomOrbit.Visual.localPosition = Vector3.zero;
				newCustomOrbit.CreateOrbit(SelectedObject);
				newCustomOrbit.CreateVisual();
				SelectMapObject(newCustomOrbit);
				FocusToObject(AllMapObjects.FirstOrDefault((KeyValuePair<IMapMainObject, MapObject> m) => m.Key == newCustomOrbit.Orbit.Parent.CelestialBody).Value);
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
				GameObject gameObject = UnityEngine.Object.Instantiate(MapManeuverCourse, MapObjectsRoot);
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
			if (WarpManeuver != null)
			{
				UnityEngine.Object.Destroy(WarpManeuver.gameObject);
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
			courseItemData.TravelTime = ((WarpManeuver.Transfer.Type != ManeuverType.Transfer) ? 0f : WarpManeuver.Transfer.TravelTime);
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
			Client.Instance.CanvasManager.CanvasUI.HelmetHud.gameObject.Activate(value: false);
		}

		private void OnDisable()
		{
			Client.Instance.CanvasManager.CanvasUI.HelmetHud.gameObject.Activate(value: true);
		}

		public void ShowScanningEffect()
		{
			if (MyShip != null && MyShip is MapObjectShip && (MyShip as MapObjectShip).ScanningEffectCone != null)
			{
				(MyShip as MapObjectShip).ScanningEffectCone.transform.rotation = (MyShip as MapObjectShip).ScanningCone.transform.rotation;
				(MyShip as MapObjectShip).ScanningEffectCone.transform.localScale = (MyShip as MapObjectShip).ScanningCone.transform.localScale * (MyShip as MapObjectShip).ScanningEffectConeScaleMultiplier;
				(MyShip as MapObjectShip).ScanningEffectCone.Activate(value: true);
			}
		}

		public void HideScanningEffect()
		{
			if (MyShip != null && MyShip is MapObjectShip && (MyShip as MapObjectShip).ScanningEffectCone != null)
			{
				(MyShip as MapObjectShip).ScanningEffectCone.Activate(value: false);
			}
		}

		public void SaveMapDetails()
		{
			NavigationMapDetails navigationMapDetails = new NavigationMapDetails();
			navigationMapDetails.Unknown = (from m in Client.Instance.Map.AllMapObjects
				where m.Key is SpaceObjectVessel && m.Value.RadarVisibilityType == RadarVisibilityType.Unknown
				select new UnknownMapObjectDetails
				{
					GUID = m.Key.GUID,
					SpawnRuleID = (m.Key as SpaceObjectVessel).VesselData.SpawnRuleID,
					LastKnownOrbit = (m.Key as SpaceObjectVessel).LastKnownMapOrbit.GetOrbitData(m.Key as SpaceObjectVessel)
				}).ToList();
			Client.Instance.NetworkController.SendToGameServer(new NavigationMapDetailsMessage
			{
				NavMapDetails = navigationMapDetails
			});
		}
	}
}
