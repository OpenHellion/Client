using System.Collections;
using System.Collections.Generic;
using OpenHellion.Networking;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class Chat : MonoBehaviour
	{
		public GameObject ChatGO;

		public GameObject ChatInputBox;

		public Transform ContentTrans;

		public GameObject ChatItemPref;

		public InputField ChatInput;

		public Scrollbar ScrollBarVertical;

		private GameObject ScrollBarHandle;

		public List<ChatMessage> Messages = new List<ChatMessage>();

		public int KeepMessegesCount = 30;

		public Color GlobalColor = Colors.White;

		public Color LocalColor = Colors.BlueScan;

		public Color SystemColor = Colors.Orange;

		private bool local;

		private void Start()
		{
			ScrollBarHandle = ScrollBarVertical.transform.Find("SlidingArea/Handle").gameObject;
		}

		private void Update()
		{
			if (!Client.Instance.CanvasManager.DisableChat)
			{
				if (InputManager.GetButtonDown(InputManager.AxisNames.Y) && !ChatInputBox.activeInHierarchy && !Client.Instance.CanvasManager.Console.gameObject.activeInHierarchy && !Client.Instance.CanvasManager.IsInputFieldIsActive && !Client.Instance.CanvasManager.OverlayCanvasIsOn && !InputManager.GetButton(InputManager.AxisNames.G))
				{
					ShowChat(true);
				}
				if (Client.Instance.IsChatOpened && (InputManager.GetKeyDown(KeyCode.KeypadEnter) || InputManager.GetKeyDown(KeyCode.Return)))
				{
					ShowChat(false);
				}
				if (Client.Instance.IsChatOpened && InputManager.GetAxis(InputManager.AxisNames.MouseWheel).IsNotEpsilonZero() && (ScrollBarVertical.value >= 0f || ScrollBarVertical.value <= 1f))
				{
					ScrollBarVertical.value += InputManager.GetAxis(InputManager.AxisNames.MouseWheel) * 0.2f;
				}
				if (Client.Instance.IsChatOpened && InputManager.GetKeyDown(KeyCode.Escape))
				{
					CloseChat();
				}
				if (InputManager.GetButtonDown(InputManager.AxisNames.Mouse1))
				{
					ChatInput.Select();
				}
				else if (InputManager.GetButtonUp(InputManager.AxisNames.Mouse1))
				{
					ChatInput.MoveTextEnd(false);
				}
			}
		}

		public void CreateSystemMessage(SystemMessagesTypes type, object[] param)
		{
			if (type == SystemMessagesTypes.RestartServerTime)
			{
				Client.Instance.CanvasManager.CanvasUI.Notification(string.Format(Localization.SystemChat[(int)type], param[0], param[1]), CanvasUI.NotificationType.Alert);
				return;
			}
			GameObject gameObject = Object.Instantiate(ChatItemPref, base.transform.position, base.transform.rotation);
			gameObject.transform.SetParent(ContentTrans);
			gameObject.SetActive(true);
			gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			string empty = string.Empty;
			switch (type)
			{
			case SystemMessagesTypes.Custom:
				empty = param[0].ToString();
				break;
			case SystemMessagesTypes.DoomedOutpostSpawned:
				empty = string.Format(Localization.SystemChat[(int)type], param[0], param[1], param[2]);
				break;
			case SystemMessagesTypes.ShipTimerAllreadyStarted:
			case SystemMessagesTypes.ShipInRange:
				empty = Localization.SystemChat[(int)type];
				break;
			case SystemMessagesTypes.ShipWillArriveIn:
				empty = string.Format(Localization.SystemChat[(int)type], param[0]);
				break;
			default:
				empty = Localization.SystemChat[0];
				break;
			}
			float num = 0f;
			float num2 = empty.Length;
			num = ((num2 <= 75f) ? 30f : ((!(num2 > 75f) || !(num2 <= 150f)) ? 90f : 60f));
			gameObject.GetComponent<LayoutElement>().minHeight = num;
			gameObject.transform.Find("UsernameText").GetComponent<Text>().text = Localization.System.ToUpper();
			Text component = gameObject.transform.Find("BodyText").GetComponent<Text>();
			component.color = SystemColor;
			component.text = empty;
			ChatMessage component2 = gameObject.GetComponent<ChatMessage>();
			Messages.Add(component2);
			if (Messages.Count > KeepMessegesCount)
			{
				Messages[0].RemoveThisMessage();
				Messages.RemoveAt(0);
			}
			Client.Instance.CanvasManager.CanvasUI.Notification(empty, CanvasUI.NotificationType.Alert);
		}

		public void CreateMessage(string user, string body, bool local)
		{
			GameObject chatItem = Object.Instantiate(ChatItemPref, base.transform.position, base.transform.rotation);
			chatItem.transform.SetParent(ContentTrans);
			chatItem.SetActive(true);
			chatItem.transform.localScale = new Vector3(1f, 1f, 1f);
			float num = 0f;
			float num2 = body.Length;
			num = ((num2 <= 75f) ? 30f : ((!(num2 > 75f) || !(num2 <= 150f)) ? 90f : 60f));
			chatItem.GetComponent<LayoutElement>().minHeight = num;
			chatItem.transform.Find("UsernameText").GetComponent<Text>().text = user;
			Text component = chatItem.transform.Find("BodyText").GetComponent<Text>();
			component.color = ((!local) ? GlobalColor : LocalColor);
			component.text = body;
			ChatMessage component2 = chatItem.GetComponent<ChatMessage>();
			Messages.Add(component2);
			if (Messages.Count > KeepMessegesCount)
			{
				Messages[0].RemoveThisMessage();
				Messages.RemoveAt(0);
			}
		}

		private void SendChatMessage(string msg)
		{
			TextChatMessage textChatMessage = new TextChatMessage();
			textChatMessage.MessageText = msg;
			textChatMessage.Local = local;
			NetworkController.Instance.SendToGameServer(textChatMessage);
		}

		private IEnumerator ResetInputAndShowChat(bool show)
		{
			if (show)
			{
				InputManager.ResetInputAxis();
				yield return new WaitForSeconds(0.2f);
			}
			if (show)
			{
				MyPlayer.Instance.FpsController.ToggleMovement(false);
			}
			else if (MyPlayer.Instance.LockedToTrigger == null && !MyPlayer.Instance.InLockState && !MyPlayer.Instance.InInteractState && !MyPlayer.Instance.InLerpingState)
			{
				MyPlayer.Instance.FpsController.ToggleMovement(true);
			}
		}

		private void ShowChat(bool show)
		{
			if (Client.Instance.CanvasManager.DisableChat)
			{
				return;
			}
			Client.Instance.CanvasManager.IsInputFieldIsActive = show;
			Client.Instance.IsChatOpened = show;
			StartCoroutine(ResetInputAndShowChat(show));
			ScrollBarHandle.SetActive(show);
			ChatInputBox.SetActive(show);
			OverrideChatMessages(show);
			ChatInput.textComponent.color = ((!local) ? GlobalColor : LocalColor);
			if (show)
			{
				FocusInput();
				return;
			}
			if (ChatInput.text == "/l" || ChatInput.text == "/L")
			{
				local = true;
				ChatInput.text = string.Empty;
				ShowChat(true);
				return;
			}
			if (ChatInput.text == "/g" || ChatInput.text == "/G")
			{
				local = false;
				ChatInput.text = string.Empty;
				ShowChat(true);
				return;
			}
			if (ChatInput.text.Length > 0)
			{
				CreateMessage("me:", ChatInput.text, local);
				string text = ChatInput.text;
				if (text.Length > 250)
				{
					text = text.Substring(0, 250);
				}
				SendChatMessage(text);
			}
			ChatInput.text = string.Empty;
		}

		public void CloseChat()
		{
			ShowChat(false);
			if (MyPlayer.Instance.LockedToTrigger == null)
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
			Client.Instance.CanvasManager.IsInputFieldIsActive = true;
		}
	}
}
