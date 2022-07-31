using DigitalOpus.MB.Core;
using UnityEngine;

public class MB3_TestBakeAllWithSameMaterial : MonoBehaviour
{
	public GameObject[] listOfObjsToCombineGood;

	public GameObject[] listOfObjsToCombineBad;

	private void Start()
	{
		testCombine();
	}

	private void testCombine()
	{
		MB3_MeshCombinerSingle mB3_MeshCombinerSingle = new MB3_MeshCombinerSingle();
		Debug.Log("About to bake 1");
		mB3_MeshCombinerSingle.AddDeleteGameObjects(listOfObjsToCombineGood, null);
		mB3_MeshCombinerSingle.Apply();
		mB3_MeshCombinerSingle.UpdateGameObjects(listOfObjsToCombineGood);
		mB3_MeshCombinerSingle.Apply();
		mB3_MeshCombinerSingle.AddDeleteGameObjects(null, listOfObjsToCombineGood);
		mB3_MeshCombinerSingle.Apply();
		Debug.Log("Did bake 1");
		Debug.Log("About to bake 2 should get error that one material doesn't match");
		mB3_MeshCombinerSingle.AddDeleteGameObjects(listOfObjsToCombineBad, null);
		mB3_MeshCombinerSingle.Apply();
		Debug.Log("Did bake 2");
		Debug.Log("Doing same with multi mesh combiner");
		MB3_MultiMeshCombiner mB3_MultiMeshCombiner = new MB3_MultiMeshCombiner();
		Debug.Log("About to bake 3");
		mB3_MultiMeshCombiner.AddDeleteGameObjects(listOfObjsToCombineGood, null);
		mB3_MultiMeshCombiner.Apply();
		mB3_MultiMeshCombiner.UpdateGameObjects(listOfObjsToCombineGood);
		mB3_MultiMeshCombiner.Apply();
		mB3_MultiMeshCombiner.AddDeleteGameObjects(null, listOfObjsToCombineGood);
		mB3_MultiMeshCombiner.Apply();
		Debug.Log("Did bake 3");
		Debug.Log("About to bake 4  should get error that one material doesn't match");
		mB3_MultiMeshCombiner.AddDeleteGameObjects(listOfObjsToCombineBad, null);
		mB3_MultiMeshCombiner.Apply();
		Debug.Log("Did bake 4");
	}
}
