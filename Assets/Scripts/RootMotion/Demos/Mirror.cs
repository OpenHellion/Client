using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	public class Mirror : MonoBehaviour
	{
		public Transform target;

		private Transform[] children = new Transform[0];

		private Transform[] targetChildren = new Transform[0];

		private FullBodyBipedIK ik;

		private void Start()
		{
			if (target.gameObject.activeInHierarchy && targetChildren.Length <= 0)
			{
				children = GetComponentsInChildren<Transform>();
				targetChildren = target.GetComponentsInChildren<Transform>();
				ik = target.GetComponent<FullBodyBipedIK>();
				if (ik != null)
				{
					IKSolverFullBodyBiped solver = ik.solver;
					solver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPostUpdate,
						new IKSolver.UpdateDelegate(OnPostFBBIK));
				}
			}
		}

		private void OnPostFBBIK()
		{
			for (int i = 1; i < children.Length; i++)
			{
				for (int j = 1; j < targetChildren.Length; j++)
				{
					if (children[i].name == targetChildren[j].name)
					{
						children[i].localPosition = targetChildren[j].localPosition;
						children[i].localRotation = targetChildren[j].localRotation;
						break;
					}
				}
			}
		}

		private void OnDestroy()
		{
			if (ik != null)
			{
				IKSolverFullBodyBiped solver = ik.solver;
				solver.OnPostUpdate =
					(IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPostUpdate,
						new IKSolver.UpdateDelegate(OnPostFBBIK));
			}
		}
	}
}
