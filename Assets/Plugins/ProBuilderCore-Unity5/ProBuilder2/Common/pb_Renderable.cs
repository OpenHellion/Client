using System;
using UnityEngine;

namespace ProBuilder2.Common
{
	[Serializable]
	public class pb_Renderable : ScriptableObject
	{
		public Mesh mesh;

		public Material[] materials;

		public Transform transform;

		public static pb_Renderable CreateInstance(Mesh InMesh, Material[] InMaterials, Transform transform = null)
		{
			pb_Renderable pb_Renderable2 = ScriptableObject.CreateInstance<pb_Renderable>();
			pb_Renderable2.mesh = InMesh;
			pb_Renderable2.materials = InMaterials;
			pb_Renderable2.transform = transform;
			return pb_Renderable2;
		}

		public static pb_Renderable CreateInstance(Mesh InMesh, Material InMaterial, Transform transform = null)
		{
			pb_Renderable pb_Renderable2 = ScriptableObject.CreateInstance<pb_Renderable>();
			pb_Renderable2.mesh = InMesh;
			pb_Renderable2.materials = new Material[1] { InMaterial };
			pb_Renderable2.transform = transform;
			return pb_Renderable2;
		}

		public static pb_Renderable CreateInstance()
		{
			pb_Renderable pb_Renderable2 = CreateInstance(new Mesh(), (Material)null, (Transform)null);
			pb_Renderable2.mesh.name = "pb_Renderable::Mesh";
			pb_Renderable2.mesh.hideFlags = HideFlags.DontSave;
			pb_Renderable2.mesh.MarkDynamic();
			pb_Renderable2.hideFlags = HideFlags.DontSave;
			return pb_Renderable2;
		}

		public static void DestroyInstance(UnityEngine.Object ren)
		{
			UnityEngine.Object.DestroyImmediate(ren);
		}

		private void OnDestroy()
		{
			if (mesh != null)
			{
				UnityEngine.Object.DestroyImmediate(mesh);
			}
		}
	}
}
