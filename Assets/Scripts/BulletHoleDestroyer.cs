using System.Collections;
using UnityEngine;

public class BulletHoleDestroyer : MonoBehaviour
{
	public float DestroyTime;

	public float FadeTime;

	private float time;

	private float lerpStep;

	private bool fadeCalled;

	private void Start()
	{
		time = Time.time;
		lerpStep = 1f / FadeTime;
	}

	private void Update()
	{
		if (Time.time - time > DestroyTime && !fadeCalled)
		{
			StartCoroutine(FadeBulletHole());
			fadeCalled = true;
		}
	}

	private IEnumerator FadeBulletHole()
	{
		Material mat = GetComponent<Renderer>().material;
		Color startColor = mat.color;
		Color endColor = new Color(mat.color.r, mat.color.g, mat.color.b, 0f);
		float lerpHelper = 0f;
		while (mat.color.a > 0.1f)
		{
			mat.color = Color.Lerp(startColor, endColor, lerpHelper);
			yield return new WaitForEndOfFrame();
			lerpHelper += lerpStep * Time.deltaTime;
		}
		Destroy(gameObject);
	}
}
