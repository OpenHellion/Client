using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OpenHellion;
using OpenHellion.IO;
using OpenHellion.Net;
using OpenHellion.Social;
using OpenHellion.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class Chat : MonoBehaviour
	{
		/// <summary>
		///		The chat state we are currently reading.
		/// </summary>
		public enum ChatState
		{
			Global,
			Party,
			Local
		}

		public GameObject ChatInputBox;

		public Transform ContentTrans;

		public GameObject ChatItemPref;

		public InputField ChatInput;

		public Scrollbar ScrollBarVertical;

		public GameObject ScrollBarHandle;

		public List<ChatMessage> Messages = new List<ChatMessage>();

		public int KeepMessagesCount = 30;

		public Color GlobalColor = Colors.White;

		public Color LocalColor = Colors.BlueScan;

		public Color SystemColor = Colors.Orange;

		private ChatState _chatState;

		[FormerlySerializedAs("_worldState")] [SerializeField] private World _world;

		private void Start()
		{
			NakamaClient.OnChatMessageReceived += ReceiveMessage;
		}

		private void Update()
		{
			if (!Settings.SettingsData.GameSettings.DisableChat)
			{
				if (ControlsSubsystem.GetButtonDown(ControlsSubsystem.ConfigAction.Chat) && !ChatInputBox.activeInHierarchy &&
				    !_world.InGameGUI.Console.gameObject.activeInHierarchy &&
				    !_world.InGameGUI.IsInputFieldIsActive && !_world.InGameGUI.OverlayCanvasIsOn &&
				    !ControlsSubsystem.GetButton(ControlsSubsystem.ConfigAction.Drop))
				{
					CloseChat();
				}

				if (_world.IsChatOpened && Keyboard.current.enterKey.wasPressedThisFrame)
				{
					CloseChat();
				}

				if (_world.IsChatOpened && Mouse.current.scroll.y.ReadValue().IsNotEpsilonZero() &&
				    (ScrollBarVertical.value >= 0f || ScrollBarVertical.value <= 1f))
				{
					ScrollBarVertical.value += Mouse.current.scroll.y.ReadValue() * 0.2f;
				}

				if (_world.IsChatOpened && Keyboard.current.escapeKey.wasPressedThisFrame)
				{
					CloseChat();
				}

				if (Mouse.current.leftButton.wasPressedThisFrame)
				{
					ChatInput.Select();
				}
				else if (Mouse.current.leftButton.wasReleasedThisFrame)
				{
					ChatInput.MoveTextEnd(false);
				}
			}
		}

		private void OnDestroy()
		{
			NakamaClient.OnChatMessageReceived -= ReceiveMessage;
		}

		public void CreateSystemMessage(SystemMessagesTypes type, object[] param)
		{
			if (type == SystemMessagesTypes.RestartServerTime)
			{
				_world.InGameGUI.Notification(
					string.Format(Localization.SystemChat[(int)type], param[0], param[1]),
					InGameGUI.NotificationType.Alert);
				return;
			}

			GameObject chatItem = Instantiate(ChatItemPref, transform.position, transform.rotation);
			chatItem.transform.SetParent(ContentTrans);
			chatItem.SetActive(true);
			chatItem.transform.localScale = new Vector3(1f, 1f, 1f);
			string messageText;
			switch (type)
			{
				case SystemMessagesTypes.Custom:
					messageText = param[0].ToString();
					break;
				case SystemMessagesTypes.DoomedOutpostSpawned:
					messageText = string.Format(Localization.SystemChat[(int)type], param[0], param[1], param[2]);
					break;
				case SystemMessagesTypes.ShipTimerAllreadyStarted:
				case SystemMessagesTypes.ShipInRange:
					messageText = Localization.SystemChat[(int)type];
					break;
				case SystemMessagesTypes.ShipWillArriveIn:
					messageText = string.Format(Localization.SystemChat[(int)type], param[0]);
					break;
				default:
					messageText = Localization.SystemChat[0];
					break;
			}

			float messageLength = messageText.Length;
			float messageHeight = messageLength <= 75f ? 30f :
				!(messageLength > 75f) || !(messageLength <= 150f) ? 90f : 60f;
			chatItem.GetComponent<LayoutElement>().minHeight = messageHeight;
			chatItem.transform.Find("UsernameText").GetComponent<Text>().text = Localization.System.ToUpper();
			Text component = chatItem.transform.Find("BodyText").GetComponent<Text>();
			component.color = SystemColor;
			component.text = messageText;
			ChatMessage chatMessage = chatItem.GetComponent<ChatMessage>();
			Messages.Add(chatMessage);
			if (Messages.Count > KeepMessagesCount)
			{
				Messages[0].RemoveThisMessage();
				Messages.RemoveAt(0);
			}

			_world.InGameGUI.Notification(messageText, InGameGUI.NotificationType.Alert);
		}

		private void ReceiveMessage(string username, string messageText)
		{
			// TODO: Workaround. All chat states should be supported.
			CreateMessageElement(username, messageText, _chatState is ChatState.Local);
		}

		public void CreateMessageElement(string username, string messageText, bool local)
		{
			GameObject chatItem = Instantiate(ChatItemPref, transform.position, transform.rotation);
			chatItem.transform.SetParent(ContentTrans);
			chatItem.SetActive(true);
			chatItem.transform.localScale = new Vector3(1f, 1f, 1f);
			float messageHeight = messageText.Length <= 75f ? 30f :
				!(messageText.Length > 75f) || !(messageText.Length <= 150f) ? 90f : 60f;
			chatItem.GetComponent<LayoutElement>().minHeight = messageHeight;
			chatItem.transform.Find("UsernameText").GetComponent<Text>().text = username;

			Text textComponent = chatItem.transform.Find("BodyText").GetComponent<Text>();
			textComponent.color = !local ? GlobalColor : LocalColor;
			textComponent.text = messageText;

			ChatMessage messageComponent = chatItem.GetComponent<ChatMessage>();
			Messages.Add(messageComponent);
			if (Messages.Count > KeepMessagesCount)
			{
				Messages[0].RemoveThisMessage();
				Messages.RemoveAt(0);
			}
		}

		private void SendChatMessage(string messageText)
		{
			if (_chatState is ChatState.Local)
			{
				TextChatMessage textChatMessage = new TextChatMessage();
				textChatMessage.MessageText = messageText;
				textChatMessage.Local = true;
				NetworkController.Send(textChatMessage);
			}
			else
			{
				// TODO: Do this through nakama.
			}
		}

		private IEnumerator ResetInputAndShowChat(bool show)
		{
			if (show)
			{
				ControlsSubsystem.ResetInputAxis();
				yield return new WaitForSeconds(0.2f);
			}

			if (show)
			{
				MyPlayer.Instance.FpsController.ToggleMovement(false);
			}
			else if (MyPlayer.Instance.LockedToTrigger is null && !MyPlayer.Instance.InLockState &&
			         !MyPlayer.Instance.InInteractState && !MyPlayer.Instance.InLerpingState)
			{
				MyPlayer.Instance.FpsController.ToggleMovement(true);
			}
		}

		// Shows or hides the chat box.
		private async UniTaskVoid ShowChat(bool show)
		{
			if (Settings.SettingsData.GameSettings.DisableChat)
			{
				return;
			}

			_world.InGameGUI.IsInputFieldIsActive = show;
			_world.IsChatOpened = show;
			StartCoroutine(ResetInputAndShowChat(show));
			ScrollBarHandle.SetActive(show);
			ChatInputBox.SetActive(show);
			OverrideChatMessages(show);
			ChatInput.textComponent.color = _chatState is ChatState.Global ? GlobalColor : LocalColor;
			if (show)
			{
				FocusInput();
				return;
			}

			await ParseMessageCommands();

			if (ChatInput.text.Length > 0)
			{
				CreateMessageElement("me:", ChatInput.text, _chatState is ChatState.Local);
				string truncatedString = ChatInput.text;
				if (truncatedString.Length > 250)
				{
					truncatedString = truncatedString.Substring(0, 250);
				}

				SendChatMessage(truncatedString);
			}

			ChatInput.text = string.Empty;
		}

		// Parses message and modifies the state directly.
		private async UniTask ParseMessageCommands()
		{
			if (ChatInput.text.Equals("/l", StringComparison.OrdinalIgnoreCase))
			{
				_chatState = ChatState.Local;
				ChatInput.text = string.Empty;
			}
			else if (ChatInput.text.Equals("/g", StringComparison.OrdinalIgnoreCase))
			{
				if (await NakamaClient.JoinChatRoom(ChatState.Global))
				{
					_chatState = ChatState.Global;
					ChatInput.text = string.Empty;
				}
				else
				{
					Debug.LogError("Setting chat mode to party failed.");
				}
			}
			else if (ChatInput.text.Equals("/p", StringComparison.OrdinalIgnoreCase))
			{
				if (await NakamaClient.JoinChatRoom(ChatState.Party))
				{
					_chatState = ChatState.Party;
					ChatInput.text = string.Empty;
				}
				else
				{
					Debug.LogError("Setting chat mode to party failed.");
				}
			}

			// TODO: add the rest of the states.
		}

		public void CloseChat()
		{
			ShowChat(false).Forget();
			if (MyPlayer.Instance.LockedToTrigger is null)
			{
				MyPlayer.Instance.FpsController.ToggleMovement(true);
			}
		}

		private void OverrideChatMessages(bool val)
		{
			foreach (ChatMessage message in Messages)
			{
				message.ShowMessage(val);
			}
		}

		private void FocusInput()
		{
			ChatInput.Select();
			ChatInput.ActivateInputField();
			_world.InGameGUI.IsInputFieldIsActive = true;
		}
	}
}
