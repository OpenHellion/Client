using UnityEngine;

namespace RootMotion.Demos
{
	public class UserControlAI : UserControlThirdPerson
	{
		public Transform moveTarget;

		public float stoppingDistance = 0.5f;

		public float stoppingThreshold = 1.5f;

		protected override void Update()
		{
			float num = ((!walkByDefault) ? 1f : 0.5f);
			Vector3 vector = moveTarget.position - base.transform.position;
			vector.y = 0f;
			float num2 = ((!(state.move != Vector3.zero)) ? (stoppingDistance * stoppingThreshold) : stoppingDistance);
			state.move = ((!(vector.magnitude > num2)) ? Vector3.zero : (vector.normalized * num));
		}
	}
}
