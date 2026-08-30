using UnityEngine;
using ZeroGravity;
using OpenHellion.UI;

[CreateAssetMenu(menuName = "Starting point option UI Data")]
public class StartingPointOptionData : ScriptableObject
{
	public MainMenuGUI.StartingPointOption Type;

	public Sprite Background;

	[LocalizeField] public string Title;

	[LocalizeField] public string Description;

	[LocalizeField] public string DisabledDescription;
}
