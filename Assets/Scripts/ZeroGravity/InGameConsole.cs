using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenHellion;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.UI;
using OpenHellion.Net;
using OpenHellion.UI;
using UnityEngine.InputSystem;

namespace ZeroGravity
{
	public class InGameConsole : MonoBehaviour
	{
		private int CurrentScreen;

		public List<GameObject> Screens;

		public List<GameObject> Options;

		public Transform ItemsHolder;

		public ScrollRect Scroll;

		public InputField Input;

		public GameObject TextElement;

		private int maxElements = 20;

		public List<Tuple<GameObject, bool>> Elements = new List<Tuple<GameObject, bool>>();

		public Toggle GodMode;

		private Tuple<GameObject, bool> lastSelectedStackItem;

		public GameObject SpawnOptionUI;

		public GameObject NetworkingButton;

		public Text NetworkingReceived;

		public Text NetworkingSent;

		private bool NetworkingActive;

		[SerializeField] private InGameGUI _inGameGUI;

		private void Start()
		{
			CreateItemSpawnOptions();
			NetworkingButton.Activate(true);
		}

		private void Update()
		{
			if (CurrentScreen == 0)
			{
				if (Keyboard.current.enterKey.wasPressedThisFrame && Input.text != string.Empty)
				{
					SubmitText();
				}
				else if (Keyboard.current.upArrowKey.wasPressedThisFrame && Elements.Count > 0)
				{
					lastSelectedStackItem = Elements.FindLast((Tuple<GameObject, bool> m) =>
						m.Item2 && (Elements.IndexOf(lastSelectedStackItem) > Elements.IndexOf(m) ||
						            lastSelectedStackItem == null));
					if (lastSelectedStackItem != null)
					{
						Input.text = lastSelectedStackItem.Item1.GetComponent<Text>().text;
						Input.Select();
						Input.ActivateInputField();
					}
				}
				else if (Keyboard.current.downArrowKey.wasPressedThisFrame && Elements.Count > 0)
				{
					lastSelectedStackItem = Elements.FindLast((Tuple<GameObject, bool> m) =>
						m.Item2 && (Elements.IndexOf(lastSelectedStackItem) < Elements.IndexOf(m) ||
						            lastSelectedStackItem == null));
					if (lastSelectedStackItem != null)
					{
						Input.text = lastSelectedStackItem.Item1.GetComponent<Text>().text;
						Input.Select();
						Input.ActivateInputField();
					}
				}
			}

			if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.f2Key.wasPressedThisFrame)
			{
				Close();
			}

			if (NetworkingActive)
			{
				UpdateNewtorking();
			}
		}

		public void CreateTextElement(string text, Color? color = null, bool userEntry = false)
		{
			if (Elements.Count >= maxElements)
			{
				Destroy(Elements[0].Item1);
				Elements.RemoveAt(0);
			}

			GameObject gameObject = Instantiate(TextElement, Scroll.content);
			gameObject.transform.SetAsLastSibling();
			gameObject.GetComponent<Text>().text = text;
			if (color.HasValue)
			{
				gameObject.GetComponent<Text>().color = color.Value;
			}

			gameObject.SetActive(value: true);
			gameObject.transform.Reset();
			Scroll.normalizedPosition = new Vector2(0f, 0f);
			Canvas.ForceUpdateCanvases();
			Elements.Add(new Tuple<GameObject, bool>(gameObject, userEntry));
			if (userEntry)
			{
				lastSelectedStackItem = null;
			}
		}

		public void SubmitText()
		{
			NetworkController.Send(new ConsoleMessage
			{
				Text = Input.text
			});
			CreateTextElement(Input.text, null, userEntry: true);
			Input.text = string.Empty;
			Input.ActivateInputField();
			Input.Select();
		}

		public void Open()
		{
			_inGameGUI.IsInputFieldIsActive = true;
			SetScreen(0);
			Globals.ToggleCursor(true);
			MyPlayer.Instance.FpsController.ToggleMovement(false);
			MyPlayer.Instance.FpsController.ToggleAttached(true);
			if (!MyPlayer.Instance.FpsController.IsZeroG)
			{
				MyPlayer.Instance.FpsController.ResetVelocity();
			}

			lastSelectedStackItem = null;
			Canvas.ForceUpdateCanvases();
			base.gameObject.SetActive(value: true);
			Input.ActivateInputField();
		}

		public void Close()
		{
			_inGameGUI.IsInputFieldIsActive = false;
			gameObject.SetActive(value: false);
			if (!MyPlayer.Instance.IsLockedToTrigger)
			{
				Globals.ToggleCursor(false);
				MyPlayer.Instance.FpsController.ToggleAttached(false);
				MyPlayer.Instance.FpsController.ToggleMovement(!MyPlayer.Instance.SittingOnPilotSeat);
				if (MyPlayer.Instance.SittingOnPilotSeat)
				{
					MyPlayer.Instance.FpsController.ToggleCameraAttachToHeadBone(true);
				}

				MyPlayer.Instance.FpsController.ToggleAutoFreeLook(MyPlayer.Instance.SittingOnPilotSeat ||
				                                                   MyPlayer.Instance.InLadderTrigger ||
				                                                   (MyPlayer.Instance.InLockState &&
				                                                    !MyPlayer.Instance.IsLockedToTrigger &&
				                                                    MyPlayer.Instance.Parent is SpaceObjectVessel &&
				                                                    (MyPlayer.Instance.Parent as SpaceObjectVessel)
				                                                    .SpawnPoints.Values
				                                                    .FirstOrDefault((SceneSpawnPoint m) =>
					                                                    m.PlayerGUID == MyPlayer.Instance.Guid) !=
				                                                    null));
			}
			else if (MyPlayer.Instance.IsDrivingShip || MyPlayer.Instance.ShipControlMode == ShipControlMode.Docking)
			{
				Globals.ToggleCursor(false);
			}
		}

		public void Spawn(string itemToSpawn)
		{
			NetworkController.Send(new ConsoleMessage
			{
				Text = "spawn " + itemToSpawn
			});
		}

		public void Action(string actionToDo)
		{
			NetworkController.Send(new ConsoleMessage
			{
				Text = actionToDo
			});
		}

		public void ToggleGodmod()
		{
			string empty = string.Empty;
			empty = ((!GodMode.isOn) ? "0" : "1");
			NetworkController.Send(new ConsoleMessage
			{
				Text = "god " + empty
			});
		}

		public void SetScreen(int option)
		{
			NetworkingActive = false;
			foreach (GameObject option2 in Options)
			{
				option2.Activate(value: false);
			}

			foreach (GameObject screen in Screens)
			{
				screen.Activate(value: false);
			}

			Screens[option].Activate(value: true);
			Options[option].Activate(value: true);
			CurrentScreen = option;
			if (option == 0)
			{
				Input.ActivateInputField();
				Input.Select();
				Input.text = string.Empty;
				Scroll.normalizedPosition = new Vector2(0f, 0f);
			}

			if (option == 5)
			{
				NetworkingActive = true;
			}
		}

		private void CreateItemSpawnOptions()
		{
			List<DynamicObjectData> list = new List<DynamicObjectData>(
				StaticData.DynamicObjectsDataList.Values.Where((DynamicObjectData m) =>
					m.DefaultAuxData.Category == ItemCategory.Weapons));
			List<DynamicObjectData> list2 = new List<DynamicObjectData>(
				StaticData.DynamicObjectsDataList.Values.Where((DynamicObjectData m) =>
					m.DefaultAuxData.Category == ItemCategory.Magazines));
			List<DynamicObjectData> list3 = new List<DynamicObjectData>(
				StaticData.DynamicObjectsDataList.Values.Where((DynamicObjectData m) =>
					m.DefaultAuxData.Category == ItemCategory.Medical));
			List<DynamicObjectData> list4 = new List<DynamicObjectData>(
				StaticData.DynamicObjectsDataList.Values.Where((DynamicObjectData m) =>
					m.DefaultAuxData.Category == ItemCategory.Suits));
			List<DynamicObjectData> list5 = new List<DynamicObjectData>(
				StaticData.DynamicObjectsDataList.Values.Where((DynamicObjectData m) =>
					m.DefaultAuxData.Category == ItemCategory.Tools));
			List<DynamicObjectData> list6 = new List<DynamicObjectData>(
				StaticData.DynamicObjectsDataList.Values.Where((DynamicObjectData m) =>
					m.DefaultAuxData.Category == ItemCategory.Parts));
			List<DynamicObjectData> list7 = new List<DynamicObjectData>(
				StaticData.DynamicObjectsDataList.Values.Where((DynamicObjectData m) =>
					m.DefaultAuxData.Category == ItemCategory.Utility));
			List<DynamicObjectData> list8 = new List<DynamicObjectData>(
				StaticData.DynamicObjectsDataList.Values.Where((DynamicObjectData m) =>
					m.DefaultAuxData.Category == ItemCategory.Containers));
			List<DynamicObjectData> list9 = new List<DynamicObjectData>(
				StaticData.DynamicObjectsDataList.Values.Where((DynamicObjectData m) =>
					m.DefaultAuxData.Category == ItemCategory.General));
			InstantiateItems(list, Colors.Red);
			InstantiateItems(list2, Colors.Red);
			InstantiateItems(list3, Colors.Green);
			InstantiateItems(list4, Colors.Yellow);
			InstantiateItems(list7, Colors.PowerRed);
			InstantiateItems(list5, Colors.Blue);
			InstantiateItems(list8, Colors.Orange);
			InstantiateItems(list6, Colors.Cyan);
			InstantiateItems(list9, Colors.Black);
		}

		private void InstantiateItems(List<DynamicObjectData> list, Color col)
		{
			foreach (DynamicObjectData item in list)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(SpawnOptionUI, ItemsHolder);
				ConsoleSpawnOptionUI component = gameObject.GetComponent<ConsoleSpawnOptionUI>();
				component.Console = this;
				if (item.ItemType == ItemType.MachineryPart)
				{
					MachineryPartData machineryPartData = item.DefaultAuxData as MachineryPartData;
					component.Name.text = machineryPartData.PartType.ToLocalizedString();
					component.Icon.sprite = SpriteManager.Instance.GetSprite(machineryPartData.PartType);
				}
				else if (item.ItemType == ItemType.GenericItem)
				{
					GenericItemData genericItemData = item.DefaultAuxData as GenericItemData;
					component.Name.text = genericItemData.SubType.ToLocalizedString();
					component.Icon.sprite = SpriteManager.Instance.GetSprite(genericItemData.SubType);
				}
				else
				{
					component.Name.text = item.ItemType.ToLocalizedString();
					component.Icon.sprite = SpriteManager.Instance.GetSprite(item.ItemType);
				}

				component.SetSpawnOption(Path.GetFileName(item.PrefabPath));
				component.GetComponent<Image>().color = col;
			}
		}

		public void UpdateNewtorking()
		{
			NetworkingReceived.text = MyPlayer.Instance.ReceivedPacketStatistics;
			NetworkingSent.text = MyPlayer.Instance.SentPacketStatistics;
		}

		public void RestartNetworking()
		{
			MyPlayer.Instance.ResetStatistics();
		}
	}
}
