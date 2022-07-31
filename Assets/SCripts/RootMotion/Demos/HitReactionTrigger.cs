using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	public class HitReactionTrigger : MonoBehaviour
	{
		[SerializeField]
		private HitReaction hitReaction;

		[SerializeField]
		private float hitForce = 1f;

		private string colliderName;

		private void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hitInfo = default(RaycastHit);
				if (Physics.Raycast(ray, out hitInfo, 100f))
				{
					hitReaction.Hit(hitInfo.collider, ray.direction * hitForce, hitInfo.point);
					colliderName = hitInfo.collider.name;
				}
			}
		}

		private void OnGUI()
		{
			GUILayout.Label("LMB to shoot the Dummy, RMB to rotate the camera.");
			if (colliderName != string.Empty)
			{
				GUILayout.Label("Last Bone Hit: " + colliderName);
			}
		}
	}
}
