using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FillOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	public float fillSpeed = 0.3f;

	public float targetValue = 1f;

	public Image bar;

	private float helper;

	public bool IsFilling;

	private void Start()
	{
	}

	private void Update()
	{
		if (IsFilling)
		{
			if (bar.fillAmount != targetValue)
			{
				ImageFill(bar, targetValue, fillSpeed);
			}
			else
			{
				IsFilling = false;
			}
		}
	}

	private void ImageFill(Image bar, float target, float speed)
	{
		helper += speed * Time.deltaTime;
		bar.fillAmount = Mathf.Clamp01(Mathf.Lerp(bar.fillAmount, targetValue, helper));
		if (helper > 1f)
		{
			helper = 0f;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		IsFilling = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		IsFilling = false;
		bar.fillAmount = 0f;
		helper = 0f;
	}
}
