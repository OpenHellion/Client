using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.UI;

namespace ZeroGravity
{
	public class QuestSystemUI : MonoBehaviour
	{
		[CompilerGenerated]
		private sealed class _003CSetQuestLog_003Ec__AnonStorey0
		{
			internal QuestTrigger qt;

			internal QuestSystemUI _0024this;

			internal bool _003C_003Em__0(QuestCutSceneData m)
			{
				return m.QuestTriggerID == qt.ID && m.QuestID == _0024this.SelectedQuest.Quest.ID;
			}
		}

		public QuestUI QuestItem;

		public TaskUI TaskItem;

		public Transform QuestsHolder;

		public List<QuestUI> AllQuests = new List<QuestUI>();

		public Text AvailableQuests;

		public GameObject SkipQuest;

		public QuestUI SelectedQuest;

		public GameObject QuestHolder;

		public Transform DescriptionHolder;

		public Text SelectedQuestName;

		public Text SelectedQuestDescription;

		public GameObject QuestLog;

		public Transform LogHolder;

		public GameObject LogBody;

		public GameObject NoLogAvailable;

		public GameObject LogHeading;

		public QuestCutSceneUI LogUI;

		private List<QuestCutSceneUI> logElements = new List<QuestCutSceneUI>();

		private void Start()
		{
		}

		private void Update()
		{
		}

		public void Toggle(bool val)
		{
			base.gameObject.SetActive(val);
			if (val)
			{
				AvailableQuests.text = Localization.AvailableQuests + " (" + AllQuests.Count + ")".ToUpper();
				RefreshSelectedQuest();
			}
		}

		public void RefreshSelectedQuest()
		{
			foreach (QuestUI allQuest in AllQuests)
			{
				allQuest.RefreshQuestUI();
			}
			DescriptionHolder.DestroyAll<TaskUI>();
			if (SelectedQuest == null && AllQuests.Count > 0)
			{
				SelectedQuest = AllQuests[0];
				SelectedQuest.RefreshQuestUI();
			}
			if (SelectedQuest != null)
			{
				SelectedQuestName.text = SelectedQuest.Quest.Name;
				SelectedQuestDescription.text = SelectedQuest.Quest.Description;
				if (SelectedQuest.Quest.QuestTriggers.Count > 0)
				{
					foreach (QuestTrigger questTrigger in SelectedQuest.Quest.QuestTriggers)
					{
						if (questTrigger.Status != 0 && questTrigger.Type != QuestTriggerType.Activate)
						{
							TaskUI taskUI = Object.Instantiate(TaskItem, DescriptionHolder);
							taskUI.transform.Reset();
							if (questTrigger.Status == QuestStatus.Completed)
							{
								taskUI.transform.SetAsLastSibling();
							}
							else
							{
								taskUI.transform.SetAsFirstSibling();
							}
							taskUI.Name.text = questTrigger.Name;
							taskUI.CompletedObject.SetActive(questTrigger.Status == QuestStatus.Completed);
							if (questTrigger.Description != null)
							{
								taskUI.Description.text = questTrigger.Description;
							}
						}
					}
				}
				SkipQuest.Activate(!SelectedQuest.Quest.IsFinished && (SelectedQuest.Quest.CanSkip || SelectedQuest.Quest.QuestObject.Skippable));
				SetQuestLog();
				QuestHolder.SetActive(true);
				QuestLog.SetActive(true);
			}
			else
			{
				SkipQuest.Activate(false);
				QuestHolder.SetActive(false);
				QuestLog.SetActive(false);
			}
		}

		public QuestUI CreateQuestUI(Quest quest)
		{
			QuestUI questUI = Object.Instantiate(QuestItem, QuestsHolder);
			questUI.transform.Reset();
			questUI.transform.SetAsFirstSibling();
			questUI.gameObject.SetActive(true);
			questUI.QuestSystem = this;
			questUI.Quest = quest;
			questUI.RefreshQuestUI();
			AllQuests.Add(questUI);
			return questUI;
		}

		public void SetQuestLog()
		{
			logElements.Clear();
			LogHolder.DestroyAll<Transform>(true);
			if (!(SelectedQuest != null))
			{
				return;
			}
			using (List<QuestTrigger>.Enumerator enumerator = SelectedQuest.Quest.QuestTriggers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					_003CSetQuestLog_003Ec__AnonStorey0 _003CSetQuestLog_003Ec__AnonStorey = new _003CSetQuestLog_003Ec__AnonStorey0();
					_003CSetQuestLog_003Ec__AnonStorey.qt = enumerator.Current;
					_003CSetQuestLog_003Ec__AnonStorey._0024this = this;
					if (_003CSetQuestLog_003Ec__AnonStorey.qt.Status != QuestStatus.Completed)
					{
						continue;
					}
					QuestCutSceneData questCutSceneData = Client.Instance.CanvasManager.CanvasUI.QuestCutScene.QuestCollection.CutScenes.FirstOrDefault(_003CSetQuestLog_003Ec__AnonStorey._003C_003Em__0);
					if (!(questCutSceneData != null))
					{
						continue;
					}
					if (_003CSetQuestLog_003Ec__AnonStorey.qt.Name != null && _003CSetQuestLog_003Ec__AnonStorey.qt.Name != string.Empty)
					{
						GameObject gameObject = Object.Instantiate(LogHeading, LogHolder);
						gameObject.transform.Reset();
						gameObject.GetComponent<Text>().text = _003CSetQuestLog_003Ec__AnonStorey.qt.Name;
					}
					if (Client.Instance.CanvasManager.CanvasUI.QuestCutScene.DontPlayCutScenes)
					{
						continue;
					}
					foreach (QuestCutSceneData.QuestCutSceneElement element in questCutSceneData.Elements)
					{
						if (!element.SkipInJournal)
						{
							CreateLogItem(element);
						}
					}
				}
			}
			LogBody.SetActive(logElements.Count > 0);
			NoLogAvailable.SetActive(logElements.Count <= 0);
		}

		private void CreateLogItem(QuestCutSceneData.QuestCutSceneElement element)
		{
			QuestCutSceneUI questCutSceneUI = Object.Instantiate(LogUI, LogHolder);
			questCutSceneUI.transform.Reset();
			questCutSceneUI.transform.SetAsLastSibling();
			questCutSceneUI.GetComponent<Animator>().enabled = false;
			questCutSceneUI.AutoDestroy = false;
			questCutSceneUI.gameObject.SetActive(true);
			questCutSceneUI.Icon.sprite = element.Character.CharacterImage;
			questCutSceneUI.Content.text = element.Dialogue.Replace('|', ' ');
			questCutSceneUI.CharacterName.text = element.Character.CharacterName;
			logElements.Add(questCutSceneUI);
		}

		public void SkipQuestAction()
		{
			Client.Instance.NetworkController.SendToGameServer(new SkipQuestMessage
			{
				QuestID = SelectedQuest.Quest.ID
			});
		}
	}
}
