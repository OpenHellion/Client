using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
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
		[CompilerGenerated]
		private sealed class _003COnDoubleClick_003Ec__AnonStorey1
		{
			internal MapObject mapObject;

			internal bool _003C_003Em__0(KeyValuePair<IMapMainObject, MapObject> m)
			{
				return m.Key == (mapObject as MapObjectCustomOrbit).Orbit.Parent.CelestialBody;
			}
		}

		[CompilerGenerated]
		private sealed class _003COnInteract_003Ec__AnonStorey2
		{
			internal SpaceObjectVessel homeVessel;

			internal bool _003C_003Em__0(KeyValuePair<IMapMainObject, MapObject> m)
			{
				return m.Key.GUID == homeVessel.GUID || m.Key.GUID == homeVessel.MainVessel.GUID;
			}
		}

		[CompilerGenerated]
		private sealed class _003CShowAllChildObjects_003Ec__AnonStorey3
		{
			internal MapObject obj;

			internal bool _003C_003Em__0(KeyValuePair<IMapMainObject, MapObject> m)
			{
				return m.Key == obj.Orbit.Parent.CelestialBody;
			}
		}

		[CompilerGenerated]
		private sealed class _003CHideAllChildObjects_003Ec__AnonStorey4
		{
			internal MapObject obj;

			internal bool _003C_003Em__0(KeyValuePair<IMapMainObject, MapObject> m)
			{
				return m.Key == obj.Orbit.Parent.CelestialBody;
			}
		}

		[CompilerGenerated]
		private sealed class _003CCreateCustomOrbit_003Ec__AnonStorey5
		{
			internal MapObjectCustomOrbit newCustomOrbit;

			internal bool _003C_003Em__0(KeyValuePair<IMapMainObject, MapObject> m)
			{
				return m.Key == newCustomOrbit.Orbit.Parent.CelestialBody;
			}
		}

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

		private bool doObjectGrouping;

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

		[CompilerGenerated]
		private static Func<RaycastHit, bool> _003C_003Ef__am_0024cache0;

		[CompilerGenerated]
		private static Func<RaycastHit, MapObject> _003C_003Ef__am_0024cache1;

		[CompilerGenerated]
		private static Func<MapObject, bool> _003C_003Ef__am_0024cache2;

		[CompilerGenerated]
		private static Func<MapObject, double> _003C_003Ef__am_0024cache3;

		[CompilerGenerated]
		private static Func<RaycastHit, bool> _003C_003Ef__am_0024cache4;

		[CompilerGenerated]
		private static Func<RaycastHit, double> _003C_003Ef__am_0024cache5;

		[CompilerGenerated]
		private static Func<KeyValuePair<IMapMainObject, MapObject>, bool> _003C_003Ef__am_0024cache6;

		[CompilerGenerated]
		private static Func<KeyValuePair<IMapMainObject, MapObject>, UnknownMapObjectDetails> _003C_003Ef__am_0024cache7;

		private void Start()
		{
			MapScanEffect = UnityEngine.Object.Instantiate(ScanEffectPrefab, MapObjectsRoot);
			MapScanEffect.name = "ScanEffect";
			MapScanEffect.SetActive(false);
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
				RaycastHit[] source = Physics.RaycastAll(ray, float.PositiveInfinity, layerMask);
				if (_003C_003Ef__am_0024cache0 == null)
				{
					_003C_003Ef__am_0024cache0 = _003CUpdate_003Em__0;
				}
				RaycastHit[] source2 = source.Where(_003C_003Ef__am_0024cache0).ToArray();
				if (_003C_003Ef__am_0024cache1 == null)
				{
					_003C_003Ef__am_0024cache1 = _003CUpdate_003Em__1;
				}
				IEnumerable<MapObject> source3 = source2.Select(_003C_003Ef__am_0024cache1);
				if (_003C_003Ef__am_0024cache2 == null)
				{
					_003C_003Ef__am_0024cache2 = _003CUpdate_003Em__2;
				}
				IEnumerable<MapObject> source4 = source3.Where(_003C_003Ef__am_0024cache2);
				if (_003C_003Ef__am_0024cache3 == null)
				{
					_003C_003Ef__am_0024cache3 = _003CUpdate_003Em__3;
				}
				VesselsGroup = source4.OrderBy(_003C_003Ef__am_0024cache3).ToList();
				if (_003C_003Ef__am_0024cache4 == null)
				{
					_003C_003Ef__am_0024cache4 = _003CUpdate_003Em__4;
				}
				IOrderedEnumerable<RaycastHit> source5 = source2.OrderBy(_003C_003Ef__am_0024cache4);
				if (_003C_003Ef__am_0024cache5 == null)
				{
					_003C_003Ef__am_0024cache5 = _003CUpdate_003Em__5;
				}
				raycastHit = source5.ThenBy(_003C_003Ef__am_0024cache5).FirstOrDefault();
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
						bool? obj;
						if ((object)mapObjectShip == null)
						{
							obj = null;
						}
						else
						{
							GameObject scanningCone = mapObjectShip.ScanningCone;
							obj = (((object)scanningCone != null) ? new bool?(scanningCone.activeInHierarchy) : null);
						}
						if (obj == true)
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
			_003COnDoubleClick_003Ec__AnonStorey1 _003COnDoubleClick_003Ec__AnonStorey = new _003COnDoubleClick_003Ec__AnonStorey1();
			_003COnDoubleClick_003Ec__AnonStorey.mapObject = collider.GetComponentInParent<MapObject>();
			if (_003COnDoubleClick_003Ec__AnonStorey.mapObject != null)
			{
				if (_003COnDoubleClick_003Ec__AnonStorey.mapObject is MapObjectCustomOrbit)
				{
					FocusToObject(AllMapObjects.FirstOrDefault(_003COnDoubleClick_003Ec__AnonStorey._003C_003Em__0).Value);
				}
				else
				{
					FocusToObject(_003COnDoubleClick_003Ec__AnonStorey.mapObject);
				}
			}
		}

		public void OnRightClick(Collider collider)
		{
			if (collider == null)
			{
				return;
			}
			MapObjectShip mapObjectShip = SelectedObject as MapObjectShip;
			if (!(collider != null))
			{
				return;
			}
			bool? obj;
			if ((object)mapObjectShip == null)
			{
				obj = null;
			}
			else
			{
				GameObject scanningCone = mapObjectShip.ScanningCone;
				obj = (((object)scanningCone != null) ? new bool?(scanningCone.activeInHierarchy) : null);
			}
			if (obj == true)
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
			_003COnInteract_003Ec__AnonStorey2 _003COnInteract_003Ec__AnonStorey = new _003COnInteract_003Ec__AnonStorey2();
			Panel = Client.Instance.InGamePanels.Navigation;
			Panel.gameObject.SetActive(true);
			base.gameObject.SetActive(true);
			doObjectGrouping = true;
			MapCamera.enabled = true;
			MyPlayer.Instance.FpsController.MainCamera.enabled = false;
			MyPlayer.Instance.FpsController.NearCamera.enabled = false;
			MyPlayer.Instance.FpsController.FarCamera.enabled = false;
			MyPlayer.Instance.PlanetsCameraRoot.gameObject.SetActive(false);
			MyPlayer.Instance.SunCameraRoot.gameObject.SetActive(false);
			Client.Instance.CanvasManager.CanvasUI.QuestIndicators.AddMarkersOnMap();
			MapObject value;
			if (MyParentShip != null && AllMapObjects.TryGetValue(MyParentShip, out value))
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
			_003COnInteract_003Ec__AnonStorey.homeVessel = Client.Instance.GetVessel(MyPlayer.Instance.HomeStationGUID);
			if (_003COnInteract_003Ec__AnonStorey.homeVessel != null)
			{
				Home = AllMapObjects.FirstOrDefault(_003COnInteract_003Ec__AnonStorey._003C_003Em__0).Value;
			}
			ShowAllChildObjects(FocusObject);
			Panel.OnInteract(MyParentShip);
			SelectMapObject(FocusObject);
			OldFocusObject = SelectedObject;
			Client.Instance.ToggleCursor(true);
			Client.Instance.CanvasManager.OverlayCanvasIsOn = true;
			Client.Instance.InputModule.ToggleCustomCursorPosition(false);
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
			doObjectGrouping = false;
			base.gameObject.SetActive(false);
			MapCamera.enabled = false;
			MyPlayer.Instance.FpsController.MainCamera.enabled = true;
			MyPlayer.Instance.FpsController.NearCamera.enabled = true;
			MyPlayer.Instance.FpsController.FarCamera.enabled = true;
			MyPlayer.Instance.PlanetsCameraRoot.gameObject.SetActive(true);
			MyPlayer.Instance.SunCameraRoot.gameObject.SetActive(true);
			Panel.OnDetach();
			Client.Instance.ToggleCursor(false);
			Client.Instance.CanvasManager.OverlayCanvasIsOn = false;
			Client.Instance.InputModule.ToggleCustomCursorPosition(true);
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
			gameObject.Activate(true);
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
			gameObject.Activate(true);
		}

		public void InitializeMapObjectStartWarpNear(Vector3D position, Quaternion rotation)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(MapObjectWarpStartNear, MapObjectsRoot);
			MapObjectWarpStart component = gameObject.GetComponent<MapObjectWarpStart>();
			component.WarpStartPosition = position;
			component.WarpConeRotation = rotation;
			component.Name = Localization.WarpSignature.ToUpper();
			gameObject.Activate(true);
		}

		public void InitializeMapObjectStartWarpFar(Vector3D position, double minMaxScale)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(MapObjectWarpStartFar, MapObjectsRoot);
			MapObjectFixedPosition component = gameObject.GetComponent<MapObjectFixedPosition>();
			component.FixedPosition = position;
			component.MinMaxScale = minMaxScale;
			component.Name = Localization.WarpSignature.ToUpper();
			component.Description = Localization.WarpSignatureDescription;
			gameObject.Activate(true);
		}

		public void UpdateObjectData(IMapMainObject obj)
		{
			if (obj is CelestialBody)
			{
				CelestialBody celestialBody = obj as CelestialBody;
				UpdateCelestialParent(obj);
				return;
			}
			MapObject value;
			AllMapObjects.TryGetValue(obj, out value);
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
			MapObject value;
			MapObject value2;
			if (obj.GUID != 1 && obj.Orbit.Parent.CelestialBody != null && AllMapObjects.ContainsKey(obj.Orbit.Parent.CelestialBody) && AllMapObjects.TryGetValue(obj, out value) && AllMapObjects.TryGetValue(obj.Orbit.Parent.CelestialBody, out value2))
			{
				value.transform.SetParent((value2 as MapObjectCelestial).CelestialObjects.transform);
				value.transform.localPosition = Vector3.zero;
				value.Visual.localPosition = Vector3.zero;
			}
		}

		public void UpdateParent(IMapMainObject obj)
		{
			MapObject value;
			MapObject value2;
			if (obj.Orbit.Parent.CelestialBody != null && AllMapObjects.ContainsKey(obj.Orbit.Parent.CelestialBody) && AllMapObjects.TryGetValue(obj, out value) && AllMapObjects.TryGetValue(obj.Orbit.Parent.CelestialBody, out value2))
			{
				value.transform.SetParent((value2 as MapObjectCelestial).ChildObjects.transform);
				value.transform.localPosition = Vector3.zero;
				value.Visual.localPosition = Vector3.zero;
			}
		}

		public void RemoveMapObject(IMapMainObject obj)
		{
			MapObject value;
			if (AllMapObjects.TryGetValue(obj, out value) && value != null)
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
			_003CShowAllChildObjects_003Ec__AnonStorey3 _003CShowAllChildObjects_003Ec__AnonStorey = new _003CShowAllChildObjects_003Ec__AnonStorey3();
			_003CShowAllChildObjects_003Ec__AnonStorey.obj = obj;
			if (_003CShowAllChildObjects_003Ec__AnonStorey.obj == Sun)
			{
				(_003CShowAllChildObjects_003Ec__AnonStorey.obj as MapObjectCelestial).ChildObjects.gameObject.SetActive(true);
				MapObjectCelestial[] componentsInChildren = (_003CShowAllChildObjects_003Ec__AnonStorey.obj as MapObjectCelestial).CelestialObjects.GetComponentsInChildren<MapObjectCelestial>();
				foreach (MapObjectCelestial mapObjectCelestial in componentsInChildren)
				{
					mapObjectCelestial.ChildObjects.gameObject.SetActive(false);
				}
				return;
			}
			MapObject obj2 = _003CShowAllChildObjects_003Ec__AnonStorey.obj;
			object obj3;
			if ((object)obj2 == null)
			{
				obj3 = null;
			}
			else
			{
				OrbitParameters orbit = obj2.Orbit;
				obj3 = ((orbit != null) ? orbit.Parent.CelestialBody : null);
			}
			if (obj3 == null)
			{
				return;
			}
			if (_003CShowAllChildObjects_003Ec__AnonStorey.obj.Orbit.Parent.CelestialBody == Sun.MainObject)
			{
				ToggleAllChildObjects(_003CShowAllChildObjects_003Ec__AnonStorey.obj, true);
				return;
			}
			MapObject value = AllMapObjects.FirstOrDefault(_003CShowAllChildObjects_003Ec__AnonStorey._003C_003Em__0).Value;
			if (value.Orbit.Parent != null && value.Orbit.Parent.CelestialBody == Sun.MainObject)
			{
				ToggleAllChildObjects(value, true);
			}
			else
			{
				ShowAllChildObjects(value);
			}
		}

		private void HideAllChildObjects(MapObject obj)
		{
			_003CHideAllChildObjects_003Ec__AnonStorey4 _003CHideAllChildObjects_003Ec__AnonStorey = new _003CHideAllChildObjects_003Ec__AnonStorey4();
			_003CHideAllChildObjects_003Ec__AnonStorey.obj = obj;
			if (!(_003CHideAllChildObjects_003Ec__AnonStorey.obj == Sun) && _003CHideAllChildObjects_003Ec__AnonStorey.obj.Orbit.Parent.CelestialBody != null)
			{
				MapObject value = AllMapObjects.FirstOrDefault(_003CHideAllChildObjects_003Ec__AnonStorey._003C_003Em__0).Value;
				if (value.MainObject == Sun.MainObject)
				{
					ToggleAllChildObjects(value, false);
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
				(Sun as MapObjectCelestial).ChildObjects.gameObject.SetActive(true);
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

		private void OnDestroy()
		{
			doObjectGrouping = false;
		}

		public void CreateCustomOrbit(bool useParentNameInName = true, string name = "CustomOrbit")
		{
			if (SelectedObject is MapObjectCelestial)
			{
				_003CCreateCustomOrbit_003Ec__AnonStorey5 _003CCreateCustomOrbit_003Ec__AnonStorey = new _003CCreateCustomOrbit_003Ec__AnonStorey5();
				GameObject gameObject = UnityEngine.Object.Instantiate(MapObjectCustomOrbit, MapObjectsRoot);
				if (useParentNameInName)
				{
					gameObject.name = name + " around " + SelectedObject.name;
				}
				else
				{
					gameObject.name = name;
				}
				_003CCreateCustomOrbit_003Ec__AnonStorey.newCustomOrbit = gameObject.GetComponent<MapObjectCustomOrbit>();
				AllCustomOrbits.Add(_003CCreateCustomOrbit_003Ec__AnonStorey.newCustomOrbit);
				_003CCreateCustomOrbit_003Ec__AnonStorey.newCustomOrbit.transform.SetParent((SelectedObject as MapObjectCelestial).ChildObjects.transform);
				_003CCreateCustomOrbit_003Ec__AnonStorey.newCustomOrbit.transform.localPosition = Vector3.zero;
				_003CCreateCustomOrbit_003Ec__AnonStorey.newCustomOrbit.Visual.localPosition = Vector3.zero;
				_003CCreateCustomOrbit_003Ec__AnonStorey.newCustomOrbit.CreateOrbit(SelectedObject);
				_003CCreateCustomOrbit_003Ec__AnonStorey.newCustomOrbit.CreateVisual();
				SelectMapObject(_003CCreateCustomOrbit_003Ec__AnonStorey.newCustomOrbit);
				FocusToObject(AllMapObjects.FirstOrDefault(_003CCreateCustomOrbit_003Ec__AnonStorey._003C_003Em__0).Value);
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
			Client.Instance.CanvasManager.CanvasUI.HelmetHud.gameObject.Activate(false);
		}

		private void OnDisable()
		{
			Client.Instance.CanvasManager.CanvasUI.HelmetHud.gameObject.Activate(true);
		}

		public void ShowScanningEffect()
		{
			if (MyShip != null && MyShip is MapObjectShip && (MyShip as MapObjectShip).ScanningEffectCone != null)
			{
				(MyShip as MapObjectShip).ScanningEffectCone.transform.rotation = (MyShip as MapObjectShip).ScanningCone.transform.rotation;
				(MyShip as MapObjectShip).ScanningEffectCone.transform.localScale = (MyShip as MapObjectShip).ScanningCone.transform.localScale * (MyShip as MapObjectShip).ScanningEffectConeScaleMultiplier;
				(MyShip as MapObjectShip).ScanningEffectCone.Activate(true);
			}
		}

		public void HideScanningEffect()
		{
			if (MyShip != null && MyShip is MapObjectShip && (MyShip as MapObjectShip).ScanningEffectCone != null)
			{
				(MyShip as MapObjectShip).ScanningEffectCone.Activate(false);
			}
		}

		public void SaveMapDetails()
		{
			NavigationMapDetails navigationMapDetails = new NavigationMapDetails();
			Dictionary<IMapMainObject, MapObject> allMapObjects = Client.Instance.Map.AllMapObjects;
			if (_003C_003Ef__am_0024cache6 == null)
			{
				_003C_003Ef__am_0024cache6 = _003CSaveMapDetails_003Em__6;
			}
			IEnumerable<KeyValuePair<IMapMainObject, MapObject>> source = allMapObjects.Where(_003C_003Ef__am_0024cache6);
			if (_003C_003Ef__am_0024cache7 == null)
			{
				_003C_003Ef__am_0024cache7 = _003CSaveMapDetails_003Em__7;
			}
			navigationMapDetails.Unknown = source.Select(_003C_003Ef__am_0024cache7).ToList();
			Client.Instance.NetworkController.SendToGameServer(new NavigationMapDetailsMessage
			{
				NavMapDetails = navigationMapDetails
			});
		}

		[CompilerGenerated]
		private static bool _003CUpdate_003Em__0(RaycastHit m)
		{
			return m.collider.GetComponentInParent<MapObject>() != null;
		}

		[CompilerGenerated]
		private static MapObject _003CUpdate_003Em__1(RaycastHit m)
		{
			return m.collider.GetComponentInParent<MapObjectVessel>();
		}

		[CompilerGenerated]
		private static bool _003CUpdate_003Em__2(MapObject m)
		{
			return (((object)m != null) ? m.MainObject : null) != null;
		}

		[CompilerGenerated]
		private static double _003CUpdate_003Em__3(MapObject m)
		{
			return (m.TruePosition - MyPlayer.Instance.Parent.Position).SqrMagnitude;
		}

		[CompilerGenerated]
		private static bool _003CUpdate_003Em__4(RaycastHit m)
		{
			return m.collider.GetComponentInParent<MapObjectFixedPosition>() != null;
		}

		[CompilerGenerated]
		private static double _003CUpdate_003Em__5(RaycastHit m)
		{
			return (m.collider.GetComponentInParent<MapObject>().TruePosition - MyPlayer.Instance.Parent.Position).SqrMagnitude;
		}

		[CompilerGenerated]
		private static bool _003CSaveMapDetails_003Em__6(KeyValuePair<IMapMainObject, MapObject> m)
		{
			return m.Key is SpaceObjectVessel && m.Value.RadarVisibilityType == RadarVisibilityType.Unknown;
		}

		[CompilerGenerated]
		private static UnknownMapObjectDetails _003CSaveMapDetails_003Em__7(KeyValuePair<IMapMainObject, MapObject> m)
		{
			return new UnknownMapObjectDetails
			{
				GUID = m.Key.GUID,
				SpawnRuleID = (m.Key as SpaceObjectVessel).VesselData.SpawnRuleID,
				LastKnownOrbit = (m.Key as SpaceObjectVessel).LastKnownMapOrbit.GetOrbitData(m.Key as SpaceObjectVessel)
			};
		}
	}
}
