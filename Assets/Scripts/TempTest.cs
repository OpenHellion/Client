using UnityEngine;

public class TempTest : MonoBehaviour
{
	[Range(1000f, 4.399461E+11f)] public double DistanceFromSun;

	public bool IsExposedToSunlight;

	public double heatCollectionFactor = 10000.0;

	public double heatDissipationFactor = 20.0;

	public double mass = 1142.0;

	public double currentTemperature = 25.0;

	private const double baseSunHeatTransferPerSec = 9.4E+21;

	public bool reset;

	private void Update()
	{
		if (reset)
		{
			currentTemperature = 0.0;
			reset = !reset;
		}

		double num = 0.0;
		double num2 = 0.0;
		if (IsExposedToSunlight)
		{
			num = 9.4E+21 * heatCollectionFactor / mass / (DistanceFromSun * DistanceFromSun);
		}

		num2 = heatDissipationFactor / mass * (currentTemperature + 273.15);
		currentTemperature = (float)(currentTemperature + (num - num2) * (double)Time.deltaTime);
	}
}
