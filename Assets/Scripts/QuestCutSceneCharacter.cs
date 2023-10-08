using UnityEngine;

[CreateAssetMenu(fileName = "Cut Scene Character", menuName = "Quests/Character")]
public class QuestCutSceneCharacter : ScriptableObject
{
	public string CharacterName;

	public Sprite CharacterImage;

	[Multiline] public string CharacterInfo;
}
