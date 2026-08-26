using System.Collections.Generic;
using UnityEngine;

namespace ProBuilder2.Common
{
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	public class pb_MeshRenderer : pb_MonoBehaviourSingleton<pb_MeshRenderer>
	{
		[SerializeField]
		private HashSet<pb_Renderable> m_Renderables = new HashSet<pb_Renderable>();

		private readonly HideFlags SceneCameraHideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.NotEditable;

		private int clamp(int val, int min, int max)
		{
			return (val < min) ? min : ((val <= max) ? val : max);
		}

		public static void Add(pb_Renderable renderable)
		{
			pb_MonoBehaviourSingleton<pb_MeshRenderer>.instance.m_Renderables.Add(renderable);
		}

		public static void Remove(pb_Renderable renderable)
		{
			if (pb_MonoBehaviourSingleton<pb_MeshRenderer>.instance.m_Renderables.Contains(renderable))
			{
				pb_MonoBehaviourSingleton<pb_MeshRenderer>.instance.m_Renderables.Remove(renderable);
			}
		}

		private void OnRenderObject()
		{
			if ((Camera.current.gameObject.hideFlags & SceneCameraHideFlags) != SceneCameraHideFlags || Camera.current.name != "SceneCamera")
			{
				return;
			}
			int num = 0;
			foreach (pb_Renderable renderable in m_Renderables)
			{
				if (renderable.materials == null)
				{
					Debug.Log("renderable.materials == null -> " + base.name);
				}
				Material[] materials = renderable.materials;
				if (renderable.mesh == null)
				{
					Debug.Log("renderable mesh is null");
					continue;
				}
				for (int i = 0; i < renderable.mesh.subMeshCount; i++)
				{
					num = clamp(i, 0, materials.Length - 1);
					if (materials[num] == null || !materials[num].SetPass(0))
					{
						Debug.Log("material is null");
					}
					else
					{
						Graphics.DrawMeshNow(renderable.mesh, (!(renderable.transform != null)) ? Matrix4x4.identity : renderable.transform.localToWorldMatrix, i);
					}
				}
			}
		}

		private void OnDestroy()
		{
			foreach (pb_Renderable renderable in m_Renderables)
			{
				Object.DestroyImmediate(renderable);
			}
		}
	}
}
