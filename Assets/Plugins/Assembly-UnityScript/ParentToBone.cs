using System;
using UnityEngine;

[Serializable]
public class ParentToBone : MonoBehaviour
{
	public GameObject parentObject;

	public void Start()
	{
		if ((bool)parentObject)
		{
			transform.parent = parentObject.transform;
		}
	}

	public void Update()
	{
	}
}
