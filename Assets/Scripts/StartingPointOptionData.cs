using UnityEngine;
using ZeroGravity;

[CreateAssetMenu(menuName = "Starting point option UI Data")]
public class StartingPointOptionData : ScriptableObject
{
	public CanvasManager.StartingPointOption Type;

	public Sprite Background;

	[LocalizeField]
	public string Title;

	[LocalizeField]
	public string Description;

	[LocalizeField]
	public string DisabledDescription;
}
