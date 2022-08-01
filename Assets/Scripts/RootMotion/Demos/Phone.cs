using System.Collections;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	public class Phone : MonoBehaviour
	{
		[Tooltip("The collider that is used for triggering the picking up interaction.")]
		public Collider pickUpCollider;

		[Tooltip("InteractionObject of the picking up interaction.")]
		public InteractionObject pickUpObject;

		[Tooltip("Root of the phone's display that has all the buttons parented to it.")]
		public GameObject display;

		private Transform parent;

		private void OnPickUp()
		{
			pickUpCollider.enabled = false;
			parent = base.transform.parent;
			base.transform.parent = pickUpObject.lastUsedInteractionSystem.transform;
			GetComponent<Rigidbody>().isKinematic = true;
			StartCoroutine(EnableDisplay());
		}

		private IEnumerator EnableDisplay()
		{
			yield return new WaitForSeconds(1f);
			display.SetActive(true);
		}

		private void OnButton1()
		{
		}

		private void OnButton2()
		{
			pickUpObject.lastUsedInteractionSystem.ResumeAll();
			pickUpCollider.enabled = true;
			display.SetActive(false);
		}

		private void DropPhone()
		{
			base.transform.parent = parent;
			GetComponent<Rigidbody>().isKinematic = false;
		}
	}
}
