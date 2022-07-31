using UnityEngine;
using UnityEngine.UI;
using ZeroGravity;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class FlexibleUIButton : FlexibleUI
{
	public enum ButtonType
	{
		Default = 0,
		Confirm = 1,
		Decline = 2,
		Action = 3
	}

	private Image image;

	private Image hover;

	private Text text;

	private Button button;

	public ButtonType buttonType;

	protected override void OnSkinUI()
	{
		base.OnSkinUI();
		image = GetComponent<Image>();
		hover = base.transform.Find("Hover").GetComponent<Image>();
		button = GetComponent<Button>();
		button.transition = Selectable.Transition.ColorTint;
		button.targetGraphic = hover;
		ColorBlock colors = button.colors;
		colors.normalColor = skinData.normal;
		colors.highlightedColor = skinData.highlight;
		colors.pressedColor = skinData.pressed;
		colors.disabledColor = skinData.disabled;
		button.colors = colors;
		image.sprite = skinData.buttonSprite;
		image.type = Image.Type.Sliced;
		switch (buttonType)
		{
		case ButtonType.Default:
			hover.color = skinData.defaultColor;
			break;
		case ButtonType.Confirm:
			hover.color = skinData.confirmColor;
			text = base.transform.Find("Text").GetComponent<Text>();
			text.text = Localization.Confirm.ToUpper();
			break;
		case ButtonType.Decline:
			hover.color = skinData.declineColor;
			text = base.transform.Find("Text").GetComponent<Text>();
			text.text = Localization.Cancel.ToUpper();
			break;
		case ButtonType.Action:
			hover.color = skinData.actionColor;
			text = GetComponentInChildren<Text>();
			text.resizeTextMaxSize = 26;
			image.color = new Color(1f, 1f, 1f, 0.1f);
			break;
		}
	}
}
