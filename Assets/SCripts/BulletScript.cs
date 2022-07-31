using UnityEngine;

public class BulletScript : MonoBehaviour
{
	public float BulletSpeed;

	public bool DirectionUp;

	private float StartTime;

	private float DestructionTime = 20f;

	public float Damage;

	private void Start()
	{
		StartTime = Time.time;
	}

	private void Update()
	{
		if (Time.time - StartTime > DestructionTime)
		{
			Object.Destroy(base.gameObject);
		}
		Vector3 position = default(Vector3);
		position = ((!DirectionUp) ? new Vector3(base.transform.position.x, base.transform.position.y - Time.deltaTime * BulletSpeed, base.transform.position.z) : new Vector3(base.transform.position.x, base.transform.position.y + Time.deltaTime * BulletSpeed, base.transform.position.z));
		base.transform.position = position;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		other.GetComponent<Enemy>().CurrentHp -= Damage;
		Object.Destroy(base.gameObject);
	}
}
