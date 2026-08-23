using UnityEngine;

public class ManipulatorSingle : MonoBehaviour
{
	private GameObject loptica;

	private LineRenderer lineRend;

	private BoxCollider boxCollider;

	private GameObject parentGO;

	private void Start()
	{
		parentGO = transform.parent.gameObject;
		lineRend = GetComponent<LineRenderer>();
		loptica = transform.Find("Sphere").gameObject;
		boxCollider = transform.Find("Collider").GetComponent<BoxCollider>();
	}

	private void Update()
	{
		float num = Vector3.Distance(transform.localPosition, loptica.transform.localPosition);
		Vector3[] positions = new Vector3[2]
		{
			new Vector3(0f, 0f, 0f),
			new Vector3(0f, 0f, num)
		};
		lineRend.SetPositions(positions);
		boxCollider.center = new Vector3(0f, 0f, num / 2f);
		boxCollider.size = new Vector3(0.5f, 0.5f, num);
	}
}
