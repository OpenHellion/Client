using System;
using OpenHellion;
using OpenHellion.Net;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class NameTagUI : MonoBehaviour
	{
		private SpaceObjectVessel _currentParentVessel;

		private int _currentSceneId;

		public GameObject MainCanvas;

		public InputField CustomInputField;

		private bool _isLockedToTrigger;

		private SceneNameTag _nameTag;

		private World _world;

		private void Awake()
		{
			_world ??= GameObject.Find("/World").GetComponent<World>();
		}

		private void Update()
		{
			if (_isLockedToTrigger)
			{
				if (Keyboard.current.tabKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
				{
					ToggleNameCanvas(_nameTag, false, _currentParentVessel, _currentSceneId);
				}

				if (Keyboard.current.enterKey.wasPressedThisFrame)
				{
					ConfirmInput();
				}
			}
		}

		public void ToggleNameCanvas(SceneNameTag snt, bool toggle, SpaceObjectVessel parentVessel, int inSceneID)
		{
			_nameTag = snt;
			_currentParentVessel = parentVessel;
			_currentSceneId = inSceneID;
			ToggleCanvas(toggle);
			if (toggle)
			{
				CustomInputField.text = snt.NameTagText;
				CustomInputField.Select();
				CustomInputField.ActivateInputField();
				_world.InWorldPanels.Interact();
			}
			else
			{
				snt.NameTagText = CustomInputField.text;
				_world.InWorldPanels.Detach();
			}
		}

		public void ConfirmInput()
		{
			NameTagMessage nameTagMessage = new NameTagMessage();
			nameTagMessage.ID = new VesselObjectID
			{
				VesselGUID = _currentParentVessel.GUID,
				InSceneID = _currentSceneId
			};
			nameTagMessage.NameTagText = CustomInputField.text;
			NetworkController.Instance.SendToGameServer(nameTagMessage);
			ToggleNameCanvas(_nameTag, false, _currentParentVessel, _currentSceneId);
		}

		public void CancelButton()
		{
			ToggleNameCanvas(_nameTag, false, _currentParentVessel, _currentSceneId);
		}

		public void ToggleCanvas(bool toggle)
		{
			_world.InGameGUI.IsInputFieldIsActive = toggle;
			_isLockedToTrigger = toggle;
			MainCanvas.SetActive(toggle);
			Globals.ToggleCursor(toggle);
			_world.InGameGUI.OverlayCanvasIsOn = toggle;
			MyPlayer.Instance.FpsController.ResetVelocity();
			MyPlayer.Instance.FpsController.ToggleAttached(toggle);
			MyPlayer.Instance.FpsController.ToggleMovement(!toggle);
			MyPlayer.Instance.FpsController.ToggleCameraMovement(!toggle);
			if (toggle)
			{
				CustomInputField.ActivateInputField();
				CustomInputField.Select();
				_world.InWorldPanels.Interact();
			}
			else
			{
				_world.InWorldPanels.Detach();
			}
		}
	}
}
