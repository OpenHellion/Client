using UnityEngine;

namespace RootMotion.FinalIK
{
	public class IKExecutionOrder : MonoBehaviour
	{
		public IK[] IKComponents;

		private void Start()
		{
			for (int i = 0; i < IKComponents.Length; i++)
			{
				IKComponents[i].enabled = false;
			}
		}

		private void LateUpdate()
		{
			for (int i = 0; i < IKComponents.Length; i++)
			{
				IKComponents[i].GetIKSolver().Update();
			}
		}
	}
}
