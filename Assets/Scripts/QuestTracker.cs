using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity;
using ZeroGravity.Data;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.UI;

public class QuestTracker : MonoBehaviour
{
	public Dictionary<QuestTrigger, TaskUI> Tasks = new Dictionary<QuestTrigger, TaskUI>();

	public TaskUI TaskUIPrefab;

	public Transform Content;

	public Quest CurrentQuest;

	public Animator Animator;

	public Text QuestName;

	public Text AltQuestName;

	public Text AltTriggerName;

	public void AddTasks()
	{
		foreach (TaskUI value in Tasks.Values)
		{
			if (value != null)
			{
				Object.Destroy(value.gameObject);
			}
		}
		Tasks.Clear();
		foreach (QuestTrigger questTrigger in CurrentQuest.QuestTriggers)
		{
			if (questTrigger.Type != QuestTriggerType.Activate)
			{
				TaskUI taskUI = Object.Instantiate(TaskUIPrefab, Content);
				taskUI.transform.Reset();
				taskUI.transform.SetAsLastSibling();
				taskUI.Name.text = questTrigger.Name;
				taskUI.gameObject.SetActive(questTrigger.Status == QuestStatus.Active);
				taskUI.QuestTracker = this;
				Tasks.Add(questTrigger, taskUI);
			}
		}
		QuestName.text = Localization.GetLocalizedField(CurrentQuest.Name, useDefault: true).ToUpper();
	}

	public void RefreshTasks()
	{
		foreach (QuestTrigger key in Tasks.Keys)
		{
			if (key.Status == QuestStatus.Completed)
			{
				Tasks[key].Animator.SetBool("Completed", value: true);
			}
		}
	}

	public void RefreshBatch()
	{
		bool flag = true;
		foreach (QuestTrigger key in Tasks.Keys)
		{
			if (key.Status == QuestStatus.Active)
			{
				Tasks[key].gameObject.SetActive(value: true);
				flag = false;
			}
		}
		if (!flag)
		{
			return;
		}
		Quest quest = null;
		HideActiveTasks();
		if (CurrentQuest.QuestObject.QuestToTrackWhenCompleted != null)
		{
			quest = Client.Instance.Quests.FirstOrDefault((Quest m) => m.ID == CurrentQuest.QuestObject.QuestToTrackWhenCompleted.ID);
			Client.Instance.CanvasManager.CanvasUI.SetCurrentTrackingQuest(quest);
		}
	}

	public void ShowTasks()
	{
		if (CurrentQuest.Status != QuestStatus.Completed)
		{
			base.gameObject.SetActive(value: true);
			Animator.SetBool("Close", value: false);
			AddTasks();
		}
	}

	public void ShowTasks(float delay)
	{
		CurrentQuest = Client.Instance.CanvasManager.CanvasUI.CurrentTrackingQuest;
		Invoke("ShowTasks", delay);
	}

	public void HideActiveTasks()
	{
		foreach (TaskUI value in Tasks.Values)
		{
			value.Animator.SetTrigger("Close");
		}
		Animator.SetBool("Close", value: true);
	}

	public void TriggerNonTrackedTask(QuestTrigger trigger)
	{
		AltQuestName.text = trigger.Quest.Name;
		AltTriggerName.text = trigger.Name;
		Animator.SetTrigger("Switch");
	}
}
