using UnityEngine;
using UnityEngine.Serialization;

namespace ZeroGravity.UI
{
	public class ChatMessage : MonoBehaviour
	{
		[FormerlySerializedAs("thresholdTime")]
		public float ThresholdTime;

		private float _startTime;

		public bool IsOverriden;


		private void Update()
		{
			if (!IsOverriden)
			{
				_startTime += Time.deltaTime;
				if (_startTime >= ThresholdTime)
				{
					gameObject.SetActive(false);
				}
			}
		}

		public void ShowMessage(bool val)
		{
			IsOverriden = val;
			if (_startTime >= ThresholdTime)
			{
				gameObject.SetActive(val);
			}
		}

		public void RemoveThisMessage()
		{
			IsOverriden = true;
			Destroy(gameObject);
		}
	}
}
