using DigitalOpus.MB.Core;
using UnityEngine;

public class MB3_MeshBaker : MB3_MeshBakerCommon
{
	[SerializeField] protected MB3_MeshCombinerSingle _meshCombiner = new MB3_MeshCombinerSingle();

	public override MB3_MeshCombiner meshCombiner
	{
		get { return _meshCombiner; }
	}

	public void BuildSceneMeshObject()
	{
		_meshCombiner.BuildSceneMeshObject();
	}

	public virtual bool ShowHide(GameObject[] gos, GameObject[] deleteGOs)
	{
		return _meshCombiner.ShowHideGameObjects(gos, deleteGOs);
	}

	public virtual void ApplyShowHide()
	{
		_meshCombiner.ApplyShowHide();
	}

	public override bool AddDeleteGameObjects(GameObject[] gos, GameObject[] deleteGOs, bool disableRendererInSource)
	{
		_meshCombiner.name = base.name + "-mesh";
		return _meshCombiner.AddDeleteGameObjects(gos, deleteGOs, disableRendererInSource);
	}

	public override bool AddDeleteGameObjectsByID(GameObject[] gos, int[] deleteGOinstanceIDs,
		bool disableRendererInSource)
	{
		_meshCombiner.name = base.name + "-mesh";
		return _meshCombiner.AddDeleteGameObjectsByID(gos, deleteGOinstanceIDs, disableRendererInSource);
	}
}
