using System;
using System.Collections;
using UnityEngine;

public class SunFlareEffect : MonoBehaviour
{
	public Shader FlareShader;

	public void UpdateFlareBrightness(float Value)
	{
		IEnumerator enumerator = base.transform.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Transform transform = (Transform)enumerator.Current;
				if (transform.gameObject.GetComponent<Renderer>().sharedMaterial.shader == FlareShader)
				{
					transform.gameObject.GetComponent<Renderer>().material.SetFloat("_GlobalIntensity", Value);
				}
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = enumerator as IDisposable) != null)
			{
				disposable.Dispose();
			}
		}
	}
}
