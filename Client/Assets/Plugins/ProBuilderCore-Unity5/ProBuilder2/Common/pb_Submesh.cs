using System;
using UnityEngine;

namespace ProBuilder2.Common
{
	[Serializable]
	public class pb_Submesh
	{
		public int[] indices;

		public MeshTopology topology;

		public Material material;

		public pb_Submesh(Material material, MeshTopology topology, int[] indices)
		{
			this.indices = indices;
			this.topology = topology;
			this.material = material;
		}

		public pb_Submesh(Mesh mesh, int subMeshIndex, Material material)
		{
			indices = mesh.GetIndices(subMeshIndex);
			topology = mesh.GetTopology(subMeshIndex);
			this.material = material;
		}

		public override string ToString()
		{
			return string.Format("{0}, {1}, {2}", (!(material != null)) ? "null" : material.name, topology.ToString(), (indices == null) ? "0" : indices.Length.ToString());
		}
	}
}
