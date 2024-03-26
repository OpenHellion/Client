using UnityEngine;
using ZeroGravity.Math;

namespace ZeroGravity.UI
{
	public class DockingPanelRadarTarget : MonoBehaviour
	{
		public Transform TargetTrans;

		public Transform ParentTrans;

		public float r1 = 37f;

		public float r2 = 56f;

		public float r3 = 88f;

		public float CameraFov = 60f;

		private void Update()
		{
			if (!(TargetTrans == null) && !(ParentTrans == null))
			{
				Vector3 vec = Vector3.ProjectOnPlane(TargetTrans.position - ParentTrans.position, ParentTrans.up);
				float num = MathHelper.AngleSigned(ParentTrans.forward, vec, ParentTrans.up);
				Vector3 vec2 = Vector3.ProjectOnPlane(TargetTrans.position - ParentTrans.position, ParentTrans.right);
				float num2 = MathHelper.AngleSigned(ParentTrans.forward, vec2, ParentTrans.right);
				Vector3 vector =
					Vector3.ProjectOnPlane(TargetTrans.position - ParentTrans.position, ParentTrans.forward);
				float f = Mathf.Atan2(num, num2) + (float)System.Math.PI / 2f;
				float num3 = 1f;
				num2 = Mathf.Abs(num2);
				num = Mathf.Abs(num);
				float num4 = Mathf.Max(num, num2);
				float num5 = CameraFov / 2f;
				num3 = ((num4 < num5)
					? (r1 * MathHelper.ProportionalValue(num4, 0f, num5, 0f, 1f))
					: ((!(num4 >= num5) || !(num4 < 90f))
						? (r2 + (r3 - r2) * MathHelper.ProportionalValue(num4, 90f, 180f, 0f, 1f))
						: (r1 + (r2 - r1) * MathHelper.ProportionalValue(num4, num5, 90f, 0f, 1f))));
				float y = -1f * Mathf.Sin(f) * num3;
				float x = -1f * Mathf.Cos(f) * num3;
				transform.localPosition = new Vector2(x, y);
			}
		}
	}
}
