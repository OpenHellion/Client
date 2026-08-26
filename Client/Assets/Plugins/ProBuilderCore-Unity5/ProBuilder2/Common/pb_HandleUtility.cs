using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ProBuilder2.Common
{
	public static class pb_HandleUtility
	{
		private const float MAX_EDGE_SELECT_DISTANCE = 20f;

		[CompilerGenerated]
		private static Comparison<pb_RaycastHit> _003C_003Ef__am_0024cache0;

		public static bool FaceRaycast(Ray InWorldRay, pb_Object mesh, out pb_RaycastHit hit, HashSet<pb_Face> ignore = null)
		{
			return FaceRaycast(InWorldRay, mesh, out hit, float.PositiveInfinity, Culling.Front, ignore);
		}

		public static bool FaceRaycast(Ray InWorldRay, pb_Object mesh, out pb_RaycastHit hit, float distance, Culling cullingMode, HashSet<pb_Face> ignore = null)
		{
			InWorldRay.origin -= mesh.transform.position;
			InWorldRay.origin = mesh.transform.worldToLocalMatrix * InWorldRay.origin;
			InWorldRay.direction = mesh.transform.worldToLocalMatrix * InWorldRay.direction;
			Vector3[] vertices = mesh.vertices;
			float OutDistance = 0f;
			Vector3 OutPoint = Vector3.zero;
			float num = float.PositiveInfinity;
			int num2 = -1;
			Vector3 inNormal = Vector3.zero;
			for (int i = 0; i < mesh.faces.Length; i++)
			{
				if (ignore != null && ignore.Contains(mesh.faces[i]))
				{
					continue;
				}
				int[] indices = mesh.faces[i].indices;
				for (int j = 0; j < indices.Length; j += 3)
				{
					Vector3 vector = vertices[indices[j]];
					Vector3 vector2 = vertices[indices[j + 1]];
					Vector3 vector3 = vertices[indices[j + 2]];
					Vector3 vector4 = Vector3.Cross(vector2 - vector, vector3 - vector);
					float num3 = Vector3.Dot(InWorldRay.direction, vector4);
					bool flag = false;
					switch (cullingMode)
					{
					case Culling.Front:
						if (num3 > 0f)
						{
							flag = true;
						}
						break;
					case Culling.Back:
						if (num3 < 0f)
						{
							flag = true;
						}
						break;
					}
					if (!flag && pb_Math.RayIntersectsTriangle(InWorldRay, vector, vector2, vector3, out OutDistance, out OutPoint) && !(OutDistance > num) && !(OutDistance > distance))
					{
						inNormal = vector4;
						num2 = i;
						num = OutDistance;
					}
				}
			}
			hit = new pb_RaycastHit(num, InWorldRay.GetPoint(num), inNormal, num2);
			return num2 > -1;
		}

		public static bool FaceRaycast(Ray InWorldRay, pb_Object mesh, out List<pb_RaycastHit> hits, float distance, Culling cullingMode, HashSet<pb_Face> ignore = null)
		{
			InWorldRay.origin -= mesh.transform.position;
			InWorldRay.origin = mesh.transform.worldToLocalMatrix * InWorldRay.origin;
			InWorldRay.direction = mesh.transform.worldToLocalMatrix * InWorldRay.direction;
			Vector3[] vertices = mesh.vertices;
			float OutDistance = 0f;
			Vector3 OutPoint = Vector3.zero;
			hits = new List<pb_RaycastHit>();
			for (int i = 0; i < mesh.faces.Length; i++)
			{
				if (ignore != null && ignore.Contains(mesh.faces[i]))
				{
					continue;
				}
				int[] indices = mesh.faces[i].indices;
				for (int j = 0; j < indices.Length; j += 3)
				{
					Vector3 vector = vertices[indices[j]];
					Vector3 vector2 = vertices[indices[j + 1]];
					Vector3 vector3 = vertices[indices[j + 2]];
					if (!pb_Math.RayIntersectsTriangle(InWorldRay, vector, vector2, vector3, out OutDistance, out OutPoint))
					{
						continue;
					}
					Vector3 vector4 = Vector3.Cross(vector2 - vector, vector3 - vector);
					if (cullingMode != Culling.Front)
					{
						if (cullingMode != 0)
						{
							if (cullingMode != Culling.FrontBack)
							{
								continue;
							}
						}
						else
						{
							float num = Vector3.Dot(InWorldRay.direction, vector4);
							if (!(num > 0f))
							{
								continue;
							}
						}
					}
					else
					{
						float num = Vector3.Dot(InWorldRay.direction, -vector4);
						if (!(num > 0f))
						{
							continue;
						}
					}
					hits.Add(new pb_RaycastHit(OutDistance, InWorldRay.GetPoint(OutDistance), vector4, i));
				}
			}
			return hits.Count > 0;
		}

		public static Ray InverseTransformRay(this Transform transform, Ray InWorldRay)
		{
			Vector3 origin = InWorldRay.origin;
			origin -= transform.position;
			origin = transform.worldToLocalMatrix * origin;
			Vector3 direction = transform.worldToLocalMatrix.MultiplyVector(InWorldRay.direction);
			return new Ray(origin, direction);
		}

		public static bool WorldRaycast(Ray InWorldRay, Transform transform, Vector3[] vertices, int[] triangles, out pb_RaycastHit hit, float distance = float.PositiveInfinity, Culling cullingMode = Culling.Front)
		{
			Ray inRay = transform.InverseTransformRay(InWorldRay);
			return MeshRaycast(inRay, vertices, triangles, out hit, distance, cullingMode);
		}

		public static bool MeshRaycast(Ray InRay, Vector3[] vertices, int[] triangles, out pb_RaycastHit hit, float distance = float.PositiveInfinity, Culling cullingMode = Culling.Front)
		{
			float num = float.PositiveInfinity;
			Vector3 normal = new Vector3(0f, 0f, 0f);
			int num2 = -1;
			Vector3 origin = InRay.origin;
			Vector3 direction = InRay.direction;
			for (int i = 0; i < triangles.Length; i += 3)
			{
				Vector3 vert = vertices[triangles[i]];
				Vector3 vert2 = vertices[triangles[i + 1]];
				Vector3 vert3 = vertices[triangles[i + 2]];
				if (pb_Math.RayIntersectsTriangle2(origin, direction, vert, vert2, vert3, ref distance, ref normal))
				{
					num2 = i / 3;
					num = distance;
					break;
				}
			}
			hit = new pb_RaycastHit(num, InRay.GetPoint(num), normal, num2);
			return num2 > -1;
		}

		public static bool EdgeRaycast(Camera cam, Vector2 mousePosition, pb_Object mesh, pb_Edge[] edges, Vector3[] verticesInWorldSpace, out pb_Edge edge)
		{
			float num = float.PositiveInfinity;
			float num2 = 0f;
			edge = pb_Edge.Empty;
			GameObject gameObject = ObjectRaycast(cam, mousePosition, (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject)));
			if (gameObject == null || gameObject != mesh.gameObject)
			{
				int width = Screen.width;
				int height = Screen.height;
				for (int i = 0; i < edges.Length; i++)
				{
					Vector3 vector = verticesInWorldSpace[edges[i].x];
					Vector3 vector2 = verticesInWorldSpace[edges[i].y];
					num2 = DistancePoint2DToLine(cam, mousePosition, vector, vector2);
					if (!(num2 < num) || !(num2 < 20f))
					{
						continue;
					}
					Vector3 vector3 = cam.WorldToScreenPoint(vector);
					if (!(vector3.z <= 0f) && !(vector3.x < 0f) && !(vector3.y < 0f) && !(vector3.x > (float)width) && !(vector3.y > (float)height))
					{
						Vector3 vector4 = cam.WorldToScreenPoint(vector2);
						if (!(vector4.z <= 0f) && !(vector4.x < 0f) && !(vector4.y < 0f) && !(vector4.x > (float)width) && !(vector4.y > (float)height))
						{
							num = num2;
							edge = edges[i];
						}
					}
				}
			}
			else
			{
				Ray inWorldRay = cam.ScreenPointToRay(mousePosition);
				if (FaceRaycast(inWorldRay, mesh, out List<pb_RaycastHit> hits, float.PositiveInfinity, Culling.FrontBack, (HashSet<pb_Face>)null))
				{
					List<pb_RaycastHit> list = hits;
					if (_003C_003Ef__am_0024cache0 == null)
					{
						_003C_003Ef__am_0024cache0 = _003CEdgeRaycast_003Em__0;
					}
					list.Sort(_003C_003Ef__am_0024cache0);
					Vector3[] vertices = mesh.vertices;
					for (int j = 0; j < hits.Count; j++)
					{
						if (PointIsOccluded(cam, mesh, mesh.transform.TransformPoint(hits[j].point)))
						{
							continue;
						}
						pb_Edge[] allEdges = mesh.faces[hits[j].face].GetAllEdges();
						for (int k = 0; k < allEdges.Length; k++)
						{
							pb_Edge pb_Edge2 = allEdges[k];
							float num3 = pb_Math.DistancePointLineSegment(hits[j].point, vertices[pb_Edge2.x], vertices[pb_Edge2.y]);
							if (num3 < num)
							{
								num = num3;
								edge = pb_Edge2;
							}
						}
						if (Vector3.Dot(inWorldRay.direction, mesh.transform.TransformDirection(hits[j].normal)) < 0f)
						{
							break;
						}
					}
					if (edge.IsValid() && DistancePoint2DToLine(cam, mousePosition, mesh.transform.TransformPoint(vertices[edge.x]), mesh.transform.TransformPoint(vertices[edge.y])) > 20f)
					{
						edge = pb_Edge.Empty;
					}
				}
			}
			return edge.IsValid();
		}

		public static GameObject ObjectRaycast(Camera cam, Vector2 mousePosition, GameObject[] objects)
		{
			return null;
		}

		public static float DistancePoint2DToLine(Camera cam, Vector2 mousePosition, Vector3 worldPosition1, Vector3 worldPosition2)
		{
			Vector2 v = cam.WorldToScreenPoint(worldPosition1);
			Vector2 w = cam.WorldToScreenPoint(worldPosition2);
			return pb_Math.DistancePointLineSegment(mousePosition, v, w);
		}

		public static bool PointIsOccluded(Camera cam, pb_Object pb, Vector3 worldPoint)
		{
			Vector3 normalized = (cam.transform.position - worldPoint).normalized;
			Ray inWorldRay = new Ray(worldPoint + normalized * 0.0001f, normalized);
			pb_RaycastHit hit;
			return FaceRaycast(inWorldRay, pb, out hit, Vector3.Distance(cam.transform.position, worldPoint), Culling.Back, (HashSet<pb_Face>)null);
		}

		public static bool IsOccluded(Camera cam, pb_Object pb, pb_Face face)
		{
			Vector3 zero = Vector3.zero;
			int num = face.distinctIndices.Length;
			for (int i = 0; i < num; i++)
			{
				zero += pb.vertices[face.distinctIndices[i]];
			}
			zero *= 1f / (float)num;
			return PointIsOccluded(cam, pb, pb.transform.TransformPoint(zero));
		}

		[CompilerGenerated]
		private static int _003CEdgeRaycast_003Em__0(pb_RaycastHit x, pb_RaycastHit y)
		{
			return x.distance.CompareTo(y.distance);
		}
	}
}
