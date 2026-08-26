using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	public class TouchWalls : MonoBehaviour
	{
		[Serializable]
		public class EffectorLink
		{
			public bool enabled = true;

			public FullBodyBipedEffector effectorType;

			public InteractionObject interactionObject;

			public Transform spherecastFrom;

			public float spherecastRadius = 0.1f;

			public float minDistance = 0.3f;

			public LayerMask touchLayers;

			public float lerpSpeed = 10f;

			public float minSwitchTime = 0.2f;

			public float releaseDistance = 0.4f;

			public bool sliding;

			private Vector3 raycastDirectionLocal;

			private float raycastDistance;

			private bool inTouch;

			private RaycastHit hit = default(RaycastHit);

			private Vector3 targetPosition;

			private Quaternion targetRotation;

			private bool initiated;

			private float nextSwitchTime;

			private float speedF;

			public void Initiate(InteractionSystem interactionSystem)
			{
				raycastDirectionLocal =
					spherecastFrom.InverseTransformDirection(interactionObject.transform.position -
					                                         spherecastFrom.position);
				raycastDistance = Vector3.Distance(spherecastFrom.position, interactionObject.transform.position);
				interactionSystem.OnInteractionStart = (InteractionSystem.InteractionDelegate)Delegate.Combine(
					interactionSystem.OnInteractionStart,
					new InteractionSystem.InteractionDelegate(OnInteractionStart));
				interactionSystem.OnInteractionResume = (InteractionSystem.InteractionDelegate)Delegate.Combine(
					interactionSystem.OnInteractionResume,
					new InteractionSystem.InteractionDelegate(OnInteractionResume));
				interactionSystem.OnInteractionStop = (InteractionSystem.InteractionDelegate)Delegate.Combine(
					interactionSystem.OnInteractionStop, new InteractionSystem.InteractionDelegate(OnInteractionStop));
				hit.normal = Vector3.forward;
				targetPosition = interactionObject.transform.position;
				targetRotation = interactionObject.transform.rotation;
				initiated = true;
			}

			private bool FindWalls(Vector3 direction)
			{
				if (!enabled)
				{
					return false;
				}

				bool result = Physics.SphereCast(spherecastFrom.position, spherecastRadius, direction, out hit,
					raycastDistance, touchLayers);
				if (hit.distance < minDistance)
				{
					result = false;
				}

				return result;
			}

			public void Update(InteractionSystem interactionSystem)
			{
				if (!initiated)
				{
					return;
				}

				Vector3 vector = spherecastFrom.TransformDirection(raycastDirectionLocal);
				hit.point = spherecastFrom.position + vector;
				bool flag = FindWalls(vector);
				if (!inTouch)
				{
					if (flag && Time.time > nextSwitchTime)
					{
						interactionObject.transform.parent = null;
						interactionSystem.StartInteraction(effectorType, interactionObject, true);
						nextSwitchTime = Time.time + minSwitchTime / interactionSystem.speed;
						targetPosition = hit.point;
						targetRotation = Quaternion.LookRotation(-hit.normal);
						interactionObject.transform.position = targetPosition;
						interactionObject.transform.rotation = targetRotation;
					}
				}
				else
				{
					if (!flag)
					{
						StopTouch(interactionSystem);
					}
					else if (!interactionSystem.IsPaused(effectorType) || sliding)
					{
						targetPosition = hit.point;
						targetRotation = Quaternion.LookRotation(-hit.normal);
					}

					if (Vector3.Distance(interactionObject.transform.position, hit.point) > releaseDistance)
					{
						if (flag)
						{
							targetPosition = hit.point;
							targetRotation = Quaternion.LookRotation(-hit.normal);
						}
						else
						{
							StopTouch(interactionSystem);
						}
					}
				}

				float b = ((inTouch && (!interactionSystem.IsPaused(effectorType) ||
				                        !(interactionObject.transform.position == targetPosition)))
					? 1f
					: 0f);
				speedF = Mathf.Lerp(speedF, b, Time.deltaTime * 3f * interactionSystem.speed);
				float t = Time.deltaTime * lerpSpeed * speedF * interactionSystem.speed;
				interactionObject.transform.position =
					Vector3.Lerp(interactionObject.transform.position, targetPosition, t);
				interactionObject.transform.rotation =
					Quaternion.Slerp(interactionObject.transform.rotation, targetRotation, t);
			}

			private void StopTouch(InteractionSystem interactionSystem)
			{
				interactionObject.transform.parent = interactionSystem.transform;
				nextSwitchTime = Time.time + minSwitchTime / interactionSystem.speed;
				if (interactionSystem.IsPaused(effectorType))
				{
					interactionSystem.ResumeInteraction(effectorType);
					return;
				}

				speedF = 0f;
				targetPosition = hit.point;
				targetRotation = Quaternion.LookRotation(-hit.normal);
			}

			private void OnInteractionStart(FullBodyBipedEffector effectorType, InteractionObject interactionObject)
			{
				if (effectorType == this.effectorType && !(interactionObject != this.interactionObject))
				{
					inTouch = true;
				}
			}

			private void OnInteractionResume(FullBodyBipedEffector effectorType, InteractionObject interactionObject)
			{
				if (effectorType == this.effectorType && !(interactionObject != this.interactionObject))
				{
					inTouch = false;
				}
			}

			private void OnInteractionStop(FullBodyBipedEffector effectorType, InteractionObject interactionObject)
			{
				if (effectorType == this.effectorType && !(interactionObject != this.interactionObject))
				{
					inTouch = false;
				}
			}

			public void Destroy(InteractionSystem interactionSystem)
			{
				if (initiated)
				{
					interactionSystem.OnInteractionStart = (InteractionSystem.InteractionDelegate)Delegate.Remove(
						interactionSystem.OnInteractionStart,
						new InteractionSystem.InteractionDelegate(OnInteractionStart));
					interactionSystem.OnInteractionResume = (InteractionSystem.InteractionDelegate)Delegate.Remove(
						interactionSystem.OnInteractionResume,
						new InteractionSystem.InteractionDelegate(OnInteractionResume));
					interactionSystem.OnInteractionStop = (InteractionSystem.InteractionDelegate)Delegate.Remove(
						interactionSystem.OnInteractionStop,
						new InteractionSystem.InteractionDelegate(OnInteractionStop));
				}
			}
		}

		public InteractionSystem interactionSystem;

		public EffectorLink[] effectorLinks;

		private void Start()
		{
			EffectorLink[] array = effectorLinks;
			foreach (EffectorLink effectorLink in array)
			{
				effectorLink.Initiate(interactionSystem);
			}
		}

		private void FixedUpdate()
		{
			for (int i = 0; i < effectorLinks.Length; i++)
			{
				effectorLinks[i].Update(interactionSystem);
			}
		}

		private void OnDestroy()
		{
			if (interactionSystem != null)
			{
				for (int i = 0; i < effectorLinks.Length; i++)
				{
					effectorLinks[i].Destroy(interactionSystem);
				}
			}
		}
	}
}
