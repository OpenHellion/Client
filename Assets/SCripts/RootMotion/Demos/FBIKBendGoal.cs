using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	public class FBIKBendGoal : MonoBehaviour
	{
		public FullBodyBipedIK ik;

		public FullBodyBipedChain chain;

		public float weight;

		private void Start()
		{
			Debug.Log("FBIKBendGoal is deprecated, you can now a bend goal from the custom inspector of the FullBodyBipedIK component.");
		}

		private void Update()
		{
			if (!(ik == null))
			{
				ik.solver.GetBendConstraint(chain).bendGoal = base.transform;
				ik.solver.GetBendConstraint(chain).weight = weight;
			}
		}
	}
}
