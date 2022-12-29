using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.CharacterMovement;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.UI;
using OpenHellion.Networking;

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

		[CompilerGenerated]
		private static Func<SceneSpawnPoint, bool> _003C_003Ef__am_0024cache0;

		[CompilerGenerated]
		private static Func<DynamicObjectData, bool> _003C_003Ef__am_0024cache1;

		[CompilerGenerated]
		private static Func<DynamicObjectData, bool> _003C_003Ef__am_0024cache2;

		[CompilerGenerated]
		private static Func<DynamicObjectData, bool> _003C_003Ef__am_0024cache3;

		[CompilerGenerated]
		private static Func<DynamicObjectData, bool> _003C_003Ef__am_0024cache4;

		[CompilerGenerated]
		private static Func<DynamicObjectData, bool> _003C_003Ef__am_0024cache5;

		[CompilerGenerated]
		private static Func<DynamicObjectData, bool> _003C_003Ef__am_0024cache6;

		[CompilerGenerated]
		private static Func<DynamicObjectData, bool> _003C_003Ef__am_0024cache7;

		[CompilerGenerated]
		private static Func<DynamicObjectData, bool> _003C_003Ef__am_0024cache8;

		[CompilerGenerated]
		private static Func<DynamicObjectData, bool> _003C_003Ef__am_0024cache9;

		private void Start()
		{
			CreateItemSpawnOptions();
			NetworkingButton.Activate(Client.Instance.ExperimentalBuild);
		}

		private void Update()
		{
			if (CurrentScreen == 0)
			{
				if ((InputManager.GetKeyDown(KeyCode.KeypadEnter) || InputManager.GetKeyDown(KeyCode.Return)) && Input.text != string.Empty)
				{
					SubmitText();
				}
				else if (InputManager.GetKeyDown(KeyCode.UpArrow) && Elements.Count > 0)
				{
					lastSelectedStackItem = Elements.FindLast(_003CUpdate_003Em__0);
					if (lastSelectedStackItem != null)
					{
						Input.text = lastSelectedStackItem.Item1.GetComponent<Text>().text;
						Input.Select();
						Input.ActivateInputField();
					}
				}
				else if (InputManager.GetKeyDown(KeyCode.DownArrow) && Elements.Count > 0)
				{
					lastSelectedStackItem = Elements.FindLast(_003CUpdate_003Em__1);
					if (lastSelectedStackItem != null)
					{
						Input.text = lastSelectedStackItem.Item1.GetComponent<Text>().text;
						Input.Select();
						Input.ActivateInputField();
					}
				}
			}
			if (InputManager.GetKeyDown(KeyCode.Escape) || InputManager.GetKeyDown(KeyCode.F2))
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
				UnityEngine.Object.Destroy(Elements[0].Item1);
				Elements.RemoveAt(0);
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(TextElement, Scroll.content);
			gameObject.transform.SetAsLastSibling();
			gameObject.GetComponent<Text>().text = text;
			if (color.HasValue)
			{
				gameObject.GetComponent<Text>().color = color.Value;
			}
			gameObject.SetActive(true);
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
			NetworkController.Instance.SendToGameServer(new ConsoleMessage
			{
				Text = Input.text
			});
			CreateTextElement(Input.text, null, true);
			Input.text = string.Empty;
			Input.ActivateInputField();
			Input.Select();
		}

		public void Open()
		{
			Client.Instance.CanvasManager.IsInputFieldIsActive = true;
			SetScreen(0);
			Client.Instance.ToggleCursor(true);
			Client.Instance.InputModule.ToggleCustomCursorPosition(false);
			MyPlayer.Instance.FpsController.ToggleMovement(false);
			MyPlayer.Instance.FpsController.ToggleAttached(true);
			if (!MyPlayer.Instance.FpsController.IsZeroG)
			{
				MyPlayer.Instance.FpsController.ResetVelocity();
			}
			lastSelectedStackItem = null;
			Canvas.ForceUpdateCanvases();
			base.gameObject.SetActive(true);
			Input.ActivateInputField();
		}

		public void Close()
		{
			Client.Instance.CanvasManager.IsInputFieldIsActive = false;
			base.gameObject.SetActive(false);
			if (!MyPlayer.Instance.IsLockedToTrigger)
			{
				Client.Instance.ToggleCursor(false);
				Client.Instance.InputModule.ToggleCustomCursorPosition(true);
				MyPlayer.Instance.FpsController.ToggleAttached(false);
				MyPlayer.Instance.FpsController.ToggleMovement(!MyPlayer.Instance.SittingOnPilotSeat);
				if (MyPlayer.Instance.SittingOnPilotSeat)
				{
					MyPlayer.Instance.FpsController.ToggleCameraAttachToHeadBone(true);
				}
				MyCharacterController fpsController = MyPlayer.Instance.FpsController;
				int isActive;
				if (!MyPlayer.Instance.SittingOnPilotSeat && !MyPlayer.Instance.InLadderTrigger)
				{
					if (MyPlayer.Instance.InLockState && !MyPlayer.Instance.IsLockedToTrigger && MyPlayer.Instance.Parent is SpaceObjectVessel)
					{
						Dictionary<int, SceneSpawnPoint>.ValueCollection values = (MyPlayer.Instance.Parent as SpaceObjectVessel).SpawnPoints.Values;
						if (_003C_003Ef__am_0024cache0 == null)
						{
							_003C_003Ef__am_0024cache0 = _003CClose_003Em__2;
						}
						isActive = ((values.FirstOrDefault(_003C_003Ef__am_0024cache0) != null) ? 1 : 0);
					}
					else
					{
						isActive = 0;
					}
				}
				else
				{
					isActive = 1;
				}
				fpsController.ToggleAutoFreeLook((byte)isActive != 0);
			}
			else if (MyPlayer.Instance.IsDrivingShip || MyPlayer.Instance.ShipControlMode == ShipControlMode.Docking)
			{
				Client.Instance.ToggleCursor(false);
				Client.Instance.InputModule.ToggleCustomCursorPosition(true);
			}
		}

		public void Spawn(string itemToSpawn)
		{
			NetworkController.Instance.SendToGameServer(new ConsoleMessage
			{
				Text = "spawn " + itemToSpawn
			});
		}

		public void Action(string actionToDo)
		{
			NetworkController.Instance.SendToGameServer(new ConsoleMessage
			{
				Text = actionToDo
			});
		}

		public void ToggleGodmod()
		{
			string empty = string.Empty;
			empty = ((!GodMode.isOn) ? "0" : "1");
			NetworkController.Instance.SendToGameServer(new ConsoleMessage
			{
				Text = "god " + empty
			});
		}

		public void SetScreen(int option)
		{
			NetworkingActive = false;
			foreach (GameObject option2 in Options)
			{
				option2.Activate(false);
			}
			foreach (GameObject screen in Screens)
			{
				screen.Activate(false);
			}
			Screens[option].Activate(true);
			Options[option].Activate(true);
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
			Dictionary<short, DynamicObjectData>.ValueCollection values = StaticData.DynamicObjectsDataList.Values;
			if (_003C_003Ef__am_0024cache1 == null)
			{
				_003C_003Ef__am_0024cache1 = _003CCreateItemSpawnOptions_003Em__3;
			}
			List<DynamicObjectData> list = new List<DynamicObjectData>(values.Where(_003C_003Ef__am_0024cache1));
			Dictionary<short, DynamicObjectData>.ValueCollection values2 = StaticData.DynamicObjectsDataList.Values;
			if (_003C_003Ef__am_0024cache2 == null)
			{
				_003C_003Ef__am_0024cache2 = _003CCreateItemSpawnOptions_003Em__4;
			}
			List<DynamicObjectData> list2 = new List<DynamicObjectData>(values2.Where(_003C_003Ef__am_0024cache2));
			Dictionary<short, DynamicObjectData>.ValueCollection values3 = StaticData.DynamicObjectsDataList.Values;
			if (_003C_003Ef__am_0024cache3 == null)
			{
				_003C_003Ef__am_0024cache3 = _003CCreateItemSpawnOptions_003Em__5;
			}
			List<DynamicObjectData> list3 = new List<DynamicObjectData>(values3.Where(_003C_003Ef__am_0024cache3));
			Dictionary<short, DynamicObjectData>.ValueCollection values4 = StaticData.DynamicObjectsDataList.Values;
			if (_003C_003Ef__am_0024cache4 == null)
			{
				_003C_003Ef__am_0024cache4 = _003CCreateItemSpawnOptions_003Em__6;
			}
			List<DynamicObjectData> list4 = new List<DynamicObjectData>(values4.Where(_003C_003Ef__am_0024cache4));
			Dictionary<short, DynamicObjectData>.ValueCollection values5 = StaticData.DynamicObjectsDataList.Values;
			if (_003C_003Ef__am_0024cache5 == null)
			{
				_003C_003Ef__am_0024cache5 = _003CCreateItemSpawnOptions_003Em__7;
			}
			List<DynamicObjectData> list5 = new List<DynamicObjectData>(values5.Where(_003C_003Ef__am_0024cache5));
			Dictionary<short, DynamicObjectData>.ValueCollection values6 = StaticData.DynamicObjectsDataList.Values;
			if (_003C_003Ef__am_0024cache6 == null)
			{
				_003C_003Ef__am_0024cache6 = _003CCreateItemSpawnOptions_003Em__8;
			}
			List<DynamicObjectData> list6 = new List<DynamicObjectData>(values6.Where(_003C_003Ef__am_0024cache6));
			Dictionary<short, DynamicObjectData>.ValueCollection values7 = StaticData.DynamicObjectsDataList.Values;
			if (_003C_003Ef__am_0024cache7 == null)
			{
				_003C_003Ef__am_0024cache7 = _003CCreateItemSpawnOptions_003Em__9;
			}
			List<DynamicObjectData> list7 = new List<DynamicObjectData>(values7.Where(_003C_003Ef__am_0024cache7));
			Dictionary<short, DynamicObjectData>.ValueCollection values8 = StaticData.DynamicObjectsDataList.Values;
			if (_003C_003Ef__am_0024cache8 == null)
			{
				_003C_003Ef__am_0024cache8 = _003CCreateItemSpawnOptions_003Em__A;
			}
			List<DynamicObjectData> list8 = new List<DynamicObjectData>(values8.Where(_003C_003Ef__am_0024cache8));
			Dictionary<short, DynamicObjectData>.ValueCollection values9 = StaticData.DynamicObjectsDataList.Values;
			if (_003C_003Ef__am_0024cache9 == null)
			{
				_003C_003Ef__am_0024cache9 = _003CCreateItemSpawnOptions_003Em__B;
			}
			List<DynamicObjectData> list9 = new List<DynamicObjectData>(values9.Where(_003C_003Ef__am_0024cache9));
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
					component.Icon.sprite = Client.Instance.SpriteManager.GetSprite(machineryPartData.PartType);
				}
				else if (item.ItemType == ItemType.GenericItem)
				{
					GenericItemData genericItemData = item.DefaultAuxData as GenericItemData;
					component.Name.text = genericItemData.SubType.ToLocalizedString();
					component.Icon.sprite = Client.Instance.SpriteManager.GetSprite(genericItemData.SubType);
				}
				else
				{
					component.Name.text = item.ItemType.ToLocalizedString();
					component.Icon.sprite = Client.Instance.SpriteManager.GetSprite(item.ItemType);
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

		[CompilerGenerated]
		private bool _003CUpdate_003Em__0(Tuple<GameObject, bool> m)
		{
			return m.Item2 && (Elements.IndexOf(lastSelectedStackItem) > Elements.IndexOf(m) || lastSelectedStackItem == null);
		}

		[CompilerGenerated]
		private bool _003CUpdate_003Em__1(Tuple<GameObject, bool> m)
		{
			return m.Item2 && (Elements.IndexOf(lastSelectedStackItem) < Elements.IndexOf(m) || lastSelectedStackItem == null);
		}

		[CompilerGenerated]
		private static bool _003CClose_003Em__2(SceneSpawnPoint m)
		{
			return m.PlayerGUID == MyPlayer.Instance.GUID;
		}

		[CompilerGenerated]
		private static bool _003CCreateItemSpawnOptions_003Em__3(DynamicObjectData m)
		{
			return m.DefaultAuxData.Category == ItemCategory.Weapons;
		}

		[CompilerGenerated]
		private static bool _003CCreateItemSpawnOptions_003Em__4(DynamicObjectData m)
		{
			return m.DefaultAuxData.Category == ItemCategory.Magazines;
		}

		[CompilerGenerated]
		private static bool _003CCreateItemSpawnOptions_003Em__5(DynamicObjectData m)
		{
			return m.DefaultAuxData.Category == ItemCategory.Medical;
		}

		[CompilerGenerated]
		private static bool _003CCreateItemSpawnOptions_003Em__6(DynamicObjectData m)
		{
			return m.DefaultAuxData.Category == ItemCategory.Suits;
		}

		[CompilerGenerated]
		private static bool _003CCreateItemSpawnOptions_003Em__7(DynamicObjectData m)
		{
			return m.DefaultAuxData.Category == ItemCategory.Tools;
		}

		[CompilerGenerated]
		private static bool _003CCreateItemSpawnOptions_003Em__8(DynamicObjectData m)
		{
			return m.DefaultAuxData.Category == ItemCategory.Parts;
		}

		[CompilerGenerated]
		private static bool _003CCreateItemSpawnOptions_003Em__9(DynamicObjectData m)
		{
			return m.DefaultAuxData.Category == ItemCategory.Utility;
		}

		[CompilerGenerated]
		private static bool _003CCreateItemSpawnOptions_003Em__A(DynamicObjectData m)
		{
			return m.DefaultAuxData.Category == ItemCategory.Containers;
		}

		[CompilerGenerated]
		private static bool _003CCreateItemSpawnOptions_003Em__B(DynamicObjectData m)
		{
			return m.DefaultAuxData.Category == ItemCategory.General;
		}
	}
}
