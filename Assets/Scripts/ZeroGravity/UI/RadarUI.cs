using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using ZeroGravity.Math;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class RadarUI : MonoBehaviour
	{
		[CompilerGenerated]
		private sealed class _003CUpdateRadarList_003Ec__AnonStorey0
		{
			internal Vector3D myPos;

			internal List<ArtificialBody> list;

			internal RadarUI _0024this;

			internal bool _003C_003Em__0(TargetObject m)
			{
				return m.AB == null || !(m.AB is SpaceObjectVessel) || !(m.AB as SpaceObjectVessel).IsMainVessel || m.Distance > (float)_0024this.currentRadarRange;
			}

			internal bool _003C_003Em__1(ArtificialBody m)
			{
				return m is SpaceObjectVessel && (m as SpaceObjectVessel).VesselData != null && (m as SpaceObjectVessel).IsMainVessel && !(m as SpaceObjectVessel).IsWarpOnline && (myPos - m.Position).Magnitude <= (double)_0024this.currentRadarRange && !list.Contains(m);
			}
		}

		[HideInInspector]
		public bool CanRadarWork;

		[HideInInspector]
		public bool IsActive = true;

		public GameObject Root;

		public List<TargetObject> AllTargets = new List<TargetObject>();

		public TargetObject SelectedTarget;

		public Transform OverlayTargets;

		public TargetOverlayUI TargetOnScreen;

		public TargetObject HoveredTarget;

		public GameObject ParametersHolder;

		public Text TargetName;

		public Text TargetDistance;

		public Text TargetLateral;

		public Text TargetDirectional;

		public GameObject RadarCroshair;

		[CompilerGenerated]
		private static Func<TargetObject, ArtificialBody> _003C_003Ef__am_0024cache0;

		[CompilerGenerated]
		private static Func<TargetObject, float> _003C_003Ef__am_0024cache1;

		[CompilerGenerated]
		private static Func<TargetObject, bool> _003C_003Ef__am_0024cache2;

		private int currentRadarRange
		{
			get
			{
				return (int)(10000f * ((!(MyPlayer.Instance.CurrentHelmet != null)) ? 1f : MyPlayer.Instance.CurrentHelmet.TierMultiplier));
			}
		}

		private void Start()
		{
			IsActive = true;
		}

		private void Update()
		{
			if (!CanRadarWork && base.gameObject.activeInHierarchy)
			{
				SetTarget();
				base.gameObject.SetActive(false);
				ToggleTargeting(false);
				return;
			}
			if (CanRadarWork && !base.gameObject.activeInHierarchy)
			{
				ToggleTargeting(true);
				base.gameObject.SetActive(true);
			}
			UpdateRadarList();
			if (AllTargets.Count <= 0)
			{
				SetTarget();
			}
			else if (AllTargets.Count > 0 && SelectedTarget == null)
			{
				SetTarget(AllTargets[0]);
			}
			if (AllTargets.Count > 0)
			{
				UpdateOverlayTargets();
				CalculateScaleAndRotation();
			}
			if (HoveredTarget != null && InputController.GetKey(KeyCode.Mouse0))
			{
				int index = AllTargets.IndexOf(HoveredTarget);
				SetTarget(AllTargets[index]);
			}
			if (Mouse.current.scroll.y.ReadValue().IsNotEpsilonZero() && AllTargets.Count > 0)
			{
				float axis = Mouse.current.scroll.y.ReadValue();
				int num = AllTargets.IndexOf(SelectedTarget);
				if (axis > 0f)
				{
					num = ((num - 1 < 0) ? (AllTargets.Count - 1) : (num - 1));
				}
				else if (axis < 0f)
				{
					num = ((AllTargets.Count - 1 > num) ? (num + 1) : 0);
				}
				SetTarget(AllTargets[num]);
			}
			RadarCroshair.Activate(!Client.Instance.CanvasManager.CanvasUI.IsCroshairActive);
		}

		public void UpdateRadarList()
		{
			_003CUpdateRadarList_003Ec__AnonStorey0 _003CUpdateRadarList_003Ec__AnonStorey = new _003CUpdateRadarList_003Ec__AnonStorey0();
			_003CUpdateRadarList_003Ec__AnonStorey._0024this = this;
			_003CUpdateRadarList_003Ec__AnonStorey.myPos = MyPlayer.Instance.Parent.Position + MyPlayer.Instance.transform.position.ToVector3D();
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
			}
			if (!AllTargets.Contains(SelectedTarget))
			{
				SetTarget(AllTargets.FirstOrDefault());
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
				if (targetOverlayUI.Name.text != targetOverlayUI.Target.Name)
				{
					targetOverlayUI.Name.text = targetOverlayUI.Target.Name;
				}
				Vector3 pos = ((!targetOverlayUI.Target.AB.IsDummyObject) ? targetOverlayUI.Target.AB.transform.position : ((targetOverlayUI.Target.AB.Position - MyPlayer.Instance.Parent.Position).ToVector3() - MyPlayer.Instance.transform.position));
				Vector2 localPoint = Vector2.zero;
				Vector3 arrowUp;
				Vector3 positionOnOverlay = GetPositionOnOverlay(pos, out arrowUp);
				RectTransformUtility.ScreenPointToLocalPointInRectangle(OverlayTargets.GetComponent<RectTransform>(), positionOnOverlay, Client.Instance.CanvasManager.Canvas.worldCamera, out localPoint);
				targetOverlayUI.transform.localPosition = localPoint;
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
					targetOverlayUI.Distance.gameObject.Activate(true);
					targetOverlayUI.OffScreenTarget.gameObject.Activate(false);
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
				targetOverlayUI.Distance.text = FormatHelper.DistanceFormat(((targetOverlayUI.Target.AB.Position - MyPlayer.Instance.Parent.Position).ToVector3() - MyPlayer.Instance.transform.position).magnitude);
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

		private void CalculateScaleAndRotation()
		{
			if (SelectedTarget != null)
			{
				if (!ParametersHolder.activeInHierarchy)
				{
					ParametersHolder.SetActive(true);
				}
				Vector3 vector = (SelectedTarget.AB.Position - MyPlayer.Instance.Parent.Position).ToVector3() - MyPlayer.Instance.transform.position;
				Vector3 vector2 = (SelectedTarget.AB.Velocity - MyPlayer.Instance.Parent.Velocity).ToVector3() - MyPlayer.Instance.rigidBody.velocity;
				Vector3 vector3 = Vector3.Project(vector2, vector.normalized);
				Vector3 vector4 = Vector3.ProjectOnPlane(vector2, vector.normalized);
				float magnitude = vector.magnitude;
				float num = 0f - Vector3.Dot(vector2, vector.normalized);
				TargetDirectional.color = ((!(num >= 0f)) ? Colors.FormatedRed : Colors.White);
				TargetDirectional.text = num.ToString("0.0") + " m/s";
				TargetLateral.text = vector4.magnitude.ToString("0.0") + " m/s";
				TargetDistance.color = ((!(magnitude <= 150f)) ? Colors.White : Colors.FormatedRed);
				TargetDistance.text = FormatHelper.DistanceFormat(magnitude);
			}
		}

		private void SetTarget(TargetObject target = null)
		{
			if (target != null && target.AB != null)
			{
				SelectedTarget = target;
				TargetName.text = SelectedTarget.Name;
				if (!ParametersHolder.activeInHierarchy)
				{
					ParametersHolder.SetActive(true);
				}
				if (IsActive)
				{
					ToggleStarDust(true);
				}
			}
			else
			{
				ParametersHolder.Activate(false);
				ToggleStarDust(false);
			}
		}

		private void ToggleStarDust(bool value)
		{
			if (value && SelectedTarget != null)
			{
				if (AllTargets.Count > 0)
				{
					ParticleSystem.MainModule main = MyPlayer.Instance.FpsController.StarDustParticle.main;
					main.customSimulationSpace = SelectedTarget.AB.transform;
					MyPlayer.Instance.FpsController.StarDustParticle.gameObject.SetActive(true);
					MyPlayer.Instance.FpsController.StarDustParticle.Play();
				}
			}
			else
			{
				MyPlayer.Instance.FpsController.StarDustParticle.gameObject.SetActive(false);
				MyPlayer.Instance.FpsController.StarDustParticle.Stop();
			}
		}

		public void ToggleTargeting(bool val)
		{
			IsActive = val;
			Root.SetActive(IsActive);
			ToggleStarDust(IsActive);
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
