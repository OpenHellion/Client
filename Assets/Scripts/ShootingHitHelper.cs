using UnityEngine;
using ZeroGravity.Objects;

public class ShootingHitHelper : MonoBehaviour
{
	public SpaceObject MainObject;

	private void Start()
	{
		if (MainObject == null)
		{
			MainObject = GetComponentInParent<SpaceObject>();
		}
	}
}
