using System.Collections.Generic;
using System.Linq;
using TriInspector;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Math;
using ZeroGravity.Objects;
using ZeroGravity.UI;

namespace ZeroGravity.ShipComponents
{
	public class DockingPanel : MonoBehaviour
	{
		public class DockingPortUI
		{
			public SceneDockingPort Port;

			public DockingPanelUIItem UI;
		}

		private UpdateTimer Timer = new UpdateTimer(0.35f);

		public float ThrustModifier = 0.2f;

		private Ship ParentShip;

		public List<Ship> AvailableTargetShips = new List<Ship>();

		public RectTransform AvailableDockingPortsTransform;

		public RectTransform TargetedModulePortsTranform;

		public DockingPanelUIItem ListItemPrefab;

		public List<DockingPortUI> MyDockingPortsUI = new List<DockingPortUI>();

		private List<DockingPortUI> MyVisibleDockingPortsUI = new List<DockingPortUI>();

		private List<SceneDockingPort> DockingPorts = new List<SceneDockingPort>();

		private int myCurrentPort;

		public SceneDockingPort TargetDockingPort;

		public List<DockingPortUI> TargetedModulePorts = new List<DockingPortUI>();

		private int currentTargetedModulePortIndex;

		private bool changeTargetedModulePort;

		public GameObject HasTargetPanel;

		public Text DistanceValText;

		public GameObject DistanceGraph;

		public GameObject DirectionalGraph;

		public GameObject RCSPanel;

		public Image RcsFuelSlider;

		public Image FillerRcs;

		public Text RcsFuelText;

		public Text RCSFuelStatus;

		private bool TestStabilization;

		private int dockingPortTriggerLayer;

		private Transform MainCameraParent;

		private Vector3 MainCameraPosition;

		private Quaternion MainCameraRotation;

		private Vector3 MainCameraScale;

		private float radarScanningRange = 3000f;

		[SerializeField]
		private Material DockingEffectMat;

		public DockingPanelRadarTarget RadarTarget;

		public Transform PanelCentarRect;

		public float timer = 2f;

		public float timerCounter;

		public bool timerPositive = true;

		private GameObject currentRootBlinker;

		[SerializeField]
		private Text OnSpeedValText;

		[SerializeField]
		private Text OffSpeedText;

		[SerializeField]
		private Text OffSpeedValText;

		[SerializeField]
		private Image RollAngleIndicator;

		public Image RotationIndicator;

		[SerializeField]
		private Image lateralXE;

		[SerializeField]
		private Image lateralXW;

		[SerializeField]
		private Image lateralYN;

		[SerializeField]
		private Image lateralYS;

		[SerializeField]
		private Text ValForX;

		[SerializeField]
		private Text ValForY;

		public Text ControlsForChangingPort;

		public GameObject DockingTips;

		[Title("Target module")]
		public GameObject NoTargetModule;

		public GameObject AvailablePortsHolder;

		public Image TargetModuleIcon;

		public Text TargetModuleName;

		public SceneDockingPort DockingPort => (DockingPorts.Count - 1 >= myCurrentPort) ? DockingPorts[myCurrentPort] : null;

		public bool IsDockingEnabled => ParentShip != null && !ParentShip.DockingControlsDisabled && MyDockingPortsUI.Count > 0;

		private void Awake()
		{
			dockingPortTriggerLayer = 1 << LayerMask.NameToLayer("Triggers");
		}

		private void Start()
		{
			RCSPanel.SetActive(ParentShip.RCS != null);
			ControlsForChangingPort.text = string.Format(Localization.ControlChangeDockingPort, GetControlName(InputManager.AxisNames.R)).ToUpper();
		}

		private void Update()
		{
			if (!Client.IsGameBuild || (MyPlayer.Instance.ShipControlMode != ShipControlMode.Docking && !(MyPlayer.Instance.LockedToTrigger is SceneTriggerDockingPanel)))
			{
				return;
			}
			if (Timer.Update())
			{
				ReloadRadarElements();
			}
			if (!Client.Instance.CanvasManager.ConsoleIsUp)
			{
				if (InputManager.GetButtonDown(InputManager.AxisNames.R))
				{
					TargetDockingPort = null;
					changeTargetedModulePort = true;
					SwitchPort();
					ReloadRadarElements();
					UpdateTargetedModule();
				}
				if (InputManager.GetAxis(InputManager.AxisNames.MouseWheel).IsNotEpsilonZero() && TargetedModulePorts.Count > 0)
				{
					TargetedModulePorts[currentTargetedModulePortIndex].UI.IsSelected = false;
					TargetedModulePorts[currentTargetedModulePortIndex].UI.Distance = string.Empty;
					float axis = InputManager.GetAxis(InputManager.AxisNames.MouseWheel);
					if (axis > 0f)
					{
						if (currentTargetedModulePortIndex - 1 >= 0)
						{
							currentTargetedModulePortIndex--;
						}
						else
						{
							currentTargetedModulePortIndex = TargetedModulePorts.Count - 1;
						}
					}
					else if (axis < 0f)
					{
						if (TargetedModulePorts.Count - 1 >= currentTargetedModulePortIndex + 1)
						{
							currentTargetedModulePortIndex++;
						}
						else
						{
							currentTargetedModulePortIndex = 0;
						}
					}
					ChangeTargetedPort();
				}
				bool buttonDown = InputManager.GetButtonDown(InputManager.AxisNames.UpArrow);
				bool buttonDown2 = InputManager.GetButtonDown(InputManager.AxisNames.DownArrow);
				if (TargetedModulePorts.Count > 0 && (buttonDown || buttonDown2))
				{
					TargetedModulePorts[currentTargetedModulePortIndex].UI.IsSelected = false;
					TargetedModulePorts[currentTargetedModulePortIndex].UI.Distance = string.Empty;
					if (buttonDown2)
					{
						if (TargetedModulePorts.Count - 1 >= currentTargetedModulePortIndex + 1)
						{
							currentTargetedModulePortIndex++;
						}
						else
						{
							currentTargetedModulePortIndex = 0;
						}
					}
					else if (buttonDown)
					{
						if (currentTargetedModulePortIndex - 1 >= 0)
						{
							currentTargetedModulePortIndex--;
						}
						else
						{
							currentTargetedModulePortIndex = TargetedModulePorts.Count - 1;
						}
					}
					ChangeTargetedPort();
				}
			}
			SceneDockingPort dockingPort = DockingPort;
			if (TargetDockingPort != null && dockingPort != null)
			{
				Vector3 vector = (TargetDockingPort.ParentShip.Position - dockingPort.ParentShip.Position).ToVector3();
				Vector3 vector2 = Quaternion.LookRotation(dockingPort.ParentShip.Forward, dockingPort.ParentShip.Up).Inverse() * (dockingPort.ParentShip.Velocity - TargetDockingPort.ParentShip.Velocity).ToVector3();
				Vector3 vector3 = Vector3.Project((dockingPort.ParentShip.Velocity - TargetDockingPort.ParentShip.Velocity).ToVector3(), dockingPort.CameraPosition.forward);
				Vector3 vector4 = dockingPort.CameraPosition.rotation.Inverse() * Vector3.ProjectOnPlane(vector2, dockingPort.CameraPosition.forward);
				ValForX.text = FormatHelper.DistanceFormat(vector4.x) + "/s";
				ValForY.text = FormatHelper.DistanceFormat(vector4.y) + "/s";
				Vector3 vector5 = vector4;
				if (vector5.x > 0f)
				{
					lateralXW.fillAmount = 0f;
					lateralXE.fillAmount = Mathf.Clamp(Mathf.Abs(vector5.x) / 3f, 0f, 1f);
				}
				else
				{
					lateralXE.fillAmount = 0f;
					lateralXW.fillAmount = Mathf.Clamp(Mathf.Abs(vector5.x) / 3f, 0f, 1f);
				}
				if (vector5.y < 0f)
				{
					lateralYN.fillAmount = 0f;
					lateralYS.fillAmount = Mathf.Clamp(Mathf.Abs(vector5.y) / 3f, 0f, 1f);
				}
				else
				{
					lateralYS.fillAmount = 0f;
					lateralYN.fillAmount = Mathf.Clamp(Mathf.Abs(vector5.y) / 3f, 0f, 1f);
				}
				Vector3 relativePosition = GetRelativePosition(TargetDockingPort.CameraPosition.position);
				float f = vector3.magnitude * (float)MathHelper.Sign(Vector3.Dot(vector.normalized, vector3.normalized));
				OnSpeedValText.text = f.ToString("f1");
				OffSpeedValText.text = vector4.magnitude.ToString("f1");
				DirectionalGraph.transform.localScale = new Vector3(Mathf.Clamp(Mathf.Abs(f) / 5f, 0.01f, 1f), 1f, 1f);
				if (Mathf.Abs(f) <= 1f)
				{
					DirectionalGraph.GetComponent<Image>().color = Colors.Green;
				}
				else if (Mathf.Abs(f) > 1f && Mathf.Abs(f) <= 5f)
				{
					DirectionalGraph.GetComponent<Image>().color = Colors.Orange;
				}
				else
				{
					DirectionalGraph.GetComponent<Image>().color = Colors.Red;
				}
				Quaternion quaternion = dockingPort.SceneDockingTriggerTrans.rotation * Quaternion.LookRotation(-TargetDockingPort.SceneDockingTriggerTrans.forward, TargetDockingPort.SceneDockingTriggerTrans.up).Inverse();
				float num = quaternion.eulerAngles.x;
				float num2 = quaternion.eulerAngles.y;
				float num3 = quaternion.eulerAngles.z;
				if (num2 > 180f)
				{
					num2 -= 360f;
				}
				if (num > 180f)
				{
					num -= 360f;
				}
				if (num3 > 180f)
				{
					num3 -= 360f;
				}
				float num4 = vector3.magnitude * (float)MathHelper.Sign(Vector3.Dot(vector.normalized, vector3.normalized));
				Vector3 vector6 = TargetDockingPort.SceneDockingTriggerTrans.position - dockingPort.SceneDockingTriggerTrans.position;
				float num5 = Vector3.SignedAngle(TargetDockingPort.SceneDockingTriggerTrans.up, Vector3.ProjectOnPlane(dockingPort.SceneDockingTriggerTrans.up, TargetDockingPort.SceneDockingTriggerTrans.forward), TargetDockingPort.SceneDockingTriggerTrans.forward);
				float num6 = MathHelper.AngleSigned(dockingPort.SceneDockingTriggerTrans.forward, -TargetDockingPort.SceneDockingTriggerTrans.forward, dockingPort.SceneDockingTriggerTrans.up);
				float num7 = Vector3.Distance(dockingPort.SceneDockingTriggerTrans.position, TargetDockingPort.SceneDockingTriggerTrans.position);
				if (!RotationIndicator.gameObject.activeInHierarchy)
				{
					RotationIndicator.gameObject.SetActive(value: true);
				}
				RollAngleIndicator.fillClockwise = num5 < 0f;
				RollAngleIndicator.fillAmount = Mathf.Abs(num5) / 360f;
				if (Mathf.Abs(num5) >= 5f)
				{
					RotationIndicator.color = Colors.Red;
					RollAngleIndicator.color = Colors.Red;
				}
				else
				{
					RotationIndicator.color = Colors.Green;
					RollAngleIndicator.color = Colors.Green;
				}
				DistanceValText.text = FormatHelper.DistanceFormat(num7);
				DistanceGraph.transform.localScale = new Vector3(Mathf.Clamp(Mathf.Abs(num7) / 50f, 0.01f, 1f), 1f, 1f);
				if (Mathf.Abs(num7) <= 5f)
				{
					DistanceGraph.GetComponent<Image>().color = Colors.Green;
				}
				else if (Mathf.Abs(num7) > 5f && Mathf.Abs(num7) <= 50f)
				{
					DistanceGraph.GetComponent<Image>().color = Colors.Orange;
				}
				else
				{
					DistanceGraph.GetComponent<Image>().color = Colors.Red;
				}
				if (Mathf.Abs(num) < 10f && Mathf.Abs(num2) < 10f && Mathf.Abs(num3) < 10f && num7 < 2f && num4 < 3f)
				{
					if (dockingPort.ParentShip.Engine != null && TargetDockingPort.ParentShip.Engine == null && !TargetDockingPort.ParentShip.IsDocked && TargetDockingPort.ParentShip.AllDockedVessels.Count == 0 && !dockingPort.ParentShip.IsDocked && dockingPort.ParentShip.AllDockedVessels.Count == 0)
					{
						TargetDockingPort.ToggleDockTo(dockingPort, toggle: true);
					}
					else
					{
						dockingPort.ToggleDockTo(TargetDockingPort, toggle: true);
					}
					TargetDockingPort = null;
				}
				if (MyPlayer.Instance.IsAdmin && (Input.GetKeyDown(KeyCode.Keypad9) || Input.GetKeyDown(KeyCode.Keypad8)))
				{
					if (dockingPort.ParentShip.Engine != null && TargetDockingPort.ParentShip.Engine == null && !TargetDockingPort.ParentShip.IsDocked && TargetDockingPort.ParentShip.AllDockedVessels.Count == 0 && !dockingPort.ParentShip.IsDocked && dockingPort.ParentShip.AllDockedVessels.Count == 0)
					{
						TargetDockingPort.ToggleDockTo(dockingPort, toggle: true);
					}
					else if (Input.GetKeyDown(KeyCode.Keypad9))
					{
						dockingPort.ToggleDockTo(TargetDockingPort, toggle: true);
					}
					else
					{
						TargetDockingPort.ToggleDockTo(dockingPort, toggle: true);
					}
					TargetDockingPort = null;
				}
				RadarTarget.gameObject.Activate(value: true);
				RadarTarget.ParentTrans = null;
				RadarTarget.ParentTrans = dockingPort.CameraPosition;
				if (TargetDockingPort != null)
				{
					RadarTarget.TargetTrans = TargetDockingPort.CameraPosition;
				}
			}
			else
			{
				RadarTarget.gameObject.Activate(value: false);
				RotationIndicator.gameObject.Activate(value: false);
				RollAngleIndicator.fillAmount = 0f;
				lateralXE.fillAmount = 0f;
				lateralXW.fillAmount = 0f;
				lateralYN.fillAmount = 0f;
				lateralYS.fillAmount = 0f;
			}
			if (timerCounter > timer || timerCounter < 0f)
			{
				timerPositive = !timerPositive;
			}
			if (timerPositive)
			{
				timerCounter += Time.deltaTime;
			}
			else
			{
				timerCounter -= Time.deltaTime;
			}
		}

		private void ChangeTargetedPort()
		{
			TargetedModulePorts[currentTargetedModulePortIndex].UI.IsSelected = true;
			TargetedModulePorts[currentTargetedModulePortIndex].UI.Distance = Localization.Selected.ToUpper();
			if (currentRootBlinker != null)
			{
				currentRootBlinker.SetActive(value: false);
			}
			currentRootBlinker = TargetedModulePorts[currentTargetedModulePortIndex].Port.DockingVisualROOT;
			if (currentRootBlinker != null)
			{
				currentRootBlinker.SetActive(value: true);
			}
			TargetDockingPort = TargetedModulePorts[currentTargetedModulePortIndex].Port;
			if (TargetedModulePorts.Count > 8)
			{
				float y = 34.5f * (float)currentTargetedModulePortIndex - 34.5f;
				TargetedModulePortsTranform.anchoredPosition = new Vector2(0f, y);
			}
			else if (TargetedModulePorts.Count == 0)
			{
				TargetedModulePortsTranform.anchoredPosition = new Vector2(0f, 0f);
			}
			UpdateTargetedModule();
		}

		private void UpdateTargetedModule()
		{
			if (TargetDockingPort != null)
			{
				NoTargetModule.Activate(value: false);
				TargetModuleIcon.sprite = Client.Instance.SpriteManager.GetSprite(TargetDockingPort.ParentShip);
				TargetModuleName.text = TargetDockingPort.ParentShip.MainVessel.CustomName;
				AvailablePortsHolder.Activate(value: true);
			}
			else
			{
				NoTargetModule.Activate(value: true);
				AvailablePortsHolder.Activate(value: false);
			}
		}

		private Vector3 GetRelativePosition(Vector3 pos)
		{
			return (pos - MyPlayer.Instance.transform.position).normalized * (base.transform.position - MyPlayer.Instance.transform.position).magnitude + MyPlayer.Instance.transform.position;
		}

		public void OnInteract(Ship ship)
		{
			ParentShip = ship;
			MainCameraPosition = MyPlayer.Instance.FpsController.MainCamera.transform.localPosition;
			MainCameraRotation = MyPlayer.Instance.FpsController.MainCamera.transform.localRotation;
			MainCameraScale = MyPlayer.Instance.FpsController.MainCamera.transform.localScale;
			MainCameraParent = MyPlayer.Instance.FpsController.MainCamera.transform.parent;
			TargetDockingPort = null;
			AvailableTargetShips.Clear();
			foreach (DockingPortUI targetedModulePort in TargetedModulePorts)
			{
				Object.Destroy(targetedModulePort.UI.gameObject);
			}
			TargetedModulePorts.Clear();
			RefreshDockingPorts();
			if (myCurrentPort > DockingPorts.Count - 1)
			{
				myCurrentPort = 0;
			}
			SceneDockingPort dockingPort = DockingPort;
			if (dockingPort != null)
			{
				MyPlayer.Instance.FpsController.MainCamera.transform.parent = dockingPort.CameraPosition.parent;
				MyPlayer.Instance.FpsController.MainCamera.transform.position = dockingPort.CameraPosition.position;
				MyPlayer.Instance.FpsController.MainCamera.transform.rotation = dockingPort.CameraPosition.rotation;
				MyPlayer.Instance.FpsController.MainCamera.transform.localScale = dockingPort.CameraPosition.localScale;
			}
			changeTargetedModulePort = true;
			MakeMyDockingPorts();
			ReloadRadarElements();
			DockingTips.Activate(Client.Instance.CanvasManager.ShowTips);
			MakeTargetModulePorts();
			base.gameObject.SetActive(value: true);
		}

		private void RefreshDockingPorts()
		{
			DockingPorts.Clear();
			if (MyPlayer.Instance.Parent is SpaceObjectVessel && (MyPlayer.Instance.Parent as SpaceObjectVessel).SceneID == GameScenes.SceneID.AltCorp_Shuttle_CECA && MyPlayer.Instance.ShipControlMode == ShipControlMode.Docking)
			{
				foreach (SceneDockingPort value in ParentShip.MainVessel.DockingPorts.Values)
				{
					if (!value.LocalyDisabled && value.DockedToPort == null)
					{
						DockingPorts.Add(value);
					}
				}
				foreach (SpaceObjectVessel allDockedVessel in ParentShip.MainVessel.AllDockedVessels)
				{
					foreach (SceneDockingPort value2 in allDockedVessel.DockingPorts.Values)
					{
						if (!value2.LocalyDisabled && value2.DockedToPort == null)
						{
							DockingPorts.Add(value2);
						}
					}
				}
			}
			else
			{
				SceneDockingPort[] componentsInChildren = ParentShip.GeometryRoot.GetComponentsInChildren<SceneDockingPort>(includeInactive: true);
				foreach (SceneDockingPort sceneDockingPort in componentsInChildren)
				{
					if (!sceneDockingPort.LocalyDisabled)
					{
						DockingPorts.Add(sceneDockingPort);
					}
				}
			}
			DockingPorts = (from m in DockingPorts
				where !m.Locked
				orderby m.ParentShip != ParentShip, m.ParentShip.GUID, m.Name
				select m).ToList();
		}

		public void OnDetach()
		{
			MyPlayer.Instance.FpsController.MainCamera.transform.parent = MainCameraParent;
			MyPlayer.Instance.FpsController.MainCamera.transform.localPosition = MainCameraPosition;
			MyPlayer.Instance.FpsController.MainCamera.transform.localRotation = MainCameraRotation;
			MyPlayer.Instance.FpsController.MainCamera.transform.localScale = MainCameraScale;
			if (currentRootBlinker != null)
			{
				currentRootBlinker.SetActive(value: false);
			}
			RadarTarget.gameObject.SetActive(value: false);
			SceneDockingPort[] componentsInChildren = ParentShip.GeometryRoot.GetComponentsInChildren<SceneDockingPort>(includeInactive: true);
			foreach (SceneDockingPort dockingPort in componentsInChildren)
			{
				Client.Instance.CubemapRenderer.RemoveDockingPort(dockingPort);
			}
			base.gameObject.SetActive(value: false);
		}

		private void SwitchPort()
		{
			if (MyDockingPortsUI.Count > 1)
			{
				MyDockingPortsUI[myCurrentPort].UI.IsSelected = false;
				MyDockingPortsUI[myCurrentPort].UI.DistanceText.text = string.Empty;
				do
				{
					if (MyDockingPortsUI.Count - 1 >= myCurrentPort + 1)
					{
						myCurrentPort++;
					}
					else
					{
						myCurrentPort = 0;
					}
				}
				while (DockingPorts[myCurrentPort].IgnoreThisDockingPort);
				MyDockingPortsUI[myCurrentPort].UI.IsSelected = true;
				MyDockingPortsUI[myCurrentPort].UI.DistanceText.text = Localization.Selected.ToUpper();
				MyPlayer.Instance.FpsController.MainCamera.transform.parent = MyDockingPortsUI[myCurrentPort].Port.CameraPosition.parent;
				MyPlayer.Instance.FpsController.MainCamera.transform.position = MyDockingPortsUI[myCurrentPort].Port.CameraPosition.position;
				MyPlayer.Instance.FpsController.MainCamera.transform.rotation = MyDockingPortsUI[myCurrentPort].Port.CameraPosition.rotation;
				MyPlayer.Instance.FpsController.MainCamera.transform.localScale = MyDockingPortsUI[myCurrentPort].Port.CameraPosition.localScale;
			}
			if (MyVisibleDockingPortsUI.Count > 8)
			{
				float y = 34.5f * (float)MyVisibleDockingPortsUI.IndexOf(MyDockingPortsUI[myCurrentPort]) - 34.5f;
				AvailableDockingPortsTransform.anchoredPosition = new Vector2(0f, y);
			}
			else if (MyVisibleDockingPortsUI.Count == 0)
			{
				AvailableDockingPortsTransform.anchoredPosition = new Vector2(0f, 0f);
			}
		}

		private void ReloadRadarElements()
		{
			List<Ship> list = new List<Ship>(AvailableTargetShips);
			foreach (ArtificialBody ab in Client.Instance.SolarSystem.ArtificialBodies.Where((ArtificialBody m) => m is SpaceObjectVessel && (m as SpaceObjectVessel).MainVessel != ParentShip.MainVessel))
			{
				float num = (float)(ParentShip.Position - ab.Position).Magnitude;
				Ship ship = AvailableTargetShips.Find((Ship m) => m == ab as Ship);
				if (ab is Ship && num < radarScanningRange)
				{
					if (ship == null)
					{
						AvailableTargetShips.Add(ab as Ship);
					}
					else
					{
						list.Remove(ship);
					}
				}
			}
			if (list.Count > 0)
			{
				foreach (Ship item in list)
				{
					AvailableTargetShips.Remove(item);
				}
				changeTargetedModulePort = true;
			}
			if (changeTargetedModulePort)
			{
				TargetDockingPort = null;
				MakeTargetModulePorts();
				changeTargetedModulePort = false;
			}
			if (ParentShip != null && ParentShip.RCS != null)
			{
				float num2 = ParentShip.RCS.ResourceContainers[0].Quantity / ParentShip.RCS.ResourceContainers[0].Capacity;
				RcsFuelSlider.fillAmount = num2;
				RcsFuelText.text = ParentShip.RCS.ResourceContainers[0].Quantity.ToString("f0") + " / " + ParentShip.RCS.ResourceContainers[0].Capacity.ToString("f0");
				if (num2 <= 0.2f)
				{
					FillerRcs.color = Colors.Red;
					RCSFuelStatus.gameObject.SetActive(value: true);
					if (num2 > 0.01f)
					{
						RCSFuelStatus.text = Localization.LowFuel.ToUpper();
					}
					else if (num2 <= 0.01f)
					{
						RCSFuelStatus.text = Localization.NoFuel.ToUpper();
					}
				}
				else
				{
					FillerRcs.color = Colors.White;
					RCSFuelStatus.gameObject.SetActive(value: false);
				}
			}
			UpdateTargetPortsUI();
		}

		private void MakeMyDockingPorts()
		{
			foreach (DockingPortUI item in MyDockingPortsUI)
			{
				item.UI.gameObject.SetActive(value: false);
				Object.Destroy(item.UI.gameObject);
			}
			MyDockingPortsUI.Clear();
			MyVisibleDockingPortsUI.Clear();
			foreach (SceneDockingPort dockingPort in DockingPorts)
			{
				DockingPortUI dockingPortUI = new DockingPortUI();
				dockingPortUI.Port = dockingPort;
				dockingPortUI.UI = MakeDockingPortUI(dockingPort);
				MyDockingPortsUI.Add(dockingPortUI);
				if (!dockingPort.IgnoreThisDockingPort)
				{
					MyVisibleDockingPortsUI.Add(dockingPortUI);
				}
			}
			if (myCurrentPort > DockingPorts.Count - 1)
			{
				myCurrentPort = 0;
			}
			if (MyDockingPortsUI.Count > 0)
			{
				MyDockingPortsUI[myCurrentPort].UI.IsSelected = true;
				MyDockingPortsUI[myCurrentPort].UI.Distance = Localization.Selected.ToUpper();
			}
		}

		private DockingPanelUIItem MakeDockingPortUI(SceneDockingPort port)
		{
			DockingPanelUIItem dockingPanelUIItem = Object.Instantiate(ListItemPrefab, AvailableDockingPortsTransform);
			dockingPanelUIItem.gameObject.SetActive(!port.IgnoreThisDockingPort && !port.Locked);
			dockingPanelUIItem.transform.localScale = Vector3.one;
			dockingPanelUIItem.gameObject.SetActive(!port.IgnoreThisDockingPort);
			dockingPanelUIItem.IsSelected = false;
			dockingPanelUIItem.NameText.text = ((!(port.ParentShip == ParentShip)) ? (GameScenes.GetShortVesselClassName(port.ParentShip.SceneID) + "-") : string.Empty) + port.Name;
			dockingPanelUIItem.DistanceText.text = string.Empty;
			return dockingPanelUIItem;
		}

		private void MakeTargetModulePorts()
		{
			if (currentRootBlinker != null)
			{
				currentRootBlinker.SetActive(value: false);
			}
			foreach (DockingPortUI targetedModulePort in TargetedModulePorts)
			{
				Object.Destroy(targetedModulePort.UI.gameObject);
			}
			TargetedModulePorts.Clear();
			if (AvailableTargetShips.Count > 0)
			{
				foreach (Ship availableTargetShip in AvailableTargetShips)
				{
					foreach (SceneDockingPort item in availableTargetShip.DockingPorts.Values.Where((SceneDockingPort m) => !m.Locked))
					{
						if (item.DockedToPort == null && SceneHelper.CompareTags(item.DockType, DockingPort.DockType) && item.ParentShip.IsPlayerAuthorizedOrNoSecurity(MyPlayer.Instance))
						{
							DockingPortUI dockingPortUI = new DockingPortUI();
							dockingPortUI.Port = item;
							dockingPortUI.UI = MakeTargetModulePort(item);
							TargetedModulePorts.Add(dockingPortUI);
						}
					}
				}
			}
			currentTargetedModulePortIndex = 0;
			UpdateTargetPortsUI();
		}

		private void UpdateTargetPortsUI()
		{
			if (TargetedModulePorts.Count > 0)
			{
				NoTargetModule.Activate(value: false);
				TargetedModulePorts[currentTargetedModulePortIndex].UI.IsSelected = true;
				TargetedModulePorts[currentTargetedModulePortIndex].UI.Distance = Localization.Selected.ToUpper();
				currentRootBlinker = TargetedModulePorts[currentTargetedModulePortIndex].Port.DockingVisualROOT;
				if (currentRootBlinker != null)
				{
					currentRootBlinker.SetActive(value: true);
				}
				TargetDockingPort = TargetedModulePorts[currentTargetedModulePortIndex].Port;
				HasTargetPanel.Activate(value: true);
				UpdateTargetedModule();
			}
			else
			{
				HasTargetPanel.Activate(value: false);
				UpdateTargetedModule();
			}
		}

		private DockingPanelUIItem MakeTargetModulePort(SceneDockingPort port)
		{
			DockingPanelUIItem dockingPanelUIItem = Object.Instantiate(ListItemPrefab, TargetedModulePortsTranform);
			dockingPanelUIItem.transform.localScale = Vector3.one;
			dockingPanelUIItem.gameObject.SetActive(value: true);
			dockingPanelUIItem.IsSelected = false;
			dockingPanelUIItem.NameText.text = ((!(port.ParentShip == ParentShip)) ? (GameScenes.GetShortVesselClassName(port.ParentShip.SceneID) + "-") : string.Empty) + port.Name;
			dockingPanelUIItem.DistanceText.text = string.Empty;
			return dockingPanelUIItem;
		}

		public string GetControlName(InputManager.AxisNames axName)
		{
			string axisKeyName = InputManager.GetAxisKeyName(axName);
			if (axisKeyName == "None")
			{
				axisKeyName = InputManager.GetAxisKeyName(axName, getPositive: true, getNegative: true, getAlt: true);
			}
			return axisKeyName;
		}

		public void UpdateDockingPorts()
		{
			RefreshDockingPorts();
			MakeMyDockingPorts();
			TargetDockingPort = null;
			changeTargetedModulePort = true;
			SwitchPort();
			ReloadRadarElements();
			UpdateTargetedModule();
		}

		private void OnEnable()
		{
			Client.Instance.CanvasManager.CanvasUI.HelmetHud.gameObject.Activate(value: false);
		}

		private void OnDisable()
		{
			Client.Instance.CanvasManager.CanvasUI.HelmetHud.gameObject.Activate(value: true);
		}
	}
}
