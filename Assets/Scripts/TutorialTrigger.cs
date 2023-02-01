using UnityEngine;
using ZeroGravity;
using ZeroGravity.Objects;

public class TutorialTrigger : MonoBehaviour
{
	public int Tutorial;

	private void OnTriggerEnter(Collider other)
	{
		if (other.GetComponentInParent<MyPlayer>() != null)
		{
			Client.Instance.CanvasManager.CanvasUI.ShowTutorialUI(Tutorial);
			Destroy(gameObject);
		}
	}
}
