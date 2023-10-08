using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	public class OffsetEffector : OffsetModifier
	{
		[Serializable]
		public class EffectorLink
		{
			public FullBodyBipedEffector effectorType;

			public float weightMultiplier = 1f;

			[HideInInspector] public Vector3 localPosition;
		}

		public EffectorLink[] effectorLinks;

		protected override void Start()
		{
			base.Start();
			EffectorLink[] array = effectorLinks;
			foreach (EffectorLink effectorLink in array)
			{
				effectorLink.localPosition =
					base.transform.InverseTransformPoint(ik.solver.GetEffector(effectorLink.effectorType).bone
						.position);
				if (effectorLink.effectorType == FullBodyBipedEffector.Body)
				{
					ik.solver.bodyEffector.effectChildNodes = false;
				}
			}
		}

		protected override void OnModifyOffset()
		{
			EffectorLink[] array = effectorLinks;
			foreach (EffectorLink effectorLink in array)
			{
				Vector3 vector = base.transform.TransformPoint(effectorLink.localPosition);
				ik.solver.GetEffector(effectorLink.effectorType).positionOffset +=
					(vector - (ik.solver.GetEffector(effectorLink.effectorType).bone.position +
					           ik.solver.GetEffector(effectorLink.effectorType).positionOffset)) * weight *
					effectorLink.weightMultiplier;
			}
		}
	}
}
