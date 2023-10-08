using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using ZeroGravity.Data;
using ZeroGravity.Objects;

[CreateAssetMenu(fileName = "Quest", menuName = "Quests/Quest")]
public class QuestObject : ScriptableObject
{
	[Serializable]
	public class TaskBatch
	{
		public List<QuestTaskObject> TasksInBatch = new List<QuestTaskObject>();

		[FormerlySerializedAs("BatchDependency"),
		 Tooltip("Shoud any or all tasks have to be completed for the batch to finish?")]
		public QuestTriggerDependencyTpe RequireTasks = QuestTriggerDependencyTpe.All;
	}

	public QuestObject ParentQuest;

	[ReorderableList(ListStyle.Boxed, "Quest", Foldable = true)]
	public List<QuestObject> DependencyQuests = new List<QuestObject>();

	public uint ID;

	[ReorderableList(ListStyle.Boxed, "Batch", Foldable = true)]
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
