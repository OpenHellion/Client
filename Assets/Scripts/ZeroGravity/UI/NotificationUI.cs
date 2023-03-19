using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	public class NotificationUI : MonoBehaviour
	{
		public CanvasUI.NotificationType Type;

		public Image Icon;

		public TextMeshProUGUI Content;

		private float timer;

		public float TimerTreshold = 3f;

		public Sprite IconSprite
		{
			get
			{
				return Client.Instance.SpriteManager.GetSprite(Type);
			}
		}

		private void Start()
		{
			Icon.sprite = IconSprite;
			Icon.color = Colors.Notification[Type];
		}

		public void Activate(float delay)
		{
			Invoke(nameof(ActivateWithDelay), delay);
		}

		private void ActivateWithDelay()
		{
			gameObject.SetActive(true);
		}
	}
}
