using UnityEngine;
using UnityEngine.UI;

public class PilotScreenTarget : MonoBehaviour
{
	public bool isActive;

	public string Name;

	public bool IsManeuver;

	public Image HorizontalCompass;

	public Image VerticalCompass;

	public Image OffTargetCompassHorizontal;

	public Image OffTargetCompassVertical;

	public Image OnTargetOnScreen;

	public Image OnTargetOffScreen;

	public Image OffTargetOnScreen;

	public Image OffTargetOffScreen;

	public bool DrawCompass;

	public bool DrawOnScreen;

	public bool DrawOffScreen;

	public Sprite ActiveTargetOnScreenSprite;

	public Sprite ActiveTargetOffScreenSprite;

	public Sprite ActiveTargetVerticalCompass;

	public Sprite ActiveTargetHorizontalCompass;

	public Sprite InactiveTargetOnScreenSprite;

	public Sprite InactiveTargetOffScreenSprite;

	public Sprite InactiveTargetVerticalCompass;

	public Sprite InactiveTargetHorizontalCompass;

	public Sprite ActiveOffTargetSprite;

	public Sprite InactiveOffTargetSprite;

	public Sprite ManeuverSprite;

	public Image TargetOffVector;

	public bool IsActive
	{
		get
		{
			return isActive;
		}
		set
		{
			isActive = value;
			if (value)
			{
				if (!IsManeuver)
				{
					HorizontalCompass.sprite = ActiveTargetHorizontalCompass;
					VerticalCompass.sprite = ActiveTargetVerticalCompass;
					OnTargetOnScreen.sprite = ActiveTargetOnScreenSprite;
					OnTargetOffScreen.sprite = ActiveTargetOffScreenSprite;
					OffTargetCompassHorizontal.sprite = ActiveOffTargetSprite;
					OffTargetCompassVertical.sprite = ActiveOffTargetSprite;
					OffTargetOnScreen.sprite = ActiveOffTargetSprite;
					OffTargetOffScreen.sprite = ActiveOffTargetSprite;
				}
				else
				{
					HorizontalCompass.sprite = ManeuverSprite;
					VerticalCompass.sprite = ManeuverSprite;
					OnTargetOnScreen.sprite = ManeuverSprite;
					OnTargetOffScreen.sprite = ManeuverSprite;
				}
			}
			else
			{
				HorizontalCompass.sprite = InactiveTargetHorizontalCompass;
				VerticalCompass.sprite = InactiveTargetVerticalCompass;
				OnTargetOnScreen.sprite = InactiveTargetOnScreenSprite;
				OnTargetOffScreen.sprite = InactiveTargetOffScreenSprite;
				OffTargetCompassHorizontal.sprite = InactiveOffTargetSprite;
				OffTargetCompassVertical.sprite = InactiveOffTargetSprite;
				OffTargetOnScreen.sprite = InactiveOffTargetSprite;
				OffTargetOffScreen.sprite = InactiveOffTargetSprite;
			}
		}
	}

	public void Initialize(bool compass, bool onScreen, bool offScreen)
	{
		DrawCompass = compass;
		DrawOnScreen = onScreen;
		DrawOffScreen = offScreen;
		if (!IsManeuver)
		{
			HorizontalCompass.gameObject.SetActive(compass);
			VerticalCompass.gameObject.SetActive(compass);
			OffTargetCompassHorizontal.gameObject.SetActive(compass);
			OffTargetCompassVertical.gameObject.SetActive(compass);
			OnTargetOnScreen.gameObject.SetActive(onScreen);
			OnTargetOffScreen.gameObject.SetActive(onScreen);
			OffTargetOnScreen.gameObject.SetActive(offScreen);
			OffTargetOffScreen.gameObject.SetActive(offScreen);
		}
		else
		{
			OnTargetOnScreen.gameObject.SetActive(onScreen);
			OnTargetOffScreen.gameObject.SetActive(onScreen);
			HorizontalCompass.gameObject.SetActive(compass);
			VerticalCompass.gameObject.SetActive(compass);
			HorizontalCompass.sprite = ManeuverSprite;
			VerticalCompass.sprite = ManeuverSprite;
			OnTargetOnScreen.sprite = ManeuverSprite;
			OnTargetOffScreen.sprite = ManeuverSprite;
		}
	}

	public void ResetRectTransform()
	{
		RectTransform component = GetComponent<RectTransform>();
		component.position = new Vector3(0f, 0f, 0f);
		component.localPosition = new Vector3(0f, 0f, 0f);
		component.localScale = new Vector3(1f, 1f, 1f);
		component.localEulerAngles = new Vector3(0f, 0f, 0f);
		component.offsetMax = new Vector2(0f, 0f);
		component.offsetMin = new Vector2(0f, 0f);
	}
}
