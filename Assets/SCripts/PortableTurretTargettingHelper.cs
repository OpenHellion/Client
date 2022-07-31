using UnityEngine;
using ZeroGravity.Objects;

public class PortableTurretTargettingHelper : MonoBehaviour
{
	[SerializeField]
	private PortableTurret turret;

	private void OnTriggerEnter(Collider coli)
	{
		turret.OnTriggerEnterBehaviour(coli);
	}

	private void OnTriggerExit(Collider coli)
	{
		turret.OnTriggerExitBehaviour(coli);
	}
}
