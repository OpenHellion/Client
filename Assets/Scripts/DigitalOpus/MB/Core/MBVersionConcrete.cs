using System;
using UnityEngine;

namespace DigitalOpus.MB.Core
{
	public class MBVersionConcrete : MBVersionInterface
	{
		private Vector2 _HALF_UV = new Vector2(0.5f, 0.5f);

		public string version()
		{
			return "3.16.1";
		}

		public int GetMajorVersion()
		{
			string unityVersion = Application.unityVersion;
			string[] array = unityVersion.Split('.');
			return int.Parse(array[0]);
		}

		public int GetMinorVersion()
		{
			string unityVersion = Application.unityVersion;
			string[] array = unityVersion.Split('.');
			return int.Parse(array[1]);
		}

		public bool GetActive(GameObject go)
		{
			return go.activeInHierarchy;
		}

		public void SetActive(GameObject go, bool isActive)
		{
			go.SetActive(isActive);
		}

		public void SetActiveRecursively(GameObject go, bool isActive)
		{
			go.SetActive(isActive);
		}

		public UnityEngine.Object[] FindSceneObjectsOfType(Type t)
		{
			return UnityEngine.Object.FindObjectsOfType(t);
		}

		public bool IsRunningAndMeshNotReadWriteable(Mesh m)
		{
			if (Application.isPlaying)
			{
				return !m.isReadable;
			}

			return false;
		}

		public Vector2[] GetMeshUV1s(Mesh m, MB2_LogLevel LOG_LEVEL)
		{
			if (LOG_LEVEL >= MB2_LogLevel.warn)
			{
				MB2_Log.LogDebug("UV1 does not exist in Unity 5+");
			}

			Vector2[] array = m.uv;
			if (array.Length == 0)
			{
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug(string.Concat("Mesh ", m, " has no uv1s. Generating"));
				}

				if (LOG_LEVEL >= MB2_LogLevel.warn)
				{
					Debug.LogWarning(string.Concat("Mesh ", m, " didn't have uv1s. Generating uv1s."));
				}

				array = new Vector2[m.vertexCount];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = _HALF_UV;
				}
			}

			return array;
		}

		public Vector2[] GetMeshUV3orUV4(Mesh m, bool get3, MB2_LogLevel LOG_LEVEL)
		{
			Vector2[] array = ((!get3) ? m.uv4 : m.uv3);
			if (array.Length == 0)
			{
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug(string.Concat("Mesh ", m, " has no uv", (!get3) ? "4" : "3", ". Generating"));
				}

				array = new Vector2[m.vertexCount];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = _HALF_UV;
				}
			}

			return array;
		}

		public void MeshClear(Mesh m, bool t)
		{
			m.Clear(t);
		}

		public void MeshAssignUV3(Mesh m, Vector2[] uv3s)
		{
			m.uv3 = uv3s;
		}

		public void MeshAssignUV4(Mesh m, Vector2[] uv4s)
		{
			m.uv4 = uv4s;
		}

		public Vector4 GetLightmapTilingOffset(Renderer r)
		{
			return r.lightmapScaleOffset;
		}

		public Transform[] GetBones(Renderer r)
		{
			if (r is SkinnedMeshRenderer)
			{
				return ((SkinnedMeshRenderer)r).bones;
			}

			if (r is MeshRenderer)
			{
				return new Transform[1] { r.transform };
			}

			Debug.LogError("Could not getBones. Object does not have a renderer");
			return null;
		}
	}
}
