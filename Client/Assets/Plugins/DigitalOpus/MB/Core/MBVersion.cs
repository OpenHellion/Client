using System;
using UnityEngine;

namespace DigitalOpus.MB.Core
{
	public class MBVersion
	{
		private static MBVersionInterface _MBVersion;

		private static MBVersionInterface _CreateMBVersionConcrete()
		{
			Type type = null;
			type = typeof(MBVersionConcrete);
			return (MBVersionInterface)Activator.CreateInstance(type);
		}

		public static string version()
		{
			if (_MBVersion == null)
			{
				_MBVersion = _CreateMBVersionConcrete();
			}

			return _MBVersion.version();
		}

		public static int GetMajorVersion()
		{
			if (_MBVersion == null)
			{
				_MBVersion = _CreateMBVersionConcrete();
			}

			return _MBVersion.GetMajorVersion();
		}

		public static int GetMinorVersion()
		{
			if (_MBVersion == null)
			{
				_MBVersion = _CreateMBVersionConcrete();
			}

			return _MBVersion.GetMinorVersion();
		}

		public static bool GetActive(GameObject go)
		{
			if (_MBVersion == null)
			{
				_MBVersion = _CreateMBVersionConcrete();
			}

			return _MBVersion.GetActive(go);
		}

		public static void SetActive(GameObject go, bool isActive)
		{
			if (_MBVersion == null)
			{
				_MBVersion = _CreateMBVersionConcrete();
			}

			_MBVersion.SetActive(go, isActive);
		}

		public static void SetActiveRecursively(GameObject go, bool isActive)
		{
			if (_MBVersion == null)
			{
				_MBVersion = _CreateMBVersionConcrete();
			}

			_MBVersion.SetActiveRecursively(go, isActive);
		}

		public static UnityEngine.Object[] FindSceneObjectsOfType(Type t)
		{
			if (_MBVersion == null)
			{
				_MBVersion = _CreateMBVersionConcrete();
			}

			return _MBVersion.FindSceneObjectsOfType(t);
		}

		public static bool IsRunningAndMeshNotReadWriteable(Mesh m)
		{
			if (_MBVersion == null)
			{
				_MBVersion = _CreateMBVersionConcrete();
			}

			return _MBVersion.IsRunningAndMeshNotReadWriteable(m);
		}

		public static Vector2[] GetMeshUV3orUV4(Mesh m, bool get3, MB2_LogLevel LOG_LEVEL)
		{
			if (_MBVersion == null)
			{
				_MBVersion = _CreateMBVersionConcrete();
			}

			return _MBVersion.GetMeshUV3orUV4(m, get3, LOG_LEVEL);
		}

		public static void MeshClear(Mesh m, bool t)
		{
			if (_MBVersion == null)
			{
				_MBVersion = _CreateMBVersionConcrete();
			}

			_MBVersion.MeshClear(m, t);
		}

		public static void MeshAssignUV3(Mesh m, Vector2[] uv3s)
		{
			if (_MBVersion == null)
			{
				_MBVersion = _CreateMBVersionConcrete();
			}

			_MBVersion.MeshAssignUV3(m, uv3s);
		}

		public static void MeshAssignUV4(Mesh m, Vector2[] uv4s)
		{
			if (_MBVersion == null)
			{
				_MBVersion = _CreateMBVersionConcrete();
			}

			_MBVersion.MeshAssignUV4(m, uv4s);
		}

		public static Vector4 GetLightmapTilingOffset(Renderer r)
		{
			if (_MBVersion == null)
			{
				_MBVersion = _CreateMBVersionConcrete();
			}

			return _MBVersion.GetLightmapTilingOffset(r);
		}

		public static Transform[] GetBones(Renderer r)
		{
			if (_MBVersion == null)
			{
				_MBVersion = _CreateMBVersionConcrete();
			}

			return _MBVersion.GetBones(r);
		}
	}
}
