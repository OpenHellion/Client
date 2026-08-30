using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Math;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.UI
{
	public class AirlockUI : MonoBehaviour
	{
		[Title("AIRLOCK UI")] public AirLockControls MyAirlock;

		public GameObject MainScreen;

		public Text PressureValue;

		public Image AirlockPressureFiller;

		public GameObject NoTank;

		public Image AirTankFiller;

		public Text AirTankValue;

		private float MaxAirTank;

		private float CurrentAirTank;

		public GameObject AirTankDanger;

		public Text TankStatus;

		public Button InnerDoorButton;

		public Text InnerDoorText;

		public Button OuterDoorButton;

		public Text OuterDoorText;

		public Button PressurizeButton;

		public Button DepressurizeButton;

		[SerializeField] private Sprite OpenDoorSprite;

		[SerializeField] private Sprite CloseDoorSprite;

		public GameObject StopAction;

		public GameObject AlertBox;

		public GameObject PressureControls;

		public GameObject Unauthorized;

		private SpaceObjectVessel ParentVessel;

		private List<SpaceObjectVessel> AllVessels = new List<SpaceObjectVessel>();

		private List<ResourceContainer> AirTanks = new List<ResourceContainer>();

		private int forceOpen;

		private float currentPressure;

		private void Start()
		{
		}

		private void Update()
		{
			NoTank.SetActive(AirTanks.Count == 0);
			UpdateAirTank();
			if (MyAirlock.Room.AirPressureChangeRate.IsNotEpsilonZero())
			{
				if (currentPressure > MyAirlock.Room.AirPressure)
				{
					currentPressure += MyAirlock.Room.AirPressureChangeRate * Time.deltaTime;
				}
				else
				{
					currentPressure += MyAirlock.Room.AirPressureChangeRate * Time.deltaTime;
				}
			}
			else
			{
				currentPressure = MyAirlock.Room.AirPressure;
			}

			currentPressure = MathHelper.Clamp(currentPressure, 0f, 1f);
			PressureValue.text = currentPressure.ToString("F1");
			AirlockPressureFiller.fillAmount = currentPressure;
			PressurizeButton.interactable = MyAirlock.Room.AirPressureChangeRate == 0f &&
			                                MyAirlock.Room.AirPressure < MyAirlock.pressurizeTarget &&
			                                !MyAirlock.InnerDoor.IsOpen && !MyAirlock.OuterDoor.IsOpen &&
			                                CurrentAirTank >= MyAirlock.pressurizeTarget;
			DepressurizeButton.interactable = MyAirlock.Room.AirPressureChangeRate == 0f &&
			                                  MyAirlock.Room.AirPressure > MyAirlock.depressurizeTarget &&
			                                  !MyAirlock.InnerDoor.IsOpen && !MyAirlock.OuterDoor.IsOpen &&
			                                  CurrentAirTank != MaxAirTank;
			StopAction.SetActive(MyAirlock.Room.PressurizationStatus != RoomPressurizationStatus.None);
			if (!MyAirlock.InnerDoor.IsOpen)
			{
				InnerDoorButton.GetComponent<Image>().color = Colors.White;
				InnerDoorButton.GetComponent<Image>().sprite = OpenDoorSprite;
				InnerDoorText.text = Localization.OpenDoor.ToUpper();
			}
			else
			{
				InnerDoorButton.GetComponent<Image>().color =
					new Color(1f, 1f, 1f, System.Math.Abs(Time.time % 2f - 1f));
				InnerDoorButton.GetComponent<Image>().sprite = CloseDoorSprite;
				InnerDoorText.text = Localization.CloseDoor.ToUpper();
			}

			if (!MyAirlock.OuterDoor.IsOpen)
			{
				OuterDoorButton.GetComponent<Image>().color = Colors.White;
				OuterDoorButton.GetComponent<Image>().sprite = OpenDoorSprite;
				OuterDoorText.text = Localization.OpenDoor.ToUpper();
			}
			else
			{
				OuterDoorButton.GetComponent<Image>().color =
					new Color(1f, 1f, 1f, System.Math.Abs(Time.time % 2f - 1f));
				OuterDoorButton.GetComponent<Image>().sprite = CloseDoorSprite;
				OuterDoorText.text = Localization.CloseDoor.ToUpper();
			}
		}

		public void Depresurize()
		{
			if (MyAirlock.InnerDoor.IsOpen)
			{
				InnerDoorControl();
			}

			if (MyAirlock.OuterDoor.IsOpen)
			{
				OutterDoorControl();
			}

			MyAirlock.Room.ChangeAirPressure(MyAirlock.depressurizeTarget);
			MyAirlock.lastCommandTime = Time.time;
		}

		public void Presurize()
		{
			if (MyAirlock.InnerDoor.IsOpen)
			{
				InnerDoorControl();
			}

			if (MyAirlock.OuterDoor.IsOpen)
			{
				OutterDoorControl();
			}

			MyAirlock.Room.ChangeAirPressure(MyAirlock.pressurizeTarget);
			MyAirlock.lastCommandTime = Time.time;
		}

		public void InnerDoorControl()
		{
			if (!MyAirlock.InnerDoor.IsOpen)
			{
				if (!MyAirlock.InnerDoor.IsSafeToOpen())
				{
					AlertBox.SetActive(value: true);
					MainScreen.SetActive(value: false);
					forceOpen = 1;
				}
				else
				{
					MyAirlock.InnerDoorExecutor.ChangeState(MyAirlock.InnerDoorOpenState);
				}
			}
			else
			{
				MyAirlock.InnerDoorExecutor.ChangeState(MyAirlock.InnerDoorClosedState);
			}
		}

		public void OutterDoorControl()
		{
			if (!MyAirlock.OuterDoor.IsOpen)
			{
				if (!MyAirlock.OuterDoor.IsSafeToOpen())
				{
					AlertBox.SetActive(value: true);
					MainScreen.SetActive(value: false);
					forceOpen = 2;
				}
				else
				{
					MyAirlock.OuterDoorExecutor.ChangeState(MyAirlock.OuterDoorOpenState);
				}
			}
			else
			{
				MyAirlock.OuterDoorExecutor.ChangeState(MyAirlock.OuterDoorClosedState);
			}
		}

		public void WarningDoorOpen()
		{
			if (forceOpen == 1)
			{
				MyAirlock.InnerDoorExecutor.ChangeState(MyAirlock.InnerDoorForceOpen);
			}
			else if (forceOpen == 2)
			{
				MyAirlock.OuterDoorExecutor.ChangeState(MyAirlock.OuterDoorForceOpen);
			}

			AlertBox.SetActive(value: false);
			MainScreen.SetActive(value: true);
			forceOpen = 0;
		}

		public void CancelWarningDoor()
		{
			AlertBox.SetActive(value: false);
			MainScreen.SetActive(value: true);
		}

		public void StopActions()
		{
			MyAirlock.Room.ChangeAirPressure(null);
		}

		public void UpdateAirTank()
		{
			if (AirTanks.Count == 0)
			{
				return;
			}

			float num = 0f;
			float num2 = 0f;
			foreach (ResourceContainer airTank in AirTanks)
			{
				num += airTank.Capacity;
				num2 += airTank.Quantity;
			}

			CurrentAirTank = num2;
			MaxAirTank = num;
			AirTankFiller.fillAmount = CurrentAirTank / MaxAirTank;
			AirTankValue.text = FormatHelper.CurrentMax(CurrentAirTank, MaxAirTank);
			AirTankDanger.SetActive(CurrentAirTank == 0f || CurrentAirTank == MaxAirTank);
			if (CurrentAirTank == 0f)
			{
				AirTankValue.color = Colors.Red;
				TankStatus.text = Localization.AirTank.ToUpper() + " " + Localization.Empty.ToUpper() +
				                  " - <color=#AE1515>" + Localization.UnableToPressurize.ToUpper() + "</color>";
			}
			else if (CurrentAirTank == MaxAirTank)
			{
				AirTankValue.color = Colors.Green;
				TankStatus.text = Localization.AirTank.ToUpper() + " " + Localization.Full.ToUpper() +
				                  " - <color=#AE1515>" + Localization.UnableToDepressurize.ToUpper() + "</color>";
			}
			else
			{
				AirTankValue.color = Colors.White;
				TankStatus.text = Localization.AirTank.ToUpper();
			}
		}

		public void OnInteract()
		{
			GetVesselAndAirTanks();
			currentPressure = MyAirlock.Room.AirPressure;
			PressureValue.text = FormatHelper.FormatValue(currentPressure);
			AirlockPressureFiller.fillAmount = currentPressure;
			AlertBox.SetActive(value: false);
			MainScreen.SetActive(value: true);
			Unauthorized.Activate(!MyAirlock.MySceneTrigger.CheckAuthorization());
			gameObject.SetActive(value: true);
		}

		public void OnDetach()
		{
			gameObject.SetActive(value: false);
		}

		public void GetVesselAndAirTanks()
		{
			AllVessels.Clear();
			AirTanks.Clear();
			ParentVessel = MyAirlock.Room.ParentVessel;
			AllVessels.Add(ParentVessel.MainVessel);
			foreach (SpaceObjectVessel allDockedVessel in ParentVessel.MainVessel.AllDockedVessels)
			{
				AllVessels.Add(allDockedVessel);
			}

			foreach (SpaceObjectVessel allVessel in AllVessels)
			{
				foreach (ResourceContainer item in from m in allVessel.GeometryRoot
					         .GetComponentsInChildren<ResourceContainer>()
				         where m.DistributionSystemType == DistributionSystemType.Air
				         select m)
				{
					AirTanks.Add(item);
				}
			}
		}

		public void ExitButton()
		{
			MyPlayer.Instance.LockedToTrigger.CancelInteract(MyPlayer.Instance);
		}
	}
}
