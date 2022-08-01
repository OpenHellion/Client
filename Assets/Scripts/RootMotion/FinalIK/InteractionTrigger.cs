using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[HelpURL("https://www.youtube.com/watch?v=-TDZpNjt2mk&index=15&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6")]
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Interaction System/Interaction Trigger")]
	public class InteractionTrigger : MonoBehaviour
	{
		[Serializable]
		public class CharacterPosition
		{
			[Tooltip("If false, will not care where the character stands, as long as it is in contact with the trigger collider.")]
			public bool use;

			[Tooltip("The offset of the character's position relative to the trigger in XZ plane. Y position of the character is unlimited as long as it is contact with the collider.")]
			public Vector2 offset;

			[Tooltip("Angle offset from the default forward direction.")]
			[Range(-180f, 180f)]
			public float angleOffset;

			[Tooltip("Max angular offset of the character's forward from the direction of this trigger.")]
			[Range(0f, 180f)]
			public float maxAngle = 45f;

			[Tooltip("Max offset of the character's position from this range's center.")]
			public float radius = 0.5f;

			[Tooltip("If true, will rotate the trigger around it's Y axis relative to the position of the character, so the object can be interacted with from all sides.")]
			public bool orbit;

			[Tooltip("Fixes the Y axis of the trigger to Vector3.up. This makes the trigger symmetrical relative to the object. For example a gun will be able to be picked up from the same direction relative to the barrel no matter which side the gun is resting on.")]
			public bool fixYAxis;

			public Vector3 offset3D
			{
				get
				{
					return new Vector3(offset.x, 0f, offset.y);
				}
			}

			public Vector3 direction3D
			{
				get
				{
					return Quaternion.AngleAxis(angleOffset, Vector3.up) * Vector3.forward;
				}
			}

			public bool IsInRange(Transform character, Transform trigger, out float error)
			{
				error = 0f;
				if (!use)
				{
					return true;
				}
				error = 180f;
				if (radius <= 0f)
				{
					return false;
				}
				if (maxAngle <= 0f)
				{
					return false;
				}
				Vector3 forward = trigger.forward;
				if (fixYAxis)
				{
					forward.y = 0f;
				}
				if (forward == Vector3.zero)
				{
					return false;
				}
				Vector3 normal = ((!fixYAxis) ? trigger.up : Vector3.up);
				Quaternion quaternion = Quaternion.LookRotation(forward, normal);
				Vector3 vector = trigger.position + quaternion * offset3D;
				Vector3 vector2 = ((!orbit) ? vector : trigger.position);
				Vector3 tangent = character.position - vector2;
				Vector3.OrthoNormalize(ref normal, ref tangent);
				tangent *= Vector3.Project(character.position - vector2, tangent).magnitude;
				if (orbit)
				{
					float magnitude = offset.magnitude;
					float magnitude2 = tangent.magnitude;
					if (magnitude2 < magnitude - radius || magnitude2 > magnitude + radius)
					{
						return false;
					}
				}
				else if (tangent.magnitude > radius)
				{
					return false;
				}
				Vector3 tangent2 = quaternion * direction3D;
				Vector3.OrthoNormalize(ref normal, ref tangent2);
				if (orbit)
				{
					Vector3 vector3 = vector - trigger.position;
					if (vector3 == Vector3.zero)
					{
						vector3 = Vector3.forward;
					}
					Quaternion rotation = Quaternion.LookRotation(vector3, normal);
					tangent = Quaternion.Inverse(rotation) * tangent;
					float angle = Mathf.Atan2(tangent.x, tangent.z) * 57.29578f;
					tangent2 = Quaternion.AngleAxis(angle, normal) * tangent2;
				}
				float num = Vector3.Angle(tangent2, character.forward);
				if (num > maxAngle)
				{
					return false;
				}
				error = num / maxAngle * 180f;
				return true;
			}
		}

		[Serializable]
		public class CameraPosition
		{
			[Tooltip("What the camera should be looking at to trigger the interaction?")]
			public Collider lookAtTarget;

			[Tooltip("The direction from the lookAtTarget towards the camera (in lookAtTarget's space).")]
			public Vector3 direction = -Vector3.forward;

			[Tooltip("Max distance from the lookAtTarget to the camera.")]
			public float maxDistance = 0.5f;

			[Tooltip("Max angle between the direction and the direction towards the camera.")]
			[Range(0f, 180f)]
			public float maxAngle = 45f;

			[Tooltip("Fixes the Y axis of the trigger to Vector3.up. This makes the trigger symmetrical relative to the object.")]
			public bool fixYAxis;

			public Quaternion GetRotation()
			{
				Vector3 forward = lookAtTarget.transform.forward;
				if (fixYAxis)
				{
					forward.y = 0f;
				}
				if (forward == Vector3.zero)
				{
					return Quaternion.identity;
				}
				Vector3 upwards = ((!fixYAxis) ? lookAtTarget.transform.up : Vector3.up);
				return Quaternion.LookRotation(forward, upwards);
			}

			public bool IsInRange(Transform raycastFrom, RaycastHit hit, Transform trigger, out float error)
			{
				error = 0f;
				if (lookAtTarget == null)
				{
					return true;
				}
				error = 180f;
				if (raycastFrom == null)
				{
					return false;
				}
				if (hit.collider != lookAtTarget)
				{
					return false;
				}
				if (hit.distance > maxDistance)
				{
					return false;
				}
				if (direction == Vector3.zero)
				{
					return false;
				}
				if (maxDistance <= 0f)
				{
					return false;
				}
				if (maxAngle <= 0f)
				{
					return false;
				}
				Vector3 to = GetRotation() * direction;
				float num = Vector3.Angle(raycastFrom.position - hit.point, to);
				if (num > maxAngle)
				{
					return false;
				}
				error = num / maxAngle * 180f;
				return true;
			}
		}

		[Serializable]
		public class Range
		{
			[Serializable]
			public class Interaction
			{
				[Tooltip("The InteractionObject to interact with.")]
				public InteractionObject interactionObject;

				[Tooltip("The effectors to interact with.")]
				public FullBodyBipedEffector[] effectors;
			}

			[HideInInspector]
			[SerializeField]
			public string name;

			[HideInInspector]
			[SerializeField]
			public bool show = true;

			[Tooltip("The range for the character's position and rotation.")]
			public CharacterPosition characterPosition;

			[Tooltip("The range for the character camera's position and rotation.")]
			public CameraPosition cameraPosition;

			[Tooltip("Definitions of the interactions associated with this range.")]
			public Interaction[] interactions;

			public bool IsInRange(Transform character, Transform raycastFrom, RaycastHit raycastHit, Transform trigger, out float maxError)
			{
				maxError = 0f;
				float error = 0f;
				float error2 = 0f;
				if (!characterPosition.IsInRange(character, trigger, out error))
				{
					return false;
				}
				if (!cameraPosition.IsInRange(raycastFrom, raycastHit, trigger, out error2))
				{
					return false;
				}
				maxError = Mathf.Max(error, error2);
				return true;
			}
		}

		[Tooltip("The valid ranges of the character's and/or it's camera's position for triggering interaction when the character is in contact with the collider of this trigger.")]
		public Range[] ranges = new Range[0];

		[ContextMenu("TUTORIAL VIDEO")]
		private void OpenTutorial4()
		{
			Application.OpenURL("https://www.youtube.com/watch?v=-TDZpNjt2mk&index=15&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6");
		}

		[ContextMenu("Support Group")]
		private void SupportGroup()
		{
			Application.OpenURL("https://groups.google.com/forum/#!forum/final-ik");
		}

		[ContextMenu("Asset Store Thread")]
		private void ASThread()
		{
			Application.OpenURL("http://forum.unity3d.com/threads/final-ik-full-body-ik-aim-look-at-fabrik-ccd-ik-1-0-released.222685/");
		}

		private void Start()
		{
		}

		public int GetBestRangeIndex(Transform character, Transform raycastFrom, RaycastHit raycastHit)
		{
			if (GetComponent<Collider>() == null)
			{
				Warning.Log("Using the InteractionTrigger requires a Collider component.", base.transform);
				return -1;
			}
			int result = -1;
			float num = 180f;
			float maxError = 0f;
			for (int i = 0; i < ranges.Length; i++)
			{
				if (ranges[i].IsInRange(character, raycastFrom, raycastHit, base.transform, out maxError) && maxError <= num)
				{
					num = maxError;
					result = i;
				}
			}
			return result;
		}
	}
}
