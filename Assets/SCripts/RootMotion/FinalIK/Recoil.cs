using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public class Recoil : OffsetModifier
	{
		[Serializable]
		public class RecoilOffset
		{
			[Serializable]
			public class EffectorLink
			{
				[Tooltip("Type of the FBBIK effector to use")]
				public FullBodyBipedEffector effector;

				[Tooltip("Weight of using this effector")]
				public float weight;
			}

			[Tooltip("Offset vector for the associated effector when doing recoil.")]
			public Vector3 offset;

			[Tooltip("When firing before the last recoil has faded, how much of the current recoil offset will be maintained?")]
			[Range(0f, 1f)]
			public float additivity = 1f;

			[Tooltip("Max additive recoil for automatic fire.")]
			public float maxAdditiveOffsetMag = 0.2f;

			[Tooltip("Linking this recoil offset to FBBIK effectors.")]
			public EffectorLink[] effectorLinks;

			private Vector3 additiveOffset;

			private Vector3 lastOffset;

			public void Start()
			{
				if (!(additivity <= 0f))
				{
					additiveOffset = Vector3.ClampMagnitude(lastOffset * additivity, maxAdditiveOffsetMag);
				}
			}

			public void Apply(IKSolverFullBodyBiped solver, Quaternion rotation, float masterWeight, float length, float timeLeft)
			{
				additiveOffset = Vector3.Lerp(Vector3.zero, additiveOffset, timeLeft / length);
				lastOffset = rotation * (offset * masterWeight) + rotation * additiveOffset;
				EffectorLink[] array = effectorLinks;
				foreach (EffectorLink effectorLink in array)
				{
					solver.GetEffector(effectorLink.effector).positionOffset += lastOffset * effectorLink.weight;
				}
			}
		}

		[Serializable]
		public enum Handedness
		{
			Right = 0,
			Left = 1
		}

		[Tooltip("Reference to the AimIK component. Optional, only used to getting the aiming direction.")]
		public AimIK aimIK;

		[Tooltip("Which hand is holding the weapon?")]
		public Handedness handedness;

		[Tooltip("Check for 2-handed weapons.")]
		public bool twoHanded = true;

		[Tooltip("Weight curve for the recoil offsets. Recoil procedure is as long as this curve.")]
		public AnimationCurve recoilWeight;

		[Tooltip("How much is the magnitude randomized each time Recoil is called?")]
		public float magnitudeRandom = 0.1f;

		[Tooltip("How much is the rotation randomized each time Recoil is called?")]
		public Vector3 rotationRandom;

		[Tooltip("Rotating the primary hand bone for the recoil (in local space).")]
		public Vector3 handRotationOffset;

		[Tooltip("Time of blending in another recoil when doing automatic fire.")]
		public float blendTime;

		[Space(10f)]
		[Tooltip("FBBIK effector position offsets for the recoil (in aiming direction space).")]
		public RecoilOffset[] offsets;

		private float magnitudeMlp = 1f;

		private float endTime = -1f;

		private Quaternion handRotation;

		private Quaternion secondaryHandRelativeRotation;

		private Quaternion randomRotation;

		private float length = 1f;

		private bool initiated;

		private float blendWeight;

		private float w;

		private IKEffector primaryHandEffector
		{
			get
			{
				if (handedness == Handedness.Right)
				{
					return ik.solver.rightHandEffector;
				}
				return ik.solver.leftHandEffector;
			}
		}

		private IKEffector secondaryHandEffector
		{
			get
			{
				if (handedness == Handedness.Right)
				{
					return ik.solver.leftHandEffector;
				}
				return ik.solver.rightHandEffector;
			}
		}

		private Transform primaryHand
		{
			get
			{
				return primaryHandEffector.bone;
			}
		}

		private Transform secondaryHand
		{
			get
			{
				return secondaryHandEffector.bone;
			}
		}

		public void Fire(float magnitude)
		{
			float num = magnitude * UnityEngine.Random.value * magnitudeRandom;
			magnitudeMlp = magnitude + num;
			randomRotation = Quaternion.Euler(rotationRandom * UnityEngine.Random.value);
			RecoilOffset[] array = offsets;
			foreach (RecoilOffset recoilOffset in array)
			{
				recoilOffset.Start();
			}
			if (Time.time < endTime)
			{
				blendWeight = 0f;
			}
			else
			{
				blendWeight = 1f;
			}
			Keyframe[] keys = recoilWeight.keys;
			length = keys[keys.Length - 1].time;
			endTime = Time.time + length;
		}

		protected override void OnModifyOffset()
		{
			if (!(Time.time >= endTime))
			{
				if (!initiated && ik != null)
				{
					initiated = true;
					IKSolverFullBodyBiped solver = ik.solver;
					solver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPostUpdate, new IKSolver.UpdateDelegate(AfterFBBIK));
				}
				blendTime = Mathf.Max(blendTime, 0f);
				if (blendTime > 0f)
				{
					blendWeight = Mathf.Min(blendWeight + Time.deltaTime * (1f / blendTime), 1f);
				}
				else
				{
					blendWeight = 1f;
				}
				float b = recoilWeight.Evaluate(length - (endTime - Time.time)) * magnitudeMlp;
				w = Mathf.Lerp(w, b, blendWeight);
				Quaternion quaternion = ((!(aimIK != null)) ? ik.references.root.rotation : Quaternion.LookRotation(aimIK.solver.IKPosition - aimIK.solver.transform.position, ik.references.root.up));
				quaternion = randomRotation * quaternion;
				RecoilOffset[] array = offsets;
				foreach (RecoilOffset recoilOffset in array)
				{
					recoilOffset.Apply(ik.solver, quaternion, w, length, endTime - Time.time);
				}
				Quaternion quaternion2 = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(randomRotation * primaryHand.rotation * handRotationOffset), w);
				handRotation = quaternion2 * primaryHand.rotation;
				if (twoHanded)
				{
					Vector3 vector = primaryHand.InverseTransformPoint(secondaryHand.position);
					secondaryHandRelativeRotation = Quaternion.Inverse(primaryHand.rotation) * secondaryHand.rotation;
					Vector3 vector2 = primaryHand.position + primaryHandEffector.positionOffset;
					Vector3 vector3 = vector2 + handRotation * vector;
					secondaryHandEffector.positionOffset += vector3 - (secondaryHand.position + secondaryHandEffector.positionOffset);
				}
			}
		}

		private void AfterFBBIK()
		{
			if (!(Time.time >= endTime))
			{
				primaryHand.rotation = handRotation;
				if (twoHanded)
				{
					secondaryHand.rotation = primaryHand.rotation * secondaryHandRelativeRotation;
				}
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (ik != null && initiated)
			{
				IKSolverFullBodyBiped solver = ik.solver;
				solver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPostUpdate, new IKSolver.UpdateDelegate(AfterFBBIK));
			}
		}
	}
}
