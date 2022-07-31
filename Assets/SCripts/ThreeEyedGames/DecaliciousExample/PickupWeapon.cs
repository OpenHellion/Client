using UnityEngine;

namespace ThreeEyedGames.DecaliciousExample
{
	public class PickupWeapon : IInteract
	{
		public override void Interact()
		{
			Camera.main.GetComponent<ShootDecal>().enabled = true;
			Object.Destroy(base.gameObject);
		}
	}
}
