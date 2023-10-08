using UnityEngine;
using ZeroGravity.LevelDesign;
using ZeroGravity.Objects;

namespace ZeroGravity.CharacterMovement
{
	public class TriggerEventHelper : MonoBehaviour
	{
		[SerializeField] private MyCharacterController characterController;

		[SerializeField] private OtherCharacterController otherCharacterController;

		private void OnTriggerEnter(Collider coli)
		{
			SceneColliderPlayer component = coli.GetComponent<SceneColliderPlayer>();
			if (!(component != null))
			{
				return;
			}

			if (characterController != null && characterController.isActiveAndEnabled)
			{
				characterController.ToggleInPlayerCollider(true);
				SpaceObjectVessel spaceObjectVessel =
					coli.GetComponentInParent<GeometryRoot>().MainObject as SpaceObjectVessel;
				characterController.NearbyVessel = ((!(spaceObjectVessel.DockedToMainVessel != null))
					? spaceObjectVessel
					: spaceObjectVessel.DockedToMainVessel);
			}
			else if (otherCharacterController != null && otherCharacterController.isActiveAndEnabled)
			{
				otherCharacterController.PlayerColliderToggle(true);
			}
		}

		private void OnTriggerExit(Collider coli)
		{
			SceneColliderPlayer component = coli.GetComponent<SceneColliderPlayer>();
			if (component != null)
			{
				if (characterController != null && characterController.isActiveAndEnabled)
				{
					characterController.ToggleInPlayerCollider(false);
					characterController.NearbyVessel = null;
				}
				else if (otherCharacterController != null && otherCharacterController.isActiveAndEnabled)
				{
					otherCharacterController.PlayerColliderToggle(false);
				}
			}
		}
	}
}
