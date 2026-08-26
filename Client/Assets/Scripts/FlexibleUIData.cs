using UnityEngine;

[CreateAssetMenu(menuName = "Flexible UI Data")]
public class FlexibleUIData : ScriptableObject
{
	public Sprite buttonSprite;

	public Sprite menuButtonSprite;

	[Title("TRANSITION COLORS")] public Color normal;

	public Color highlight;

	public Color pressed;

	public Color disabled;

	[Title("TYPE COLORS")] public Color defaultColor;

	public Color iconColor;

	public Color confirmColor;

	public Color declineColor;

	public Color actionColor;
}
