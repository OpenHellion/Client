using DigitalOpus.MB.Core;
using UnityEngine;

public class MB3_MultiMeshBaker : MB3_MeshBakerCommon
{
	[SerializeField]
	protected MB3_MultiMeshCombiner _meshCombiner = new MB3_MultiMeshCombiner();

	public override MB3_MeshCombiner meshCombiner
	{
		get
		{
			return _meshCombiner;
		}
	}

	public override bool AddDeleteGameObjects(GameObject[] gos, GameObject[] deleteGOs, bool disableRendererInSource)
	{
		if (_meshCombiner.resultSceneObject == null)
		{
			_meshCombiner.resultSceneObject = new GameObject("CombinedMesh-" + base.name);
		}
		meshCombiner.name = base.name + "-mesh";
		return _meshCombiner.AddDeleteGameObjects(gos, deleteGOs, disableRendererInSource);
	}

	public override bool AddDeleteGameObjectsByID(GameObject[] gos, int[] deleteGOs, bool disableRendererInSource)
	{
		if (_meshCombiner.resultSceneObject == null)
		{
			_meshCombiner.resultSceneObject = new GameObject("CombinedMesh-" + base.name);
		}
		meshCombiner.name = base.name + "-mesh";
		return _meshCombiner.AddDeleteGameObjectsByID(gos, deleteGOs, disableRendererInSource);
	}
}
