using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public class AimPoser : MonoBehaviour
	{
		[Serializable]
		public class Pose
		{
			public bool visualize = true;

			public string name;

			public Vector3 direction;

			public float yaw = 75f;

			public float pitch = 45f;

			private float angleBuffer;

			public bool IsInDirection(Vector3 d)
			{
				if (direction == Vector3.zero)
				{
					return false;
				}

				if (yaw <= 0f || pitch <= 0f)
				{
					return false;
				}

				if (yaw < 180f)
				{
					Vector3 vector = new Vector3(direction.x, 0f, direction.z);
					if (vector == Vector3.zero)
					{
						vector = Vector3.forward;
					}

					Vector3 from = new Vector3(d.x, 0f, d.z);
					float num = Vector3.Angle(from, vector);
					if (num > yaw + angleBuffer)
					{
						return false;
					}
				}

				if (pitch >= 180f)
				{
					return true;
				}

				float num2 = Vector3.Angle(Vector3.up, direction);
				float num3 = Vector3.Angle(Vector3.up, d);
				return Mathf.Abs(num3 - num2) < pitch + angleBuffer;
			}

			public void SetAngleBuffer(float value)
			{
				angleBuffer = value;
			}
		}

		public float angleBuffer = 5f;

		public Pose[] poses = new Pose[0];

		public Pose GetPose(Vector3 localDirection)
		{
			if (poses.Length == 0)
			{
				return null;
			}

			for (int i = 0; i < poses.Length - 1; i++)
			{
				if (poses[i].IsInDirection(localDirection))
				{
					return poses[i];
				}
			}

			return poses[poses.Length - 1];
		}

		public void SetPoseActive(Pose pose)
		{
			for (int i = 0; i < poses.Length; i++)
			{
				poses[i].SetAngleBuffer((poses[i] != pose) ? 0f : angleBuffer);
			}
		}
	}
}
