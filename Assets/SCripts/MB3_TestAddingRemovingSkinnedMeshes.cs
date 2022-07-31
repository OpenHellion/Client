using System.Collections;
using UnityEngine;

public class MB3_TestAddingRemovingSkinnedMeshes : MonoBehaviour
{
	public MB3_MeshBaker meshBaker;

	public GameObject[] g;

	private void Start()
	{
		StartCoroutine(TestScript());
	}

	private IEnumerator TestScript()
	{
		Debug.Log("Test 1 adding 0,1,2");
		GameObject[] a4 = new GameObject[3]
		{
			g[0],
			g[1],
			g[2]
		};
		meshBaker.AddDeleteGameObjects(a4, null, true);
		meshBaker.Apply();
		meshBaker.meshCombiner.CheckIntegrity();
		yield return new WaitForSeconds(3f);
		Debug.Log("Test 2 remove 1 and add 3,4,5");
		GameObject[] d3 = new GameObject[1] { g[1] };
		a4 = new GameObject[3]
		{
			g[3],
			g[4],
			g[5]
		};
		meshBaker.AddDeleteGameObjects(a4, d3, true);
		meshBaker.Apply();
		meshBaker.meshCombiner.CheckIntegrity();
		yield return new WaitForSeconds(3f);
		Debug.Log("Test 3 remove 0,2,5 and add 1");
		d3 = new GameObject[3]
		{
			g[3],
			g[4],
			g[5]
		};
		a4 = new GameObject[1] { g[1] };
		meshBaker.AddDeleteGameObjects(a4, d3, true);
		meshBaker.Apply();
		meshBaker.meshCombiner.CheckIntegrity();
		yield return new WaitForSeconds(3f);
		Debug.Log("Test 3 remove all remaining");
		d3 = new GameObject[3]
		{
			g[0],
			g[1],
			g[2]
		};
		meshBaker.AddDeleteGameObjects(null, d3, true);
		meshBaker.Apply();
		meshBaker.meshCombiner.CheckIntegrity();
		yield return new WaitForSeconds(3f);
		Debug.Log("Test 3 add all");
		meshBaker.AddDeleteGameObjects(g, null, true);
		meshBaker.Apply();
		meshBaker.meshCombiner.CheckIntegrity();
		yield return new WaitForSeconds(1f);
		Debug.Log("Done");
	}
}
