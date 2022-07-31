using System.Collections;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	public class ResetInteractionObject : MonoBehaviour
	{
		public float resetDelay = 1f;

		private Vector3 defaultPosition;

		private Quaternion defaultRotation;

		private Transform defaultParent;

		private Rigidbody r;

		private void Start()
		{
			defaultPosition = base.transform.position;
			defaultRotation = base.transform.rotation;
			defaultParent = base.transform.parent;
			r = GetComponent<Rigidbody>();
		}

		private void OnPickUp(Transform t)
		{
			StopAllCoroutines();
			StartCoroutine(ResetObject(Time.time + resetDelay));
		}

		private IEnumerator ResetObject(float resetTime)
		{
			while (Time.time < resetTime)
			{
				yield return null;
			}
			Poser poser = base.transform.parent.GetComponent<Poser>();
			if (poser != null)
			{
				poser.poseRoot = null;
				poser.weight = 0f;
			}
			base.transform.parent = defaultParent;
			base.transform.position = defaultPosition;
			base.transform.rotation = defaultRotation;
			if (r != null)
			{
				r.isKinematic = false;
			}
		}
	}
}
