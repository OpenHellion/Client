using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
	public float MaxHp = 100f;

	public float CurrentHp = 100f;

	public float MovementSpeed = 175f;

	public float StartX;

	public float TargetX;

	public float direction = 1f;

	public GameObject GameCanvas;

	public GameObject BulletPref;

	private void Start()
	{
		StartX = base.transform.position.x;
		TargetX = StartX + GetComponent<RectTransform>().rect.width * 1f;
	}

	private void Update()
	{
		if (CurrentHp < 50f)
		{
			GetComponent<Image>().color = Color.yellow;
		}
		if (CurrentHp < 20f)
		{
			GetComponent<Image>().color = Color.red;
		}
		if (CurrentHp < 0f)
		{
			Object.Destroy(base.gameObject);
		}
		if (base.transform.position.x <= StartX)
		{
			direction = 1f;
		}
		if (base.transform.position.x >= TargetX)
		{
			direction = -1f;
		}
		Vector3 position = new Vector3(base.transform.position.x + MovementSpeed * Time.deltaTime * direction, base.transform.position.y, base.transform.position.z);
		base.transform.position = position;
	}
}
