using UnityEngine;

public class MB_SkinnedMeshSceneController : MonoBehaviour
{
	public GameObject swordPrefab;

	public GameObject hatPrefab;

	public GameObject glassesPrefab;

	public GameObject workerPrefab;

	public GameObject targetCharacter;

	public MB3_MeshBaker skinnedMeshBaker;

	private GameObject swordInstance;

	private GameObject glassesInstance;

	private GameObject hatInstance;

	private void Start()
	{
		GameObject gameObject = Object.Instantiate(workerPrefab);
		gameObject.transform.position = new Vector3(1.31f, 0.985f, -0.25f);
		Animation component = gameObject.GetComponent<Animation>();
		component.wrapMode = WrapMode.Loop;
		component.cullingType = AnimationCullingType.AlwaysAnimate;
		component.Play("run");
		GameObject[] gos = new GameObject[1] { gameObject.GetComponentInChildren<SkinnedMeshRenderer>().gameObject };
		skinnedMeshBaker.AddDeleteGameObjects(gos, null, true);
		skinnedMeshBaker.Apply();
	}

	private void OnGUI()
	{
		if (GUILayout.Button("Add/Remove Sword"))
		{
			if (swordInstance == null)
			{
				Transform parent = SearchHierarchyForBone(targetCharacter.transform, "RightHandAttachPoint");
				swordInstance = Object.Instantiate(swordPrefab);
				swordInstance.transform.parent = parent;
				swordInstance.transform.localPosition = Vector3.zero;
				swordInstance.transform.localRotation = Quaternion.identity;
				swordInstance.transform.localScale = Vector3.one;
				GameObject[] gos = new GameObject[1]
					{ swordInstance.GetComponentInChildren<MeshRenderer>().gameObject };
				skinnedMeshBaker.AddDeleteGameObjects(gos, null, true);
				skinnedMeshBaker.Apply();
			}
			else if (skinnedMeshBaker.CombinedMeshContains(swordInstance.GetComponentInChildren<MeshRenderer>()
				         .gameObject))
			{
				GameObject[] deleteGOs = new GameObject[1]
					{ swordInstance.GetComponentInChildren<MeshRenderer>().gameObject };
				skinnedMeshBaker.AddDeleteGameObjects(null, deleteGOs, true);
				skinnedMeshBaker.Apply();
				Object.Destroy(swordInstance);
				swordInstance = null;
			}
		}

		if (GUILayout.Button("Add/Remove Hat"))
		{
			if (hatInstance == null)
			{
				Transform parent2 = SearchHierarchyForBone(targetCharacter.transform, "HeadAttachPoint");
				hatInstance = Object.Instantiate(hatPrefab);
				hatInstance.transform.parent = parent2;
				hatInstance.transform.localPosition = Vector3.zero;
				hatInstance.transform.localRotation = Quaternion.identity;
				hatInstance.transform.localScale = Vector3.one;
				GameObject[] gos2 = new GameObject[1] { hatInstance.GetComponentInChildren<MeshRenderer>().gameObject };
				skinnedMeshBaker.AddDeleteGameObjects(gos2, null, true);
				skinnedMeshBaker.Apply();
			}
			else if (skinnedMeshBaker.CombinedMeshContains(
				         hatInstance.GetComponentInChildren<MeshRenderer>().gameObject))
			{
				GameObject[] deleteGOs2 = new GameObject[1]
					{ hatInstance.GetComponentInChildren<MeshRenderer>().gameObject };
				skinnedMeshBaker.AddDeleteGameObjects(null, deleteGOs2, true);
				skinnedMeshBaker.Apply();
				Object.Destroy(hatInstance);
				hatInstance = null;
			}
		}

		if (GUILayout.Button("Add/Remove Glasses"))
		{
			if (glassesInstance == null)
			{
				Transform parent3 = SearchHierarchyForBone(targetCharacter.transform, "NoseAttachPoint");
				glassesInstance = Object.Instantiate(glassesPrefab);
				glassesInstance.transform.parent = parent3;
				glassesInstance.transform.localPosition = Vector3.zero;
				glassesInstance.transform.localRotation = Quaternion.identity;
				glassesInstance.transform.localScale = Vector3.one;
				GameObject[] gos3 = new GameObject[1]
					{ glassesInstance.GetComponentInChildren<MeshRenderer>().gameObject };
				skinnedMeshBaker.AddDeleteGameObjects(gos3, null, true);
				skinnedMeshBaker.Apply();
			}
			else if (skinnedMeshBaker.CombinedMeshContains(glassesInstance.GetComponentInChildren<MeshRenderer>()
				         .gameObject))
			{
				GameObject[] deleteGOs3 = new GameObject[1]
					{ glassesInstance.GetComponentInChildren<MeshRenderer>().gameObject };
				skinnedMeshBaker.AddDeleteGameObjects(null, deleteGOs3, true);
				skinnedMeshBaker.Apply();
				Object.Destroy(glassesInstance);
				glassesInstance = null;
			}
		}
	}

	public Transform SearchHierarchyForBone(Transform current, string name)
	{
		if (current.name.Equals(name))
		{
			return current;
		}

		for (int i = 0; i < current.childCount; i++)
		{
			Transform transform = SearchHierarchyForBone(current.GetChild(i), name);
			if (transform != null)
			{
				return transform;
			}
		}

		return null;
	}
}
