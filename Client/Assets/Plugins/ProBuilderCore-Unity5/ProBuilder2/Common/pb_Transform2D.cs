using UnityEngine;

namespace ProBuilder2.Common
{
	public class pb_Transform2D
	{
		public Vector2 position;

		public float rotation;

		public Vector2 scale;

		public pb_Transform2D(Vector2 position, float rotation, Vector2 scale)
		{
			this.position = position;
			this.rotation = rotation;
			this.scale = scale;
		}

		public Vector2 TransformPoint(Vector2 p)
		{
			p += position;
			p.RotateAroundPoint(p, rotation);
			p.ScaleAroundPoint(p, scale);
			return p;
		}

		public override string ToString()
		{
			return string.Concat("T: ", position, "\nR: ", rotation, '°', "\nS: ", scale);
		}
	}
}
