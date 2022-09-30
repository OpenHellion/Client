using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ProBuilder2.Common
{
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	public class pb_LineRenderer : pb_MonoBehaviourSingleton<pb_LineRenderer>
	{
		private HideFlags SceneCameraHideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.NotEditable;

		private pb_ObjectPool<Mesh> pool;

		[HideInInspector]
		public List<Mesh> gizmos = new List<Mesh>();

		[HideInInspector]
		public Material mat;

		[CompilerGenerated]
		private static Func<Mesh> _003C_003Ef__mg_0024cache0;

		[CompilerGenerated]
		private static Func<Mesh> _003C_003Ef__mg_0024cache1;

		private static Mesh MeshConstructor()
		{
			Mesh mesh = new Mesh();
			mesh.hideFlags = pb_Constant.EDITOR_OBJECT_HIDE_FLAGS;
			mesh.name = "pb_LineRenderer::Mesh";
			return mesh;
		}

		public override void OnEnable()
		{
			base.OnEnable();
			if (_003C_003Ef__mg_0024cache0 == null)
			{
				_003C_003Ef__mg_0024cache0 = MeshConstructor;
			}
			pool = new pb_ObjectPool<Mesh>(1, 8, _003C_003Ef__mg_0024cache0, null);
		}

		private void OnDisable()
		{
			pool.Empty();
		}

		public override void Awake()
		{
			base.Awake();
			base.gameObject.hideFlags = HideFlags.HideAndDontSave;
			mat = new Material(Shader.Find("ProBuilder/UnlitVertexColor"));
			mat.name = "pb_LineRenderer_Material";
			mat.SetColor("_Color", Color.white);
			mat.hideFlags = pb_Constant.EDITOR_OBJECT_HIDE_FLAGS;
		}

		private void OnDestroy()
		{
			foreach (Mesh gizmo in gizmos)
			{
				if (gizmo != null)
				{
					UnityEngine.Object.DestroyImmediate(gizmo);
				}
			}
			UnityEngine.Object.DestroyImmediate(mat);
		}

		public void AddLineSegments(Vector3[] segments, Color[] colors)
		{
			if (pool == null)
			{
				if (_003C_003Ef__mg_0024cache1 == null)
				{
					_003C_003Ef__mg_0024cache1 = MeshConstructor;
				}
				pool = new pb_ObjectPool<Mesh>(1, 4, _003C_003Ef__mg_0024cache1, null);
			}
			Mesh mesh = pool.Get();
			mesh.Clear();
			mesh.name = "pb_LineRenderer::Mesh_" + mesh.GetInstanceID();
			mesh.MarkDynamic();
			int num = segments.Length;
			int num2 = colors.Length;
			mesh.vertices = segments;
			int[] array = new int[num];
			Color[] array2 = new Color[num];
			int num3 = 0;
			for (int i = 0; i < num; i++)
			{
				array[i] = i;
				ref Color reference = ref array2[i];
				reference = colors[num3 % num2];
				if (i % 2 == 1)
				{
					num3++;
				}
			}
			mesh.subMeshCount = 1;
			mesh.SetIndices(array, MeshTopology.Lines, 0);
			mesh.uv = new Vector2[mesh.vertexCount];
			mesh.colors = array2;
			mesh.hideFlags = pb_Constant.EDITOR_OBJECT_HIDE_FLAGS;
			gizmos.Add(mesh);
		}

		public void Clear()
		{
			for (int i = 0; i < gizmos.Count; i++)
			{
				pool.Put(gizmos[i]);
			}
			gizmos.Clear();
		}

		private void OnRenderObject()
		{
			if (!(mat == null) && (Camera.current.gameObject.hideFlags & SceneCameraHideFlags) == SceneCameraHideFlags && !(Camera.current.name != "SceneCamera"))
			{
				mat.SetPass(0);
				for (int i = 0; i < gizmos.Count && gizmos[i] != null; i++)
				{
					Graphics.DrawMeshNow(gizmos[i], Vector3.zero, Quaternion.identity, 0);
				}
			}
		}
	}
}
