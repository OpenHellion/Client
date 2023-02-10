using System;
using System.Collections.Generic;
using System.Linq;
using TriInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class CanvasUI : MonoBehaviour
	{
		public enum NotificationType
		{
			General,
			Quest,
			Alert
		}

		[Title("CROSHAIRS")]
		public bool IsCroshairActive;

		public Image Croshair;

		public Animator CroshairAnim;

		public Image DotCroshair;

		public Text ItemName;

		[Title("TOOLTIP")]
		public GameObject TooltipEquip;

		public GameObject Tooltip;

		public GameObject ThrowingItem;

		public Image ThrowingFiller;

		private bool isThrowing;

		private float throwTimer;

		public GameObject Help;

		[Title("TUTORIAL")]
		public List<int> ShownTutorials = new List<int>();

		public GameObject TutorialUIPref;

		public GameObject TutorialAdditionalInfo;

		[Title("ALERT")]
		public GameObject AlertObject;

		public Text AlertMessage;

		[Title("Helmet hud")]
		public HelmetHudUI HelmetHud;

		[Title("NOTIFICATIONS")]
		public NotificationUI NotificationUI;

		private bool isNotificationActive;

		private float notificationTimer;

		public GameObject Notifications;

		public Transform NotificationsHolder;

		[Title("QUEST NOTIFICATIONS")]
		public QuestCutScene QuestCutScene;

		public GameObject TrackingQuest;

		private Animator trackingQuestAnimator;

		public Transform TrackingHolder;

		public TaskUI TrackingTaskPrefab;

		public Quest CurrentTrackingQuest;

		public Text TrackingQuestName;

		public GameObject QuestIndicatorPrefab;

		public QuestIndicators QuestIndicators;

		public QuestTitleUI QuestTitleUI;

		public QuestTracker QuestTracker;

		public List<QuestTrigger> ActivatedTriggers = new List<QuestTrigger>();

		public GameObject SeasonEnd;

		private void Start()
		{
			trackingQuestAnimator = TrackingQuest.GetComponent<Animator>();
			UpdateTooltipKeys();
			SetCurrentTrackingQuest(Client.Instance.Quests.FirstOrDefault((Quest m) => m.Status == QuestStatus.Active));
			UpdateQuestObjects();
		}

		private void Update()
		{
			if (isThrowing && throwTimer <= Client.DROP_MAX_TIME)
			{
				throwTimer += Time.deltaTime;
				ThrowingFiller.fillAmount = throwTimer / Client.DROP_MAX_TIME;
			}
			if (isNotificationActive)
			{
				notificationTimer += Time.deltaTime;
				if (notificationTimer > 4f && NotificationsHolder.childCount == 0)
				{
					Notifications.SetActive(value: false);
					isNotificationActive = false;
					notificationTimer = 0f;
				}
			}
			if (MyPlayer.Instance != null && MyPlayer.Instance.Inventory != null)
			{
				if (!TooltipEquip.activeInHierarchy && MyPlayer.Instance.Inventory.ItemInHands != null && MyPlayer.Instance.Inventory.ItemInHands.IsInvetoryEquipable)
				{
					ToggleTooltipEquip(val: true);
				}
				else if (TooltipEquip.activeInHierarchy && (MyPlayer.Instance.Inventory.ItemInHands == null || !MyPlayer.Instance.Inventory.ItemInHands.IsInvetoryEquipable))
				{
					ToggleTooltipEquip(val: false);
				}
			}
			if (!IsCroshairActive && Croshair.gameObject.activeInHierarchy)
			{
				CroshairAnim.SetTrigger("hide");
			}
		}

		public void CheckDotCroshair()
		{
			DotCroshair.gameObject.SetActive(Client.Instance.CanvasManager.ShowCrosshair);
			if (MyPlayer.Instance != null && MyPlayer.Instance.CurrentStance == MyPlayer.PlayerStance.Special)
			{
				DotCroshair.gameObject.SetActive(value: false);
			}
		}

		public void ToggleCroshair(bool show, BaseSceneTrigger lookingAtTrigger = null, bool canLoot = false)
		{
			IsCroshairActive = show;
			if (IsCroshairActive)
			{
				Croshair.gameObject.Activate(value: true);
			}
			else if (CroshairAnim.gameObject.activeSelf)
			{
				CroshairAnim.SetTrigger("hide");
			}
			ToggleItemName(show);
			if (!show)
			{
				DotCroshair.gameObject.SetActive(Client.Instance.CanvasManager.ShowCrosshair);
				Tooltip.SetActive(value: false);
			}
			else
			{
				DotCroshair.gameObject.SetActive(value: false);
				SetTooltip(lookingAtTrigger, canLoot);
			}
			if (Client.Instance.CanvasManager.ShowTips && lookingAtTrigger != null && lookingAtTrigger.Glossary != null)
			{
				Help.Activate(value: true);
			}
			else
			{
				Help.Activate(value: false);
			}
		}

		private void SetTooltip(BaseSceneTrigger lookingAtTrigger = null, bool canLoot = false)
		{
			if (canLoot)
			{
				SetItemName(null);
				Tooltip.SetActive(value: true);
				Tooltip.GetComponentInChildren<Text>().text = string.Format(Localization.HoldToLoot, InputController.GetAxisKeyName(InputController.Actions.Interact));
			}
			else if (lookingAtTrigger != null && Client.Instance.CanvasManager.ShowTips)
			{
				Tooltip.SetActive(value: true);
				if (lookingAtTrigger is BaseSceneAttachPoint && (lookingAtTrigger as BaseSceneAttachPoint).InteractionTip != null)
				{
					Tooltip.GetComponentInChildren<Text>().text = (lookingAtTrigger as BaseSceneAttachPoint).InteractionTip;
				}
				else if (lookingAtTrigger.StandardTip != 0)
				{
					if (lookingAtTrigger.StandardTip == Localization.StandardInteractionTip.ExitCryo)
					{
						Client.Instance.CanvasManager.ToggleDefaultInteractionTip();
					}
					Tooltip.GetComponentInChildren<Text>().text = lookingAtTrigger.StandardTip.ToLocalizedString();
				}
				else
				{
					Tooltip.SetActive(value: false);
				}
			}
			else
			{
				Tooltip.SetActive(value: false);
			}
		}

		public void SetItemName(Item item)
		{
			if (item == null)
			{
				ItemName.text = string.Empty;
				ItemName.color = Colors.White;
				return;
			}
			string text = item.Name;
			ItemName.text = string.Empty;
			ItemName.text = text.Replace("<br>", "\n");
			ItemName.color = Colors.Tier[item.Tier];
		}

		private void ToggleItemName(bool selected)
		{
			if (selected)
			{
				if (!ItemName.gameObject.activeInHierarchy)
				{
					ItemName.gameObject.SetActive(value: true);
				}
			}
			else if (ItemName.gameObject.activeInHierarchy)
			{
				ItemName.gameObject.SetActive(value: false);
			}
		}

		public void ToggleTooltipEquip(bool val)
		{
			if (val)
			{
				if (MyPlayer.Instance.Inventory.ItemInHands is Outfit && MyPlayer.Instance.CurrentOutfit == null)
				{
					TooltipEquip.SetActive(value: true);
					return;
				}
				InventorySlot inventorySlot = MyPlayer.Instance.Inventory.FindEmptyOutfitSlot(MyPlayer.Instance.Inventory.ItemInHands, museBeEquip: true);
				if (inventorySlot != null)
				{
					TooltipEquip.SetActive(value: true);
				}
			}
			else
			{
				TooltipEquip.SetActive(val);
			}
		}

		public void UpdateTooltipKeys()
		{
			TooltipEquip.GetComponentInChildren<Text>(includeInactive: true).text = string.Format(Localization.HoldToEquip, InputController.GetAxisKeyName(InputController.Actions.Equip));
		}

		public void ThrowingItemToggle(bool val)
		{
			ThrowingItem.SetActive(val);
			isThrowing = val;
			if (!val)
			{
				throwTimer = 0f;
				ThrowingFiller.fillAmount = 0f;
			}
		}

		public void ShowTutorialUI(int tut)
		{
			if (Client.Instance.CanvasManager.ShowTutorial && !ShownTutorials.Contains(tut))
			{
				string text = Localization.TutorialText[0];
				if (Localization.TutorialText.ContainsKey(tut))
				{
					text = Localization.TutorialText[tut];
					ShownTutorials.Add(tut);
				}
				if (text.Length != 0 && tut != 0)
				{
					GameObject gameObject = GameObject.Instantiate(TutorialUIPref, TutorialUIPref.transform.parent);
					gameObject.SetActive(value: true);
					gameObject.transform.SetAsFirstSibling();
					TutorialUI component = gameObject.GetComponent<TutorialUI>();
					component.MessageText.text = text;
				}
				if (tut == 1)
				{
					TutorialAdditionalInfo.SetActive(value: true);
					TutorialAdditionalInfo.transform.SetAsFirstSibling();
					TutorialAdditionalInfo.GetComponentInChildren<Text>().text = Localization.Tut_1AdditionalInfo;
				}
				else
				{
					TutorialAdditionalInfo.SetActive(value: false);
				}
			}
		}

		public void Alert(string msg)
		{
			AlertMessage.text = msg;
			AlertObject.SetActive(value: true);
		}

		public void Notification(string msg, NotificationType tip)
		{
			NotificationUI notificationUI = Instantiate(NotificationUI, NotificationsHolder);
			notificationUI.transform.Reset();
			notificationUI.transform.SetAsFirstSibling();
			notificationUI.Activate(NotificationsHolder.childCount * 0.1f);
			notificationUI.Type = tip;
			notificationUI.Content.text = msg;
			isNotificationActive = true;
			notificationTimer = 0f;
			Notifications.SetActive(value: true);
		}

		public void SetCurrentTrackingQuest(Quest quest)
		{
			CurrentTrackingQuest = quest;
			if (quest != null)
			{
				QuestTracker.ShowTasks(1f);
			}
			UpdateQuestMarkers();
		}

		public void UpdateQuestMarkers()
		{
			UnityEngine.SceneManagement.Scene activeScene = SceneManager.GetActiveScene();
			List<GameObject> list = new List<GameObject>();
			activeScene.GetRootGameObjects(list);
			foreach (GameObject item in list)
			{
				SceneQuestTrigger[] componentsInChildren = item.GetComponentsInChildren<SceneQuestTrigger>();
				foreach (SceneQuestTrigger sceneQuestTrigger in componentsInChildren)
				{
					if (sceneQuestTrigger.QuestTrigger != null)
					{
						sceneQuestTrigger.UpdateQuestTriggerMarker();
						sceneQuestTrigger.UpdateAvailableQuestMarker();
					}
				}
			}
		}

		public void UpdateQuestObjects()
		{
			UnityEngine.SceneManagement.Scene activeScene = SceneManager.GetActiveScene();
			List<GameObject> list = new List<GameObject>();
			activeScene.GetRootGameObjects(list);
			foreach (GameObject item in list)
			{
				SceneQuestTrigger[] componentsInChildren = item.GetComponentsInChildren<SceneQuestTrigger>();
				foreach (SceneQuestTrigger sceneQuestTrigger in componentsInChildren)
				{
					if (sceneQuestTrigger.QuestTrigger != null)
					{
						sceneQuestTrigger.OnQuestTriggerUpdate();
					}
				}
			}
		}

		public void TriggerQuestEvents(QuestTrigger trigger)
		{
			UnityEngine.SceneManagement.Scene activeScene = SceneManager.GetActiveScene();
			List<GameObject> list = new List<GameObject>();
			activeScene.GetRootGameObjects(list);
			foreach (GameObject item in list)
			{
				foreach (SceneQuestTrigger item2 in from m in item.GetComponentsInChildren<SceneQuestTrigger>()
					where m.Task == trigger.TaskObject
					select m)
				{
					if (item2.QuestTrigger != null)
					{
						item2.TriggerEvents();
					}
				}
			}
		}

		public void QuestTriggerUpdate(QuestTrigger trigger, bool playCutScenes = true)
		{
			if (trigger.Quest == CurrentTrackingQuest)
			{
				if (trigger.Type == QuestTriggerType.Task || trigger.Type == QuestTriggerType.Complete)
				{
					if (playCutScenes)
					{
						QuestTracker.RefreshTasks();
						QuestCutScene.PlayCutScene(trigger);
					}
					UpdateQuestMarkers();
					if (trigger.Type == QuestTriggerType.Complete && playCutScenes && trigger.Quest.Status == QuestStatus.Completed)
					{
						QuestTitleUI.CompleteQuest(trigger.Quest);
						if (trigger.TaskObject.LastTask)
						{
							SeasonEnd.Activate(value: true);
						}
						SetCurrentTrackingQuest(null);
					}
				}
			}
			else if (trigger.Type == QuestTriggerType.Activate)
			{
				if (trigger.Quest.QuestObject.TrackOnPickUp || CurrentTrackingQuest == null)
				{
					QuestTracker.HideActiveTasks();
					SetCurrentTrackingQuest(trigger.Quest);
					QuestTracker.ShowTasks(5f);
				}
				if (playCutScenes)
				{
					QuestTitleUI.ShowTitle(trigger.Quest);
				}
				if (playCutScenes)
				{
					QuestCutScene.PlayCutScene(trigger);
				}
				Client.Instance.CanvasManager.QuickTip(Localization.GetLocalizedField("NewQuestAvailable"));
			}
			else
			{
				if (playCutScenes)
				{
					QuestCutScene.PlayCutScene(trigger);
				}
				if (CurrentTrackingQuest == null)
				{
					SetCurrentTrackingQuest(trigger.Quest);
					QuestTracker.ShowTasks(5f);
				}
				else
				{
					QuestTracker.TriggerNonTrackedTask(trigger);
				}
				UpdateQuestMarkers();
			}
			UpdateQuestObjects();
			TriggerQuestEvents(trigger);
		}
	}
}
