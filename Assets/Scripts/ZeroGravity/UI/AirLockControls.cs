using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using ZeroGravity.LevelDesign;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class AirLockControls : MonoBehaviour
	{
		public SceneTriggerAirlockPanel MySceneTrigger;

		[Space(10f)] public SceneTriggerRoom Room;

		public SceneDoor InnerDoor;

		public SceneDoor OuterDoor;

		[FormerlySerializedAs("InnerDoorExecuter")] [Space(10f)]
		public SceneTriggerExecutor InnerDoorExecutor;

		public string InnerDoorOpenState;

		public string InnerDoorClosedState;

		public string InnerDoorForceOpen;

		[FormerlySerializedAs("OuterDoorExecuter")] [Space(10f)]
		public SceneTriggerExecutor OuterDoorExecutor;

		public string OuterDoorOpenState;

		public string OuterDoorClosedState;

		public string OuterDoorForceOpen;

		private float? TargetPressure;

		public Text ScreenCanvasPressure;

		public Image ScreenCanvasStatus;

		[SerializeField] private Sprite SafeSprite;

		[SerializeField] private Sprite UnsafeSprite;

		[SerializeField] private Sprite CyclingSprite;

		public GameObject CyclingScreen;

		public float lastCommandTime;

		public SceneTriggerRoom InnerRoom
		{
			get { return (!(InnerDoor.Room1 != Room)) ? InnerDoor.Room2 : InnerDoor.Room1; }
		}

		public SceneTriggerRoom OuterRoom
		{
			get { return (!(OuterDoor.Room1 != Room)) ? OuterDoor.Room2 : OuterDoor.Room1; }
		}

		public float pressurizeTarget
		{
			get
			{
				float num = ((!(InnerRoom == null)) ? InnerRoom.AirPressure : 0f);
				float num2 = ((!(OuterRoom == null)) ? OuterRoom.AirPressure : 0f);
				return (!(num > num2)) ? num2 : num;
			}
		}

		public float depressurizeTarget
		{
			get
			{
				float num = ((!(InnerRoom == null)) ? InnerRoom.AirPressure : 0f);
				float num2 = ((!(OuterRoom == null)) ? OuterRoom.AirPressure : 0f);
				return (!(num < num2)) ? num2 : num;
			}
		}

		private void Start()
		{
			if (Room != null)
			{
				Room.AddBehaviourScript(this);
			}

			if (InnerRoom != null)
			{
				InnerRoom.AddBehaviourScript(this);
			}

			if (OuterRoom != null)
			{
				OuterRoom.AddBehaviourScript(this);
			}

			if (Room != null)
			{
				UpdateUI();
			}
		}

		public void UpdateUI()
		{
			if (Room is not null)
			{
				RefreshScreenSaver();
			}
		}

		public void RefreshScreenSaver()
		{
			ScreenCanvasPressure.text = Room.AirPressure.ToString("f1");
			if (Room.PressurizationStatus != 0)
			{
				ScreenCanvasStatus.sprite = CyclingSprite;
				CyclingScreen.SetActive(true);
			}
			else if ((double)Mathf.Abs(((!(MyPlayer.Instance.CurrentRoomTrigger != null))
				         ? 0f
				         : MyPlayer.Instance.CurrentRoomTrigger.AirPressure) - Room.AirPressure) < 0.1)
			{
				ScreenCanvasStatus.sprite = SafeSprite;
				CyclingScreen.SetActive(false);
			}
			else
			{
				ScreenCanvasStatus.sprite = UnsafeSprite;
				CyclingScreen.SetActive(false);
			}
		}
	}
}
