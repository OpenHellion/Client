using UnityEngine;

namespace RootMotion.FinalIK
{
	public abstract class Grounder : MonoBehaviour
	{
		public delegate void GrounderDelegate();

		[Tooltip("The master weight. Use this to fade in/out the grounding effect.")]
		[Range(0f, 1f)]
		public float weight = 1f;

		[Tooltip("The Grounding solver. Not to confuse with IK solvers.")]
		public Grounding solver = new Grounding();

		public GrounderDelegate OnPreGrounder;

		public GrounderDelegate OnPostGrounder;

		protected bool initiated;

		public abstract void Reset();

		protected Vector3 GetSpineOffsetTarget()
		{
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < solver.legs.Length; i++)
			{
				zero += GetLegSpineBendVector(solver.legs[i]);
			}
			return zero;
		}

		protected void LogWarning(string message)
		{
			Warning.Log(message, base.transform);
		}

		private Vector3 GetLegSpineBendVector(Grounding.Leg leg)
		{
			Vector3 legSpineTangent = GetLegSpineTangent(leg);
			float num = (Vector3.Dot(solver.root.forward, legSpineTangent.normalized) + 1f) * 0.5f;
			float magnitude = (leg.IKPosition - leg.transform.position).magnitude;
			return legSpineTangent * magnitude * num;
		}

		private Vector3 GetLegSpineTangent(Grounding.Leg leg)
		{
			Vector3 tangent = leg.transform.position - solver.root.position;
			if (!solver.rotateSolver || solver.root.up == Vector3.up)
			{
				return new Vector3(tangent.x, 0f, tangent.z);
			}
			Vector3 normal = solver.root.up;
			Vector3.OrthoNormalize(ref normal, ref tangent);
			return tangent;
		}

		protected abstract void OpenUserManual();

		protected abstract void OpenScriptReference();
	}
}
