using System;
using OpenHellion.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace ZeroGravity.UI
{
	[RequireComponent(typeof(Image))]
	[Obsolete]
	public class TooltipOnHoverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		private GameObject _tooltip;

		private TMP_Text _heading;

		private TMP_Text _content;

		private bool _show;

		private void Awake()
		{
			var inWorldPanels = GameObject.Find("/InWorldPanels").GetComponent<InWorldPanels>();
			if (inWorldPanels is null)
			{
				Debug.LogWarning("Tried to get InWorldPanels tooltip, but it couldn't be found. Destroying object. " + gameObject.name);
				Destroy(gameObject);
				return;
			}

			_tooltip = inWorldPanels.ToolTip;
			_heading = inWorldPanels.TooltipHeading;
			_content = inWorldPanels.TooltipContent;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			_show = true;
			Invoke(nameof(ShowTooltip), 0.5f);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			_show = false;
			HideTooltip();
		}

		private void ShowTooltip()
		{
			if (!_show)
			{
				return;
			}
			string localizedField = Localization.GetLocalizedField(name + "Tooltip");
			if (localizedField != null)
			{
				string text = Localization.GetLocalizedField(name + "Title");
				if (text == null)
				{
					text = "Info";
				}
				_heading.text = text.ToUpper();
				_content.text = localizedField;
				if (Mouse.current.position.x.ReadValue() > Screen.width / 2)
				{
					_tooltip.GetComponent<RectTransform>().pivot = new Vector2(1f, 1f);
				}
				else
				{
					_tooltip.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f);
				}
				_tooltip.transform.position = Mouse.current.position.ReadValue();
				_tooltip.SetActive(true);
			}
			else
			{
				_tooltip.SetActive(false);
			}
		}

		private void HideTooltip()
		{
			_tooltip.SetActive(false);
		}
	}
}
