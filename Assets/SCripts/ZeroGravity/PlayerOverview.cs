using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Objects;

namespace ZeroGravity
{
	public class PlayerOverview : MonoBehaviour
	{
		public InventoryUI Inventory;

		public QuestSystemUI Quests;

		public BlueprintsUI Blueprints;

		public GlossaryUI Glossary;

		public List<GameObject> Options;

		private void Start()
		{
		}

		private void Update()
		{
		}

		public void Toggle(bool val, int screen = 0, bool gloss = false)
		{
			if (MyPlayer.Instance.InIteractLayer)
			{
				return;
			}
			Client.Instance.ToggleCursor(val);
			MyPlayer.Instance.FpsController.ToggleCameraMovement(!val);
			MyPlayer.Instance.FpsController.ToggleMovement(!val);
			Client.Instance.InputModule.ToggleCustomCursorPosition(!val);
			if (MyPlayer.Instance.CurrentActiveItem != null)
			{
				MyPlayer.Instance.CurrentActiveItem.PrimaryReleased();
			}
			base.gameObject.SetActive(val);
			if (val)
			{
				if (!MyPlayer.Instance.FpsController.IsZeroG && !MyPlayer.Instance.FpsController.IsJump)
				{
					MyPlayer.Instance.FpsController.ResetVelocity();
				}
				if (gloss)
				{
					ToggleGlossary(true);
				}
				else
				{
					SetScreen(screen);
				}
			}
			else
			{
				Client.Instance.CanvasManager.PlayerOverview.Inventory.LootingTarget = null;
			}
		}

		public void ToggleGlossary(bool val)
		{
			RefreshNav(3);
			Inventory.Toggle(false);
			Quests.Toggle(false);
			Blueprints.Toggle(false);
			Glossary.Toggle(val);
		}

		public void SetScreen(int scr)
		{
			RefreshNav(scr);
			switch (scr)
			{
			case 0:
				Blueprints.Toggle(false);
				Inventory.Toggle(true);
				Quests.Toggle(false);
				Glossary.Toggle(false);
				break;
			case 1:
				Blueprints.Toggle(false);
				Inventory.Toggle(false);
				Glossary.Toggle(false);
				Quests.Toggle(true);
				break;
			case 2:
				Inventory.Toggle(false);
				Quests.Toggle(false);
				Glossary.Toggle(false);
				Blueprints.Toggle(true);
				break;
			}
		}

		private void RefreshNav(int opt)
		{
			foreach (GameObject option in Options)
			{
				option.SetActive(false);
			}
			Options[opt].SetActive(true);
		}
	}
}
