using UnityEngine;
using UnityEngine.UI;

public class ChangeSliderValue : MonoBehaviour
{
	public Slider mySlider;

	public float editValue;

	public void addOnValue()
	{
		mySlider.value += editValue;
	}

	public void removeOnValue()
	{
		mySlider.value -= editValue;
	}
}
