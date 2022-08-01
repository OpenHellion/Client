using UnityEngine;
using ZeroGravity;

public class PowerSupplyPanelUIHelper : MonoBehaviour
{
	public GameObject PowerSupplyPanelRoot;

	public GameObject PowerSupplyPanelPrefab;

	private void Start()
	{
		GameObject gameObject = Object.Instantiate(PowerSupplyPanelPrefab, PowerSupplyPanelRoot.transform);
		gameObject.transform.Reset();
		gameObject.transform.localScale = Vector3.one;
	}
}
