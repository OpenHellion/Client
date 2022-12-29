using OpenHellion.Networking;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class NameTagUI : MonoBehaviour
	{
		private SpaceObjectVessel currentParentVessel;

		private int currentSceneId;

		public GameObject MainCanvas;

		public InputField CustomInputField;

		private bool isLockedToTrigger;

		private SceneNameTag thisNameTag;

		private void Start()
		{
		}

		private void Update()
		{
			if (isLockedToTrigger)
			{
				if (InputManager.GetKeyDown(KeyCode.Tab) || InputManager.GetKeyDown(KeyCode.Escape))
				{
					ToggleNameCanvas(thisNameTag, false, currentParentVessel, currentSceneId);
				}
				if (InputManager.GetKeyDown(KeyCode.KeypadEnter) || InputManager.GetKeyDown(KeyCode.Return))
				{
					ConfirmInput();
				}
			}
		}

		public void ToggleNameCanvas(SceneNameTag snt, bool toggle, SpaceObjectVessel parentVessel, int _InSceneID)
		{
			thisNameTag = snt;
			currentParentVessel = parentVessel;
			currentSceneId = _InSceneID;
			ToggleCanvas(toggle);
			if (toggle)
			{
				CustomInputField.text = snt.NameTagText;
				CustomInputField.Select();
				CustomInputField.ActivateInputField();
				Client.Instance.InGamePanels.Interact();
			}
			else
			{
				snt.NameTagText = CustomInputField.text;
				Client.Instance.InGamePanels.Detach();
			}
		}

		public void ConfirmInput()
		{
			NameTagMessage nameTagMessage = new NameTagMessage();
			nameTagMessage.ID = new VesselObjectID
			{
				VesselGUID = currentParentVessel.GUID,
				InSceneID = currentSceneId
			};
			nameTagMessage.NameTagText = CustomInputField.text;
			NetworkController.Instance.SendToGameServer(nameTagMessage);
			ToggleNameCanvas(thisNameTag, false, currentParentVessel, currentSceneId);
		}

		public void CancelButton()
		{
			ToggleNameCanvas(thisNameTag, false, currentParentVessel, currentSceneId);
		}

		public void ToggleCanvas(bool toggle)
		{
			Client.Instance.CanvasManager.IsInputFieldIsActive = toggle;
			isLockedToTrigger = toggle;
			MainCanvas.SetActive(toggle);
			Client.Instance.ToggleCursor(toggle);
			Client.Instance.InputModule.ToggleCustomCursorPosition(!toggle);
			Client.Instance.CanvasManager.OverlayCanvasIsOn = toggle;
			MyPlayer.Instance.FpsController.ResetVelocity();
			MyPlayer.Instance.FpsController.ToggleAttached(toggle);
			MyPlayer.Instance.FpsController.ToggleMovement(!toggle);
			MyPlayer.Instance.FpsController.ToggleCameraMovement(!toggle);
			if (toggle)
			{
				CustomInputField.ActivateInputField();
				CustomInputField.Select();
				Client.Instance.InGamePanels.Interact();
			}
			else
			{
				Client.Instance.InGamePanels.Detach();
			}
		}
	}
}
