using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Objects;

[CreateAssetMenu(fileName = "QuestTask", menuName = "Quests/Quest Task")]
public class QuestTaskObject : ScriptableObject
{
	public LocationObject Location;

	public List<SceneTagObject> Tags = new List<SceneTagObject>();

	public CelestialBodyGUID Celestial;

	public string NameTag;

	[Tooltip("If it is not empty, in-world Quest Indicator will use this name.")]
	public string NameOnIndicator;

	public string DescriptionTag;

	public QuestCutSceneData CutScene;

	public GameScenes.SceneID SceneID = GameScenes.SceneID.None;

	[HideInInspector] public uint QuestID;

	[HideInInspector] public uint QuestTriggerID;

	[HideInInspector] public Quest Quest;

	[HideInInspector] public QuestTrigger QuestTrigger;

	public bool LastTask;

	public string IndicatorName
	{
		get
		{
			if (NameOnIndicator == string.Empty)
			{
				return NameTag;
			}

			return NameOnIndicator;
		}
	}

	public virtual QuestTriggerData CreateQuestTriggerData()
	{
		QuestTriggerData data = new QuestTriggerData();
		FillData(ref data);
		return data;
	}

	protected virtual void FillData(ref QuestTriggerData data)
	{
		data.Celestial = Celestial;
		data.ID = QuestTriggerID;
		data.Station = LocationObject.GetNameFromObject(Location);
		data.Tag = SceneTagObject.TagsToString(Tags);
	}
}
