using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProBuilder2.Common
{
	public class pb_Vertex : IEquatable<pb_Vertex>
	{
		public Vector3 position;

		public Color color;

		public Vector3 normal;

		public Vector4 tangent;

		public Vector2 uv0;

		public Vector2 uv2;

		public Vector4 uv3;

		public Vector4 uv4;

		public bool hasPosition;

		public bool hasColor;

		public bool hasNormal;

		public bool hasTangent;

		public bool hasUv0;

		public bool hasUv2;

		public bool hasUv3;

		public bool hasUv4;

		public pb_Vertex(bool hasAllValues = false)
		{
			hasPosition = hasAllValues;
			hasColor = hasAllValues;
			hasNormal = hasAllValues;
			hasTangent = hasAllValues;
			hasUv0 = hasAllValues;
			hasUv2 = hasAllValues;
			hasUv3 = hasAllValues;
			hasUv4 = hasAllValues;
		}

		public pb_Vertex(pb_Vertex v)
		{
			position = v.position;
			hasPosition = v.hasPosition;
			color = v.color;
			hasColor = v.hasColor;
			uv0 = v.uv0;
			hasUv0 = v.hasUv0;
			normal = v.normal;
			hasNormal = v.hasNormal;
			tangent = v.tangent;
			hasTangent = v.hasTangent;
			uv2 = v.uv2;
			hasUv2 = v.hasUv2;
			uv3 = v.uv3;
			hasUv3 = v.hasUv3;
			uv4 = v.uv4;
			hasUv4 = v.hasUv4;
		}

		public override bool Equals(object other)
		{
			return other is pb_Vertex && Equals(other as pb_Vertex);
		}

		public bool Equals(pb_Vertex other)
		{
			if (other == null)
			{
				return false;
			}
			return position.Approx3(other.position) && color.ApproxC(other.color) && normal.Approx3(other.normal) && tangent.Approx4(other.tangent) && uv0.Approx2(other.uv0) && uv2.Approx2(other.uv2) && uv3.Approx4(other.uv3) && uv4.Approx4(other.uv4);
		}

		public override int GetHashCode()
		{
			int num = 783 + pb_Vector.GetHashCode(position);
			num = num * 29 + pb_Vector.GetHashCode(uv0);
			return num * 31 + pb_Vector.GetHashCode(normal);
		}

		public static pb_Vertex operator +(pb_Vertex a, pb_Vertex b)
		{
			pb_Vertex pb_Vertex2 = new pb_Vertex(a);
			pb_Vertex2.Add(b);
			return pb_Vertex2;
		}

		public void Add(pb_Vertex b)
		{
			position += b.position;
			color += b.color;
			normal += b.normal;
			tangent += b.tangent;
			uv0 += b.uv0;
			uv2 += b.uv2;
			uv3 += b.uv3;
			uv4 += b.uv4;
		}

		public static pb_Vertex operator -(pb_Vertex a, pb_Vertex b)
		{
			pb_Vertex pb_Vertex2 = new pb_Vertex(a);
			pb_Vertex2.Subtract(b);
			return pb_Vertex2;
		}

		public void Subtract(pb_Vertex b)
		{
			position -= b.position;
			color -= b.color;
			normal -= b.normal;
			tangent -= b.tangent;
			uv0 -= b.uv0;
			uv2 -= b.uv2;
			uv3 -= b.uv3;
			uv4 -= b.uv4;
		}

		public static pb_Vertex operator *(pb_Vertex a, float value)
		{
			pb_Vertex pb_Vertex2 = new pb_Vertex(a);
			pb_Vertex2.Multiply(value);
			return pb_Vertex2;
		}

		public void Multiply(float value)
		{
			position *= value;
			color *= value;
			normal *= value;
			tangent *= value;
			uv0 *= value;
			uv2 *= value;
			uv3 *= value;
			uv4 *= value;
		}

		public static pb_Vertex operator /(pb_Vertex a, float value)
		{
			pb_Vertex pb_Vertex2 = new pb_Vertex(a);
			pb_Vertex2.Divide(value);
			return pb_Vertex2;
		}

		public void Divide(float value)
		{
			position /= value;
			color /= value;
			normal /= value;
			tangent /= value;
			uv0 /= value;
			uv2 /= value;
			uv3 /= value;
			uv4 /= value;
		}

		public void Normalize()
		{
			position.Normalize();
			Vector4 vector = color;
			vector.Normalize();
			color.r = vector.x;
			color.g = vector.y;
			color.b = vector.z;
			color.a = vector.w;
			normal.Normalize();
			tangent.Normalize();
			uv0.Normalize();
			uv2.Normalize();
			uv3.Normalize();
			uv4.Normalize();
		}

		public override string ToString()
		{
			return position.ToString();
		}

		public static pb_Vertex[] GetVertices(pb_Object pb, IList<int> indices = null)
		{
			int vertexCount = pb.vertexCount;
			int num = indices?.Count ?? pb.vertexCount;
			pb_Vertex[] array = new pb_Vertex[num];
			Vector3[] vertices = pb.vertices;
			Color[] colors = pb.colors;
			Vector2[] uv = pb.uv;
			Vector3[] normals = pb.msh.normals;
			Vector4[] tangents = pb.msh.tangents;
			Vector2[] array2 = pb.msh.uv2;
			List<Vector4> list = new List<Vector4>();
			List<Vector4> list2 = new List<Vector4>();
			pb.GetUVs(2, list);
			pb.GetUVs(3, list2);
			bool flag = vertices != null && vertices.Count() == vertexCount;
			bool flag2 = colors != null && colors.Count() == vertexCount;
			bool flag3 = normals != null && normals.Count() == vertexCount;
			bool flag4 = tangents != null && tangents.Count() == vertexCount;
			bool flag5 = uv != null && uv.Count() == vertexCount;
			bool flag6 = array2 != null && array2.Count() == vertexCount;
			bool flag7 = list != null && list.Count() == vertexCount;
			bool flag8 = list2 != null && list2.Count() == vertexCount;
			for (int i = 0; i < num; i++)
			{
				array[i] = new pb_Vertex();
				int num2 = indices?[i] ?? i;
				if (flag)
				{
					array[i].hasPosition = true;
					array[i].position = vertices[num2];
				}
				if (flag2)
				{
					array[i].hasColor = true;
					array[i].color = colors[num2];
				}
				if (flag3)
				{
					array[i].hasNormal = true;
					array[i].normal = normals[num2];
				}
				if (flag4)
				{
					array[i].hasTangent = true;
					array[i].tangent = tangents[num2];
				}
				if (flag5)
				{
					array[i].hasUv0 = true;
					array[i].uv0 = uv[num2];
				}
				if (flag6)
				{
					array[i].hasUv2 = true;
					array[i].uv2 = array2[num2];
				}
				if (flag7)
				{
					array[i].hasUv3 = true;
					array[i].uv3 = list[num2];
				}
				if (flag8)
				{
					array[i].hasUv4 = true;
					array[i].uv4 = list2[num2];
				}
			}
			return array;
		}

		public static pb_Vertex[] GetVertices(Mesh m)
		{
			if (m == null)
			{
				return null;
			}
			int vertexCount = m.vertexCount;
			pb_Vertex[] array = new pb_Vertex[vertexCount];
			Vector3[] vertices = m.vertices;
			Color[] colors = m.colors;
			Vector3[] normals = m.normals;
			Vector4[] tangents = m.tangents;
			Vector2[] uv = m.uv;
			Vector2[] array2 = m.uv2;
			List<Vector4> list = new List<Vector4>();
			List<Vector4> list2 = new List<Vector4>();
			m.GetUVs(2, list);
			m.GetUVs(3, list2);
			bool flag = vertices != null && vertices.Count() == vertexCount;
			bool flag2 = colors != null && colors.Count() == vertexCount;
			bool flag3 = normals != null && normals.Count() == vertexCount;
			bool flag4 = tangents != null && tangents.Count() == vertexCount;
			bool flag5 = uv != null && uv.Count() == vertexCount;
			bool flag6 = array2 != null && array2.Count() == vertexCount;
			bool flag7 = list != null && list.Count() == vertexCount;
			bool flag8 = list2 != null && list2.Count() == vertexCount;
			for (int i = 0; i < vertexCount; i++)
			{
				array[i] = new pb_Vertex();
				if (flag)
				{
					array[i].hasPosition = true;
					array[i].position = vertices[i];
				}
				if (flag2)
				{
					array[i].hasColor = true;
					array[i].color = colors[i];
				}
				if (flag3)
				{
					array[i].hasNormal = true;
					array[i].normal = normals[i];
				}
				if (flag4)
				{
					array[i].hasTangent = true;
					array[i].tangent = tangents[i];
				}
				if (flag5)
				{
					array[i].hasUv0 = true;
					array[i].uv0 = uv[i];
				}
				if (flag6)
				{
					array[i].hasUv2 = true;
					array[i].uv2 = array2[i];
				}
				if (flag7)
				{
					array[i].hasUv3 = true;
					array[i].uv3 = list[i];
				}
				if (flag8)
				{
					array[i].hasUv4 = true;
					array[i].uv4 = list2[i];
				}
			}
			return array;
		}

		public static void GetArrays(IList<pb_Vertex> vertices, out Vector3[] position, out Color[] color, out Vector2[] uv0, out Vector3[] normal, out Vector4[] tangent, out Vector2[] uv2, out List<Vector4> uv3, out List<Vector4> uv4)
		{
			GetArrays(vertices, out position, out color, out uv0, out normal, out tangent, out uv2, out uv3, out uv4, AttributeType.All);
		}

		public static void GetArrays(IList<pb_Vertex> vertices, out Vector3[] position, out Color[] color, out Vector2[] uv0, out Vector3[] normal, out Vector4[] tangent, out Vector2[] uv2, out List<Vector4> uv3, out List<Vector4> uv4, AttributeType attributes)
		{
			int count = vertices.Count;
			bool flag = (attributes & AttributeType.Position) == AttributeType.Position;
			bool flag2 = (attributes & AttributeType.Color) == AttributeType.Color;
			bool flag3 = (attributes & AttributeType.UV0) == AttributeType.UV0;
			bool flag4 = (attributes & AttributeType.Normal) == AttributeType.Normal;
			bool flag5 = (attributes & AttributeType.Tangent) == AttributeType.Tangent;
			bool flag6 = (attributes & AttributeType.UV1) == AttributeType.UV1;
			bool flag7 = (attributes & AttributeType.UV2) == AttributeType.UV2;
			bool flag8 = (attributes & AttributeType.UV3) == AttributeType.UV3;
			position = ((!flag) ? null : new Vector3[count]);
			color = ((!flag2) ? null : new Color[count]);
			uv0 = ((!flag3) ? null : new Vector2[count]);
			normal = ((!flag4) ? null : new Vector3[count]);
			tangent = ((!flag5) ? null : new Vector4[count]);
			uv2 = ((!flag6) ? null : new Vector2[count]);
			uv3 = ((!flag7) ? null : new List<Vector4>(count));
			uv4 = ((!flag8) ? null : new List<Vector4>(count));
			for (int i = 0; i < count; i++)
			{
				if (flag)
				{
					ref Vector3 reference = ref position[i];
					reference = vertices[i].position;
				}
				if (flag2)
				{
					ref Color reference2 = ref color[i];
					reference2 = vertices[i].color;
				}
				if (flag3)
				{
					ref Vector2 reference3 = ref uv0[i];
					reference3 = vertices[i].uv0;
				}
				if (flag4)
				{
					ref Vector3 reference4 = ref normal[i];
					reference4 = vertices[i].normal;
				}
				if (flag5)
				{
					ref Vector4 reference5 = ref tangent[i];
					reference5 = vertices[i].tangent;
				}
				if (flag6)
				{
					ref Vector2 reference6 = ref uv2[i];
					reference6 = vertices[i].uv2;
				}
				if (flag7)
				{
					uv3.Add(vertices[i].uv3);
				}
				if (flag8)
				{
					uv4.Add(vertices[i].uv4);
				}
			}
		}

		public static void SetMesh(Mesh m, IList<pb_Vertex> vertices)
		{
			Vector3[] vertices2 = null;
			Color[] colors = null;
			Vector2[] uv = null;
			Vector3[] normals = null;
			Vector4[] tangents = null;
			Vector2[] array = null;
			List<Vector4> list = null;
			List<Vector4> list2 = null;
			GetArrays(vertices, out vertices2, out colors, out uv, out normals, out tangents, out array, out list, out list2);
			m.Clear();
			pb_Vertex pb_Vertex2 = vertices[0];
			if (pb_Vertex2.hasPosition)
			{
				m.vertices = vertices2;
			}
			if (pb_Vertex2.hasColor)
			{
				m.colors = colors;
			}
			if (pb_Vertex2.hasUv0)
			{
				m.uv = uv;
			}
			if (pb_Vertex2.hasNormal)
			{
				m.normals = normals;
			}
			if (pb_Vertex2.hasTangent)
			{
				m.tangents = tangents;
			}
			if (pb_Vertex2.hasUv2)
			{
				m.uv2 = array;
			}
			if (pb_Vertex2.hasUv3 && list != null)
			{
				m.SetUVs(2, list);
			}
			if (pb_Vertex2.hasUv4 && list2 != null)
			{
				m.SetUVs(3, list2);
			}
		}

		public static pb_Vertex Average(IList<pb_Vertex> vertices, IList<int> indices = null)
		{
			pb_Vertex pb_Vertex2 = new pb_Vertex();
			int num = indices?.Count ?? vertices.Count;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			for (int i = 0; i < num; i++)
			{
				int index = indices?[i] ?? i;
				pb_Vertex2.position += vertices[index].position;
				pb_Vertex2.color += vertices[index].color;
				pb_Vertex2.uv0 += vertices[index].uv0;
				if (vertices[index].hasNormal)
				{
					num2++;
					pb_Vertex2.normal += vertices[index].normal;
				}
				if (vertices[index].hasTangent)
				{
					num3++;
					pb_Vertex2.tangent += vertices[index].tangent;
				}
				if (vertices[index].hasUv2)
				{
					num4++;
					pb_Vertex2.uv2 += vertices[index].uv2;
				}
				if (vertices[index].hasUv3)
				{
					num5++;
					pb_Vertex2.uv3 += vertices[index].uv3;
				}
				if (vertices[index].hasUv4)
				{
					num6++;
					pb_Vertex2.uv4 += vertices[index].uv4;
				}
			}
			pb_Vertex2.position *= 1f / (float)num;
			pb_Vertex2.color *= 1f / (float)num;
			pb_Vertex2.uv0 *= 1f / (float)num;
			pb_Vertex2.normal *= 1f / (float)num2;
			pb_Vertex2.tangent *= 1f / (float)num3;
			pb_Vertex2.uv2 *= 1f / (float)num4;
			pb_Vertex2.uv3 *= 1f / (float)num5;
			pb_Vertex2.uv4 *= 1f / (float)num6;
			return pb_Vertex2;
		}

		public static pb_Vertex Mix(pb_Vertex x, pb_Vertex y, float a)
		{
			float num = 1f - a;
			pb_Vertex pb_Vertex2 = new pb_Vertex();
			pb_Vertex2.position = x.position * num + y.position * a;
			pb_Vertex2.color = x.color * num + y.color * a;
			pb_Vertex2.uv0 = x.uv0 * num + y.uv0 * a;
			if (x.hasNormal && y.hasNormal)
			{
				pb_Vertex2.normal = x.normal * num + y.normal * a;
			}
			else if (x.hasNormal)
			{
				pb_Vertex2.normal = x.normal;
			}
			else if (y.hasNormal)
			{
				pb_Vertex2.normal = y.normal;
			}
			if (x.hasTangent && y.hasTangent)
			{
				pb_Vertex2.tangent = x.tangent * num + y.tangent * a;
			}
			else if (x.hasTangent)
			{
				pb_Vertex2.tangent = x.tangent;
			}
			else if (y.hasTangent)
			{
				pb_Vertex2.tangent = y.tangent;
			}
			if (x.hasUv2 && y.hasUv2)
			{
				pb_Vertex2.uv2 = x.uv2 * num + y.uv2 * a;
			}
			else if (x.hasUv2)
			{
				pb_Vertex2.uv2 = x.uv2;
			}
			else if (y.hasUv2)
			{
				pb_Vertex2.uv2 = y.uv2;
			}
			if (x.hasUv3 && y.hasUv3)
			{
				pb_Vertex2.uv3 = x.uv3 * num + y.uv3 * a;
			}
			else if (x.hasUv3)
			{
				pb_Vertex2.uv3 = x.uv3;
			}
			else if (y.hasUv3)
			{
				pb_Vertex2.uv3 = y.uv3;
			}
			if (x.hasUv4 && y.hasUv4)
			{
				pb_Vertex2.uv4 = x.uv4 * num + y.uv4 * a;
			}
			else if (x.hasUv4)
			{
				pb_Vertex2.uv4 = x.uv4;
			}
			else if (y.hasUv4)
			{
				pb_Vertex2.uv4 = y.uv4;
			}
			return pb_Vertex2;
		}
	}
}
