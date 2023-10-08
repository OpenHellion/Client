using System.Collections.Generic;
using System.Linq;
using OpenHellion;
using UnityEngine;
using UnityEngine.Serialization;
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

	private Quest _currentQuest;

	public Animator Animator;

	public Text QuestName;

	public Text AltQuestName;

	public Text AltTriggerName;

	[FormerlySerializedAs("_worldState")] [SerializeField] private World _world;

	public void AddTasks()
	{
		foreach (TaskUI value in Tasks.Values)
		{
			if (value != null)
			{
				Destroy(value.gameObject);
			}
		}

		Tasks.Clear();
		foreach (QuestTrigger questTrigger in _currentQuest.QuestTriggers)
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

		QuestName.text = Localization.GetLocalizedField(_currentQuest.Name, useDefault: true).ToUpper();
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

		HideActiveTasks();
		if (_currentQuest.QuestObject.QuestToTrackWhenCompleted != null)
		{
			Quest quest = _world.Quests.FirstOrDefault((Quest m) =>
				m.ID == _currentQuest.QuestObject.QuestToTrackWhenCompleted.ID);
			_world.InGameGUI.SetCurrentTrackingQuest(quest);
		}
	}

	public void ShowTasks()
	{
		if (_currentQuest.Status != QuestStatus.Completed)
		{
			gameObject.SetActive(value: true);
			Animator.SetBool("Close", value: false);
			AddTasks();
		}
	}

	public void ShowTasks(float delay)
	{
		_currentQuest = _world.InGameGUI.CurrentTrackingQuest;
		Invoke(nameof(ShowTasks), delay);
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
