using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	[RequireComponent(typeof(Image))]
	public class TooltipOnHoverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
	{
		private GameObject Tooltip;

		private Text Heading;

		private Text Content;

		private bool show;

		private void Start()
		{
			//Tooltip = Client.Instance.InGamePanels.ToolTip;
			//Heading = Client.Instance.InGamePanels.TooltipHeading;
			//Content = Client.Instance.InGamePanels.TooltipContent;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			show = true;
			Invoke("ShowTooltip", 0.5f);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			show = false;
			HideTooltip();
		}

		private void ShowTooltip()
		{
			if (!show)
			{
				return;
			}
			string localizedField = Localization.GetLocalizedField(base.name + "Tooltip");
			if (localizedField != null)
			{
				string text = Localization.GetLocalizedField(base.name + "Title");
				if (text == null)
				{
					text = "Info";
				}
				Heading.text = text.ToUpper();
				Content.text = localizedField;
				if (Mouse.current.position.x.ReadValue() > (float)(Screen.width / 2))
				{
					Tooltip.GetComponent<RectTransform>().pivot = new Vector2(1f, 1f);
				}
				else
				{
					Tooltip.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f);
				}
				Tooltip.transform.position = (Vector3) Mouse.current.position.ReadValue();
				Tooltip.SetActive(true);
			}
			else
			{
				Tooltip.SetActive(false);
			}
		}

		private void HideTooltip()
		{
			Tooltip.SetActive(false);
		}
	}
}
