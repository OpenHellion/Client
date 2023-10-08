using System.Collections;
using UnityEngine;

namespace ThreeEyedGames.DecaliciousExample
{
	public class ShootDecal : MonoBehaviour
	{
		public GameObject DecalPrefab;

		public float RemoveAfterSeconds = 10f;

		public float RoundsPerMin = 10f;

		public int TotalAmmo = -1;

		public AudioClip Sound;

		protected float _timeSinceLastShot;

		private void Update()
		{
			_timeSinceLastShot += Time.deltaTime;
			if (TotalAmmo == 0 || !(_timeSinceLastShot > 60f / RoundsPerMin) ||
			    (!Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1)))
			{
				return;
			}

			TotalAmmo--;
			_timeSinceLastShot = 0f;
			RaycastHit hitInfo;
			if (Physics.Raycast(GetComponent<Camera>().ViewportPointToRay(Vector3.one * 0.5f), out hitInfo))
			{
				Transform transform = Object.Instantiate(DecalPrefab, hitInfo.collider.transform, true).transform;
				transform.position = hitInfo.point;
				transform.up = hitInfo.normal;
				transform.Rotate(Vector3.up, Random.Range(0, 360), Space.Self);
				Camera.main.transform.Rotate(Vector3.right, -1f * Random.Range(0.2f, 0.4f), Space.Self);
				Camera.main.transform.Rotate(Vector3.up, Random.Range(-0.2f, 0.2f), Space.World);
				Rigidbody component = hitInfo.collider.GetComponent<Rigidbody>();
				if (component != null)
				{
					component.AddForceAtPosition(Camera.main.transform.forward * 20f, hitInfo.point, ForceMode.Impulse);
				}

				AudioSource.PlayClipAtPoint(Sound, base.transform.position);
				StartCoroutine(RemoveDecal(transform.gameObject));
			}
		}

		protected IEnumerator RemoveDecal(GameObject decal)
		{
			yield return new WaitForSeconds(RemoveAfterSeconds);
			Decalicious d = decal.GetComponent<Decalicious>();
			while (d.Fade > 0f)
			{
				d.Fade -= Time.deltaTime * 0.3f;
				yield return null;
			}

			d.Fade = 0f;
		}
	}
}
