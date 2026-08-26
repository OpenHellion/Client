using UnityEngine;

public class Projectile : MonoBehaviour
{
	public float speed;

	public Vector3 targetDirection;

	public float damage;

	public float range;

	private void FixedUpdate()
	{
		transform.Translate(targetDirection * speed * Time.deltaTime);
		range -= speed * Time.deltaTime;
		if (range <= 0f)
		{
			Destroy(gameObject);
		}

		RaycastHit hitInfo;
		if (!Physics.Raycast(transform.position, targetDirection, out hitInfo, speed * Time.deltaTime))
		{
		}
	}
}
