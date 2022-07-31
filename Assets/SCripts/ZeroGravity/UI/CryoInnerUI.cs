using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.LevelDesign;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class CryoInnerUI : MonoBehaviour
	{
		public SceneSpawnPoint SpawnPoint;

		public SceneTriggerRoom Room;

		public Text Pressure;

		public Text AirQuality;

		public GameObject Breach;

		public GameObject Fire;

		private void Start()
		{
		}

		private void Update()
		{
			if (MyPlayer.Instance.CurrentRoomTrigger == Room)
			{
				UpdateUI();
			}
		}

		public void UpdateUI()
		{
			if (!(Room == null))
			{
				AirQuality.text = (Room.AirQuality * 100f).ToString("f0");
				if (Room.AirQuality <= 0.4f || !Room.IsAirOk)
				{
					AirQuality.color = Colors.Red;
				}
				else
				{
					AirQuality.color = Colors.White;
				}
				Pressure.text = Room.AirPressure.ToString("0.0");
				if (Room.AirPressure <= 0.4f)
				{
					Pressure.color = Colors.Red;
				}
				else
				{
					Pressure.color = Colors.White;
				}
				Breach.SetActive(Room.Breach);
				Fire.SetActive(Room.Fire);
			}
		}
	}
}
