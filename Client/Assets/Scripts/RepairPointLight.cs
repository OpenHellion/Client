using UnityEngine;

[ExecuteInEditMode]
public class RepairPointLight : MonoBehaviour
{
	public Light Light;

	public AnimationCurve Curve;

	public AnimationCurve IntensityCurve;

	public float JitterLength;

	public float JitterIntensity;

	private void Awake()
	{
		Light = GetComponent<Light>();
	}
}
