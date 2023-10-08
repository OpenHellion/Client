using System;
using System.Collections;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[RequireComponent(typeof(Animator))]
	public class RagdollUtility : MonoBehaviour
	{
		public class Rigidbone
		{
			public Rigidbody r;

			public Transform t;

			public Collider collider;

			public Joint joint;

			public Rigidbody c;

			public bool updateAnchor;

			public Vector3 deltaPosition;

			public Quaternion deltaRotation;

			public float deltaTime;

			public Vector3 lastPosition;

			public Quaternion lastRotation;

			public Rigidbone(Rigidbody r)
			{
				this.r = r;
				t = r.transform;
				joint = t.GetComponent<Joint>();
				collider = t.GetComponent<Collider>();
				if (joint != null)
				{
					c = joint.connectedBody;
					updateAnchor = c != null;
				}

				lastPosition = t.position;
				lastRotation = t.rotation;
			}

			public void RecordVelocity()
			{
				deltaPosition = t.position - lastPosition;
				lastPosition = t.position;
				deltaRotation = QuaTools.FromToRotation(lastRotation, t.rotation);
				lastRotation = t.rotation;
				deltaTime = Time.deltaTime;
			}

			public void WakeUp(float velocityWeight, float angularVelocityWeight)
			{
				if (updateAnchor)
				{
					joint.connectedAnchor = t.InverseTransformPoint(c.position);
				}

				r.isKinematic = false;
				if (velocityWeight != 0f)
				{
					r.velocity = deltaPosition / deltaTime * velocityWeight;
				}

				if (angularVelocityWeight != 0f)
				{
					float angle = 0f;
					Vector3 axis = Vector3.zero;
					deltaRotation.ToAngleAxis(out angle, out axis);
					angle *= (float)Math.PI / 180f;
					angle /= deltaTime;
					axis *= angle * angularVelocityWeight;
					r.angularVelocity = Vector3.ClampMagnitude(axis, r.maxAngularVelocity);
				}

				r.WakeUp();
			}
		}

		public class Child
		{
			public Transform t;

			public Vector3 localPosition;

			public Quaternion localRotation;

			public Child(Transform transform)
			{
				t = transform;
				localPosition = t.localPosition;
				localRotation = t.localRotation;
			}

			public void FixTransform(float weight)
			{
				if (!(weight <= 0f))
				{
					if (weight >= 1f)
					{
						t.localPosition = localPosition;
						t.localRotation = localRotation;
					}
					else
					{
						t.localPosition = Vector3.Lerp(t.localPosition, localPosition, weight);
						t.localRotation = Quaternion.Lerp(t.localRotation, localRotation, weight);
					}
				}
			}

			public void StoreLocalState()
			{
				localPosition = t.localPosition;
				localRotation = t.localRotation;
			}
		}

		[Tooltip("If you have multiple IK components, then this should be the one that solves last each frame.")]
		public IK ik;

		[Tooltip("How long does it take to blend from ragdoll to animation?")]
		public float ragdollToAnimationTime = 0.2f;

		[Tooltip("If true, IK can be used on top of physical ragdoll simulation.")]
		public bool applyIkOnRagdoll;

		[Tooltip("How much velocity transfer from animation to ragdoll?")]
		public float applyVelocity = 1f;

		[Tooltip("How much angular velocity to transfer from animation to ragdoll?")]
		public float applyAngularVelocity = 1f;

		private Animator animator;

		private Rigidbone[] rigidbones = new Rigidbone[0];

		private Child[] children = new Child[0];

		private bool enableRagdollFlag;

		private AnimatorUpdateMode animatorUpdateMode;

		private IK[] allIKComponents = new IK[0];

		private bool[] fixTransforms = new bool[0];

		private float ragdollWeight;

		private float ragdollWeightV;

		private bool fixedFrame;

		private bool[] disabledIKComponents = new bool[0];

		private bool isRagdoll
		{
			get { return !rigidbones[0].r.isKinematic && !animator.enabled; }
		}

		private bool ikUsed
		{
			get
			{
				if (ik == null)
				{
					return false;
				}

				if (ik.enabled && ik.GetIKSolver().IKPositionWeight > 0f)
				{
					return true;
				}

				IK[] array = allIKComponents;
				foreach (IK iK in array)
				{
					if (iK.enabled && iK.GetIKSolver().IKPositionWeight > 0f)
					{
						return true;
					}
				}

				return false;
			}
		}

		public void EnableRagdoll()
		{
			if (!isRagdoll)
			{
				StopAllCoroutines();
				enableRagdollFlag = true;
			}
		}

		public void DisableRagdoll()
		{
			if (isRagdoll)
			{
				StoreLocalState();
				StopAllCoroutines();
				StartCoroutine(DisableRagdollSmooth());
			}
		}

		public void Start()
		{
			animator = GetComponent<Animator>();
			allIKComponents = GetComponentsInChildren<IK>();
			disabledIKComponents = new bool[allIKComponents.Length];
			fixTransforms = new bool[allIKComponents.Length];
			if (ik != null)
			{
				IKSolver iKSolver = ik.GetIKSolver();
				iKSolver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(iKSolver.OnPostUpdate,
					new IKSolver.UpdateDelegate(AfterLastIK));
			}

			Rigidbody[] componentsInChildren = GetComponentsInChildren<Rigidbody>();
			int num = ((componentsInChildren[0].gameObject == base.gameObject) ? 1 : 0);
			rigidbones = new Rigidbone[(num != 0) ? (componentsInChildren.Length - 1) : componentsInChildren.Length];
			for (int i = 0; i < rigidbones.Length; i++)
			{
				rigidbones[i] = new Rigidbone(componentsInChildren[i + num]);
			}

			Transform[] componentsInChildren2 = GetComponentsInChildren<Transform>();
			children = new Child[componentsInChildren2.Length - 1];
			for (int j = 0; j < children.Length; j++)
			{
				children[j] = new Child(componentsInChildren2[j + 1]);
			}
		}

		private IEnumerator DisableRagdollSmooth()
		{
			for (int i = 0; i < rigidbones.Length; i++)
			{
				rigidbones[i].r.isKinematic = true;
			}

			for (int j = 0; j < allIKComponents.Length; j++)
			{
				allIKComponents[j].fixTransforms = fixTransforms[j];
				if (disabledIKComponents[j])
				{
					allIKComponents[j].enabled = true;
				}
			}

			animator.updateMode = animatorUpdateMode;
			animator.enabled = true;
			while (ragdollWeight > 0f)
			{
				ragdollWeight = Mathf.SmoothDamp(ragdollWeight, 0f, ref ragdollWeightV, ragdollToAnimationTime);
				if (ragdollWeight < 0.001f)
				{
					ragdollWeight = 0f;
				}

				yield return null;
			}

			yield return null;
		}

		private void Update()
		{
			if (!isRagdoll)
			{
				return;
			}

			if (!applyIkOnRagdoll)
			{
				bool flag = false;
				for (int i = 0; i < allIKComponents.Length; i++)
				{
					if (allIKComponents[i].enabled)
					{
						flag = true;
						break;
					}
				}

				if (flag)
				{
					for (int j = 0; j < allIKComponents.Length; j++)
					{
						disabledIKComponents[j] = false;
					}
				}

				for (int k = 0; k < allIKComponents.Length; k++)
				{
					if (allIKComponents[k].enabled)
					{
						allIKComponents[k].enabled = false;
						disabledIKComponents[k] = true;
					}
				}

				return;
			}

			bool flag2 = false;
			for (int l = 0; l < allIKComponents.Length; l++)
			{
				if (disabledIKComponents[l])
				{
					flag2 = true;
					break;
				}
			}

			if (!flag2)
			{
				return;
			}

			for (int m = 0; m < allIKComponents.Length; m++)
			{
				if (disabledIKComponents[m])
				{
					allIKComponents[m].enabled = true;
				}
			}

			for (int n = 0; n < allIKComponents.Length; n++)
			{
				disabledIKComponents[n] = false;
			}
		}

		private void FixedUpdate()
		{
			if (isRagdoll && applyIkOnRagdoll)
			{
				FixTransforms(1f);
			}

			fixedFrame = true;
		}

		private void LateUpdate()
		{
			if (animator.updateMode != AnimatorUpdateMode.AnimatePhysics ||
			    (animator.updateMode == AnimatorUpdateMode.AnimatePhysics && fixedFrame))
			{
				AfterAnimation();
			}

			fixedFrame = false;
			if (!ikUsed)
			{
				OnFinalPose();
			}
		}

		private void AfterLastIK()
		{
			if (ikUsed)
			{
				OnFinalPose();
			}
		}

		private void AfterAnimation()
		{
			if (isRagdoll)
			{
				StoreLocalState();
			}
			else
			{
				FixTransforms(ragdollWeight);
			}
		}

		private void OnFinalPose()
		{
			if (!isRagdoll)
			{
				RecordVelocities();
			}

			if (enableRagdollFlag)
			{
				RagdollEnabler();
			}
		}

		private void RagdollEnabler()
		{
			StoreLocalState();
			for (int i = 0; i < allIKComponents.Length; i++)
			{
				disabledIKComponents[i] = false;
			}

			if (!applyIkOnRagdoll)
			{
				for (int j = 0; j < allIKComponents.Length; j++)
				{
					if (allIKComponents[j].enabled)
					{
						allIKComponents[j].enabled = false;
						disabledIKComponents[j] = true;
					}
				}
			}

			animatorUpdateMode = animator.updateMode;
			animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
			animator.enabled = false;
			for (int k = 0; k < rigidbones.Length; k++)
			{
				rigidbones[k].WakeUp(applyVelocity, applyAngularVelocity);
			}

			for (int l = 0; l < fixTransforms.Length; l++)
			{
				fixTransforms[l] = allIKComponents[l].fixTransforms;
				allIKComponents[l].fixTransforms = false;
			}

			ragdollWeight = 1f;
			ragdollWeightV = 0f;
			enableRagdollFlag = false;
		}

		private void RecordVelocities()
		{
			Rigidbone[] array = rigidbones;
			foreach (Rigidbone rigidbone in array)
			{
				rigidbone.RecordVelocity();
			}
		}

		private void StoreLocalState()
		{
			Child[] array = children;
			foreach (Child child in array)
			{
				child.StoreLocalState();
			}
		}

		private void FixTransforms(float weight)
		{
			Child[] array = children;
			foreach (Child child in array)
			{
				child.FixTransform(weight);
			}
		}

		private void OnDestroy()
		{
			if (ik != null)
			{
				IKSolver iKSolver = ik.GetIKSolver();
				iKSolver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(iKSolver.OnPostUpdate,
					new IKSolver.UpdateDelegate(AfterLastIK));
			}
		}
	}
}
