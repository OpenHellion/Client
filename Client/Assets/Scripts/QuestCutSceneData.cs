using System;
using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Network;

[CreateAssetMenu(fileName = "Cut Scene", menuName = "Quests/Cut Scene")]
public class QuestCutSceneData : ScriptableObject
{
	[Serializable]
	public class QuestCutSceneElement
	{
		public QuestCutSceneCharacter Character;

		[TextArea(4, 4)] public string Dialogue = string.Empty;

		public bool PlaySound;

		[ShowIf(nameof(PlaySound), true)] public string DialogueSound = string.Empty;

		public bool SkipInJournal;

		public List<CutSceneDependencyTask> TaskDependencyList = new List<CutSceneDependencyTask>();
	}

	[Serializable]
	public class CutSceneDependencyTask
	{
		public QuestTaskObject Task;

		public QuestStatus Status = QuestStatus.Completed;
	}

	public uint QuestID;

	public uint QuestTriggerID;

	public List<QuestCutSceneElement> Elements = new List<QuestCutSceneElement>();

	public string QuestName;

	public float Delay;
}
