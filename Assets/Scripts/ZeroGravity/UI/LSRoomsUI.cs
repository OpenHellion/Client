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
		[NonSerialized] public LifeSupportPanel Panel;

		[NonSerialized] public SpaceObjectVessel Vessel;

		[NonSerialized] public SceneTriggerRoom Room;

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

		private void Start()
		{
			Name.text = Room.RoomName;
			Volume.text = FormatHelper.FormatValue(Room.ConnectedRooms.Sum((SceneTriggerRoom m) => m.Volume));
			gameObject.SetActive(Vessel.IsPlayerAuthorizedOrNoSecurity(MyPlayer.Instance));
		}

		public void RefreshUI()
		{
			Pressure.text = FormatHelper.FormatValue(Room.AirPressure);
			AirQuality.text = FormatHelper.Percentage(Room.AirQuality);
			Breach.Activate(Room.ConnectedRooms.FirstOrDefault((SceneTriggerRoom m) => m.Breach) != null);
			Fire.Activate(Room.ConnectedRooms.FirstOrDefault((SceneTriggerRoom m) => m.Fire) != null);
			Gravity.Activate(Room.ConnectedRooms.FirstOrDefault((SceneTriggerRoom m) => m.GravityMalfunction) != null);
			DoorOpenWarning.Activate(Room.ConnectedRooms.Any((SceneTriggerRoom m) =>
				m.Doors.Any((SceneDoor x) => (x.Room1 == null || x.Room2 == null) && x.IsOpen)));
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
			StopActions.SetActive(value: true);
		}

		public void Depressurize()
		{
			Room.ChangeAirPressure(0f);
			StopActions.SetActive(value: true);
		}

		public void Vent()
		{
			Room.ChangeAirPressure(-1f);
			StopActions.SetActive(value: true);
		}

		public void StopAllActions()
		{
			Room.ChangeAirPressure(null);
			StopActions.SetActive(value: false);
		}

		public void ButtonStatus()
		{
			PressurizeButton.interactable = Room.AirPressure < 1f;
			VentButton.interactable = Room.AirPressure > 0f;
			DepressurizeButton.interactable = Panel.CurrentAirTank < Panel.MaxAirTank && Room.AirPressure > 0f;
		}
	}
}
