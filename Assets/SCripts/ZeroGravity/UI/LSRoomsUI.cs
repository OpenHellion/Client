using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class LSRoomsUI : MonoBehaviour
	{
		[NonSerialized]
		public LifeSupportPanel Panel;

		[NonSerialized]
		public SpaceObjectVessel Vessel;

		[NonSerialized]
		public SceneTriggerRoom Room;

		public Text Name;

		public Text Volume;

		public Image PressureBck;

		public Text Pressure;

		public Image AirQualityBck;

		public Text AirQuality;

		public Image GravityBck;

		public Text GravityValue;

		public GameObject Breach;

		public GameObject Fire;

		public GameObject Gravity;

		public Button PressurizeButton;

		public Button DepressurizeButton;

		public Button VentButton;

		public GameObject StopActions;

		public GameObject DoorOpenWarning;

		[CompilerGenerated]
		private static Func<SceneTriggerRoom, float> _003C_003Ef__am_0024cache0;

		[CompilerGenerated]
		private static Func<SceneTriggerRoom, bool> _003C_003Ef__am_0024cache1;

		[CompilerGenerated]
		private static Func<SceneTriggerRoom, bool> _003C_003Ef__am_0024cache2;

		[CompilerGenerated]
		private static Func<SceneTriggerRoom, bool> _003C_003Ef__am_0024cache3;

		[CompilerGenerated]
		private static Func<SceneTriggerRoom, bool> _003C_003Ef__am_0024cache4;

		[CompilerGenerated]
		private static Func<SceneDoor, bool> _003C_003Ef__am_0024cache5;

		private void Start()
		{
			Name.text = Room.RoomName;
			Text volume = Volume;
			List<SceneTriggerRoom> connectedRooms = Room.ConnectedRooms;
			if (_003C_003Ef__am_0024cache0 == null)
			{
				_003C_003Ef__am_0024cache0 = _003CStart_003Em__0;
			}
			volume.text = FormatHelper.FormatValue(connectedRooms.Sum(_003C_003Ef__am_0024cache0));
			base.gameObject.SetActive(Vessel.IsPlayerAuthorizedOrNoSecurity(MyPlayer.Instance));
		}

		public void RefreshUI()
		{
			Pressure.text = FormatHelper.FormatValue(Room.AirPressure);
			AirQuality.text = FormatHelper.Percentage(Room.AirQuality);
			GameObject breach = Breach;
			List<SceneTriggerRoom> connectedRooms = Room.ConnectedRooms;
			if (_003C_003Ef__am_0024cache1 == null)
			{
				_003C_003Ef__am_0024cache1 = _003CRefreshUI_003Em__1;
			}
			breach.Activate(connectedRooms.FirstOrDefault(_003C_003Ef__am_0024cache1) != null);
			GameObject fire = Fire;
			List<SceneTriggerRoom> connectedRooms2 = Room.ConnectedRooms;
			if (_003C_003Ef__am_0024cache2 == null)
			{
				_003C_003Ef__am_0024cache2 = _003CRefreshUI_003Em__2;
			}
			fire.Activate(connectedRooms2.FirstOrDefault(_003C_003Ef__am_0024cache2) != null);
			GameObject gravity = Gravity;
			List<SceneTriggerRoom> connectedRooms3 = Room.ConnectedRooms;
			if (_003C_003Ef__am_0024cache3 == null)
			{
				_003C_003Ef__am_0024cache3 = _003CRefreshUI_003Em__3;
			}
			gravity.Activate(connectedRooms3.FirstOrDefault(_003C_003Ef__am_0024cache3) != null);
			GameObject doorOpenWarning = DoorOpenWarning;
			List<SceneTriggerRoom> connectedRooms4 = Room.ConnectedRooms;
			if (_003C_003Ef__am_0024cache4 == null)
			{
				_003C_003Ef__am_0024cache4 = _003CRefreshUI_003Em__4;
			}
			doorOpenWarning.Activate(connectedRooms4.Any(_003C_003Ef__am_0024cache4));
			if (!Room.UseGravity)
			{
				GravityValue.text = "OFF";
				GravityBck.color = Colors.Red;
			}
			else
			{
				GravityValue.text = "ON";
				GravityBck.color = Colors.Green;
			}
			if (Room.AirPressure < 0.4f)
			{
				PressureBck.color = Colors.Red;
			}
			else
			{
				PressureBck.color = Colors.Green;
			}
			if (Room.AirQuality < 0.4f)
			{
				AirQualityBck.color = Colors.Red;
			}
			else
			{
				AirQualityBck.color = Colors.Green;
			}
			AnimationHelperUI.Rotate(PressureBck, Room.AirPressureChangeRate);
			AnimationHelperUI.Rotate(AirQualityBck, Room.AirQualityChangeRate);
			StopActions.SetActive(Room.PressurizationStatus != RoomPressurizationStatus.None);
			ButtonStatus();
		}

		public void Pressurize()
		{
			Room.ChangeAirPressure(1f);
			StopActions.SetActive(true);
		}

		public void Depressurize()
		{
			Room.ChangeAirPressure(0f);
			StopActions.SetActive(true);
		}

		public void Vent()
		{
			Room.ChangeAirPressure(-1f);
			StopActions.SetActive(true);
		}

		public void StopAllActions()
		{
			Room.ChangeAirPressure(null);
			StopActions.SetActive(false);
		}

		public void ButtonStatus()
		{
			PressurizeButton.interactable = Room.AirPressure < 1f;
			VentButton.interactable = Room.AirPressure > 0f;
			DepressurizeButton.interactable = Panel.CurrentAirTank < Panel.MaxAirTank && Room.AirPressure > 0f;
		}

		[CompilerGenerated]
		private static float _003CStart_003Em__0(SceneTriggerRoom m)
		{
			return m.Volume;
		}

		[CompilerGenerated]
		private static bool _003CRefreshUI_003Em__1(SceneTriggerRoom m)
		{
			return m.Breach;
		}

		[CompilerGenerated]
		private static bool _003CRefreshUI_003Em__2(SceneTriggerRoom m)
		{
			return m.Fire;
		}

		[CompilerGenerated]
		private static bool _003CRefreshUI_003Em__3(SceneTriggerRoom m)
		{
			return m.GravityMalfunction;
		}

		[CompilerGenerated]
		private static bool _003CRefreshUI_003Em__4(SceneTriggerRoom m)
		{
			List<SceneDoor> doors = m.Doors;
			if (_003C_003Ef__am_0024cache5 == null)
			{
				_003C_003Ef__am_0024cache5 = _003CRefreshUI_003Em__5;
			}
			return doors.Any(_003C_003Ef__am_0024cache5);
		}

		[CompilerGenerated]
		private static bool _003CRefreshUI_003Em__5(SceneDoor x)
		{
			return (x.Room1 == null || x.Room2 == null) && x.IsOpen;
		}
	}
}
