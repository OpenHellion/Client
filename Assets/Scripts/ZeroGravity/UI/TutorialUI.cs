using UnityEngine;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	public class TutorialUI : MonoBehaviour
	{
		public Text MessageText;

		private float timer;

		private float timerTreshold = 20f;

		private void Update()
		{
			timer += Time.deltaTime;
			if (timer > timerTreshold)
			{
				Destroy(gameObject);
			}
		}
	}
}
