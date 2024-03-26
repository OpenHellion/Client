using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Math;

namespace ZeroGravity
{
	public static class Extensions
	{
		public static char CR = '\n';

		public static char LF = '\r';

		public static float[] ToArray(this Vector3 v)
		{
			return new float[3] { v.x, v.y, v.z };
		}

		public static float[] ToArray(this Quaternion q)
		{
			return new float[4] { q.x, q.y, q.z, q.w };
		}

		public static Vector3 SignedAngles(this Vector3 v)
		{
			Vector3 result = new Vector3(v.x % 360f, v.y % 360f, v.z % 360f);
			if (result.x > 180f)
			{
				result.x -= 360f;
			}

			if (result.y > 180f)
			{
				result.y -= 360f;
			}

			if (result.z > 180f)
			{
				result.z -= 360f;
			}

			return result;
		}

		public static MeshData GetMeshData(this Mesh s)
		{
			MeshData meshData = new MeshData();
			if (s == null)
			{
				return meshData;
			}

			meshData.Vertices = new float[s.vertexCount * 3];
			for (int i = 0; i < s.vertexCount; i++)
			{
				meshData.Vertices[i * 3] = s.vertices[i].x;
				meshData.Vertices[i * 3 + 1] = s.vertices[i].y;
				meshData.Vertices[i * 3 + 2] = s.vertices[i].z;
			}

			meshData.Indices = s.GetIndices(0);
			return meshData;
		}

		public static void AddIfExists(this AnimatorOverrideController animOverride, string clipName,
			AnimationClip animClip)
		{
			if (animClip != null)
			{
				animOverride[clipName] = animClip;
			}
		}

		public static double[] ToArray(this Vector3D v)
		{
			return new double[3] { v.X, v.Y, v.Z };
		}

		public static double[] ToArray(this QuaternionD q)
		{
			return new double[4] { q.X, q.Y, q.Z, q.W };
		}

		public static float[] ToFloatArray(this Vector3D v)
		{
			return new float[3]
			{
				(float)v.X,
				(float)v.Y,
				(float)v.Z
			};
		}

		public static float[] ToFloatArray(this QuaternionD q)
		{
			return new float[4]
			{
				(float)q.X,
				(float)q.Y,
				(float)q.Z,
				(float)q.W
			};
		}

		public static Vector3D SignedAngles(this Vector3D v)
		{
			Vector3D result = new Vector3D(v.X % 360.0, v.Y % 360.0, v.Z % 360.0);
			if (result.X > 180.0)
			{
				result.X -= 360.0;
			}

			if (result.Y > 180.0)
			{
				result.Y -= 360.0;
			}

			if (result.Z > 180.0)
			{
				result.Z -= 360.0;
			}

			return result;
		}

		public static Vector3 ToVector3(this float[] arr)
		{
			if (arr.Length == 3)
			{
				return new Vector3(arr[0], arr[1], arr[2]);
			}

			return Vector3.zero;
		}

		public static Quaternion ToQuaternion(this float[] arr)
		{
			if (arr.Length == 4)
			{
				return new Quaternion(arr[0], arr[1], arr[2], arr[3]);
			}

			return Quaternion.identity;
		}

		public static Vector3D ToVector3D(this float[] arr)
		{
			if (arr.Length == 3)
			{
				return new Vector3D(arr[0], arr[1], arr[2]);
			}

			return Vector3D.Zero;
		}

		public static Vector3D ToVector3D(this double[] arr)
		{
			if (arr.Length == 3)
			{
				return new Vector3D(arr[0], arr[1], arr[2]);
			}

			return Vector3D.Zero;
		}

		public static QuaternionD ToQuaternionD(this float[] arr)
		{
			if (arr.Length == 4)
			{
				return new QuaternionD(arr[0], arr[1], arr[2], arr[3]);
			}

			return QuaternionD.Identity;
		}

		public static QuaternionD ToQuaternionD(this double[] arr)
		{
			if (arr.Length == 4)
			{
				return new QuaternionD(arr[0], arr[1], arr[2], arr[3]);
			}

			return QuaternionD.Identity;
		}

		public static bool IsNotEpsilonZero(this float val, float epsilon = float.Epsilon)
		{
			return val > epsilon || val < 0f - epsilon;
		}

		public static bool IsNotEpsilonZeroD(this double val, double epsilon = double.Epsilon)
		{
			return val > epsilon || val < 0.0 - epsilon;
		}

		public static bool IsNotEpsilonZero(this Vector3 value, float epsilon = float.Epsilon)
		{
			return MathF.Abs(value.x) > epsilon || MathF.Abs(value.y) > epsilon || MathF.Abs(value.z) > epsilon;
		}

		public static bool IsNotEpsilonZero(this Vector3D value, double epsilon = double.Epsilon)
		{
			return System.Math.Abs(value.X) > epsilon || System.Math.Abs(value.Y) > epsilon ||
			       System.Math.Abs(value.Z) > epsilon;
		}

		public static bool IsInfinity(this Vector3 value)
		{
			return float.IsInfinity(value.x) || float.IsInfinity(value.y) || float.IsInfinity(value.z);
		}

		public static bool IsNaN(this Vector3 value)
		{
			return float.IsNaN(value.x) || float.IsNaN(value.y) || float.IsNaN(value.z);
		}

		public static bool IsInfinity(this Quaternion value)
		{
			return float.IsInfinity(value.x) || float.IsInfinity(value.y) || float.IsInfinity(value.z) ||
			       float.IsInfinity(value.w);
		}

		public static bool IsNaN(this Quaternion value)
		{
			return float.IsNaN(value.x) || float.IsNaN(value.y) || float.IsNaN(value.z) || float.IsNaN(value.w);
		}

		public static bool IsInfinity(this Vector3D value)
		{
			return double.IsInfinity(value.X) || double.IsInfinity(value.Y) || double.IsInfinity(value.Z);
		}

		public static bool IsNaN(this Vector3D value)
		{
			return double.IsNaN(value.X) || double.IsNaN(value.Y) || double.IsNaN(value.Z);
		}

		public static bool IsEpsilonEqual(this float val, float other, float epsilon = float.Epsilon)
		{
			return !(MathF.Abs(val - other) > epsilon);
		}

		public static bool IsEpsilonEqualD(this double val, double other, double epsilon = double.Epsilon)
		{
			return !(System.Math.Abs(val - other) > epsilon);
		}

		public static bool IsEpsilonEqual(this Vector3 val, Vector3 other, float epsilon = float.Epsilon)
		{
			return !(Mathf.Abs(val.x - other.x) > epsilon) && !(Mathf.Abs(val.y - other.y) > epsilon) &&
			       !(Mathf.Abs(val.z - other.z) > epsilon);
		}

		public static bool IsEpsilonEqual(this Quaternion val, Quaternion other, float epsilon = float.Epsilon)
		{
			return !(Mathf.Abs(val.x - other.x) > epsilon) && !(Mathf.Abs(val.y - other.y) > epsilon) &&
			       !(Mathf.Abs(val.z - other.z) > epsilon) && !(Mathf.Abs(val.w - other.w) > epsilon);
		}

		public static bool IsEpsilonEqual(this Vector3D val, Vector3D other, double epsilon = double.Epsilon)
		{
			return !(System.Math.Abs(val.X - other.X) > epsilon) && !(System.Math.Abs(val.Y - other.Y) > epsilon) &&
			       !(System.Math.Abs(val.Z - other.Z) > epsilon);
		}

		public static Vector3D ToVector3D(this Vector3 value)
		{
			return new Vector3D(value.x, value.y, value.z);
		}

		public static Vector3 ToVector3(this Vector3D value)
		{
			return new Vector3((float)value.X, (float)value.Y, (float)value.Z);
		}

		public static QuaternionD ToQuaternionD(this Quaternion value)
		{
			return new QuaternionD(value.x, value.y, value.z, value.w);
		}

		public static Quaternion ToQuaternion(this QuaternionD value)
		{
			return new Quaternion((float)value.X, (float)value.Y, (float)value.Z, (float)value.W);
		}

		public static Vector3 FromOther(this Vector3 vec, Vector3 other)
		{
			vec.x = other.x;
			vec.y = other.y;
			vec.z = other.z;
			return vec;
		}

		public static bool IsValid(this Vector3 v)
		{
			if (double.IsNaN(v.x) || double.IsInfinity(v.x) || double.IsNaN(v.y) || double.IsInfinity(v.y) ||
			    double.IsNaN(v.z) || double.IsInfinity(v.z))
			{
				return false;
			}

			return true;
		}

		public static Vector3 RotateAroundPivot(this Vector3 vector, Vector3 pivot, Vector3 angles)
		{
			return Quaternion.Euler(angles) * (vector - pivot) + pivot;
		}

		public static double DistanceSquared(this Vector3 a, Vector3 b)
		{
			Vector3 vector = new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
			return vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
		}

		public static Vector3D FromOther(this Vector3D vec, Vector3D other)
		{
			vec.X = other.X;
			vec.Y = other.Y;
			vec.Z = other.Z;
			return vec;
		}

		public static bool IsValid(this Vector3D v)
		{
			if (double.IsNaN(v.X) || double.IsInfinity(v.X) || double.IsNaN(v.Y) || double.IsInfinity(v.Y) ||
			    double.IsNaN(v.Z) || double.IsInfinity(v.Z))
			{
				return false;
			}

			return true;
		}

		public static Vector3D RotateAroundPivot(this Vector3D vector, Vector3D pivot, Vector3D angles)
		{
			return QuaternionD.Euler(angles) * (vector - pivot) + pivot;
		}

		public static double DistanceSquared(this Vector3D a, Vector3D b)
		{
			Vector3D vector3D = new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
			return vector3D.X * vector3D.X + vector3D.Y * vector3D.Y + vector3D.Z * vector3D.Z;
		}

		public static Quaternion Inverse(this Quaternion value)
		{
			return Quaternion.Inverse(value);
		}

		public static QuaternionD Inverse(this QuaternionD value)
		{
			return QuaternionD.Inverse(value);
		}

		public static Transform FindChildByName(this Transform ThisGObj, string ThisName)
		{
			if (ThisGObj.name == ThisName)
			{
				return ThisGObj.transform;
			}

			foreach (Transform item in ThisGObj)
			{
				Transform transform = FindChildByName(item, ThisName);
				if ((bool)transform)
				{
					return transform;
				}
			}

			return null;
		}

		public static void Reset(this Transform t, bool resetScale = false)
		{
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
			if (resetScale)
			{
				t.localScale = new Vector3(1f, 1f, 1f);
			}
		}

		public static void Reset(this RectTransform t, bool resetScale = false)
		{
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
			if (resetScale)
			{
				t.localScale = Vector3.one;
			}
		}

		public static Transform Search(this Transform target, string name)
		{
			if (target.name == name)
			{
				return target;
			}

			for (int i = 0; i < target.childCount; i++)
			{
				Transform transform = Search(target.GetChild(i), name);
				if (transform != null)
				{
					return transform;
				}
			}

			return null;
		}

		public static void SetLayerRecursively(this GameObject target, string layer, params string[] skipLayers)
		{
			int mask = LayerMask.GetMask(skipLayers);
			int layer2 = LayerMask.NameToLayer(layer);
			int num = 0;
			Transform[] componentsInChildren = target.GetComponentsInChildren<Transform>(includeInactive: true);
			foreach (Transform transform in componentsInChildren)
			{
				num = 1 << transform.gameObject.layer;
				if (transform.gameObject != null && (num & mask) != num)
				{
					transform.gameObject.layer = layer2;
				}
			}
		}

		public static void SetLayerRecursively(this GameObject target, int layer, params string[] skipLayers)
		{
			int mask = LayerMask.GetMask(skipLayers);
			int num = 0;
			Transform[] componentsInChildren = target.GetComponentsInChildren<Transform>(includeInactive: true);
			foreach (Transform transform in componentsInChildren)
			{
				num = 1 << transform.gameObject.layer;
				if (transform.gameObject is not null && (num & mask) != num)
				{
					transform.gameObject.layer = layer;
				}
			}
		}

		public static T GetCopyOf<T>(this Component comp, T other) where T : Component
		{
			Type type = comp.GetType();
			if (type != other.GetType())
			{
				return null;
			}

			BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public |
			                           BindingFlags.NonPublic;
			PropertyInfo[] properties = type.GetProperties(bindingAttr);
			PropertyInfo[] array = properties;
			foreach (PropertyInfo propertyInfo in array)
			{
				if (propertyInfo.CanWrite)
				{
					try
					{
						propertyInfo.SetValue(comp, propertyInfo.GetValue(other, null), null);
					}
					catch
					{
					}
				}
			}

			FieldInfo[] fields = type.GetFields(bindingAttr);
			FieldInfo[] array2 = fields;
			foreach (FieldInfo fieldInfo in array2)
			{
				fieldInfo.SetValue(comp, fieldInfo.GetValue(other));
			}

			return comp as T;
		}

		public static List<Transform> GetChildren(this Transform transform)
		{
			List<Transform> list = new List<Transform>();
			foreach (Transform item in transform)
			{
				list.Add(item);
			}

			return list;
		}

		public static string GetPathFromParent(this Transform transform, Transform fromParent)
		{
			string text = transform.name;
			while (transform.parent != null && transform.parent != fromParent)
			{
				transform = transform.parent;
				text = transform.name + "/" + text;
			}

			return text;
		}

		public static bool IsNullOrEmpty(this string val)
		{
			return string.IsNullOrEmpty(val);
		}

		public static string CamelCaseToSpaced(this string subject)
		{
			return Regex.Replace(subject, "(\\B[A-Z])", " $1");
		}

		public static int TagCount(this string tag)
		{
			return tag.Split(';').Length;
		}

		public static List<string> ExtractTags(this string input, char s = ';')
		{
			List<string> list = new List<string>();
			string[] array = input.Split(s);
			string[] array2 = array;
			foreach (string item in array2)
			{
				list.Add(item);
			}

			return list;
		}

		public static void DestroyAll(this Transform t, bool childrenOnly = false)
		{
			foreach (GameObject item in from m in t.GetComponentsInChildren<Transform>(includeInactive: true)
			         select m.gameObject)
			{
				if (!childrenOnly || !(item == t.gameObject))
				{
					UnityEngine.Object.Destroy(item);
				}
			}
		}

		public static void DestroyAll(this GameObject go, bool childrenOnly = false)
		{
			DestroyAll(go.transform, childrenOnly);
		}

		public static void DestroyAll<T>(this Transform t, bool childrenOnly = false) where T : Component
		{
			foreach (GameObject item in from m in t.GetComponentsInChildren<T>(includeInactive: true)
			         select m.gameObject)
			{
				if (!childrenOnly || !(item == t.gameObject))
				{
					UnityEngine.Object.Destroy(item);
				}
			}
		}

		public static void DestroyAll<T>(this GameObject go, bool childrenOnly = false) where T : Component
		{
			DestroyAll<T>(go.transform, childrenOnly);
		}

		public static T GetNextInLoop<T>(this IEnumerator<T> enumerator)
		{
			if (!enumerator.MoveNext())
			{
				enumerator.Reset();
			}

			return enumerator.Current;
		}

		public static T ToEnum<T>(this string value)
		{
			return (T)Enum.Parse(typeof(T), value, ignoreCase: true);
		}

		public static void Activate(this GameObject go, bool value)
		{
			if (go.activeSelf != value)
			{
				go.SetActive(value);
			}
		}

		public static void Invoke(this MonoBehaviour mb, Action func, float delay)
		{
			mb.Invoke(func.Method.Name, delay);
		}

		public static void InvokeRepeating(this MonoBehaviour mb, Action func, float delay, float repeatRate)
		{
			mb.InvokeRepeating(func.Method.Name, delay, repeatRate);
		}

		public static void CancelInvoke(this MonoBehaviour mb, Action func)
		{
			mb.CancelInvoke(func.Method.Name);
		}

		public static bool IsInvoking(this MonoBehaviour mb, Action func)
		{
			return mb.IsInvoking(func.Method.Name);
		}
	}
}
