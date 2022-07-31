using System;
using UnityEngine;

[Serializable]
public class Rotate : MonoBehaviour
{
	[Tooltip("Euler rotation to apply in degrees per second.")]
	public Vector3 rotation;

	[Tooltip("Rotate in local space rather than world space.")]
	public bool local;

	public bool randomize;

	public Rotate()
	{
		rotation = Vector3.zero;
	}

	public virtual void Start()
	{
		if (randomize)
		{
			rotation = UnityEngine.Random.rotation * rotation;
		}
	}

	public virtual void Update()
	{
		base.transform.Rotate(rotation * Time.deltaTime, local ? Space.Self : Space.World);
	}
}
