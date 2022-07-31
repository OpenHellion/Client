using System.Collections;
using UnityEngine;

public class DisableTimed : MonoBehaviour
{
	public float DisableAfter;

	private void OnEnable()
	{
		StartCoroutine("Disable");
	}

	private IEnumerator Disable()
	{
		yield return new WaitForSeconds(DisableAfter);
		base.gameObject.SetActive(false);
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
