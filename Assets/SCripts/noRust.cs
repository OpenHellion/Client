using System;
using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class noRust : MonoBehaviour
{
	public float rust = 0.02f;

	private void Update()
	{
		NoRust(base.transform);
	}

	private void NoRust(Transform parent)
	{
		IEnumerator enumerator = parent.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Transform transform = (Transform)enumerator.Current;
				if (transform.gameObject.GetComponent<MeshRenderer>() != null)
				{
					transform.gameObject.GetComponent<MeshRenderer>().material.SetFloat("_RustAmount", rust);
				}
				NoRust(transform);
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
