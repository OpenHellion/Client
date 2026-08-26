using UnityEngine;

public class MB2_TestUpdate : MonoBehaviour
{
	public MB3_MeshBaker meshbaker;

	public MB3_MultiMeshBaker multiMeshBaker;

	public GameObject[] objsToMove;

	public GameObject objWithChangingUVs;

	private Vector2[] uvs;

	private Mesh m;

	private void Start()
	{
		meshbaker.AddDeleteGameObjects(objsToMove, null, true);
		meshbaker.AddDeleteGameObjects(new GameObject[1] { objWithChangingUVs }, null, true);
		MeshFilter component = objWithChangingUVs.GetComponent<MeshFilter>();
		m = component.sharedMesh;
		uvs = m.uv;
		meshbaker.Apply();
		multiMeshBaker.AddDeleteGameObjects(objsToMove, null, true);
		multiMeshBaker.AddDeleteGameObjects(new GameObject[1] { objWithChangingUVs }, null, true);
		component = objWithChangingUVs.GetComponent<MeshFilter>();
		m = component.sharedMesh;
		uvs = m.uv;
		multiMeshBaker.Apply();
	}

	private void LateUpdate()
	{
		meshbaker.UpdateGameObjects(objsToMove, false);
		Vector2[] uv = m.uv;
		for (int i = 0; i < uv.Length; i++)
		{
			uv[i] = Mathf.Sin(Time.time) * uvs[i];
		}

		m.uv = uv;
		meshbaker.UpdateGameObjects(new GameObject[1] { objWithChangingUVs }, true, true, true, true, true);
		meshbaker.Apply(false, true, true, true, true, false, false, false, false);
		multiMeshBaker.UpdateGameObjects(objsToMove, false);
		uv = m.uv;
		for (int j = 0; j < uv.Length; j++)
		{
			uv[j] = Mathf.Sin(Time.time) * uvs[j];
		}

		m.uv = uv;
		multiMeshBaker.UpdateGameObjects(new GameObject[1] { objWithChangingUVs }, true, true, true, true, true);
		multiMeshBaker.Apply(false, true, true, true, true, false, false, false, false);
	}
}
