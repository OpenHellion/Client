using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public class HitReaction : OffsetModifier
	{
		[Serializable]
		public abstract class HitPoint
		{
			[Tooltip("Just for visual clarity, not used at all")]
			public string name;

			[Tooltip("Linking this hit point to a collider")]
			public Collider collider;

			[Tooltip("Only used if this hit point gets hit when already processing another hit")] [SerializeField]
			private float crossFadeTime = 0.1f;

			private float length;

			private float crossFadeSpeed;

			private float lastTime;

			protected float crossFader { get; private set; }

			protected float timer { get; private set; }

			protected Vector3 force { get; private set; }

			protected Vector3 point { get; private set; }

			public void Hit(Vector3 force, Vector3 point)
			{
				if (length == 0f)
				{
					length = GetLength();
				}

				if (length <= 0f)
				{
					Debug.LogError("Hit Point WeightCurve length is zero.");
					return;
				}

				if (timer < 1f)
				{
					crossFader = 0f;
				}

				crossFadeSpeed = ((!(crossFadeTime > 0f)) ? 0f : (1f / crossFadeTime));
				CrossFadeStart();
				timer = 0f;
				this.force = force;
				this.point = point;
			}

			public void Apply(IKSolverFullBodyBiped solver, float weight)
			{
				float num = Time.time - lastTime;
				lastTime = Time.time;
				if (!(timer >= length))
				{
					timer = Mathf.Clamp(timer + num, 0f, length);
					if (crossFadeSpeed > 0f)
					{
						crossFader = Mathf.Clamp(crossFader + num * crossFadeSpeed, 0f, 1f);
					}
					else
					{
						crossFader = 1f;
					}

					OnApply(solver, weight);
				}
			}

			protected abstract float GetLength();

			protected abstract void CrossFadeStart();

			protected abstract void OnApply(IKSolverFullBodyBiped solver, float weight);
		}

		[Serializable]
		public class HitPointEffector : HitPoint
		{
			[Serializable]
			public class EffectorLink
			{
				[Tooltip("The FBBIK effector type")] public FullBodyBipedEffector effector;

				[Tooltip("The weight of this effector (could also be negative)")]
				public float weight;

				private Vector3 lastValue;

				private Vector3 current;

				public void Apply(IKSolverFullBodyBiped solver, Vector3 offset, float crossFader)
				{
					current = Vector3.Lerp(lastValue, offset * weight, crossFader);
					solver.GetEffector(effector).positionOffset += current;
				}

				public void CrossFadeStart()
				{
					lastValue = current;
				}
			}

			[Tooltip("Offset magnitude in the direction of the hit force")]
			public AnimationCurve offsetInForceDirection;

			[Tooltip("Offset magnitude in the direction of character.up")]
			public AnimationCurve offsetInUpDirection;

			[Tooltip("Linking this offset to the FBBIK effectors")]
			public EffectorLink[] effectorLinks;

			protected override float GetLength()
			{
				float num = ((offsetInForceDirection.keys.Length <= 0)
					? 0f
					: offsetInForceDirection.keys[offsetInForceDirection.length - 1].time);
				float min = ((offsetInUpDirection.keys.Length <= 0)
					? 0f
					: offsetInUpDirection.keys[offsetInUpDirection.length - 1].time);
				return Mathf.Clamp(num, min, num);
			}

			protected override void CrossFadeStart()
			{
				EffectorLink[] array = effectorLinks;
				foreach (EffectorLink effectorLink in array)
				{
					effectorLink.CrossFadeStart();
				}
			}

			protected override void OnApply(IKSolverFullBodyBiped solver, float weight)
			{
				Vector3 vector = solver.GetRoot().up * base.force.magnitude;
				Vector3 offset = offsetInForceDirection.Evaluate(base.timer) * base.force +
				                 offsetInUpDirection.Evaluate(base.timer) * vector;
				offset *= weight;
				EffectorLink[] array = effectorLinks;
				foreach (EffectorLink effectorLink in array)
				{
					effectorLink.Apply(solver, offset, base.crossFader);
				}
			}
		}

		[Serializable]
		public class HitPointBone : HitPoint
		{
			[Serializable]
			public class BoneLink
			{
				[Tooltip("Reference to the bone that this hit point rotates")]
				public Transform bone;

				[Tooltip("Weight of rotating the bone")] [Range(0f, 1f)]
				public float weight;

				private Quaternion lastValue = Quaternion.identity;

				private Quaternion current = Quaternion.identity;

				public void Apply(IKSolverFullBodyBiped solver, Quaternion offset, float crossFader)
				{
					current = Quaternion.Lerp(lastValue, Quaternion.Lerp(Quaternion.identity, offset, weight),
						crossFader);
					bone.rotation = current * bone.rotation;
				}

				public void CrossFadeStart()
				{
					lastValue = current;
				}
			}

			[Tooltip("The angle to rotate the bone around it's rigidbody's world center of mass")]
			public AnimationCurve aroundCenterOfMass;

			[Tooltip("Linking this hit point to bone(s)")]
			public BoneLink[] boneLinks;

			private Rigidbody rigidbody;

			protected override float GetLength()
			{
				return (aroundCenterOfMass.keys.Length <= 0)
					? 0f
					: aroundCenterOfMass.keys[aroundCenterOfMass.length - 1].time;
			}

			protected override void CrossFadeStart()
			{
				BoneLink[] array = boneLinks;
				foreach (BoneLink boneLink in array)
				{
					boneLink.CrossFadeStart();
				}
			}

			protected override void OnApply(IKSolverFullBodyBiped solver, float weight)
			{
				if (rigidbody == null)
				{
					rigidbody = collider.GetComponent<Rigidbody>();
				}

				if (rigidbody != null)
				{
					Vector3 axis = Vector3.Cross(base.force, base.point - rigidbody.worldCenterOfMass);
					float angle = aroundCenterOfMass.Evaluate(base.timer) * weight;
					Quaternion offset = Quaternion.AngleAxis(angle, axis);
					BoneLink[] array = boneLinks;
					foreach (BoneLink boneLink in array)
					{
						boneLink.Apply(solver, offset, base.crossFader);
					}
				}
			}
		}

		[Tooltip("Hit points for the FBBIK effectors")]
		public HitPointEffector[] effectorHitPoints;

		[Tooltip(" Hit points for bones without an effector, such as the head")]
		public HitPointBone[] boneHitPoints;

		protected override void OnModifyOffset()
		{
			HitPointEffector[] array = effectorHitPoints;
			foreach (HitPointEffector hitPointEffector in array)
			{
				hitPointEffector.Apply(ik.solver, weight);
			}

			HitPointBone[] array2 = boneHitPoints;
			foreach (HitPointBone hitPointBone in array2)
			{
				hitPointBone.Apply(ik.solver, weight);
			}
		}

		public void Hit(Collider collider, Vector3 force, Vector3 point)
		{
			if (ik == null)
			{
				Debug.LogError("No IK assigned in HitReaction");
				return;
			}

			HitPointEffector[] array = effectorHitPoints;
			foreach (HitPointEffector hitPointEffector in array)
			{
				if (hitPointEffector.collider == collider)
				{
					hitPointEffector.Hit(force, point);
				}
			}

			HitPointBone[] array2 = boneHitPoints;
			foreach (HitPointBone hitPointBone in array2)
			{
				if (hitPointBone.collider == collider)
				{
					hitPointBone.Hit(force, point);
				}
			}
		}
	}
}
