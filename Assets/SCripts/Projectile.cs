using UnityEngine;

public class Projectile : MonoBehaviour
{
	public float speed;

	public Vector3 targetDirection;

	public float damage;

	public float range;

	private void FixedUpdate()
	{
		base.transform.Translate(targetDirection * speed * Time.deltaTime);
		range -= speed * Time.deltaTime;
		if (range <= 0f)
		{
			Object.Destroy(base.gameObject);
		}
		RaycastHit hitInfo;
		if (!Physics.Raycast(base.transform.position, targetDirection, out hitInfo, speed * Time.deltaTime))
		{
		}
	}
}
