using UnityEngine;
using ZeroGravity.Data;

[CreateAssetMenu(fileName = "QuestTask", menuName = "Quests/Quest Task - Spawn Station")]
public class QuestTaskObjectSpawnStation : QuestTaskObject
{
	[Space(10f)] public string SpawnRuleName;

	public override QuestTriggerData CreateQuestTriggerData()
	{
		QuestTriggerData data = new QuestTriggerData();
		FillData(ref data);
		return data;
	}

	protected override void FillData(ref QuestTriggerData data)
	{
		base.FillData(ref data);
		data.SpawnRuleName = SpawnRuleName;
	}
}
