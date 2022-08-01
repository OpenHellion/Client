using UnityEngine;

namespace ThreeEyedGames.DecaliciousExample
{
	public class SwingIt : MonoBehaviour
	{
		public float MaxAngle = 0.1f;

		public float Speed = 1f;

		private void Update()
		{
			Vector3 localEulerAngles = base.transform.localEulerAngles;
			localEulerAngles.z = Mathf.Sin(Time.time * Speed) * MaxAngle;
			base.transform.localEulerAngles = localEulerAngles;
		}
	}
}
