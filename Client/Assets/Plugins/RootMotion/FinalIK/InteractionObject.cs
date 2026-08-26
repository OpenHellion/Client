using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[HelpURL("https://www.youtube.com/watch?v=r5jiZnsDH3M")]
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Interaction System/Interaction Object")]
	public class InteractionObject : MonoBehaviour
	{
		[Serializable]
		public class InteractionEvent
		{
			[Tooltip("The time of the event since interaction start.")]
			public float time;

			[Tooltip(
				"If true, the interaction will be paused on this event. The interaction can be resumed by InteractionSystem.ResumeInteraction() or InteractionSystem.ResumeAll;")]
			public bool pause;

			[Tooltip(
				"If true, the object will be parented to the effector bone on this event. Note that picking up like this can be done by only a single effector at a time. If you wish to pick up an object with both hands, see the Interaction PickUp2Handed demo scene.")]
			public bool pickUp;

			[Tooltip("The animations called on this event.")]
			public AnimatorEvent[] animations;

			[Tooltip("The messages sent on this event using GameObject.SendMessage().")]
			public Message[] messages;

			public void Activate(Transform t)
			{
				AnimatorEvent[] array = animations;
				foreach (AnimatorEvent animatorEvent in array)
				{
					animatorEvent.Activate(pickUp);
				}

				Message[] array2 = messages;
				foreach (Message message in array2)
				{
					message.Send(t);
				}
			}
		}

		[Serializable]
		public class Message
		{
			[Tooltip("The name of the function called.")]
			public string function;

			[Tooltip("The recipient game object.")]
			public GameObject recipient;

			private const string empty = "";

			public void Send(Transform t)
			{
				if (!(recipient == null) && !(function == string.Empty) && !(function == string.Empty))
				{
					recipient.SendMessage(function, t, SendMessageOptions.RequireReceiver);
				}
			}
		}

		[Serializable]
		public class AnimatorEvent
		{
			[Tooltip("The Animator component that will receive the AnimatorEvents.")]
			public Animator animator;

			[Tooltip("The Animation component that will receive the AnimatorEvents (Legacy).")]
			public Animation animation;

			[Tooltip("The name of the animation state.")]
			public string animationState;

			[Tooltip("The crossfading time.")] public float crossfadeTime = 0.3f;

			[Tooltip(
				"The layer of the animation state (if using Legacy, the animation state will be forced to this layer).")]
			public int layer;

			[Tooltip("Should the animation always start from 0 normalized time?")]
			public bool resetNormalizedTime;

			private const string empty = "";

			public void Activate(bool pickUp)
			{
				if (animator != null)
				{
					if (pickUp)
					{
						animator.applyRootMotion = false;
					}

					Activate(animator);
				}

				if (animation != null)
				{
					Activate(animation);
				}
			}

			private void Activate(Animator animator)
			{
				if (!(animationState == string.Empty))
				{
					if (resetNormalizedTime)
					{
						animator.CrossFade(animationState, crossfadeTime, layer, 0f);
					}
					else
					{
						animator.CrossFade(animationState, crossfadeTime, layer);
					}
				}
			}

			private void Activate(Animation animation)
			{
				if (!(animationState == string.Empty))
				{
					if (resetNormalizedTime)
					{
						animation[animationState].normalizedTime = 0f;
					}

					animation[animationState].layer = layer;
					animation.CrossFade(animationState, crossfadeTime);
				}
			}
		}

		[Serializable]
		public class WeightCurve
		{
			[Serializable]
			public enum Type
			{
				PositionWeight = 0,
				RotationWeight = 1,
				PositionOffsetX = 2,
				PositionOffsetY = 3,
				PositionOffsetZ = 4,
				Pull = 5,
				Reach = 6,
				RotateBoneWeight = 7,
				Push = 8,
				PushParent = 9,
				PoserWeight = 10
			}

			[Tooltip("The type of the curve (InteractionObject.WeightCurve.Type).")]
			public Type type;

			[Tooltip("The weight curve.")] public AnimationCurve curve;

			public float GetValue(float timer)
			{
				return curve.Evaluate(timer);
			}
		}

		[Serializable]
		public class Multiplier
		{
			[Tooltip("The curve type to multiply.")]
			public WeightCurve.Type curve;

			[Tooltip("The multiplier of the curve's value.")]
			public float multiplier = 1f;

			[Tooltip("The resulting value will be applied to this channel.")]
			public WeightCurve.Type result;

			public float GetValue(WeightCurve weightCurve, float timer)
			{
				return weightCurve.GetValue(timer) * multiplier;
			}
		}

		[Tooltip(
			"If the Interaction System has a 'Look At' LookAtIK component assigned, will use it to make the character look at the specified Transform. If unassigned, will look at this GameObject.")]
		public Transform otherLookAtTarget;

		[Tooltip(
			"The root Transform of the InteractionTargets. If null, will use this GameObject. GetComponentsInChildren<InteractionTarget>() will be used at initiation to find all InteractionTargets associated with this InteractionObject.")]
		public Transform otherTargetsRoot;

		[Tooltip(
			"If assigned, all PositionOffset channels will be applied in the rotation space of this Transform. If not, they will be in the rotation space of the character.")]
		public Transform positionOffsetSpace;

		public WeightCurve[] weightCurves;

		public Multiplier[] multipliers;

		public InteractionEvent[] events;

		private InteractionTarget[] targets = new InteractionTarget[0];

		public float length { get; private set; }

		public InteractionSystem lastUsedInteractionSystem { get; private set; }

		public Transform lookAtTarget
		{
			get
			{
				if (otherLookAtTarget != null)
				{
					return otherLookAtTarget;
				}

				return base.transform;
			}
		}

		public Transform targetsRoot
		{
			get
			{
				if (otherTargetsRoot != null)
				{
					return otherTargetsRoot;
				}

				return base.transform;
			}
		}

		[ContextMenu("TUTORIAL VIDEO (PART 1: BASICS)")]
		private void OpenTutorial1()
		{
			Application.OpenURL("https://www.youtube.com/watch?v=r5jiZnsDH3M");
		}

		[ContextMenu("TUTORIAL VIDEO (PART 2: PICKING UP...)")]
		private void OpenTutorial2()
		{
			Application.OpenURL("https://www.youtube.com/watch?v=eP9-zycoHLk");
		}

		[ContextMenu("TUTORIAL VIDEO (PART 3: ANIMATION)")]
		private void OpenTutorial3()
		{
			Application.OpenURL(
				"https://www.youtube.com/watch?v=sQfB2RcT1T4&index=14&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6");
		}

		[ContextMenu("TUTORIAL VIDEO (PART 4: TRIGGERS)")]
		private void OpenTutorial4()
		{
			Application.OpenURL(
				"https://www.youtube.com/watch?v=-TDZpNjt2mk&index=15&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6");
		}

		[ContextMenu("Support Group")]
		private void SupportGroup()
		{
			Application.OpenURL("https://groups.google.com/forum/#!forum/final-ik");
		}

		[ContextMenu("Asset Store Thread")]
		private void ASThread()
		{
			Application.OpenURL(
				"http://forum.unity3d.com/threads/final-ik-full-body-ik-aim-look-at-fabrik-ccd-ik-1-0-released.222685/");
		}

		public void Initiate()
		{
			for (int i = 0; i < weightCurves.Length; i++)
			{
				if (weightCurves[i].curve.length > 0)
				{
					float time = weightCurves[i].curve.keys[weightCurves[i].curve.length - 1].time;
					length = Mathf.Clamp(length, time, length);
				}
			}

			for (int j = 0; j < events.Length; j++)
			{
				length = Mathf.Clamp(length, events[j].time, length);
			}

			targets = targetsRoot.GetComponentsInChildren<InteractionTarget>();
		}

		public bool CurveUsed(WeightCurve.Type type)
		{
			WeightCurve[] array = weightCurves;
			foreach (WeightCurve weightCurve in array)
			{
				if (weightCurve.type == type)
				{
					return true;
				}
			}

			Multiplier[] array2 = multipliers;
			foreach (Multiplier multiplier in array2)
			{
				if (multiplier.result == type)
				{
					return true;
				}
			}

			return false;
		}

		public InteractionTarget[] GetTargets()
		{
			return targets;
		}

		public Transform GetTarget(FullBodyBipedEffector effectorType, string tag)
		{
			if (tag == string.Empty || tag == string.Empty)
			{
				return GetTarget(effectorType);
			}

			for (int i = 0; i < targets.Length; i++)
			{
				if (targets[i].effectorType == effectorType && targets[i].tag == tag)
				{
					return targets[i].transform;
				}
			}

			return base.transform;
		}

		public void OnStartInteraction(InteractionSystem interactionSystem)
		{
			lastUsedInteractionSystem = interactionSystem;
		}

		public void Apply(IKSolverFullBodyBiped solver, FullBodyBipedEffector effector, InteractionTarget target,
			float timer, float weight)
		{
			for (int i = 0; i < weightCurves.Length; i++)
			{
				float num = ((!(target == null)) ? target.GetValue(weightCurves[i].type) : 1f);
				Apply(solver, effector, weightCurves[i].type, weightCurves[i].GetValue(timer), weight * num);
			}

			for (int j = 0; j < multipliers.Length; j++)
			{
				if (multipliers[j].curve == multipliers[j].result && !Warning.logged)
				{
					Warning.Log(
						"InteractionObject Multiplier 'Curve' " + multipliers[j].curve.ToString() +
						"and 'Result' are the same.", base.transform);
				}

				int weightCurveIndex = GetWeightCurveIndex(multipliers[j].curve);
				if (weightCurveIndex != -1)
				{
					float num2 = ((!(target == null)) ? target.GetValue(multipliers[j].result) : 1f);
					Apply(solver, effector, multipliers[j].result,
						multipliers[j].GetValue(weightCurves[weightCurveIndex], timer), weight * num2);
				}
				else if (!Warning.logged)
				{
					Warning.Log(
						"InteractionObject Multiplier curve " + multipliers[j].curve.ToString() + "does not exist.",
						base.transform);
				}
			}
		}

		public float GetValue(WeightCurve.Type weightCurveType, InteractionTarget target, float timer)
		{
			int weightCurveIndex = GetWeightCurveIndex(weightCurveType);
			if (weightCurveIndex != -1)
			{
				float num = ((!(target == null)) ? target.GetValue(weightCurveType) : 1f);
				return weightCurves[weightCurveIndex].GetValue(timer) * num;
			}

			for (int i = 0; i < multipliers.Length; i++)
			{
				if (multipliers[i].result == weightCurveType)
				{
					int weightCurveIndex2 = GetWeightCurveIndex(multipliers[i].curve);
					if (weightCurveIndex2 != -1)
					{
						float num2 = ((!(target == null)) ? target.GetValue(multipliers[i].result) : 1f);
						return multipliers[i].GetValue(weightCurves[weightCurveIndex2], timer) * num2;
					}
				}
			}

			return 0f;
		}

		private void Awake()
		{
			Initiate();
		}

		private void Apply(IKSolverFullBodyBiped solver, FullBodyBipedEffector effector, WeightCurve.Type type,
			float value, float weight)
		{
			switch (type)
			{
				case WeightCurve.Type.PositionWeight:
					solver.GetEffector(effector).positionWeight =
						Mathf.Lerp(solver.GetEffector(effector).positionWeight, value, weight);
					break;
				case WeightCurve.Type.RotationWeight:
					solver.GetEffector(effector).rotationWeight =
						Mathf.Lerp(solver.GetEffector(effector).rotationWeight, value, weight);
					break;
				case WeightCurve.Type.PositionOffsetX:
					solver.GetEffector(effector).position +=
						((!(positionOffsetSpace != null)) ? solver.GetRoot().rotation : positionOffsetSpace.rotation) *
						Vector3.right * value * weight;
					break;
				case WeightCurve.Type.PositionOffsetY:
					solver.GetEffector(effector).position +=
						((!(positionOffsetSpace != null)) ? solver.GetRoot().rotation : positionOffsetSpace.rotation) *
						Vector3.up * value * weight;
					break;
				case WeightCurve.Type.PositionOffsetZ:
					solver.GetEffector(effector).position +=
						((!(positionOffsetSpace != null)) ? solver.GetRoot().rotation : positionOffsetSpace.rotation) *
						Vector3.forward * value * weight;
					break;
				case WeightCurve.Type.Pull:
					solver.GetChain(effector).pull = Mathf.Lerp(solver.GetChain(effector).pull, value, weight);
					break;
				case WeightCurve.Type.Reach:
					solver.GetChain(effector).reach = Mathf.Lerp(solver.GetChain(effector).reach, value, weight);
					break;
				case WeightCurve.Type.Push:
					solver.GetChain(effector).push = Mathf.Lerp(solver.GetChain(effector).push, value, weight);
					break;
				case WeightCurve.Type.PushParent:
					solver.GetChain(effector).pushParent =
						Mathf.Lerp(solver.GetChain(effector).pushParent, value, weight);
					break;
				case WeightCurve.Type.RotateBoneWeight:
					break;
			}
		}

		private Transform GetTarget(FullBodyBipedEffector effectorType)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (targets[i].effectorType == effectorType)
				{
					return targets[i].transform;
				}
			}

			return base.transform;
		}

		private int GetWeightCurveIndex(WeightCurve.Type weightCurveType)
		{
			for (int i = 0; i < weightCurves.Length; i++)
			{
				if (weightCurves[i].type == weightCurveType)
				{
					return i;
				}
			}

			return -1;
		}

		private int GetMultiplierIndex(WeightCurve.Type weightCurveType)
		{
			for (int i = 0; i < multipliers.Length; i++)
			{
				if (multipliers[i].result == weightCurveType)
				{
					return i;
				}
			}

			return -1;
		}

		[ContextMenu("User Manual")]
		private void OpenUserManual()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page10.html");
		}

		[ContextMenu("Scrpt Reference")]
		private void OpenScriptReference()
		{
			Application.OpenURL(
				"http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_interaction_object.html");
		}
	}
}
