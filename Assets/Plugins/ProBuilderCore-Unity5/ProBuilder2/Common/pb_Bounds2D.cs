using System.Collections.Generic;
using UnityEngine;

namespace ProBuilder2.Common
{
	public class pb_Bounds2D
	{
		public Vector2 center = Vector2.zero;

		[SerializeField]
		private Vector2 _size = Vector2.zero;

		[SerializeField]
		private Vector2 _extents = Vector2.zero;

		public Vector2 size
		{
			get
			{
				return _size;
			}
			set
			{
				_size = value;
				_extents.x = _size.x * 0.5f;
				_extents.y = _size.y * 0.5f;
			}
		}

		public Vector2 extents => _extents;

		public Vector2[] corners => new Vector2[4]
		{
			new Vector2(center.x - extents.x, center.y + extents.y),
			new Vector2(center.x + extents.x, center.y + extents.y),
			new Vector2(center.x - extents.x, center.y - extents.y),
			new Vector2(center.x + extents.x, center.y - extents.y)
		};

		public pb_Bounds2D()
		{
		}

		public pb_Bounds2D(Vector2 center, Vector2 size)
		{
			this.center = center;
			this.size = size;
		}

		public pb_Bounds2D(Vector2[] points)
		{
			SetWithPoints(points);
		}

		public pb_Bounds2D(Vector2[] points, int[] indices)
		{
			SetWithPoints(points, indices);
		}

		public pb_Bounds2D(Vector2[] points, pb_Edge[] edges)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			if (points.Length > 0 && edges.Length > 0)
			{
				num = points[edges[0].x].x;
				num3 = points[edges[0].x].y;
				num2 = num;
				num4 = num3;
				for (int i = 0; i < edges.Length; i++)
				{
					num = Mathf.Min(num, points[edges[i].x].x);
					num = Mathf.Min(num, points[edges[i].y].x);
					num3 = Mathf.Min(num3, points[edges[i].x].y);
					num3 = Mathf.Min(num3, points[edges[i].y].y);
					num2 = Mathf.Max(num2, points[edges[i].x].x);
					num2 = Mathf.Max(num2, points[edges[i].y].x);
					num4 = Mathf.Max(num4, points[edges[i].x].y);
					num4 = Mathf.Max(num4, points[edges[i].y].y);
				}
			}
			center = new Vector2((num + num2) / 2f, (num3 + num4) / 2f);
			size = new Vector3(num2 - num, num4 - num3);
		}

		public pb_Bounds2D(Vector2[] points, int length)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			if (points.Length > 0)
			{
				num = points[0].x;
				num3 = points[0].y;
				num2 = num;
				num4 = num3;
				for (int i = 1; i < length; i++)
				{
					num = Mathf.Min(num, points[i].x);
					num3 = Mathf.Min(num3, points[i].y);
					num2 = Mathf.Max(num2, points[i].x);
					num4 = Mathf.Max(num4, points[i].y);
				}
			}
			center = new Vector2((num + num2) / 2f, (num3 + num4) / 2f);
			size = new Vector3(num2 - num, num4 - num3);
		}

		public bool ContainsPoint(Vector2 point)
		{
			return !(point.x > center.x + extents.x) && !(point.x < center.x - extents.x) && !(point.y > center.y + extents.y) && !(point.y < center.y - extents.y);
		}

		public bool IntersectsLineSegment(Vector2 lineStart, Vector2 lineEnd)
		{
			if (ContainsPoint(lineStart) || ContainsPoint(lineEnd))
			{
				return true;
			}
			Vector2[] array = corners;
			return pb_Math.GetLineSegmentIntersect(array[0], array[1], lineStart, lineEnd) || pb_Math.GetLineSegmentIntersect(array[1], array[3], lineStart, lineEnd) || pb_Math.GetLineSegmentIntersect(array[3], array[2], lineStart, lineEnd) || pb_Math.GetLineSegmentIntersect(array[2], array[0], lineStart, lineEnd);
		}

		public bool Intersects(pb_Bounds2D bounds)
		{
			Vector2 vector = center - bounds.center;
			Vector2 vector2 = size + bounds.size;
			return Mathf.Abs(vector.x) * 2f < vector2.x && Mathf.Abs(vector.y) * 2f < vector2.y;
		}

		public bool Intersects(Rect rect)
		{
			Vector2 vector = center - rect.center;
			Vector2 vector2 = size + rect.size;
			return Mathf.Abs(vector.x) * 2f < vector2.x && Mathf.Abs(vector.y) * 2f < vector2.y;
		}

		public void SetWithPoints(IList<Vector2> points)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			int count = points.Count;
			if (count > 0)
			{
				num = points[0].x;
				num3 = points[0].y;
				num2 = num;
				num4 = num3;
				for (int i = 1; i < count; i++)
				{
					float x = points[i].x;
					float y = points[i].y;
					if (x < num)
					{
						num = x;
					}
					if (x > num2)
					{
						num2 = x;
					}
					if (y < num3)
					{
						num3 = y;
					}
					if (y > num4)
					{
						num4 = y;
					}
				}
			}
			center.x = (num + num2) / 2f;
			center.y = (num3 + num4) / 2f;
			_size.x = num2 - num;
			_size.y = num4 - num3;
			_extents.x = _size.x * 0.5f;
			_extents.y = _size.y * 0.5f;
		}

		public void SetWithPoints(IList<Vector2> points, IList<int> indices)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			if (points.Count > 0 && indices.Count > 0)
			{
				num = points[indices[0]].x;
				num3 = points[indices[0]].y;
				num2 = num;
				num4 = num3;
				for (int i = 1; i < indices.Count; i++)
				{
					float x = points[indices[i]].x;
					float y = points[indices[i]].y;
					if (x < num)
					{
						num = x;
					}
					if (x > num2)
					{
						num2 = x;
					}
					if (y < num3)
					{
						num3 = y;
					}
					if (y > num4)
					{
						num4 = y;
					}
				}
			}
			center.x = (num + num2) / 2f;
			center.y = (num3 + num4) / 2f;
			_size.x = num2 - num;
			_size.y = num4 - num3;
			_extents.x = _size.x * 0.5f;
			_extents.y = _size.y * 0.5f;
		}

		public static Vector2 Center(Vector2[] points, int length = -1)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			int num5 = ((length >= 1) ? length : points.Length);
			num = points[0].x;
			num3 = points[0].y;
			num2 = num;
			num4 = num3;
			for (int i = 1; i < num5; i++)
			{
				float x = points[i].x;
				float y = points[i].y;
				if (x < num)
				{
					num = x;
				}
				if (x > num2)
				{
					num2 = x;
				}
				if (y < num3)
				{
					num3 = y;
				}
				if (y > num4)
				{
					num4 = y;
				}
			}
			return new Vector2((num + num2) / 2f, (num3 + num4) / 2f);
		}

		public static Vector2 Center(Vector2[] points, int[] indices)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			int num5 = indices.Length;
			num = points[indices[0]].x;
			num3 = points[indices[0]].y;
			num2 = num;
			num4 = num3;
			for (int i = 1; i < num5; i++)
			{
				float x = points[indices[i]].x;
				float y = points[indices[i]].y;
				if (x < num)
				{
					num = x;
				}
				if (x > num2)
				{
					num2 = x;
				}
				if (y < num3)
				{
					num3 = y;
				}
				if (y > num4)
				{
					num4 = y;
				}
			}
			return new Vector2((num + num2) / 2f, (num3 + num4) / 2f);
		}

		public override string ToString()
		{
			return string.Concat("[cen: ", center, " size: ", size, "]");
		}
	}
}
