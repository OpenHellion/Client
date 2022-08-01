using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	public abstract class PickUp2Handed : MonoBehaviour
	{
		[SerializeField]
		private int GUIspace;

		public InteractionSystem interactionSystem;

		public InteractionObject obj;

		public Transform pivot;

		public Transform holdPoint;

		public float pickUpTime = 0.3f;

		private float holdWeight;

		private float holdWeightVel;

		private Vector3 pickUpPosition;

		private Quaternion pickUpRotation;

		private bool holding
		{
			get
			{
				return interactionSystem.IsPaused(FullBodyBipedEffector.LeftHand);
			}
		}

		private void OnGUI()
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(GUIspace);
			if (!holding)
			{
				if (GUILayout.Button("Pick Up " + obj.name))
				{
					interactionSystem.StartInteraction(FullBodyBipedEffector.LeftHand, obj, false);
					interactionSystem.StartInteraction(FullBodyBipedEffector.RightHand, obj, false);
				}
			}
			else if (GUILayout.Button("Drop " + obj.name))
			{
				interactionSystem.ResumeAll();
			}
			GUILayout.EndHorizontal();
		}

		protected abstract void RotatePivot();

		private void Start()
		{
			InteractionSystem obj = interactionSystem;
			obj.OnInteractionStart = (InteractionSystem.InteractionDelegate)Delegate.Combine(obj.OnInteractionStart, new InteractionSystem.InteractionDelegate(OnStart));
			InteractionSystem obj2 = interactionSystem;
			obj2.OnInteractionPause = (InteractionSystem.InteractionDelegate)Delegate.Combine(obj2.OnInteractionPause, new InteractionSystem.InteractionDelegate(OnPause));
			InteractionSystem obj3 = interactionSystem;
			obj3.OnInteractionResume = (InteractionSystem.InteractionDelegate)Delegate.Combine(obj3.OnInteractionResume, new InteractionSystem.InteractionDelegate(OnDrop));
		}

		private void OnPause(FullBodyBipedEffector effectorType, InteractionObject interactionObject)
		{
			if (effectorType == FullBodyBipedEffector.LeftHand && !(interactionObject != obj))
			{
				obj.transform.parent = interactionSystem.transform;
				Rigidbody component = obj.GetComponent<Rigidbody>();
				if (component != null)
				{
					component.isKinematic = true;
				}
				pickUpPosition = obj.transform.position;
				pickUpRotation = obj.transform.rotation;
				holdWeight = 0f;
				holdWeightVel = 0f;
			}
		}

		private void OnStart(FullBodyBipedEffector effectorType, InteractionObject interactionObject)
		{
			if (effectorType == FullBodyBipedEffector.LeftHand && !(interactionObject != obj))
			{
				RotatePivot();
				holdPoint.rotation = obj.transform.rotation;
			}
		}

		private void OnDrop(FullBodyBipedEffector effectorType, InteractionObject interactionObject)
		{
			if (effectorType == FullBodyBipedEffector.LeftHand && !(interactionObject != obj))
			{
				obj.transform.parent = null;
				if (obj.GetComponent<Rigidbody>() != null)
				{
					obj.GetComponent<Rigidbody>().isKinematic = false;
				}
			}
		}

		private void LateUpdate()
		{
			if (holding)
			{
				holdWeight = Mathf.SmoothDamp(holdWeight, 1f, ref holdWeightVel, pickUpTime);
				obj.transform.position = Vector3.Lerp(pickUpPosition, holdPoint.position, holdWeight);
				obj.transform.rotation = Quaternion.Lerp(pickUpRotation, holdPoint.rotation, holdWeight);
			}
		}

		private void OnDestroy()
		{
			if (!(interactionSystem == null))
			{
				InteractionSystem obj = interactionSystem;
				obj.OnInteractionStart = (InteractionSystem.InteractionDelegate)Delegate.Remove(obj.OnInteractionStart, new InteractionSystem.InteractionDelegate(OnStart));
				InteractionSystem obj2 = interactionSystem;
				obj2.OnInteractionPause = (InteractionSystem.InteractionDelegate)Delegate.Remove(obj2.OnInteractionPause, new InteractionSystem.InteractionDelegate(OnPause));
				InteractionSystem obj3 = interactionSystem;
				obj3.OnInteractionResume = (InteractionSystem.InteractionDelegate)Delegate.Remove(obj3.OnInteractionResume, new InteractionSystem.InteractionDelegate(OnDrop));
			}
		}
	}
}
