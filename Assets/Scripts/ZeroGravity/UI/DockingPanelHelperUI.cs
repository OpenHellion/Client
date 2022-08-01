using UnityEngine;

namespace ZeroGravity.UI
{
	public class DockingPanelHelperUI : MonoBehaviour
	{
		public GameObject DockingPanelRoot;

		public GameObject DockingPanelPrefab;

		private void Start()
		{
			GameObject gameObject = Object.Instantiate(DockingPanelPrefab, DockingPanelRoot.transform);
			gameObject.transform.Reset();
			gameObject.transform.localScale = Vector3.one;
		}
	}
}
