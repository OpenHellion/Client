using UnityEngine;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	public class LifeSupportGroupItemUI : MonoBehaviour
	{
		public Text RoomName;

		public Text AirFilterStatus;

		public Text AirScrubberStatus;

		public Text PressureValue;

		public Text AirValue;

		public GameObject RoomIsAirlock;

		public GameObject RegularRoom;

		public Button TurnOn;

		public Button TurnOff;

		public GameObject FireHazard;

		public GameObject GravityHazard;

		public GameObject BreachHazard;

		public GameObject TemperatureHazard;
	}
}
