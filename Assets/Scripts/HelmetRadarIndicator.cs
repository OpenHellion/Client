using UnityEngine;

public class HelmetRadarIndicator : MonoBehaviour
{
	public enum IndicatorArrow
	{
		None = 0,
		Left = 1,
		Right = 2
	}

	[SerializeField]
	private GameObject Arrow_Left;

	[SerializeField]
	private GameObject Arrow_Right;

	public bool visibleIndicator;

	public void SetArrow(IndicatorArrow ia)
	{
		if (visibleIndicator)
		{
			Arrow_Left.SetActive(ia == IndicatorArrow.Left);
			Arrow_Right.SetActive(ia == IndicatorArrow.Right);
		}
	}

	public void SetActive(bool active)
	{
		if (!visibleIndicator)
		{
			base.gameObject.SetActive(active);
			return;
		}
		if (!active)
		{
			Arrow_Left.SetActive(active);
			Arrow_Right.SetActive(active);
		}
		base.gameObject.SetActive(active);
	}
}
