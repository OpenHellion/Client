using System;
using System.Collections.Generic;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class InteractionEffector
	{
		private Poser poser;

		private IKEffector effector;

		private float timer;

		private float length;

		private float weight;

		private float fadeInSpeed;

		private float defaultPositionWeight;

		private float defaultRotationWeight;

		private float defaultPull;

		private float defaultReach;

		private float defaultPush;

		private float defaultPushParent;

		private float resetTimer;

		private bool positionWeightUsed;

		private bool rotationWeightUsed;

		private bool pullUsed;

		private bool reachUsed;

		private bool pushUsed;

		private bool pushParentUsed;

		private bool pickedUp;

		private bool defaults;

		private bool pickUpOnPostFBBIK;

		private Vector3 pickUpPosition;

		private Vector3 pausePositionRelative;

		private Quaternion pickUpRotation;

		private Quaternion pauseRotationRelative;

		private InteractionTarget interactionTarget;

		private Transform target;

		private List<bool> triggered = new List<bool>();

		private InteractionSystem interactionSystem;

		private bool started;

		public FullBodyBipedEffector effectorType { get; private set; }

		public bool isPaused { get; private set; }

		public InteractionObject interactionObject { get; private set; }

		public bool inInteraction
		{
			get
			{
				return interactionObject != null;
			}
		}

		public float progress
		{
			get
			{
				if (!inInteraction)
				{
					return 0f;
				}
				if (length == 0f)
				{
					return 0f;
				}
				return timer / length;
			}
		}

		public InteractionEffector(FullBodyBipedEffector effectorType)
		{
			this.effectorType = effectorType;
		}

		public void Initiate(InteractionSystem interactionSystem)
		{
			this.interactionSystem = interactionSystem;
			if (effector == null)
			{
				effector = interactionSystem.ik.solver.GetEffector(effectorType);
				poser = effector.bone.GetComponent<Poser>();
			}
			StoreDefaults();
		}

		private void StoreDefaults()
		{
			defaultPositionWeight = interactionSystem.ik.solver.GetEffector(effectorType).positionWeight;
			defaultRotationWeight = interactionSystem.ik.solver.GetEffector(effectorType).rotationWeight;
			defaultPull = interactionSystem.ik.solver.GetChain(effectorType).pull;
			defaultReach = interactionSystem.ik.solver.GetChain(effectorType).reach;
			defaultPush = interactionSystem.ik.solver.GetChain(effectorType).push;
			defaultPushParent = interactionSystem.ik.solver.GetChain(effectorType).pushParent;
		}

		public bool ResetToDefaults(float speed)
		{
			if (inInteraction)
			{
				return false;
			}
			if (isPaused)
			{
				return false;
			}
			if (defaults)
			{
				return false;
			}
			resetTimer = Mathf.Clamp(resetTimer -= Time.deltaTime * speed, 0f, 1f);
			if (effector.isEndEffector)
			{
				if (pullUsed)
				{
					interactionSystem.ik.solver.GetChain(effectorType).pull = Mathf.Lerp(defaultPull, interactionSystem.ik.solver.GetChain(effectorType).pull, resetTimer);
				}
				if (reachUsed)
				{
					interactionSystem.ik.solver.GetChain(effectorType).reach = Mathf.Lerp(defaultReach, interactionSystem.ik.solver.GetChain(effectorType).reach, resetTimer);
				}
				if (pushUsed)
				{
					interactionSystem.ik.solver.GetChain(effectorType).push = Mathf.Lerp(defaultPush, interactionSystem.ik.solver.GetChain(effectorType).push, resetTimer);
				}
				if (pushParentUsed)
				{
					interactionSystem.ik.solver.GetChain(effectorType).pushParent = Mathf.Lerp(defaultPushParent, interactionSystem.ik.solver.GetChain(effectorType).pushParent, resetTimer);
				}
			}
			if (positionWeightUsed)
			{
				effector.positionWeight = Mathf.Lerp(defaultPositionWeight, effector.positionWeight, resetTimer);
			}
			if (rotationWeightUsed)
			{
				effector.rotationWeight = Mathf.Lerp(defaultRotationWeight, effector.rotationWeight, resetTimer);
			}
			if (resetTimer <= 0f)
			{
				pullUsed = false;
				reachUsed = false;
				pushUsed = false;
				pushParentUsed = false;
				positionWeightUsed = false;
				rotationWeightUsed = false;
				defaults = true;
			}
			return true;
		}

		public bool Pause()
		{
			if (!inInteraction)
			{
				return false;
			}
			isPaused = true;
			pausePositionRelative = target.InverseTransformPoint(effector.position);
			pauseRotationRelative = Quaternion.Inverse(target.rotation) * effector.rotation;
			if (interactionSystem.OnInteractionPause != null)
			{
				interactionSystem.OnInteractionPause(effectorType, interactionObject);
			}
			return true;
		}

		public bool Resume()
		{
			if (!inInteraction)
			{
				return false;
			}
			isPaused = false;
			if (interactionSystem.OnInteractionResume != null)
			{
				interactionSystem.OnInteractionResume(effectorType, interactionObject);
			}
			return true;
		}

		public bool Start(InteractionObject interactionObject, string tag, float fadeInTime, bool interrupt)
		{
			if (!inInteraction)
			{
				effector.position = effector.bone.position;
				effector.rotation = effector.bone.rotation;
			}
			else if (!interrupt)
			{
				return false;
			}
			target = interactionObject.GetTarget(effectorType, tag);
			if (target == null)
			{
				return false;
			}
			interactionTarget = target.GetComponent<InteractionTarget>();
			this.interactionObject = interactionObject;
			if (interactionSystem.OnInteractionStart != null)
			{
				interactionSystem.OnInteractionStart(effectorType, interactionObject);
			}
			interactionObject.OnStartInteraction(interactionSystem);
			triggered.Clear();
			for (int i = 0; i < interactionObject.events.Length; i++)
			{
				triggered.Add(false);
			}
			if (poser != null)
			{
				if (poser.poseRoot == null)
				{
					poser.weight = 0f;
				}
				if (interactionTarget != null)
				{
					poser.poseRoot = target.transform;
				}
				else
				{
					poser.poseRoot = null;
				}
				poser.AutoMapping();
			}
			positionWeightUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.PositionWeight);
			rotationWeightUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.RotationWeight);
			pullUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.Pull);
			reachUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.Reach);
			pushUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.Push);
			pushParentUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.PushParent);
			StoreDefaults();
			timer = 0f;
			weight = 0f;
			fadeInSpeed = ((!(fadeInTime > 0f)) ? 1000f : (1f / fadeInTime));
			length = interactionObject.length;
			isPaused = false;
			pickedUp = false;
			pickUpPosition = Vector3.zero;
			pickUpRotation = Quaternion.identity;
			if (interactionTarget != null)
			{
				interactionTarget.RotateTo(effector.bone.position);
			}
			started = true;
			return true;
		}

		public void Update(Transform root, float speed)
		{
			if (!inInteraction)
			{
				if (started)
				{
					isPaused = false;
					pickedUp = false;
					defaults = false;
					resetTimer = 1f;
					started = false;
				}
				return;
			}
			if (interactionTarget != null && !interactionTarget.rotateOnce)
			{
				interactionTarget.RotateTo(effector.bone.position);
			}
			if (isPaused)
			{
				effector.position = target.TransformPoint(pausePositionRelative);
				effector.rotation = target.rotation * pauseRotationRelative;
				interactionObject.Apply(interactionSystem.ik.solver, effectorType, interactionTarget, timer, weight);
				return;
			}
			timer += Time.deltaTime * speed * ((!(interactionTarget != null)) ? 1f : interactionTarget.interactionSpeedMlp);
			weight = Mathf.Clamp(weight + Time.deltaTime * fadeInSpeed * speed, 0f, 1f);
			bool pickUp = false;
			bool pause = false;
			TriggerUntriggeredEvents(true, out pickUp, out pause);
			Vector3 b = ((!pickedUp) ? target.position : pickUpPosition);
			Quaternion b2 = ((!pickedUp) ? target.rotation : pickUpRotation);
			effector.position = Vector3.Lerp(effector.bone.position, b, weight);
			effector.rotation = Quaternion.Lerp(effector.bone.rotation, b2, weight);
			interactionObject.Apply(interactionSystem.ik.solver, effectorType, interactionTarget, timer, weight);
			if (pickUp)
			{
				PickUp(root);
			}
			if (pause)
			{
				Pause();
			}
			float value = interactionObject.GetValue(InteractionObject.WeightCurve.Type.PoserWeight, interactionTarget, timer);
			if (poser != null)
			{
				poser.weight = Mathf.Lerp(poser.weight, value, weight);
			}
			else if (value > 0f)
			{
				Warning.Log("InteractionObject " + interactionObject.name + " has a curve/multipler for Poser Weight, but the bone of effector " + effectorType.ToString() + " has no HandPoser/GenericPoser attached.", effector.bone);
			}
			if (timer >= length)
			{
				Stop();
			}
		}

		private void TriggerUntriggeredEvents(bool checkTime, out bool pickUp, out bool pause)
		{
			pickUp = false;
			pause = false;
			for (int i = 0; i < triggered.Count; i++)
			{
				if (triggered[i] || (checkTime && !(interactionObject.events[i].time < timer)))
				{
					continue;
				}
				interactionObject.events[i].Activate(effector.bone);
				if (interactionObject.events[i].pickUp)
				{
					if (timer >= interactionObject.events[i].time)
					{
						timer = interactionObject.events[i].time;
					}
					pickUp = true;
				}
				if (interactionObject.events[i].pause)
				{
					if (timer >= interactionObject.events[i].time)
					{
						timer = interactionObject.events[i].time;
					}
					pause = true;
				}
				if (interactionSystem.OnInteractionEvent != null)
				{
					interactionSystem.OnInteractionEvent(effectorType, interactionObject, interactionObject.events[i]);
				}
				triggered[i] = true;
			}
		}

		private void PickUp(Transform root)
		{
			pickUpPosition = effector.position;
			pickUpRotation = effector.rotation;
			pickUpOnPostFBBIK = true;
			pickedUp = true;
			Rigidbody component = interactionObject.targetsRoot.GetComponent<Rigidbody>();
			if (component != null)
			{
				if (!component.isKinematic)
				{
					component.isKinematic = true;
				}
				if (root.GetComponent<Collider>() != null)
				{
					Collider[] componentsInChildren = interactionObject.targetsRoot.GetComponentsInChildren<Collider>();
					Collider[] array = componentsInChildren;
					foreach (Collider collider in array)
					{
						if (!collider.isTrigger)
						{
							Physics.IgnoreCollision(root.GetComponent<Collider>(), collider);
						}
					}
				}
			}
			if (interactionSystem.OnInteractionPickUp != null)
			{
				interactionSystem.OnInteractionPickUp(effectorType, interactionObject);
			}
		}

		public bool Stop()
		{
			if (!inInteraction)
			{
				return false;
			}
			bool pickUp = false;
			bool pause = false;
			TriggerUntriggeredEvents(false, out pickUp, out pause);
			if (interactionSystem.OnInteractionStop != null)
			{
				interactionSystem.OnInteractionStop(effectorType, interactionObject);
			}
			if (interactionTarget != null)
			{
				interactionTarget.ResetRotation();
			}
			interactionObject = null;
			weight = 0f;
			timer = 0f;
			isPaused = false;
			target = null;
			defaults = false;
			resetTimer = 1f;
			if (poser != null && !pickedUp)
			{
				poser.weight = 0f;
			}
			pickedUp = false;
			started = false;
			return true;
		}

		public void OnPostFBBIK()
		{
			if (inInteraction)
			{
				float num = interactionObject.GetValue(InteractionObject.WeightCurve.Type.RotateBoneWeight, interactionTarget, timer) * weight;
				if (num > 0f)
				{
					Quaternion b = ((!pickedUp) ? effector.rotation : pickUpRotation);
					Quaternion quaternion = Quaternion.Slerp(effector.bone.rotation, b, num * num);
					effector.bone.localRotation = Quaternion.Inverse(effector.bone.parent.rotation) * quaternion;
				}
				if (pickUpOnPostFBBIK)
				{
					Vector3 position = effector.bone.position;
					effector.bone.position = pickUpPosition;
					interactionObject.targetsRoot.parent = effector.bone;
					effector.bone.position = position;
					pickUpOnPostFBBIK = false;
				}
			}
		}
	}
}
