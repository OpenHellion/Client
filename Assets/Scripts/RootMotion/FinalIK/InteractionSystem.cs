using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace RootMotion.FinalIK
{
	[HelpURL("https://www.youtube.com/watch?v=r5jiZnsDH3M")]
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Interaction System/Interaction System")]
	public class InteractionSystem : MonoBehaviour
	{
		public delegate void InteractionDelegate(FullBodyBipedEffector effectorType, InteractionObject interactionObject);

		public delegate void InteractionEventDelegate(FullBodyBipedEffector effectorType, InteractionObject interactionObject, InteractionObject.InteractionEvent interactionEvent);

		[Tooltip("If not empty, only the targets with the specified tag will be used by this Interaction System.")]
		public string targetTag = string.Empty;

		[Tooltip("The fade in time of the interaction.")]
		public float fadeInTime = 0.3f;

		[Tooltip("The master speed for all interactions.")]
		public float speed = 1f;

		[Tooltip("If > 0, lerps all the FBBIK channels used by the Interaction System back to their default or initial values when not in interaction.")]
		public float resetToDefaultsSpeed = 1f;

		[Header("Triggering")]
		[Tooltip("The collider that registers OnTriggerEnter and OnTriggerExit events with InteractionTriggers.")]
		[FormerlySerializedAs("collider")]
		public Collider characterCollider;

		[Tooltip("Will be used by Interaction Triggers that need the camera's position. Assign the first person view character camera.")]
		[FormerlySerializedAs("camera")]
		public Transform FPSCamera;

		[Tooltip("The layers that will be raycasted from the camera (along camera.forward). All InteractionTrigger look at target colliders should be included.")]
		public LayerMask camRaycastLayers;

		[Tooltip("Max distance of raycasting from the camera.")]
		public float camRaycastDistance = 1f;

		private List<InteractionTrigger> inContact = new List<InteractionTrigger>();

		private List<int> bestRangeIndexes = new List<int>();

		public InteractionDelegate OnInteractionStart;

		public InteractionDelegate OnInteractionPause;

		public InteractionDelegate OnInteractionPickUp;

		public InteractionDelegate OnInteractionResume;

		public InteractionDelegate OnInteractionStop;

		public InteractionEventDelegate OnInteractionEvent;

		public RaycastHit raycastHit;

		[Space(10f)]
		[Tooltip("Reference to the FBBIK component.")]
		[SerializeField]
		private FullBodyBipedIK fullBody;

		[Tooltip("Handles looking at the interactions.")]
		public InteractionLookAt lookAt = new InteractionLookAt();

		private InteractionEffector[] interactionEffectors = new InteractionEffector[9]
		{
			new InteractionEffector(FullBodyBipedEffector.Body),
			new InteractionEffector(FullBodyBipedEffector.LeftFoot),
			new InteractionEffector(FullBodyBipedEffector.LeftHand),
			new InteractionEffector(FullBodyBipedEffector.LeftShoulder),
			new InteractionEffector(FullBodyBipedEffector.LeftThigh),
			new InteractionEffector(FullBodyBipedEffector.RightFoot),
			new InteractionEffector(FullBodyBipedEffector.RightHand),
			new InteractionEffector(FullBodyBipedEffector.RightShoulder),
			new InteractionEffector(FullBodyBipedEffector.RightThigh)
		};

		private bool initiated;

		private Collider lastCollider;

		private Collider c;

		public bool inInteraction
		{
			get
			{
				if (!IsValid(true))
				{
					return false;
				}
				for (int i = 0; i < interactionEffectors.Length; i++)
				{
					if (interactionEffectors[i].inInteraction && !interactionEffectors[i].isPaused)
					{
						return true;
					}
				}
				return false;
			}
		}

		public FullBodyBipedIK ik
		{
			get
			{
				return fullBody;
			}
		}

		public List<InteractionTrigger> triggersInRange { get; private set; }

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
			Application.OpenURL("https://www.youtube.com/watch?v=sQfB2RcT1T4&index=14&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6");
		}

		[ContextMenu("TUTORIAL VIDEO (PART 4: TRIGGERS)")]
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

		public bool IsInInteraction(FullBodyBipedEffector effectorType)
		{
			if (!IsValid(true))
			{
				return false;
			}
			for (int i = 0; i < interactionEffectors.Length; i++)
			{
				if (interactionEffectors[i].effectorType == effectorType)
				{
					return interactionEffectors[i].inInteraction && !interactionEffectors[i].isPaused;
				}
			}
			return false;
		}

		public bool IsPaused(FullBodyBipedEffector effectorType)
		{
			if (!IsValid(true))
			{
				return false;
			}
			for (int i = 0; i < interactionEffectors.Length; i++)
			{
				if (interactionEffectors[i].effectorType == effectorType)
				{
					return interactionEffectors[i].inInteraction && interactionEffectors[i].isPaused;
				}
			}
			return false;
		}

		public bool IsPaused()
		{
			if (!IsValid(true))
			{
				return false;
			}
			for (int i = 0; i < interactionEffectors.Length; i++)
			{
				if (interactionEffectors[i].inInteraction && interactionEffectors[i].isPaused)
				{
					return true;
				}
			}
			return false;
		}

		public bool IsInSync()
		{
			if (!IsValid(true))
			{
				return false;
			}
			for (int i = 0; i < interactionEffectors.Length; i++)
			{
				if (!interactionEffectors[i].isPaused)
				{
					continue;
				}
				for (int j = 0; j < interactionEffectors.Length; j++)
				{
					if (j != i && interactionEffectors[j].inInteraction && !interactionEffectors[j].isPaused)
					{
						return false;
					}
				}
			}
			return true;
		}

		public bool StartInteraction(FullBodyBipedEffector effectorType, InteractionObject interactionObject, bool interrupt)
		{
			if (!IsValid(true))
			{
				return false;
			}
			if (interactionObject == null)
			{
				return false;
			}
			for (int i = 0; i < interactionEffectors.Length; i++)
			{
				if (interactionEffectors[i].effectorType == effectorType)
				{
					return interactionEffectors[i].Start(interactionObject, targetTag, fadeInTime, interrupt);
				}
			}
			return false;
		}

		public bool PauseInteraction(FullBodyBipedEffector effectorType)
		{
			if (!IsValid(true))
			{
				return false;
			}
			for (int i = 0; i < interactionEffectors.Length; i++)
			{
				if (interactionEffectors[i].effectorType == effectorType)
				{
					return interactionEffectors[i].Pause();
				}
			}
			return false;
		}

		public bool ResumeInteraction(FullBodyBipedEffector effectorType)
		{
			if (!IsValid(true))
			{
				return false;
			}
			for (int i = 0; i < interactionEffectors.Length; i++)
			{
				if (interactionEffectors[i].effectorType == effectorType)
				{
					return interactionEffectors[i].Resume();
				}
			}
			return false;
		}

		public bool StopInteraction(FullBodyBipedEffector effectorType)
		{
			if (!IsValid(true))
			{
				return false;
			}
			for (int i = 0; i < interactionEffectors.Length; i++)
			{
				if (interactionEffectors[i].effectorType == effectorType)
				{
					return interactionEffectors[i].Stop();
				}
			}
			return false;
		}

		public void PauseAll()
		{
			if (IsValid(true))
			{
				for (int i = 0; i < interactionEffectors.Length; i++)
				{
					interactionEffectors[i].Pause();
				}
			}
		}

		public void ResumeAll()
		{
			if (IsValid(true))
			{
				for (int i = 0; i < interactionEffectors.Length; i++)
				{
					interactionEffectors[i].Resume();
				}
			}
		}

		public void StopAll()
		{
			for (int i = 0; i < interactionEffectors.Length; i++)
			{
				interactionEffectors[i].Stop();
			}
		}

		public InteractionObject GetInteractionObject(FullBodyBipedEffector effectorType)
		{
			if (!IsValid(true))
			{
				return null;
			}
			for (int i = 0; i < interactionEffectors.Length; i++)
			{
				if (interactionEffectors[i].effectorType == effectorType)
				{
					return interactionEffectors[i].interactionObject;
				}
			}
			return null;
		}

		public float GetProgress(FullBodyBipedEffector effectorType)
		{
			if (!IsValid(true))
			{
				return 0f;
			}
			for (int i = 0; i < interactionEffectors.Length; i++)
			{
				if (interactionEffectors[i].effectorType == effectorType)
				{
					return interactionEffectors[i].progress;
				}
			}
			return 0f;
		}

		public float GetMinActiveProgress()
		{
			if (!IsValid(true))
			{
				return 0f;
			}
			float num = 1f;
			for (int i = 0; i < interactionEffectors.Length; i++)
			{
				if (interactionEffectors[i].inInteraction)
				{
					float progress = interactionEffectors[i].progress;
					if (progress > 0f && progress < num)
					{
						num = progress;
					}
				}
			}
			return num;
		}

		public bool TriggerInteraction(int index, bool interrupt)
		{
			if (!IsValid(true))
			{
				return false;
			}
			if (!TriggerIndexIsValid(index))
			{
				return false;
			}
			bool result = true;
			InteractionTrigger.Range range = triggersInRange[index].ranges[bestRangeIndexes[index]];
			for (int i = 0; i < range.interactions.Length; i++)
			{
				for (int j = 0; j < range.interactions[i].effectors.Length; j++)
				{
					if (!StartInteraction(range.interactions[i].effectors[j], range.interactions[i].interactionObject, interrupt))
					{
						result = false;
					}
				}
			}
			return result;
		}

		public bool TriggerEffectorsReady(int index)
		{
			if (!IsValid(true))
			{
				return false;
			}
			if (!TriggerIndexIsValid(index))
			{
				return false;
			}
			for (int i = 0; i < triggersInRange[index].ranges.Length; i++)
			{
				InteractionTrigger.Range range = triggersInRange[index].ranges[i];
				for (int j = 0; j < range.interactions.Length; j++)
				{
					for (int k = 0; k < range.interactions[j].effectors.Length; k++)
					{
						if (IsInInteraction(range.interactions[j].effectors[k]))
						{
							return false;
						}
					}
				}
				for (int l = 0; l < range.interactions.Length; l++)
				{
					for (int m = 0; m < range.interactions[l].effectors.Length; m++)
					{
						if (!IsPaused(range.interactions[l].effectors[m]))
						{
							continue;
						}
						for (int n = 0; n < range.interactions[l].effectors.Length; n++)
						{
							if (n != m && !IsPaused(range.interactions[l].effectors[n]))
							{
								return false;
							}
						}
					}
				}
			}
			return true;
		}

		public InteractionTrigger.Range GetTriggerRange(int index)
		{
			if (!IsValid(true))
			{
				return null;
			}
			if (index < 0 || index >= bestRangeIndexes.Count)
			{
				Warning.Log("Index out of range.", base.transform);
				return null;
			}
			return triggersInRange[index].ranges[bestRangeIndexes[index]];
		}

		public int GetClosestTriggerIndex()
		{
			if (!IsValid(true))
			{
				return -1;
			}
			if (triggersInRange.Count == 0)
			{
				return -1;
			}
			if (triggersInRange.Count == 1)
			{
				return 0;
			}
			int result = -1;
			float num = float.PositiveInfinity;
			for (int i = 0; i < triggersInRange.Count; i++)
			{
				if (triggersInRange[i] != null)
				{
					float num2 = Vector3.SqrMagnitude(triggersInRange[i].transform.position - base.transform.position);
					if (num2 < num)
					{
						result = i;
						num = num2;
					}
				}
			}
			return result;
		}

		protected virtual void Start()
		{
			if (fullBody == null)
			{
				fullBody = GetComponent<FullBodyBipedIK>();
			}
			if (fullBody == null)
			{
				Warning.Log("InteractionSystem can not find a FullBodyBipedIK component", base.transform);
				return;
			}
			IKSolverFullBodyBiped solver = fullBody.solver;
			solver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPreUpdate, new IKSolver.UpdateDelegate(OnPreFBBIK));
			IKSolverFullBodyBiped solver2 = fullBody.solver;
			solver2.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver2.OnPostUpdate, new IKSolver.UpdateDelegate(OnPostFBBIK));
			OnInteractionStart = (InteractionDelegate)Delegate.Combine(OnInteractionStart, new InteractionDelegate(LookAtInteraction));
			OnInteractionPause = (InteractionDelegate)Delegate.Combine(OnInteractionPause, new InteractionDelegate(InteractionPause));
			OnInteractionResume = (InteractionDelegate)Delegate.Combine(OnInteractionResume, new InteractionDelegate(InteractionResume));
			OnInteractionStop = (InteractionDelegate)Delegate.Combine(OnInteractionStop, new InteractionDelegate(InteractionStop));
			InteractionEffector[] array = interactionEffectors;
			foreach (InteractionEffector interactionEffector in array)
			{
				interactionEffector.Initiate(this);
			}
			triggersInRange = new List<InteractionTrigger>();
			c = GetComponent<Collider>();
			UpdateTriggerEventBroadcasting();
			initiated = true;
		}

		private void InteractionPause(FullBodyBipedEffector effector, InteractionObject interactionObject)
		{
			lookAt.isPaused = true;
		}

		private void InteractionResume(FullBodyBipedEffector effector, InteractionObject interactionObject)
		{
			lookAt.isPaused = false;
		}

		private void InteractionStop(FullBodyBipedEffector effector, InteractionObject interactionObject)
		{
			lookAt.isPaused = false;
		}

		private void LookAtInteraction(FullBodyBipedEffector effector, InteractionObject interactionObject)
		{
			lookAt.Look(interactionObject.lookAtTarget, Time.time + interactionObject.length * 0.5f);
		}

		public void OnTriggerEnter(Collider c)
		{
			if (!(fullBody == null))
			{
				InteractionTrigger component = c.GetComponent<InteractionTrigger>();
				if (!inContact.Contains(component))
				{
					inContact.Add(component);
				}
			}
		}

		public void OnTriggerExit(Collider c)
		{
			if (!(fullBody == null))
			{
				InteractionTrigger component = c.GetComponent<InteractionTrigger>();
				inContact.Remove(component);
			}
		}

		private bool ContactIsInRange(int index, out int bestRangeIndex)
		{
			bestRangeIndex = -1;
			if (!IsValid(true))
			{
				return false;
			}
			if (index < 0 || index >= inContact.Count)
			{
				Warning.Log("Index out of range.", base.transform);
				return false;
			}
			if (inContact[index] == null)
			{
				Warning.Log("The InteractionTrigger in the list 'inContact' has been destroyed", base.transform);
				return false;
			}
			bestRangeIndex = inContact[index].GetBestRangeIndex(base.transform, FPSCamera, raycastHit);
			if (bestRangeIndex == -1)
			{
				return false;
			}
			return true;
		}

		private void OnDrawGizmosSelected()
		{
			if (!Application.isPlaying)
			{
				if (fullBody == null)
				{
					fullBody = GetComponent<FullBodyBipedIK>();
				}
				if (characterCollider == null)
				{
					characterCollider = GetComponent<Collider>();
				}
			}
		}

		private void Update()
		{
			if (fullBody == null)
			{
				return;
			}
			UpdateTriggerEventBroadcasting();
			Raycasting();
			triggersInRange.Clear();
			bestRangeIndexes.Clear();
			for (int i = 0; i < inContact.Count; i++)
			{
				int bestRangeIndex = -1;
				if (inContact[i] != null && inContact[i].gameObject.activeInHierarchy && inContact[i].enabled && ContactIsInRange(i, out bestRangeIndex))
				{
					triggersInRange.Add(inContact[i]);
					bestRangeIndexes.Add(bestRangeIndex);
				}
			}
			lookAt.Update();
		}

		private void Raycasting()
		{
			if ((int)camRaycastLayers != -1 && !(FPSCamera == null))
			{
				Physics.Raycast(FPSCamera.position, FPSCamera.forward, out raycastHit, camRaycastDistance, camRaycastLayers);
			}
		}

		private void UpdateTriggerEventBroadcasting()
		{
			if (characterCollider == null)
			{
				characterCollider = c;
			}
			if (characterCollider != null && characterCollider != c)
			{
				if (characterCollider.GetComponent<TriggerEventBroadcaster>() == null)
				{
					TriggerEventBroadcaster triggerEventBroadcaster = characterCollider.gameObject.AddComponent<TriggerEventBroadcaster>();
					triggerEventBroadcaster.target = base.gameObject;
				}
				if (lastCollider != null && lastCollider != c && lastCollider != characterCollider)
				{
					TriggerEventBroadcaster component = lastCollider.GetComponent<TriggerEventBroadcaster>();
					if (component != null)
					{
						UnityEngine.Object.Destroy(component);
					}
				}
			}
			lastCollider = characterCollider;
		}

		private void LateUpdate()
		{
			if (!(fullBody == null))
			{
				for (int i = 0; i < interactionEffectors.Length; i++)
				{
					interactionEffectors[i].Update(base.transform, speed);
				}
				for (int j = 0; j < interactionEffectors.Length; j++)
				{
					interactionEffectors[j].ResetToDefaults(resetToDefaultsSpeed * speed);
				}
			}
		}

		private void OnPreFBBIK()
		{
			if (base.enabled && !(fullBody == null))
			{
				lookAt.SolveSpine();
			}
		}

		private void OnPostFBBIK()
		{
			if (base.enabled && !(fullBody == null))
			{
				for (int i = 0; i < interactionEffectors.Length; i++)
				{
					interactionEffectors[i].OnPostFBBIK();
				}
				lookAt.SolveHead();
			}
		}

		private void OnDestroy()
		{
			if (!(fullBody == null))
			{
				IKSolverFullBodyBiped solver = fullBody.solver;
				solver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPreUpdate, new IKSolver.UpdateDelegate(OnPreFBBIK));
				IKSolverFullBodyBiped solver2 = fullBody.solver;
				solver2.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver2.OnPostUpdate, new IKSolver.UpdateDelegate(OnPostFBBIK));
				OnInteractionStart = (InteractionDelegate)Delegate.Remove(OnInteractionStart, new InteractionDelegate(LookAtInteraction));
				OnInteractionPause = (InteractionDelegate)Delegate.Remove(OnInteractionPause, new InteractionDelegate(InteractionPause));
				OnInteractionResume = (InteractionDelegate)Delegate.Remove(OnInteractionResume, new InteractionDelegate(InteractionResume));
				OnInteractionStop = (InteractionDelegate)Delegate.Remove(OnInteractionStop, new InteractionDelegate(InteractionStop));
			}
		}

		private bool IsValid(bool log)
		{
			if (fullBody == null)
			{
				if (log)
				{
					Warning.Log("FBBIK is null. Will not update the InteractionSystem", base.transform);
				}
				return false;
			}
			if (!initiated)
			{
				if (log)
				{
					Warning.Log("The InteractionSystem has not been initiated yet.", base.transform);
				}
				return false;
			}
			return true;
		}

		private bool TriggerIndexIsValid(int index)
		{
			if (index < 0 || index >= triggersInRange.Count)
			{
				Warning.Log("Index out of range.", base.transform);
				return false;
			}
			if (triggersInRange[index] == null)
			{
				Warning.Log("The InteractionTrigger in the list 'inContact' has been destroyed", base.transform);
				return false;
			}
			return true;
		}

		[ContextMenu("User Manual")]
		private void OpenUserManual()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page10.html");
		}

		[ContextMenu("Scrpt Reference")]
		private void OpenScriptReference()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_interaction_system.html");
		}
	}
}
