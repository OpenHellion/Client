using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.LevelDesign;

namespace ZeroGravity.UI
{
	public class AirLockMonitor : MonoBehaviour
	{
		public SceneTriggerRoom Room;

		public SceneDoor InnerDoor;

		public SceneDoor OutterDoor;

		public SceneTriggerExecutor PresurizationControl;

		public Image AirQualityFiller;

		public Text AirQualityText;

		public Image PreassureFiller;

		public Text PreasureText;

		public Image DepressuriseImage;

		public Image PressuriseImage;

		public Sprite DepressuriseActive;

		public Sprite DepressuriseInactive;

		public Sprite PressuriseActive;

		public Sprite PressuriseInactive;

		public Image InnerDoorImage;

		public Image OuterDoorImage;

		public Sprite InnerDoorOpened;

		public Sprite InnerDoorClosed;

		public Sprite OuterDoorOpened;

		public Sprite OuterDoorClosed;

		public Image DoorImage;

		public Image LeakImage;

		public Image PowerImage;

		public Sprite DoorInactive;

		public Sprite DoorActive;

		public Sprite PowerInactive;

		public Sprite PowerActive;

		public Sprite LeakInactive;

		public Sprite LeakActive;

		private int depresurizeLerp = -1;

		private int presurizeLerp = -1;

		private int doorAlarm = -1;

		private float korak = 0.08f;

		private float korak2 = 0.12f;

		private float timer;

		private void Start()
		{
		}

		private void Update()
		{
			if (Room == null)
			{
				return;
			}

			timer += Time.deltaTime;
			if (timer < 2f)
			{
				return;
			}

			timer = 0f;
			AirQualityFiller.fillAmount = Room.AirQuality;
			AirQualityText.text = (Room.AirQuality * 100f).ToString("f0");
			PreassureFiller.fillAmount = Room.AirPressure;
			PreasureText.text = Room.AirPressure.ToString("f1");
			if (PresurizationControl != null && PresurizationControl.CurrentState == "Depressurize")
			{
				if (DepressuriseImage.sprite != DepressuriseActive)
				{
					DepressuriseImage.sprite = DepressuriseActive;
				}

				Color color = DepressuriseImage.color;
				if (depresurizeLerp > 0)
				{
					color.a = DepressuriseImage.color.a + depresurizeLerp * korak;
					if (color.a > 1f)
					{
						depresurizeLerp = -1;
					}
				}
				else
				{
					color.a = DepressuriseImage.color.a + depresurizeLerp * korak;
					if (color.a < 0f)
					{
						depresurizeLerp = 1;
					}
				}

				DepressuriseImage.color = color;
			}
			else if (DepressuriseImage.sprite != DepressuriseInactive)
			{
				DepressuriseImage.sprite = DepressuriseInactive;
				Color color2 = DepressuriseImage.color;
				color2.a = 1f;
				DepressuriseImage.color = color2;
			}

			if (PresurizationControl != null && PresurizationControl.CurrentState == "Pressurise")
			{
				if (PressuriseImage.sprite != PressuriseActive)
				{
					PressuriseImage.sprite = PressuriseActive;
				}

				Color color3 = PressuriseImage.color;
				if (presurizeLerp > 0)
				{
					color3.a = PressuriseImage.color.a + presurizeLerp * korak;
					if (color3.a > 1f)
					{
						presurizeLerp = -1;
					}
				}
				else
				{
					color3.a = PressuriseImage.color.a + presurizeLerp * korak;
					if (color3.a < 0f)
					{
						presurizeLerp = 1;
					}
				}

				PressuriseImage.color = color3;
			}
			else if (PressuriseImage.sprite != PressuriseInactive)
			{
				PressuriseImage.sprite = PressuriseInactive;
				Color color4 = PressuriseImage.color;
				color4.a = 1f;
				PressuriseImage.color = color4;
			}

			if (InnerDoor != null && InnerDoor.IsOpen)
			{
				if (InnerDoorImage.sprite != InnerDoorOpened)
				{
					InnerDoorImage.sprite = InnerDoorOpened;
				}
			}
			else if (InnerDoorImage.sprite != InnerDoorClosed)
			{
				InnerDoorImage.sprite = InnerDoorClosed;
			}

			if (OutterDoor != null && OutterDoor.IsOpen)
			{
				if (OuterDoorImage.sprite != OuterDoorOpened)
				{
					OuterDoorImage.sprite = OuterDoorOpened;
				}
			}
			else if (OuterDoorImage.sprite != OuterDoorClosed)
			{
				OuterDoorImage.sprite = OuterDoorClosed;
			}

			if (InnerDoor != null && OutterDoor != null && OutterDoor.IsOpen && InnerDoor.IsOpen)
			{
				if (DoorImage.sprite != DoorActive)
				{
					DoorImage.sprite = DoorActive;
				}

				Color color5 = DoorImage.color;
				if (doorAlarm > 0)
				{
					color5.a = DoorImage.color.a + doorAlarm * korak2;
					if (color5.a > 1f)
					{
						doorAlarm = -1;
					}
				}
				else
				{
					color5.a = DoorImage.color.a + doorAlarm * korak2;
					if (color5.a < 0f)
					{
						doorAlarm = 1;
					}
				}

				DoorImage.color = color5;
			}
			else if (DoorImage.sprite != DoorInactive)
			{
				DoorImage.sprite = DoorInactive;
				Color color6 = DoorImage.color;
				color6.a = 1f;
				DoorImage.color = color6;
			}
		}
	}
}
