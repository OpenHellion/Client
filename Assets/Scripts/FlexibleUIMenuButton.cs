using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class FlexibleUIMenuButton : FlexibleUI
{
	public enum ButtonType
	{
		Default = 0,
		Icon = 1
	}

	private Image image;

	private Image hover;

	private Image icon;

	private Button button;

	public ButtonType buttonType;

	protected override void OnSkinUI()
	{
		base.OnSkinUI();
		image = GetComponent<Image>();
		hover = base.transform.Find("Hover").GetComponent<Image>();
		icon = base.transform.Find("Icon").GetComponent<Image>();
		button = GetComponent<Button>();
		button.transition = Selectable.Transition.ColorTint;
		button.targetGraphic = hover;
		ColorBlock colors = button.colors;
		colors.normalColor = skinData.normal;
		colors.highlightedColor = skinData.highlight;
		colors.pressedColor = skinData.pressed;
		colors.disabledColor = skinData.disabled;
		button.colors = colors;
		image.sprite = skinData.menuButtonSprite;
		image.type = Image.Type.Sliced;
		switch (buttonType)
		{
			case ButtonType.Default:
				image.color = skinData.normal;
				hover.color = skinData.defaultColor;
				icon.gameObject.SetActive(false);
				image.color = new Color(1f, 1f, 1f, 0.4f);
				break;
			case ButtonType.Icon:
				image.color = skinData.defaultColor;
				hover.color = skinData.iconColor;
				icon.gameObject.SetActive(true);
				break;
		}
	}
}
