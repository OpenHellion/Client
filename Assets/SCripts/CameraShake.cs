using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
	public enum ShakeType
	{
		Warp = 0,
		Engine = 1,
		Collision = 2
	}

	[Serializable]
	public class ShakeVariants
	{
		public float shakeDuration;

		public float shakeSpeed;

		public float shakeMagnitude;

		public AnimationCurve curve;

		public float xMultiplier = 1f;

		public float yMultiplier = 1f;

		public float rotXMultiplier = 10f;

		public float rotYMultiplier = 10f;

		public float rotZMultiplier = 10f;
	}

	public List<ShakeVariants> shakeVariants;

	public ShakeType testShakeType;

	public bool indefiniteShake;

	public AnimationCurve flatCurve;

	private float currentMainMultiplier;

	private float targetMainMultiplier;

	public bool test;

	private bool needToStop;

	[ContextMenuItem("Shake", "ShkClick")]
	public float Duration = 2f;

	public float Amplitude = 2f;

	public float Frequency = 2f;

	public bool Translate = true;

	public bool Rotate = true;

	public float RotationMultiplier = 0.1f;

	public AnimationCurve Blend = new AnimationCurve();

	public List<ParticleSystem> Sparks;

	public bool UseSparks;

	public float TargetMainMultiplier
	{
		get
		{
			return targetMainMultiplier;
		}
		set
		{
			targetMainMultiplier = value;
		}
	}

	private void Update()
	{
		if (test)
		{
			test = false;
			StartCoroutine(Shake(shakeVariants[(int)testShakeType].shakeSpeed, shakeVariants[(int)testShakeType].shakeDuration, shakeVariants[(int)testShakeType].shakeMagnitude, shakeVariants[(int)testShakeType].xMultiplier, shakeVariants[(int)testShakeType].yMultiplier, shakeVariants[(int)testShakeType].rotXMultiplier, shakeVariants[(int)testShakeType].rotYMultiplier, shakeVariants[(int)testShakeType].rotZMultiplier, shakeVariants[(int)testShakeType].curve, indefiniteShake));
		}
		currentMainMultiplier = Mathf.Lerp(currentMainMultiplier, TargetMainMultiplier, Time.deltaTime);
		if (needToStop)
		{
			StopShake();
		}
	}

	public void ShakeCamera(ShakeType shakeType, bool infiniteShake)
	{
		if (!indefiniteShake)
		{
			if (shakeType == ShakeType.Warp)
			{
				targetMainMultiplier = 1f;
			}
			needToStop = false;
			StartCoroutine(Shake(shakeVariants[(int)shakeType].shakeSpeed, shakeVariants[(int)shakeType].shakeDuration, shakeVariants[(int)shakeType].shakeMagnitude, shakeVariants[(int)shakeType].xMultiplier, shakeVariants[(int)shakeType].yMultiplier, shakeVariants[(int)shakeType].rotXMultiplier, shakeVariants[(int)shakeType].rotYMultiplier, shakeVariants[(int)shakeType].rotZMultiplier, shakeVariants[(int)shakeType].curve, infiniteShake));
		}
	}

	public void Stop()
	{
		needToStop = true;
	}

	private void StopShake()
	{
		if (TargetMainMultiplier > 0f)
		{
			TargetMainMultiplier -= Time.deltaTime;
			return;
		}
		indefiniteShake = false;
		TargetMainMultiplier = 0f;
		needToStop = false;
	}

	private IEnumerator Shake(float speed, float duration, float magnitude, float xMultiplier, float yMultiplier, float rotXMultiplier, float rotYMultiplier, float rotZMultiplier, AnimationCurve damper = null, bool infSHake = false)
	{
		float elapsed = 0f;
		indefiniteShake = infSHake;
		if (indefiniteShake)
		{
			duration = -1f;
			damper = flatCurve;
		}
		while (indefiniteShake)
		{
			elapsed += Time.deltaTime;
			float damperedMag = ((damper == null) ? magnitude : (damper.Evaluate(elapsed / duration) * magnitude));
			float x = (Mathf.PerlinNoise(Time.time * speed + 5f, 0f) * damperedMag - damperedMag / 2f) * xMultiplier * currentMainMultiplier;
			float y = (Mathf.PerlinNoise(0f, Time.time * speed + 2.54f) * damperedMag - damperedMag / 2f) * yMultiplier * currentMainMultiplier;
			float rotX = (Mathf.PerlinNoise(Time.time * speed + 18f, 0f) * damperedMag - damperedMag / 2f) * rotXMultiplier * currentMainMultiplier;
			float rotY = (Mathf.PerlinNoise(0f, Time.time * speed + 100f) * damperedMag - damperedMag / 2f) * rotYMultiplier * currentMainMultiplier;
			float rotZ = (Mathf.PerlinNoise(Time.time * speed + 3.14f, 0f) * damperedMag - damperedMag / 2f) * rotZMultiplier * currentMainMultiplier;
			base.transform.localPosition = new Vector3(x, y, 0f);
			base.transform.localRotation = Quaternion.Euler(new Vector3(rotX, rotY, rotZ));
			if (duration > 0f && elapsed > duration)
			{
				break;
			}
			yield return new WaitForEndOfFrame();
		}
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = Quaternion.identity;
	}

	public void ShkClick()
	{
		CamShake(Duration, Amplitude, RotationMultiplier, Frequency, UseSparks);
	}

	public void CamShake(float duration, float amplitude, float rotationMultiplier, float frequency, bool useSparks = false)
	{
		StartCoroutine(CamShakeCoroutine(duration, amplitude, rotationMultiplier, frequency, useSparks));
	}

	private IEnumerator CamShakeCoroutine(float duration, float amplitude, float rotationMultiplier, float frequency, bool useSparks = false)
	{
		float time = 0f;
		if (useSparks)
		{
			int index = UnityEngine.Random.Range(0, Sparks.Count);
			Sparks[index].Play();
		}
		while (time < duration)
		{
			float blend = Blend.Evaluate(time / duration);
			base.transform.localPosition = new Vector3((Mathf.PerlinNoise(time * frequency, 0f) - 0.5f) * amplitude * blend, (Mathf.PerlinNoise(time * frequency, 1f) - 0.5f) * amplitude * blend, (Mathf.PerlinNoise(time * frequency, 2f) - 0.5f) * amplitude * blend);
			if (rotationMultiplier > 0f)
			{
				base.transform.localRotation = Quaternion.Euler(new Vector3((Mathf.PerlinNoise(time * frequency, 3f) - 0.5f) * amplitude * rotationMultiplier * blend, (Mathf.PerlinNoise(time * frequency, 4f) - 0.5f) * amplitude * rotationMultiplier * blend, (Mathf.PerlinNoise(time * frequency, 5f) - 0.5f) * amplitude * rotationMultiplier * blend));
			}
			time += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = Quaternion.identity;
	}
}
