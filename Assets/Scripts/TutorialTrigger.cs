using System;
using UnityEngine;
using OpenHellion;
using ZeroGravity.Objects;

public class TutorialTrigger : MonoBehaviour
{
	public int Tutorial;

	private static World _world;

	private void Awake()
	{
		_world ??= GameObject.Find("/World").GetComponent<World>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.GetComponentInParent<MyPlayer>() != null)
		{
			_world.InGameGUI.ShowTutorialUI(Tutorial);
			Destroy(gameObject);
		}
	}
}
