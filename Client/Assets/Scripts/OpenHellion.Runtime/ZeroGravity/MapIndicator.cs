using UnityEngine;

namespace ZeroGravity
{
	public class MapIndicator : MonoBehaviour
	{
		public enum IndicatorAction
		{
			hover = 0,
			unhover = 1,
			click = 2,
			release = 3
		}

		public Animator Animator
		{
			get { return GetComponent<Animator>(); }
		}

		public void SetIndicator(IndicatorAction action)
		{
			if (Animator != null)
			{
				switch (action)
				{
					case IndicatorAction.hover:
						Animator.SetBool("Hover", true);
						break;
					case IndicatorAction.unhover:
						Animator.SetBool("Hover", false);
						break;
					case IndicatorAction.click:
						Animator.SetBool("Click", true);
						break;
					case IndicatorAction.release:
						Animator.SetBool("Click", false);
						break;
				}
			}
		}
	}
}
