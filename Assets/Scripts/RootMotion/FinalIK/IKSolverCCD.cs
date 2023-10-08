using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class IKSolverCCD : IKSolverHeuristic
	{
		public IterationDelegate OnPreIteration;

		public void FadeOutBoneWeights()
		{
			if (bones.Length >= 2)
			{
				bones[0].weight = 1f;
				float num = 1f / (float)(bones.Length - 1);
				for (int i = 1; i < bones.Length; i++)
				{
					bones[i].weight = num * (float)(bones.Length - 1 - i);
				}
			}
		}

		protected override void OnInitiate()
		{
			if (firstInitiation || !Application.isPlaying)
			{
				IKPosition = bones[bones.Length - 1].transform.position;
			}

			InitiateBones();
		}

		protected override void OnUpdate()
		{
			if (IKPositionWeight <= 0f)
			{
				return;
			}

			IKPositionWeight = Mathf.Clamp(IKPositionWeight, 0f, 1f);
			if (target != null)
			{
				IKPosition = target.position;
			}

			if (XY)
			{
				IKPosition.z = bones[0].transform.position.z;
			}

			Vector3 vector = ((maxIterations <= 1) ? Vector3.zero : GetSingularityOffset());
			for (int i = 0;
			     i < maxIterations && (!(vector == Vector3.zero) || i < 1 || !(tolerance > 0f) ||
			                           !(base.positionOffset < tolerance * tolerance));
			     i++)
			{
				lastLocalDirection = localDirection;
				if (OnPreIteration != null)
				{
					OnPreIteration(i);
				}

				Solve(IKPosition + ((i != 0) ? Vector3.zero : vector));
			}

			lastLocalDirection = localDirection;
		}

		private void Solve(Vector3 targetPosition)
		{
			if (XY)
			{
				for (int num = bones.Length - 2; num > -1; num--)
				{
					float num2 = bones[num].weight * IKPositionWeight;
					if (num2 > 0f)
					{
						Vector3 vector = bones[bones.Length - 1].transform.position - bones[num].transform.position;
						Vector3 vector2 = targetPosition - bones[num].transform.position;
						float current = Mathf.Atan2(vector.x, vector.y) * 57.29578f;
						float num3 = Mathf.Atan2(vector2.x, vector2.y) * 57.29578f;
						bones[num].transform.rotation =
							Quaternion.AngleAxis(Mathf.DeltaAngle(current, num3) * num2, Vector3.back) *
							bones[num].transform.rotation;
					}

					if (useRotationLimits && bones[num].rotationLimit != null)
					{
						bones[num].rotationLimit.Apply();
					}
				}

				return;
			}

			for (int num4 = bones.Length - 2; num4 > -1; num4--)
			{
				float num5 = bones[num4].weight * IKPositionWeight;
				if (num5 > 0f)
				{
					Vector3 fromDirection = bones[bones.Length - 1].transform.position - bones[num4].transform.position;
					Vector3 toDirection = targetPosition - bones[num4].transform.position;
					Quaternion quaternion = Quaternion.FromToRotation(fromDirection, toDirection) *
					                        bones[num4].transform.rotation;
					if (num5 >= 1f)
					{
						bones[num4].transform.rotation = quaternion;
					}
					else
					{
						bones[num4].transform.rotation =
							Quaternion.Lerp(bones[num4].transform.rotation, quaternion, num5);
					}
				}

				if (useRotationLimits && bones[num4].rotationLimit != null)
				{
					bones[num4].rotationLimit.Apply();
				}
			}
		}
	}
}
