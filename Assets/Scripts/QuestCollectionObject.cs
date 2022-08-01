using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Quest Collection", menuName = "Quests/Quest Collection")]
public class QuestCollectionObject : ScriptableObject
{
	public List<QuestObject> Quests;

	public List<QuestTaskObject> Tasks;

	public List<QuestCutSceneData> CutScenes;
}
