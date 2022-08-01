using UnityEngine;
using UnityEngine.UI;

namespace RootMotion.Demos
{
	[RequireComponent(typeof(VRInteractionController))]
	public class VRInteractionUI : MonoBehaviour
	{
		[Header("Triggering Progress")]
		[Tooltip("The UI slider for showing interaction triggering progress.")]
		public Slider slider;

		[Tooltip("Alpha of the progress slider relative to the progress of triggering interactions.")]
		public AnimationCurve alphaToProgress;

		[Tooltip("Reference to the cursor that displays where the head is looking.")]
		public Transform cursor;

		private VRInteractionController interactionController;

		private Image[] sliderImages;

		private const string showCursorTag = "ShowCursor";

		private void Start()
		{
			interactionController = GetComponent<VRInteractionController>();
			sliderImages = slider.GetComponentsInChildren<Image>();
		}

		private void Update()
		{
			UpdateInteractionSlider();
			UpdateCursor();
		}

		private void UpdateInteractionSlider()
		{
			if (interactionController.triggerProgress <= 0f || interactionController.currentTrigger == null)
			{
				slider.gameObject.SetActive(false);
				return;
			}
			slider.gameObject.SetActive(true);
			slider.transform.rotation = interactionController.currentTrigger.transform.GetChild(0).rotation;
			slider.transform.position = interactionController.currentTrigger.transform.GetChild(0).position;
			slider.value = interactionController.triggerProgress;
			SetSliderAlpha(alphaToProgress.Evaluate(slider.value));
		}

		private void UpdateCursor()
		{
			if (!(interactionController.currentTrigger != null) || !(interactionController.currentTrigger.tag == "ShowCursor") || !(interactionController.interactionSystem.raycastHit.collider != null))
			{
				cursor.gameObject.SetActive(false);
				return;
			}
			cursor.gameObject.SetActive(true);
			cursor.transform.position = interactionController.interactionSystem.raycastHit.point;
		}

		private void SetSliderAlpha(float a)
		{
			ColorBlock colors = slider.colors;
			colors.normalColor = new Color(slider.colors.normalColor.r, slider.colors.normalColor.g, slider.colors.normalColor.b, a);
			slider.colors = colors;
			Image[] array = sliderImages;
			foreach (Image image in array)
			{
				image.color = new Color(image.color.r, image.color.g, image.color.b, a);
			}
		}
	}
}
