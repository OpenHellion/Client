using UnityEngine;

[CreateAssetMenu(fileName = "LogObject", menuName = "Helpers/LogObject")]
public class LogObject : ScriptableObject
{
	[Tooltip("Max characters = 60")]
	public string Title;

	public QuestCutSceneCharacter Character;

	[Tooltip("Max characters = 4000")]
	[TextArea]
	public string LogText;

	public string LogDate;

	public string LogTitle
	{
		get
		{
			if (Title == string.Empty)
			{
				return base.name;
			}
			return Title;
		}
	}
}
