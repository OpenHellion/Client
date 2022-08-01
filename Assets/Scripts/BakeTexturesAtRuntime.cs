using UnityEngine;

public class BakeTexturesAtRuntime : MonoBehaviour
{
	public GameObject target;

	private float elapsedTime;

	private MB3_TextureBaker.CreateAtlasesCoroutineResult result = new MB3_TextureBaker.CreateAtlasesCoroutineResult();

	private void OnGUI()
	{
		GUILayout.Label("Time to bake textures: " + elapsedTime);
		if (GUILayout.Button("Combine textures & build combined mesh all at once"))
		{
			MB3_MeshBaker componentInChildren = target.GetComponentInChildren<MB3_MeshBaker>();
			MB3_TextureBaker component = target.GetComponent<MB3_TextureBaker>();
			component.textureBakeResults = ScriptableObject.CreateInstance<MB2_TextureBakeResults>();
			component.resultMaterial = new Material(Shader.Find("Diffuse"));
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			component.CreateAtlases();
			elapsedTime = Time.realtimeSinceStartup - realtimeSinceStartup;
			componentInChildren.ClearMesh();
			componentInChildren.textureBakeResults = component.textureBakeResults;
			componentInChildren.AddDeleteGameObjects(component.GetObjectsToCombine().ToArray(), null, true);
			componentInChildren.Apply();
		}
		if (GUILayout.Button("Combine textures & build combined mesh using coroutine"))
		{
			Debug.Log("Starting to bake textures on frame " + Time.frameCount);
			MB3_TextureBaker component2 = target.GetComponent<MB3_TextureBaker>();
			component2.textureBakeResults = ScriptableObject.CreateInstance<MB2_TextureBakeResults>();
			component2.resultMaterial = new Material(Shader.Find("Diffuse"));
			component2.onBuiltAtlasesSuccess = OnBuiltAtlasesSuccess;
			StartCoroutine(component2.CreateAtlasesCoroutine(null, result));
		}
	}

	private void OnBuiltAtlasesSuccess()
	{
		Debug.Log("Calling success callback. baking meshes");
		MB3_MeshBaker componentInChildren = target.GetComponentInChildren<MB3_MeshBaker>();
		MB3_TextureBaker component = target.GetComponent<MB3_TextureBaker>();
		if (result.isFinished && result.success)
		{
			componentInChildren.ClearMesh();
			componentInChildren.textureBakeResults = component.textureBakeResults;
			componentInChildren.AddDeleteGameObjects(component.GetObjectsToCombine().ToArray(), null, true);
			componentInChildren.Apply();
		}
		Debug.Log("Completed baking textures on frame " + Time.frameCount);
	}
}
