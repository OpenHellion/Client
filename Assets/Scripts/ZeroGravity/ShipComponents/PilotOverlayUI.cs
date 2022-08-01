using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Math;
using ZeroGravity.Objects;
using ZeroGravity.UI;

namespace ZeroGravity.ShipComponents
{
	public class PilotOverlayUI : MonoBehaviour
	{
		[CompilerGenerated]
		private sealed class _003CUpdateRadarList_003Ec__AnonStorey0
		{
			internal Vector3D myPos;

			internal List<ArtificialBody> list;

			internal PilotOverlayUI _0024this;

			internal bool _003C_003Em__0(TargetObject m)
			{
				return m.AB == null || !(m.AB is SpaceObjectVessel) || !(m.AB as SpaceObjectVessel).IsMainVessel || (m.AB as SpaceObjectVessel).MainVessel == _0024this.ParentShip.MainVessel || m.Distance > _0024this.RadarRange[_0024this.currentRadarRange];
			}

			internal bool _003C_003Em__1(ArtificialBody m)
			{
				return m is SpaceObjectVessel && (m as SpaceObjectVessel).VesselData != null && (m as SpaceObjectVessel).IsMainVessel && !(m as SpaceObjectVessel).IsWarpOnline && _0024this.ParentShip.MainVessel != (m as SpaceObjectVessel).MainVessel && (myPos - m.Position).Magnitude <= (double)_0024this.RadarRange[_0024this.currentRadarRange] && !list.Contains(m) && m as SpaceObjectVessel != _0024this.ParentShip;
			}
		}

		public Ship ParentShip;

		public List<float> RadarRange = new List<float>();

		private int currentRadarRange;

		private float drawDistance = 100f;

		public List<TargetObject> AllTargets = new List<TargetObject>();

		public TargetObject SelectedTarget;

		public Transform OverlayTargets;

		public TargetOverlayUI TargetOnScreen;

		public TargetObject HoveredTarget;

		public GameObject CentralPanel;

		public Image CenterCroshair;

		public GameObject OffSpeedHelper;

		public GameObject ParametersHolder;

		public Text TargetDistance;

		public Text TargetLateral;

		public Text TargetDirectional;

		public GameObject EtaHolder;

		public GameObject OffTarget;

		private float ETA;

		private float SpeedTime;

		public Text EtaTimeline;

		public Image DistanceLimiterGraph;

		public Image DirectionalSpeedGraph;

		public GameObject LeftLimiter;

		public GameObject RightLimiter;

		public GameObject CollisionImminent;

		public MapObject SelectedMapObject;

		public GameObject MapObject;

		public Text MapObjectName;

		public Text MapObjectDistance;

		[Header("TARGET LIST")]
		public PilotTargetList CurrentTargetList;

		[Header("STATUS SCREEN")]
		public PilotStatusScreen CurrentStatusScreen;

		[Header("RADAR")]
		public PilotRadar CurrentRadar;

		[Header("INDICATORS")]
		private float distance;

		private float onSpeed;

		private float angle;

		private float stabilizeRotationTreshold = 0.8f;

		private float StabilizeRotationTimer;

		private Vector3D currentShipRotation;

		private Vector3D targetShipRotation;

		public RectTransform RotationIndicator;

		public RectTransform RotationIndicatorArrow;

		public GameObject RotationDanger;

		public float lateralIndicationSensitivity = 1.5f;

		public Image LatLeft;

		public Image LatRight;

		public Image LatUp;

		public Image LatDown;

		private double stabilizeToTargetMaxVelocityDiff = 2.0;

		private double stabilizeToTargetMaxPositionDiff = 100.0;

		public GameObject CanMatchTarget;

		public GameObject MatchedToTarget;

		public Text MatchedTo;

		[Header("WARP")]
		public Image ManeuverObject;

		public OffScreenTargetArrow ManeuverOffScreen;

		public GameObject ManeuverStatusHolder;

		public Text ManeuverTimer;

		public GameObject ManeuverWarning;

		public GameObject EngineHud;

		public Image PositiveEngineFiller;

		public Image NegativeEngineFiller;

		public GameObject DrivingTips;

		[CompilerGenerated]
		private static Func<TargetObject, ArtificialBody> _003C_003Ef__am_0024cache0;

		[CompilerGenerated]
		private static Func<TargetObject, float> _003C_003Ef__am_0024cache1;

		[CompilerGenerated]
		private static Func<TargetObject, bool> _003C_003Ef__am_0024cache2;

		private void Start()
		{
		}

		private void Update()
		{
			UpdateRadarList();
			RotationIndicatorUpdate();
			if (AllTargets.Count <= 0)
			{
				SelectedTarget = null;
				NoTargetAvailable();
				MatchedToTarget.SetActive(false);
				CanMatchTarget.SetActive(false);
			}
			else if (AllTargets.Count > 0 && SelectedTarget == null)
			{
				ParametersHolder.SetActive(true);
				if (SelectedTarget == null)
				{
					SelectedTarget = AllTargets[0];
				}
			}
			if (MyPlayer.Instance.FpsController.IsFreeLook && CentralPanel.activeInHierarchy)
			{
				CentralPanel.SetActive(false);
			}
			else if (!MyPlayer.Instance.FpsController.IsFreeLook && !CentralPanel.activeInHierarchy)
			{
				CentralPanel.SetActive(true);
			}
			if (CurrentTargetList.TargetListHolder != null)
			{
				CurrentTargetList.UpdateTargetList();
			}
			if (CurrentRadar != null)
			{
				CurrentRadar.UpdateRadar();
			}
			if (SelectedTarget != null && CentralPanel.activeInHierarchy)
			{
				CalculateScaleAndRotation();
			}
			if (HoveredTarget != null && InputManager.GetKey(KeyCode.Mouse0))
			{
				SelectedTarget = HoveredTarget;
			}
			if (!AllTargets.Contains(SelectedTarget) || SelectedTarget == null || (SelectedTarget.AB.Position - ParentShip.Position).Magnitude > (double)RadarRange[RadarRange.Count - 1])
			{
				if (AllTargets.Count > 0)
				{
					SelectedTarget = AllTargets[0];
				}
				else
				{
					SelectedTarget = null;
				}
			}
			if (!Client.Instance.CanvasManager.ConsoleIsUp)
			{
				if (InputManager.GetButtonDown(InputManager.AxisNames.T))
				{
					Client.Instance.OffSpeedHelper = !Client.Instance.OffSpeedHelper;
				}
				if (InputManager.GetButtonDown(InputManager.AxisNames.R) && CurrentTargetList.TargetListHolder.gameObject.activeInHierarchy)
				{
					if (RadarRange.Count - 1 > currentRadarRange)
					{
						currentRadarRange++;
					}
					else
					{
						currentRadarRange = 0;
					}
					if (CurrentTargetList != null)
					{
						CurrentTargetList.RadarRangeCurrent.text = Localization.RadarRange.ToUpper() + " " + FormatHelper.DistanceFormat(RadarRange[currentRadarRange]);
					}
				}
				if (CurrentTargetList != null)
				{
					if (InputManager.GetAxis(InputManager.AxisNames.MouseWheel).IsNotEpsilonZero() && AllTargets.Count > 0)
					{
						float axis = InputManager.GetAxis(InputManager.AxisNames.MouseWheel);
						if (axis > 0f)
						{
							int num = AllTargets.IndexOf(SelectedTarget);
							num = ((num - 1 < 0) ? (AllTargets.Count - 1) : (num - 1));
							SelectedTarget = AllTargets[num];
						}
						else if (axis < 0f)
						{
							int num2 = AllTargets.IndexOf(SelectedTarget);
							num2 = ((AllTargets.Count - 1 > num2) ? (num2 + 1) : 0);
							SelectedTarget = AllTargets[num2];
						}
					}
					if (AllTargets.Count > 0 && InputManager.GetButtonDown(InputManager.AxisNames.DownArrow))
					{
						int num3 = AllTargets.IndexOf(SelectedTarget);
						num3 = ((AllTargets.Count - 1 > num3) ? (num3 + 1) : 0);
						SelectedTarget = AllTargets[num3];
					}
					else if (AllTargets.Count > 0 && InputManager.GetButtonDown(InputManager.AxisNames.UpArrow))
					{
						int num4 = AllTargets.IndexOf(SelectedTarget);
						num4 = ((num4 - 1 < 0) ? (AllTargets.Count - 1) : (num4 - 1));
						SelectedTarget = AllTargets[num4];
					}
				}
			}
			if (CurrentStatusScreen != null)
			{
				CurrentStatusScreen.UpdateSystemsInfo();
			}
			EngineHud.SetActive(ParentShip.Engine != null && ParentShip.EngineOnLine);
			if (EngineHud.activeInHierarchy)
			{
				EngineStatusUpdate();
			}
			CheckTargetStabilization();
			UpdateManeuverTarget();
		}

		private void LateUpdate()
		{
			if (AllTargets.Count > 0)
			{
				UpdateOverlayTargets();
			}
		}

		private void CalculateScaleAndRotation()
		{
			Vector3 vector = (SelectedTarget.AB.Position - ParentShip.Position).ToVector3();
			Vector3 vector2 = (SelectedTarget.AB.Velocity - ParentShip.Velocity).ToVector3();
			Vector3 vector3 = Quaternion.LookRotation(ParentShip.Forward, ParentShip.Up).Inverse() * vector;
			Vector3 vector4 = Quaternion.LookRotation(ParentShip.Forward, ParentShip.Up).Inverse() * vector2;
			Vector3 vector5 = Quaternion.LookRotation(ParentShip.transform.forward, ParentShip.transform.up) * vector3 - (MyPlayer.Instance.FpsController.MainCamera.transform.position - ParentShip.transform.position);
			float num = vector5.magnitude - (float)SelectedTarget.AB.Radius;
			if (num > drawDistance)
			{
				vector5 = vector5.normalized * drawDistance;
			}
			else
			{
				vector5 = vector5.normalized * (vector5.magnitude - (float)SelectedTarget.AB.Radius);
			}
			Vector3 vector6 = Vector3.Project(vector2, vector.normalized);
			Vector3 vec = Vector3.ProjectOnPlane(vector2, vector.normalized);
			angle = MathHelper.AngleSigned(ParentShip.Up, vec, ParentShip.Forward);
			distance = Mathf.Abs(vector3.magnitude);
			onSpeed = 0f - Vector3.Dot(vector2, vector.normalized);
			TargetDirectional.color = ((!(onSpeed >= 0f)) ? Colors.FormatedRed : Colors.White);
			TargetDirectional.text = onSpeed.ToString("f1") + " m/s";
			TargetLateral.text = vec.magnitude.ToString("f1") + " m/s";
			TargetDistance.color = ((!(distance <= 100f)) ? Colors.White : Colors.RadarTarget);
			TargetDistance.text = FormatHelper.DistanceFormat(distance);
			EtaUpdate();
			OffTargetVelocity();
		}

		public void OnInteract(Ship ship, PilotTargetList targetList, PilotStatusScreen statusScreen, PilotRadar radar)
		{
			ParentShip = ship;
			CurrentTargetList = targetList;
			CurrentStatusScreen = statusScreen;
			CurrentRadar = radar;
			if (CurrentTargetList != null)
			{
				CurrentTargetList.RadarRangeCurrent.text = Localization.RadarRange.ToUpper() + " " + FormatHelper.DistanceFormat(RadarRange[currentRadarRange]);
			}
			DrivingTips.Activate(Client.Instance.CanvasManager.ShowTips);
			base.gameObject.SetActive(true);
		}

		public void OnDetach()
		{
			AllTargets.Clear();
			TargetOverlayUI[] componentsInChildren = OverlayTargets.GetComponentsInChildren<TargetOverlayUI>(true);
			foreach (TargetOverlayUI targetOverlayUI in componentsInChildren)
			{
				UnityEngine.Object.Destroy(targetOverlayUI.gameObject);
			}
			if (CurrentTargetList != null)
			{
				TargetInListUI[] componentsInChildren2 = CurrentTargetList.TargetListHolder.GetComponentsInChildren<TargetInListUI>(true);
				foreach (TargetInListUI targetInListUI in componentsInChildren2)
				{
					UnityEngine.Object.Destroy(targetInListUI.gameObject);
				}
			}
			if (CurrentRadar != null)
			{
				RadarShipElement[] componentsInChildren3 = CurrentRadar.Root.GetComponentsInChildren<RadarShipElement>(true);
				foreach (RadarShipElement radarShipElement in componentsInChildren3)
				{
					UnityEngine.Object.Destroy(radarShipElement.gameObject);
				}
			}
			base.gameObject.SetActive(false);
		}

		public void UpdateRadarList()
		{
			_003CUpdateRadarList_003Ec__AnonStorey0 _003CUpdateRadarList_003Ec__AnonStorey = new _003CUpdateRadarList_003Ec__AnonStorey0();
			_003CUpdateRadarList_003Ec__AnonStorey._0024this = this;
			_003CUpdateRadarList_003Ec__AnonStorey.myPos = MyPlayer.Instance.Parent.Position;
			List<TargetObject> list = AllTargets.Where(_003CUpdateRadarList_003Ec__AnonStorey._003C_003Em__0).ToList();
			foreach (TargetObject item in list)
			{
				AllTargets.Remove(item);
			}
			TargetOverlayUI[] componentsInChildren = OverlayTargets.GetComponentsInChildren<TargetOverlayUI>(true);
			foreach (TargetOverlayUI targetOverlayUI in componentsInChildren)
			{
				if (list.Contains(targetOverlayUI.Target))
				{
					UnityEngine.Object.Destroy(targetOverlayUI.gameObject);
				}
			}
			if (CurrentTargetList != null)
			{
				TargetInListUI[] componentsInChildren2 = CurrentTargetList.TargetListHolder.GetComponentsInChildren<TargetInListUI>(true);
				foreach (TargetInListUI targetInListUI in componentsInChildren2)
				{
					if (list.Contains(targetInListUI.Target))
					{
						UnityEngine.Object.Destroy(targetInListUI.gameObject);
					}
				}
			}
			if (CurrentRadar != null)
			{
				RadarShipElement[] componentsInChildren3 = CurrentRadar.Root.GetComponentsInChildren<RadarShipElement>(true);
				foreach (RadarShipElement radarShipElement in componentsInChildren3)
				{
					if (list.Contains(radarShipElement.Target))
					{
						UnityEngine.Object.Destroy(radarShipElement.gameObject);
					}
				}
			}
			List<TargetObject> allTargets = AllTargets;
			if (_003C_003Ef__am_0024cache0 == null)
			{
				_003C_003Ef__am_0024cache0 = _003CUpdateRadarList_003Em__0;
			}
			_003CUpdateRadarList_003Ec__AnonStorey.list = allTargets.Select(_003C_003Ef__am_0024cache0).ToList();
			foreach (ArtificialBody item2 in Client.Instance.SolarSystem.ArtificialBodies.Where(_003CUpdateRadarList_003Ec__AnonStorey._003C_003Em__1))
			{
				TargetObject targetObject = new TargetObject();
				targetObject.AB = item2;
				AllTargets.Add(targetObject);
				CreateOverlayTargets(targetObject);
				if (CurrentTargetList != null)
				{
					CurrentTargetList.CreateTargetInList(targetObject);
				}
				if (CurrentRadar != null)
				{
					CurrentRadar.CreateRadarTarget(targetObject);
				}
			}
		}

		public void CreateOverlayTargets(TargetObject target)
		{
			TargetOverlayUI targetOverlayUI = UnityEngine.Object.Instantiate(TargetOnScreen, OverlayTargets);
			targetOverlayUI.AB = target.AB;
			targetOverlayUI.Target = target;
			targetOverlayUI.Name.text = target.Name;
		}

		public void UpdateOverlayTargets()
		{
			TargetOverlayUI[] componentsInChildren = OverlayTargets.GetComponentsInChildren<TargetOverlayUI>(true);
			foreach (TargetOverlayUI targetOverlayUI in componentsInChildren)
			{
				Vector3 pos = ((!targetOverlayUI.Target.AB.IsDummyObject) ? targetOverlayUI.Target.AB.transform.position : (Quaternion.LookRotation(ParentShip.MainVessel.Forward, ParentShip.MainVessel.Up).Inverse() * (targetOverlayUI.Target.AB.Position - ParentShip.Position).ToVector3() + ParentShip.GeometryRoot.transform.position));
				Vector3 arrowUp;
				targetOverlayUI.transform.position = GetPositionOnOverlay(pos, out arrowUp);
				targetOverlayUI.transform.rotation = Quaternion.identity;
				if (!targetOverlayUI.gameObject.activeInHierarchy)
				{
					targetOverlayUI.gameObject.SetActive(true);
				}
				if (arrowUp != Vector3.zero)
				{
					targetOverlayUI.Distance.gameObject.Activate(false);
					targetOverlayUI.OffScreenTarget.gameObject.Activate(true);
					targetOverlayUI.OffScreenTarget.transform.up = arrowUp;
				}
				else
				{
					if (!targetOverlayUI.Distance.gameObject.activeInHierarchy)
					{
						targetOverlayUI.Distance.gameObject.SetActive(true);
					}
					if (targetOverlayUI.OffScreenTarget.gameObject.activeInHierarchy)
					{
						targetOverlayUI.OffScreenTarget.gameObject.SetActive(false);
					}
				}
				List<TargetObject> allTargets = AllTargets;
				if (_003C_003Ef__am_0024cache1 == null)
				{
					_003C_003Ef__am_0024cache1 = _003CUpdateOverlayTargets_003Em__1;
				}
				IOrderedEnumerable<TargetObject> source = allTargets.OrderBy(_003C_003Ef__am_0024cache1);
				if (_003C_003Ef__am_0024cache2 == null)
				{
					_003C_003Ef__am_0024cache2 = _003CUpdateOverlayTargets_003Em__2;
				}
				HoveredTarget = source.FirstOrDefault(_003C_003Ef__am_0024cache2);
				if (HoveredTarget != targetOverlayUI.Target && targetOverlayUI.Hovered.activeInHierarchy)
				{
					targetOverlayUI.Hovered.SetActive(false);
				}
				if (HoveredTarget != null && HoveredTarget == targetOverlayUI.Target && !targetOverlayUI.Hovered.activeInHierarchy)
				{
					targetOverlayUI.Hovered.SetActive(true);
				}
				if (SelectedTarget != targetOverlayUI.Target && targetOverlayUI.Selected.activeInHierarchy)
				{
					targetOverlayUI.Default.SetActive(true);
					targetOverlayUI.Selected.SetActive(false);
					targetOverlayUI.Distance.color = Colors.GrayDefault;
				}
				if (SelectedTarget != null && SelectedTarget == targetOverlayUI.Target && !targetOverlayUI.Selected.activeInHierarchy)
				{
					targetOverlayUI.Default.SetActive(false);
					targetOverlayUI.Selected.SetActive(true);
					targetOverlayUI.Distance.color = Colors.White;
				}
				if ((targetOverlayUI.Target == SelectedTarget || targetOverlayUI.Target == HoveredTarget) && !targetOverlayUI.NameHolder.activeInHierarchy)
				{
					targetOverlayUI.NameHolder.SetActive(true);
				}
				if (targetOverlayUI.Target != SelectedTarget && targetOverlayUI.Target != HoveredTarget && targetOverlayUI.NameHolder.activeInHierarchy)
				{
					targetOverlayUI.NameHolder.SetActive(false);
				}
				Vector3 vector = (targetOverlayUI.AB.Position - ParentShip.Position).ToVector3();
				Vector3 vector2 = Quaternion.LookRotation(ParentShip.Forward, ParentShip.Up).Inverse() * vector;
				targetOverlayUI.Distance.text = FormatHelper.DistanceFormat(vector2.magnitude);
			}
		}

		private void CheckSelectedMapObject()
		{
			if (ParentShip.NavPanel != null && ParentShip.NavPanel.Map.SelectedObject != null)
			{
				SelectedMapObject = ParentShip.NavPanel.Map.SelectedObject;
			}
			else
			{
				SelectedMapObject = null;
			}
		}

		private void UpdateSelectedMapObject()
		{
			if (SelectedMapObject != null && !ManeuverObject.gameObject.activeInHierarchy)
			{
				if (SelectedMapObject.MainObject != null)
				{
					Vector3 pos = Quaternion.LookRotation(ParentShip.MainVessel.Forward, ParentShip.MainVessel.Up).Inverse() * (SelectedMapObject.MainObject.Position - ParentShip.Position).ToVector3() + ParentShip.GeometryRoot.transform.position;
					double magnitude = (SelectedMapObject.MainObject.Position - ParentShip.MainVessel.Position).Magnitude;
					Vector3 arrowUp;
					MapObject.transform.position = GetPositionOnOverlay(pos, out arrowUp);
					MapObjectName.text = SelectedMapObject.Name.ToUpper();
					MapObjectDistance.text = FormatHelper.DistanceFormat(magnitude);
					MapObject.Activate(true);
				}
				else
				{
					Vector3D position = SelectedMapObject.Orbit.Position;
					Vector3 pos2 = Quaternion.LookRotation(ParentShip.MainVessel.Forward, ParentShip.MainVessel.Up).Inverse() * (position - ParentShip.Position).ToVector3() + ParentShip.GeometryRoot.transform.position;
					double magnitude2 = (position - ParentShip.MainVessel.Position).Magnitude;
					Vector3 arrowUp2;
					MapObject.transform.position = GetPositionOnOverlay(pos2, out arrowUp2);
					MapObjectName.text = SelectedMapObject.Name.ToUpper();
					MapObjectDistance.text = FormatHelper.DistanceFormat(magnitude2);
					MapObject.Activate(true);
				}
			}
			else
			{
				MapObject.Activate(false);
			}
		}

		private void UpdateManeuverTarget()
		{
			if (ParentShip.IsWarpOnline)
			{
				ManeuverStatusHolder.Activate(true);
				ManeuverTimer.text = Localization.ETA + " " + FormatHelper.PeriodFormat(ParentShip.EndWarpTime - Client.Instance.SolarSystem.CurrentTime);
				ManeuverWarning.Activate(true);
				ManeuverObject.gameObject.Activate(false);
			}
			else if (ParentShip.CourseStartTime > 0.0 && ParentShip.CourseWaitingActivation > 0)
			{
				ManeuverObject.gameObject.Activate(true);
				ManeuverWarning.Activate(true);
				double num = ParentShip.CourseStartTime - Client.Instance.SolarSystem.CurrentTime;
				if (num > 0.0)
				{
					ManeuverStatusHolder.Activate(true);
					ManeuverTimer.text = FormatHelper.Timer((float)num);
				}
				else
				{
					ManeuverStatusHolder.Activate(false);
					ManeuverTimer.text = string.Empty;
				}
				Vector3 pos = Quaternion.LookRotation(ParentShip.MainVessel.Forward, ParentShip.MainVessel.Up).Inverse() * ParentShip.CourseStartDirection * 10000f + ParentShip.GeometryRoot.transform.position;
				Vector3 arrowUp;
				ManeuverObject.transform.position = GetPositionOnOverlay(pos, out arrowUp);
				if (arrowUp != Vector3.zero)
				{
					ManeuverOffScreen.gameObject.Activate(true);
					ManeuverOffScreen.transform.up = arrowUp;
				}
				else
				{
					ManeuverOffScreen.gameObject.Activate(false);
				}
				if (Vector3.Angle(ParentShip.Forward, ParentShip.CourseStartDirection) < 5f)
				{
					ManeuverObject.color = Colors.Green;
				}
				else
				{
					ManeuverObject.color = Colors.Red;
				}
			}
			else
			{
				ManeuverObject.gameObject.Activate(false);
				ManeuverStatusHolder.Activate(false);
				ManeuverWarning.Activate(false);
			}
		}

		private static Vector3 GetPositionOnOverlay(Vector3 pos, out Vector3 arrowUp)
		{
			arrowUp = Vector3.zero;
			Vector3 vector = MyPlayer.Instance.FpsController.MainCamera.WorldToScreenPoint(pos);
			if (vector.x < 0f || vector.x > (float)Screen.width || vector.y < 0f || vector.y > (float)Screen.height || vector.z < 0f)
			{
				Vector3 vector2 = new Vector3(Screen.width, Screen.height, 0f) / 2f;
				Vector3 vector3 = vector - vector2;
				if (vector3.z < 0f)
				{
					vector3 = Quaternion.Euler(0f, 0f, 180f) * vector3;
				}
				vector = ((!(System.Math.Abs(vector3.x / (float)Screen.width) > System.Math.Abs(vector3.y / (float)Screen.height))) ? (vector3 / System.Math.Abs(vector3.y / ((float)Screen.height / 2f)) + vector2) : (vector3 / System.Math.Abs(vector3.x / ((float)Screen.width / 2f)) + vector2));
				arrowUp = (vector2 - vector).normalized;
				arrowUp.z = 0f;
			}
			vector.z = 0f;
			return vector;
		}

		private void RotationIndicatorUpdate()
		{
			targetShipRotation = ((!ParentShip.TargetRotation.HasValue) ? Quaternion.LookRotation(ParentShip.transform.forward, ParentShip.transform.up) : (Quaternion.LookRotation(ParentShip.Forward, ParentShip.Up).Inverse() * ParentShip.TargetRotation.Value)).ToQuaternionD().Inverse() * Vector3D.ProjectOnPlane(ParentShip.MainVessel.RotationVec, ParentShip.transform.forward.ToVector3D());
			currentShipRotation = Vector3D.Lerp(currentShipRotation, targetShipRotation, Time.deltaTime * 4f);
			Vector3 shipRotationCursor = MyPlayer.Instance.ShipRotationCursor;
			if (shipRotationCursor.magnitude > 0.01f)
			{
				float num = Mathf.Clamp(shipRotationCursor.magnitude * 4f, 0f, RotationIndicator.rect.height);
				if (!RotationIndicatorArrow.gameObject.activeInHierarchy)
				{
					RotationIndicatorArrow.gameObject.SetActive(true);
				}
				RotationIndicator.localRotation = Quaternion.Euler(0f, 0f, MathHelper.AngleSigned(Vector3.right, shipRotationCursor, Vector3.forward));
				RotationIndicatorArrow.transform.localPosition = new Vector3(0f, 0f - num, 0f);
				Color color = RotationIndicatorArrow.GetComponent<Image>().color;
				color.a = num / RotationIndicator.rect.height;
				RotationIndicatorArrow.GetComponent<Image>().color = color;
				RotationDanger.SetActive(num / RotationIndicator.rect.height > 0.9f);
				if (Client.Instance.CanvasManager.AutoStabilization && !ParentShip.IsRotationStabilized)
				{
					StabilizeRotationTimer += Time.deltaTime;
					if (shipRotationCursor.magnitude > 5f || (double)shipRotationCursor.magnitude < 0.5 || System.Math.Abs(MyPlayer.Instance.FpsController.MouseRightAxis) > float.Epsilon || System.Math.Abs(MyPlayer.Instance.FpsController.MouseUpAxis) > float.Epsilon)
					{
						StabilizeRotationTimer = 0f;
					}
					if (StabilizeRotationTimer >= stabilizeRotationTreshold)
					{
						Ship parentShip = ParentShip;
						Vector3? autoStabilize = Vector3.one;
						parentShip.ChangeStats(null, null, autoStabilize);
						StabilizeRotationTimer = 0f;
					}
				}
			}
			else if (RotationIndicatorArrow.gameObject.activeInHierarchy)
			{
				RotationIndicatorArrow.gameObject.SetActive(false);
			}
		}

		private void EtaUpdate()
		{
			OffTarget.Activate(onSpeed < 0f);
			EtaHolder.Activate(onSpeed >= 0f);
			if (onSpeed > 0f)
			{
				float num = ((distance <= 1000f) ? 30f : ((!(distance > 1000f) || !(distance <= 10000f)) ? 300f : 100f));
				EtaTimeline.text = num.ToString("0") + " s";
				ETA = Mathf.Abs(distance / onSpeed);
				if (ParentShip.Engine != null && ParentShip.EngineOnLine)
				{
					SpeedTime = Mathf.Abs(onSpeed / ParentShip.Engine.Acceleration);
				}
				else
				{
					SpeedTime = Mathf.Abs(onSpeed / ParentShip.RCS.Acceleration);
				}
				Vector3 vector = new Vector3(Mathf.Clamp01(Mathf.Abs(ETA / num)), 1f, 1f);
				if (vector.IsValid())
				{
					DistanceLimiterGraph.gameObject.transform.localScale = vector;
				}
				float num2 = DistanceLimiterGraph.GetComponent<RectTransform>().rect.width / 2f * DistanceLimiterGraph.transform.localScale.x;
				LeftLimiter.transform.localPosition = new Vector3(0f - num2, 0f, 0f);
				RightLimiter.transform.localPosition = new Vector3(num2, 0f, 0f);
				DirectionalSpeedGraph.gameObject.transform.localScale = new Vector3(Mathf.Clamp01(Mathf.Abs(SpeedTime / num)), 1f, 1f);
				CollisionImminent.Activate(ETA < SpeedTime);
				if (ETA < SpeedTime)
				{
					DirectionalSpeedGraph.color = Colors.Red50;
				}
				else
				{
					DirectionalSpeedGraph.color = Colors.Blue50;
				}
			}
			if (onSpeed > 0f)
			{
				CenterCroshair.color = Colors.BlueLight;
			}
			else if (onSpeed < 0f)
			{
				CenterCroshair.color = Colors.Red;
			}
			else
			{
				CenterCroshair.color = Colors.Green;
			}
		}

		private void OffTargetVelocity()
		{
			if (SelectedTarget != null)
			{
				Vector3 vector = Quaternion.LookRotation(ParentShip.Forward, ParentShip.Up).Inverse() * (ParentShip.Velocity - SelectedTarget.AB.Velocity).ToVector3();
				Vector3 vector2 = Vector3.ProjectOnPlane(vector, Vector3.forward);
				Vector3 vector3 = vector2;
				if (vector3.x < 0f)
				{
					LatRight.fillAmount = 0f;
					LatLeft.fillAmount = Mathf.Clamp(Mathf.Abs(vector3.x) / lateralIndicationSensitivity, 0f, 1f);
				}
				else
				{
					LatLeft.fillAmount = 0f;
					LatRight.fillAmount = Mathf.Clamp(Mathf.Abs(vector3.x) / lateralIndicationSensitivity, 0f, 1f);
				}
				if (vector3.y > 0f)
				{
					LatDown.fillAmount = 0f;
					LatUp.fillAmount = Mathf.Clamp(Mathf.Abs(vector3.y) / lateralIndicationSensitivity, 0f, 1f);
				}
				else
				{
					LatUp.fillAmount = 0f;
					LatDown.fillAmount = Mathf.Clamp(Mathf.Abs(vector3.y) / lateralIndicationSensitivity, 0f, 1f);
				}
			}
			else
			{
				LatLeft.fillAmount = 0f;
				LatRight.fillAmount = 0f;
				LatUp.fillAmount = 0f;
				LatDown.fillAmount = 0f;
			}
		}

		private void CheckTargetStabilization()
		{
			if (ParentShip.MainVessel.IsStabilized || SelectedTarget == null || (SelectedTarget.AB.Velocity - ParentShip.Velocity).Magnitude > stabilizeToTargetMaxVelocityDiff || (SelectedTarget.AB.Position - ParentShip.Position).Magnitude - SelectedTarget.AB.Radius > stabilizeToTargetMaxPositionDiff)
			{
				CanMatchTarget.SetActive(false);
			}
			else
			{
				CanMatchTarget.SetActive(true);
			}
			if (ParentShip.MainVessel.IsStabilized && !MatchedToTarget.activeInHierarchy)
			{
				SpaceObjectVessel spaceObjectVessel = ParentShip.MainVessel.StabilizeToTargetObj as SpaceObjectVessel;
				MatchedTo.text = Localization.Matched.ToUpper() + " - " + ((spaceObjectVessel.StabilizedChildren.Count > 1) ? Localization.ClusterOfVessels.ToUpper() : spaceObjectVessel.CustomName);
				MatchedToTarget.SetActive(true);
			}
			else if (!ParentShip.MainVessel.IsStabilized && MatchedToTarget.activeInHierarchy)
			{
				MatchedTo.text = string.Empty;
				MatchedToTarget.SetActive(false);
			}
		}

		public void StartTargetStabilization()
		{
			SpaceObjectVessel mainVessel = ParentShip.MainVessel;
			long? stabilizationTarget = SelectedTarget.AB.GUID;
			mainVessel.ChangeStats(null, null, null, null, null, null, null, null, null, null, null, stabilizationTarget);
		}

		private void NoTargetAvailable()
		{
			ParametersHolder.SetActive(false);
			LatLeft.fillAmount = 0f;
			LatRight.fillAmount = 0f;
			LatUp.fillAmount = 0f;
			LatDown.fillAmount = 0f;
		}

		private void EngineStatusUpdate()
		{
			if (ParentShip.EngineThrustPercentage > 0f)
			{
				PositiveEngineFiller.fillAmount = Mathf.Abs(ParentShip.EngineThrustPercentage);
				NegativeEngineFiller.fillAmount = 0f;
			}
			else if (ParentShip.EngineThrustPercentage < 0f)
			{
				PositiveEngineFiller.fillAmount = 0f;
				NegativeEngineFiller.fillAmount = Mathf.Abs(ParentShip.EngineThrustPercentage);
			}
			else
			{
				PositiveEngineFiller.fillAmount = 0f;
				NegativeEngineFiller.fillAmount = 0f;
			}
		}

		public void OffSpeedHelperActive()
		{
			OffSpeedHelper.Activate(true);
		}

		public void OffSpeedHelperInactive()
		{
			OffSpeedHelper.Activate(false);
		}

		[CompilerGenerated]
		private static ArtificialBody _003CUpdateRadarList_003Em__0(TargetObject m)
		{
			return m.AB;
		}

		[CompilerGenerated]
		private static float _003CUpdateOverlayTargets_003Em__1(TargetObject m)
		{
			return m.AngleFromCameraForward;
		}

		[CompilerGenerated]
		private static bool _003CUpdateOverlayTargets_003Em__2(TargetObject m)
		{
			return m.AngleFromCameraForward < 3f;
		}
	}
}
