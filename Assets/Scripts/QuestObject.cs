using System;
using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Objects;

[CreateAssetMenu(fileName = "Quest", menuName = "Quests/Quest")]
public class QuestObject : ScriptableObject
{
	[Serializable]
	public class TaskBatch
	{
		public List<QuestTaskObject> TasksInBatch = new List<QuestTaskObject>();

		public QuestTriggerDependencyTpe BatchDependency = QuestTriggerDependencyTpe.All;
	}

	public QuestObject ParentQuest;

	public List<QuestObject> DependencyQuests = new List<QuestObject>();

	public uint ID;

	public List<TaskBatch> QuestTaskBatches = new List<TaskBatch>();

	public string Name;

	public string Description;

	public AchievementID Achivement;

	public bool AutoActivate;

	public Quest Quest;

	public QuestObject QuestToTrackWhenCompleted;

	public bool TrackOnPickUp;

	public bool Skippable;
}
