using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZeroGravity.Data;

namespace ZeroGravity.UI
{
	public class GlossaryElementUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler,
		IEventSystemHandler
	{
		public AbstractGlossaryElement Element;

		[Title("UI")] public GlossaryUI Screen;

		public Image Icon;

		public GameObject Selected;

		public ItemCategory SubCategory;

		public void OnPointerEnter(PointerEventData eventData)
		{
			gameObject.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			Screen.SelectElement(this);
		}

		public void SetIcon()
		{
			Icon.sprite = Element.Icon;
		}

		private void Update()
		{
			Selected.Activate(Screen.Current != null && Screen.Current == this);
		}
	}
}
