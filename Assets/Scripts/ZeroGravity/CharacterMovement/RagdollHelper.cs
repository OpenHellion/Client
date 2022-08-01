using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Objects;

namespace ZeroGravity.CharacterMovement
{
	public class RagdollHelper : MonoBehaviour
	{
		private enum RagdollState
		{
			Animated = 0,
			Ragdolled = 1,
			BlendToAnim = 2
		}

		public class BodyPart
		{
			public Transform transform;

			public Quaternion storedRotation;

			public Vector3 storedPosition;
		}

		[SerializeField]
		private AnimatorHelper animHelper;

		[SerializeField]
		private Transform rootTransform;

		private MyPlayer objMyPlayer;

		private OtherPlayer objPlayer;

		private Corpse objCorpse;

		private bool cameraFollowRagdoll;

		private List<Rigidbody> ragdollRigidBodies = new List<Rigidbody>();

		private List<Collider> ragdollColliders = new List<Collider>();

		private RagdollState ragdollState;

		[SerializeField]
		private float ragdollToMecanimBlendTime = 0.5f;

		[SerializeField]
		private float mecanimToGetUpTransitionTime = 0.05f;

		private float ragdollingEndTime = -100f;

		private List<BodyPart> bodyParts = new List<BodyPart>();

		private bool wasRagdolled;

		private bool Ragdolled
		{
			get
			{
				return ragdollState != RagdollState.Animated;
			}
			set
			{
				if (value)
				{
					if (ragdollState == RagdollState.Animated)
					{
						SetKinematic(false);
						animHelper.ToggleMainAnimator(false);
						ragdollState = RagdollState.Ragdolled;
					}
				}
				else
				{
					if (ragdollState != RagdollState.Ragdolled)
					{
						return;
					}
					SetKinematic(true);
					ragdollingEndTime = Time.time;
					ragdollState = RagdollState.BlendToAnim;
					Transform bone = animHelper.GetBone(AnimatorHelper.HumanBones.Hips);
					foreach (BodyPart bodyPart in bodyParts)
					{
						if (bodyPart.transform == bone)
						{
							bodyPart.storedRotation = bodyPart.transform.rotation;
						}
						else
						{
							bodyPart.storedRotation = bodyPart.transform.localRotation;
						}
						bodyPart.storedPosition = bodyPart.transform.position;
					}
					if (objMyPlayer != null)
					{
						if (Vector3.Dot(bone.up, objMyPlayer.GravityDirection) >= 0f)
						{
							animHelper.SetParameterTrigger(AnimatorHelper.Triggers.GetUpFromBack);
						}
						else
						{
							animHelper.SetParameterTrigger(AnimatorHelper.Triggers.GetUpFromBelly);
						}
					}
					animHelper.ToggleMainAnimator(true);
				}
			}
		}

		public void AddRootRotation(Quaternion rotationDiff)
		{
			rootTransform.rotation *= rotationDiff;
			foreach (BodyPart bodyPart in bodyParts)
			{
				if (bodyPart.transform == rootTransform)
				{
					bodyPart.storedRotation = rootTransform.localRotation;
					break;
				}
			}
		}

		private void Awake()
		{
			if (animHelper == null)
			{
				animHelper = GetComponent<AnimatorHelper>();
			}
			RefreshRagdollVariables();
		}

		private void SetKinematic(bool isKinematic)
		{
			foreach (Rigidbody ragdollRigidBody in ragdollRigidBodies)
			{
				ragdollRigidBody.isKinematic = isKinematic;
				ragdollRigidBody.useGravity = false;
			}
		}

		private void LateUpdate()
		{
			if (ragdollState != RagdollState.BlendToAnim)
			{
				return;
			}
			float value = 1f - (Time.time - ragdollingEndTime - mecanimToGetUpTransitionTime) / ragdollToMecanimBlendTime;
			value = Mathf.Clamp01(value);
			foreach (BodyPart bodyPart in bodyParts)
			{
				if (bodyPart.transform == animHelper.GetBone(AnimatorHelper.HumanBones.Hips))
				{
					bodyPart.transform.position = Vector3.Lerp(bodyPart.transform.position, bodyPart.storedPosition, value);
					bodyPart.transform.rotation = Quaternion.Lerp(bodyPart.transform.rotation, bodyPart.storedRotation, value);
				}
				else
				{
					bodyPart.transform.localRotation = Quaternion.Lerp(bodyPart.transform.localRotation, bodyPart.storedRotation, value);
				}
			}
			if (value == 0f)
			{
				ragdollState = RagdollState.Animated;
			}
		}

		private void FixedUpdate()
		{
			if (objMyPlayer != null)
			{
				ApplyRagdollGravity(objMyPlayer.Gravity, objMyPlayer);
			}
			else if (objPlayer != null)
			{
				ApplyRagdollGravity(objPlayer.Gravity, objPlayer);
			}
			else if (objCorpse != null)
			{
				ApplyRagdollGravity(objCorpse.Gravity, objCorpse);
			}
		}

		private void Update()
		{
			if (objMyPlayer != null && !cameraFollowRagdoll && wasRagdolled)
			{
				if (objMyPlayer != null)
				{
					objMyPlayer.RagdollFinished();
				}
				wasRagdolled = false;
				base.enabled = false;
			}
		}

		private void ApplyRagdollGravity(Vector3 gravity, SpaceObject obj)
		{
			if (!(gravity != Vector3.zero))
			{
				return;
			}
			foreach (Rigidbody ragdollRigidBody in ragdollRigidBodies)
			{
				if (obj != null)
				{
					ragdollRigidBody.AddForce(gravity, ForceMode.Acceleration);
				}
			}
		}

		public void OnRagdollStateExit()
		{
			cameraFollowRagdoll = false;
		}

		public void ToggleRagdoll(bool enabled, SpaceObject spaceObj, Vector3 force = default(Vector3))
		{
			if (enabled)
			{
				base.enabled = true;
				cameraFollowRagdoll = true;
				wasRagdolled = true;
				Ragdolled = true;
				if (spaceObj is MyPlayer)
				{
					objMyPlayer = spaceObj as MyPlayer;
				}
				else if (spaceObj is OtherPlayer)
				{
					objPlayer = spaceObj as OtherPlayer;
				}
				else if (spaceObj is Corpse)
				{
					objCorpse = spaceObj as Corpse;
				}
				for (int i = 0; i < ragdollColliders.Count; i++)
				{
					ragdollColliders[i].isTrigger = false;
					ragdollColliders[i].material.dynamicFriction = 1f;
					ragdollColliders[i].material.staticFriction = 1f;
					ragdollRigidBodies[i].useGravity = false;
					ragdollRigidBodies[i].AddForce(force, ForceMode.VelocityChange);
				}
			}
			else
			{
				Ragdolled = false;
				for (int j = 0; j < ragdollColliders.Count; j++)
				{
					ragdollColliders[j].isTrigger = true;
					ragdollColliders[j].material.dynamicFriction = 0f;
					ragdollColliders[j].material.staticFriction = 0f;
				}
			}
		}

		public void RefreshRagdollVariables()
		{
			ragdollRigidBodies.Clear();
			ragdollColliders.Clear();
			bodyParts.Clear();
			rootTransform = animHelper.GetBone(AnimatorHelper.HumanBones.Root);
			Transform[] componentsInChildren = rootTransform.GetComponentsInChildren<Transform>();
			foreach (Transform transform in componentsInChildren)
			{
				if (transform == rootTransform || transform.GetComponent<TransitionTriggerHelper>() != null)
				{
					continue;
				}
				bodyParts.Add(new BodyPart
				{
					transform = transform
				});
				Rigidbody component = transform.GetComponent<Rigidbody>();
				if (component != null)
				{
					Collider component2 = transform.GetComponent<Collider>();
					if (component != null)
					{
						component.useGravity = false;
						ragdollRigidBodies.Add(component);
						ragdollColliders.Add(component2);
					}
				}
			}
		}

		public void ResetRagdoll()
		{
			Ragdolled = false;
			ragdollState = RagdollState.Animated;
			SetKinematic(true);
			Rigidbody[] componentsInChildren = base.transform.GetComponentsInChildren<Rigidbody>(true);
			foreach (Rigidbody rigidbody in componentsInChildren)
			{
				rigidbody.velocity = Vector3.zero;
				rigidbody.isKinematic = true;
				rigidbody.useGravity = false;
			}
			Collider[] componentsInChildren2 = base.transform.GetComponentsInChildren<Collider>(true);
			foreach (Collider collider in componentsInChildren2)
			{
				collider.isTrigger = true;
			}
			animHelper.ToggleMainAnimator(true);
			wasRagdolled = false;
			base.enabled = false;
		}

		public void RestorePositions()
		{
			foreach (BodyPart bodyPart in bodyParts)
			{
				if (bodyPart.transform == animHelper.GetBone(AnimatorHelper.HumanBones.Hips))
				{
					bodyPart.transform.position = bodyPart.storedPosition;
					bodyPart.transform.rotation = bodyPart.storedRotation;
				}
				else
				{
					bodyPart.transform.localRotation = bodyPart.storedRotation;
				}
			}
		}
	}
}
