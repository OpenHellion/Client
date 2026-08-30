using UnityEngine;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	public class InteractionCanvas : MonoBehaviour
	{
		public Text TextBox;

		private float counterTime;

		private float timeThreshold = 1f;

		private void Start()
		{
		}

		private void Update()
		{
			if (Time.time - counterTime >= timeThreshold)
			{
				counterTime = 0f;
				gameObject.SetActive(false);
			}
		}

		public void ShowCanvas(string text, float hideAfter = 1f)
		{
			timeThreshold = hideAfter;
			TextBox.text = text;
			counterTime = Time.time;
		}
	}
}
