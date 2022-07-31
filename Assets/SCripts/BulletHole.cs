using System.Collections;
using UnityEngine;

public class BulletHole : MonoBehaviour
{
	public float lifeTime = 15f;

	private void Start()
	{
		StartCoroutine(DestroyDelayed());
	}

	private IEnumerator DestroyDelayed()
	{
		yield return new WaitForSeconds(lifeTime);
		Object.Destroy(base.gameObject);
	}
}
