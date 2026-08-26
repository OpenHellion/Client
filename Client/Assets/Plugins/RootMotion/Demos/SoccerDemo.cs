using System.Collections;
using UnityEngine;

namespace RootMotion.Demos
{
	[RequireComponent(typeof(Animator))]
	public class SoccerDemo : MonoBehaviour
	{
		private Animator animator;

		private Vector3 defaultPosition;

		private Quaternion defaultRotation;

		private void Start()
		{
			animator = GetComponent<Animator>();
			defaultPosition = base.transform.position;
			defaultRotation = base.transform.rotation;
			StartCoroutine(ResetDelayed());
		}

		private IEnumerator ResetDelayed()
		{
			while (true)
			{
				yield return new WaitForSeconds(3f);
				base.transform.position = defaultPosition;
				base.transform.rotation = defaultRotation;
				animator.CrossFade("SoccerKick", 0f, 0, 0f);
				yield return null;
			}
		}
	}
}
