using UnityEngine;

namespace RootMotion.Demos
{
	public class SlowMo : MonoBehaviour
	{
		[SerializeField] private KeyCode[] keyCodes;

		[SerializeField] private bool mouse0;

		[SerializeField] private bool mouse1;

		[SerializeField] private float slowMoTimeScale = 0.3f;

		private void Update()
		{
			Time.timeScale = ((!IsSlowMotion()) ? 1f : slowMoTimeScale);
		}

		private bool IsSlowMotion()
		{
			if (mouse0 && Input.GetMouseButton(0))
			{
				return true;
			}

			if (mouse1 && Input.GetMouseButton(1))
			{
				return true;
			}

			for (int i = 0; i < keyCodes.Length; i++)
			{
				if (Input.GetKey(keyCodes[i]))
				{
					return true;
				}
			}

			return false;
		}
	}
}
