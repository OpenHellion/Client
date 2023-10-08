using OpenHellion.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	public class NotificationUI : MonoBehaviour
	{
		public InGameGUI.NotificationType Type;

		public Image Icon;

		public TextMeshProUGUI Content;

		private float _timer;

		public Sprite IconSprite => SpriteManager.Instance.GetSprite(Type);

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
