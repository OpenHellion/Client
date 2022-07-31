using TeamUtility.IO;
using UnityEngine;
using UnityEngine.UI;

public class InceptionGame : MonoBehaviour
{
	public GameObject GameCanvas;

	public GameObject Ship;

	public GameObject TurretTop;

	public GameObject TurretBot;

	public GameObject BulletPref;

	public GameObject ActiveTurret;

	public GameObject EnemyPref;

	public bool DirectionUp;

	private float ShipSpeed = 150f;

	private void Start()
	{
		ActiveTurret = TurretTop;
		DirectionUp = true;
		TurretBot.SetActive(false);
		Ship.GetComponent<Image>().color = Color.blue;
		CreateEnemies();
	}

	private void Update()
	{
		if (InputManager.GetButtonDown("Jump"))
		{
			ShootBullet();
		}
		if (InputManager.GetButtonDown("Sprint"))
		{
			ChangeActiveTurret();
		}
		if (InputManager.GetAxis("Right") > 0f)
		{
			MovementHorizontal(true);
		}
		else if (InputManager.GetAxis("Right") < 0f)
		{
			MovementHorizontal(false);
		}
		if (InputManager.GetAxis("Forward") > 0f)
		{
			MovementVertical(true);
		}
		else if (InputManager.GetAxis("Forward") < 0f)
		{
			MovementVertical(false);
		}
	}

	private void ChangeActiveTurret()
	{
		if (ActiveTurret == TurretTop)
		{
			TurretBot.SetActive(true);
			ActiveTurret = TurretBot;
			DirectionUp = false;
			TurretTop.SetActive(false);
		}
		else
		{
			TurretTop.SetActive(true);
			ActiveTurret = TurretTop;
			DirectionUp = true;
			TurretBot.SetActive(false);
		}
	}

	private void MovementHorizontal(bool moveRight)
	{
		Vector3 position = default(Vector3);
		if (moveRight)
		{
			position = new Vector3(Ship.transform.position.x + Time.deltaTime * ShipSpeed, Ship.transform.position.y, Ship.transform.position.z);
		}
		else if (!moveRight)
		{
			position = new Vector3(Ship.transform.position.x - Time.deltaTime * ShipSpeed, Ship.transform.position.y, Ship.transform.position.z);
		}
		if (position.x < GameCanvas.GetComponent<RectTransform>().rect.width && position.x > 0f)
		{
			Ship.transform.position = position;
		}
	}

	private void MovementVertical(bool moveUp)
	{
		Vector3 position = default(Vector3);
		if (moveUp)
		{
			position = new Vector3(Ship.transform.position.x, Ship.transform.position.y + Time.deltaTime * ShipSpeed, Ship.transform.position.z);
		}
		else if (!moveUp)
		{
			position = new Vector3(Ship.transform.position.x, Ship.transform.position.y - Time.deltaTime * ShipSpeed, Ship.transform.position.z);
		}
		if (position.y < GameCanvas.GetComponent<RectTransform>().rect.height && position.y > 0f)
		{
			Ship.transform.position = position;
		}
	}

	private void ShootBullet()
	{
		GameObject gameObject = Object.Instantiate(BulletPref, ActiveTurret.transform.position, base.transform.rotation);
		gameObject.transform.SetParent(GameCanvas.transform);
		gameObject.GetComponent<BulletScript>().DirectionUp = DirectionUp;
		gameObject.GetComponent<BulletScript>().BulletSpeed = 250f;
		gameObject.GetComponent<BulletScript>().Damage = 10f;
	}

	private void SpawnEnemy(Vector3 position)
	{
		GameObject gameObject = Object.Instantiate(EnemyPref, position, base.transform.rotation);
		gameObject.transform.SetParent(GameCanvas.transform);
		gameObject.GetComponent<Enemy>().GameCanvas = GameCanvas;
		gameObject.GetComponent<Enemy>().BulletPref = BulletPref;
	}

	private void CreateEnemies()
	{
		float width = GameCanvas.GetComponent<RectTransform>().rect.width;
		float height = GameCanvas.GetComponent<RectTransform>().rect.height;
		float y = height * 0.9f;
		float width2 = EnemyPref.GetComponent<RectTransform>().rect.width;
		width2 *= 1.5f;
		float f = width / width2;
		int num = Mathf.FloorToInt(f);
		float num2 = 0f;
		for (int i = 0; i < num; i++)
		{
			if (i != 0 || i != num)
			{
				num2 += width2;
				SpawnEnemy(new Vector3(num2, y, 0f));
			}
		}
	}
}
