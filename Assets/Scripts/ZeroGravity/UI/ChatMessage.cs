using UnityEngine;

namespace ZeroGravity.UI
{
	public class ChatMessage : MonoBehaviour
	{
		public float thresholdTime;

		private float startTime;

		public bool IsOverriden;

		private void Start()
		{
		}

		private void Update()
		{
			if (!IsOverriden)
			{
				startTime += Time.deltaTime;
				if (startTime >= thresholdTime)
				{
					base.gameObject.SetActive(false);
				}
			}
		}

		public void ShowMessage(bool val)
		{
			IsOverriden = val;
			if (startTime >= thresholdTime)
			{
				base.gameObject.SetActive(val);
			}
		}

		public void RemoveThisMessage()
		{
			IsOverriden = true;
			Object.Destroy(base.gameObject);
		}
	}
}
