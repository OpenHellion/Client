using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

namespace ZeroGravity.UI
{
	public class VideoProgressBar : MonoBehaviour, IDragHandler, IPointerDownHandler, IEventSystemHandler
	{
		public Camera UICamera;

		public VideoPlayer Video;

		public Image Progress;

		public void OnDrag(PointerEventData eventData)
		{
			TrySkip(eventData);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			TrySkip(eventData);
		}

		private void Update()
		{
			if (Video.frameCount != 0)
			{
				Progress.fillAmount = (float)Video.frame / (float)Video.frameCount;
			}
		}

		public void TrySkip(PointerEventData eventData)
		{
			Vector2 localPoint;
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(Progress.rectTransform, eventData.position,
				    UICamera, out localPoint))
			{
				float pct = Mathf.InverseLerp(Progress.rectTransform.rect.xMin, Progress.rectTransform.rect.xMax,
					localPoint.x);
				SkipToPercent(pct);
			}
		}

		public void SkipToPercent(float pct)
		{
			float num = (float)Video.frameCount * pct;
			Video.frame = (long)num;
		}
	}
}
