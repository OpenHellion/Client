using System;
using UnityEngine;

namespace ProBuilder2.Common
{
	public class pb_UVUtility
	{
		private static Vector2 tvec2 = Vector2.zero;

		public static void PlanarMap2(Vector3[] verts, Vector2[] uvs, int[] indices, pb_UV uvSettings, Vector3 normal)
		{
			ProjectionAxis projectionAxis = pb_Projection.VectorToProjectionAxis(normal);
			pb_Projection.PlanarProject(verts, uvs, indices, normal, projectionAxis);
			ApplyUVSettings(uvs, indices, uvSettings);
		}

		private static void ApplyUVSettings(Vector2[] uvs, int[] indices, pb_UV uvSettings)
		{
			int num = indices.Length;
			switch (uvSettings.fill)
			{
			case pb_UV.Fill.Fit:
				uvs = NormalizeUVs(uvs, indices);
				break;
			case pb_UV.Fill.Stretch:
				uvs = StretchUVs(uvs, indices);
				break;
			}
			if (!uvSettings.useWorldSpace && uvSettings.anchor != pb_UV.Anchor.None)
			{
				ApplyUVAnchor(uvs, indices, uvSettings.anchor);
			}
			if (uvSettings.scale.x != 1f || uvSettings.scale.y != 1f || uvSettings.rotation != 0f)
			{
				Vector2 origin = pb_Bounds2D.Center(uvs, indices);
				for (int i = 0; i < num; i++)
				{
					ref Vector2 reference = ref uvs[indices[i]];
					reference = uvs[indices[i]].ScaleAroundPoint(origin, uvSettings.scale);
					ref Vector2 reference2 = ref uvs[indices[i]];
					reference2 = uvs[indices[i]].RotateAroundPoint(origin, uvSettings.rotation);
				}
			}
			if (uvSettings.flipU || uvSettings.flipV || uvSettings.swapUV)
			{
				for (int j = 0; j < num; j++)
				{
					float num2 = uvs[indices[j]].x;
					float num3 = uvs[indices[j]].y;
					if (uvSettings.flipU)
					{
						num2 = 0f - num2;
					}
					if (uvSettings.flipV)
					{
						num3 = 0f - num3;
					}
					if (!uvSettings.swapUV)
					{
						uvs[indices[j]].x = num2;
						uvs[indices[j]].y = num3;
					}
					else
					{
						uvs[indices[j]].x = num3;
						uvs[indices[j]].y = num2;
					}
				}
			}
			uvSettings.localPivot = pb_Bounds2D.Center(uvs, indices);
			for (int k = 0; k < indices.Length; k++)
			{
				uvs[indices[k]].x -= uvSettings.offset.x;
				uvs[indices[k]].y -= uvSettings.offset.y;
			}
		}

		private static Vector2[] StretchUVs(Vector2[] uvs, int[] indices)
		{
			Vector2 vector = pb_Math.LargestVector2(uvs, indices) - pb_Math.SmallestVector2(uvs, indices);
			for (int i = 0; i < indices.Length; i++)
			{
				uvs[i].x = uvs[indices[i]].x / vector.x;
				uvs[i].y = uvs[indices[i]].y / vector.y;
			}
			return uvs;
		}

		private static Vector2[] NormalizeUVs(Vector2[] uvs, int[] indices)
		{
			int num = indices.Length;
			Vector2 vector = pb_Math.SmallestVector2(uvs, indices);
			for (int i = 0; i < num; i++)
			{
				uvs[indices[i]].x -= vector.x;
				uvs[indices[i]].y -= vector.y;
			}
			float num2 = pb_Math.LargestValue(pb_Math.LargestVector2(uvs, indices));
			for (int i = 0; i < num; i++)
			{
				uvs[indices[i]].x /= num2;
				uvs[indices[i]].y /= num2;
			}
			return uvs;
		}

		[Obsolete("See ApplyAnchor().")]
		private static Vector2[] JustifyUVs(Vector2[] uvs, pb_UV.Justify j)
		{
			Vector2 vector = new Vector2(0f, 0f);
			switch (j)
			{
			case pb_UV.Justify.Left:
				vector = new Vector2(pb_Math.SmallestVector2(uvs).x, 0f);
				break;
			case pb_UV.Justify.Right:
				vector = new Vector2(pb_Math.LargestVector2(uvs).x - 1f, 0f);
				break;
			case pb_UV.Justify.Top:
				vector = new Vector2(0f, pb_Math.LargestVector2(uvs).y - 1f);
				break;
			case pb_UV.Justify.Bottom:
				vector = new Vector2(0f, pb_Math.SmallestVector2(uvs).y);
				break;
			case pb_UV.Justify.Center:
				vector = pb_Math.Average(uvs) - new Vector2(0.5f, 0.5f);
				break;
			}
			for (int i = 0; i < uvs.Length; i++)
			{
				uvs[i] -= vector;
			}
			return uvs;
		}

		private static void ApplyUVAnchor(Vector2[] uvs, int[] indices, pb_UV.Anchor anchor)
		{
			tvec2.x = 0f;
			tvec2.y = 0f;
			Vector2 vector = pb_Math.SmallestVector2(uvs, indices);
			Vector2 vector2 = pb_Math.LargestVector2(uvs, indices);
			switch (anchor)
			{
			case pb_UV.Anchor.UpperLeft:
			case pb_UV.Anchor.MiddleLeft:
			case pb_UV.Anchor.LowerLeft:
				tvec2.x = vector.x;
				break;
			case pb_UV.Anchor.UpperRight:
			case pb_UV.Anchor.MiddleRight:
			case pb_UV.Anchor.LowerRight:
				tvec2.x = vector2.x - 1f;
				break;
			default:
				tvec2.x = vector.x + (vector2.x - vector.x) * 0.5f - 0.5f;
				break;
			}
			switch (anchor)
			{
			case pb_UV.Anchor.UpperLeft:
			case pb_UV.Anchor.UpperCenter:
			case pb_UV.Anchor.UpperRight:
				tvec2.y = vector2.y - 1f;
				break;
			case pb_UV.Anchor.MiddleLeft:
			case pb_UV.Anchor.MiddleCenter:
			case pb_UV.Anchor.MiddleRight:
				tvec2.y = vector.y + (vector2.y - vector.y) * 0.5f - 0.5f;
				break;
			default:
				tvec2.y = vector.y;
				break;
			}
			int num = indices.Length;
			for (int i = 0; i < num; i++)
			{
				uvs[indices[i]].x -= tvec2.x;
				uvs[indices[i]].y -= tvec2.y;
			}
		}
	}
}
