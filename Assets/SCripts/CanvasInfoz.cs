using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CanvasInfoz : MonoBehaviour
{
	public GameObject TimeObj;

	public Text TimeText;

	private void Start()
	{
		StartCoroutine(Updatetime());
	}

	private IEnumerator Updatetime()
	{
		while (true)
		{
			DateTime today = DateTime.Now;
			TimeText.text = today.ToString("HH:mm");
			yield return new WaitForSeconds(20f);
		}
	}
}
