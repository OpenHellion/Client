using UnityEngine;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	public class WarningUI : MonoBehaviour
	{
		public enum Warnings
		{
			None = 0,
			Breach = 1,
			Fire = 2,
			Gravity = 3
		}

		public Warnings WarningType;

		public Image Icon;

		private void Start()
		{
		}
	}
}
