using System.Collections.Generic;
using OpenHellion.Net;
using UnityEngine;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;

namespace ZeroGravity.Objects
{
	[RequireComponent(typeof(SceneTriggerTurret))]
	public class Turret : MonoBehaviour
	{
		private float lastFireTime;

		private Transform mainCamera;

		private ShotData shotData;

		private List<TurretMod> turretMods;

		private TurretMod curretntMod;

		public List<Transform> tipsOfTheGun;

		public List<int> shootingOrder;

		public int shootingStep;

		private int currentGun;

		[SerializeField]
		private Transform baseTransform;

		[SerializeField]
		private Transform gunsTransform;

		[SerializeField]
		private Transform leftGunTransform;

		[SerializeField]
		private Transform rightGunTransform;

		private float lerpValue;

		private void Start()
		{
			if (Client.IsGameBuild)
			{
				mainCamera = MyPlayer.Instance.FpsController.MainCamera.transform;
			}
			EventSystem.AddListener(typeof(TurretShootingMessage), ShootingDataListener);
		}

		private void ShootingDataListener(NetworkData data)
		{
		}

		public void SetNewRotation(float x, float y, bool freezeBase)
		{
			Vector3 forward = gunsTransform.forward;
			Vector3 up = gunsTransform.up;
			if (x != 0f)
			{
				Vector3 vector = Vector3.Cross(-forward, up);
				forward = Quaternion.AngleAxis(x, vector) * forward;
				up = Vector3.Cross(forward, vector);
			}
			if (freezeBase)
			{
				lerpValue = 0f;
				Vector3 forward2 = leftGunTransform.forward;
				Vector3 forward3 = rightGunTransform.forward;
				if (y != 0f)
				{
					forward2 = Quaternion.AngleAxis(y, baseTransform.up) * forward2;
					forward3 = Quaternion.AngleAxis(y, baseTransform.up) * forward3;
				}
			}
			else
			{
				lerpValue += Time.deltaTime;
				leftGunTransform.forward = Vector3.Lerp(leftGunTransform.forward, gunsTransform.forward, lerpValue);
				rightGunTransform.forward = Vector3.Lerp(rightGunTransform.forward, gunsTransform.forward, lerpValue);
				Vector3 forward4 = baseTransform.forward;
				if (y != 0f)
				{
					forward4 = Quaternion.AngleAxis(y, baseTransform.up) * forward4;
				}
			}
		}

		public void RotateToNewRotation(Transform transformToRotate, Vector3 rotation)
		{
			transformToRotate.rotation = Quaternion.Euler(rotation);
		}

		public void Shoot()
		{
			int num = currentGun;
			while (currentGun < num + shootingStep)
			{
				currentGun++;
			}
		}

		private void PlayerShootingMessageListener(NetworkData data)
		{
			PlayerShootingMessage playerShootingMessage = data as PlayerShootingMessage;
			PlayerHitMessage playerHitMessage = new PlayerHitMessage();
			playerHitMessage.HitIndentifier = playerShootingMessage.HitIndentifier;
			playerHitMessage.HitSuccessfull = false;
		}

		private void OnDestroy()
		{
			EventSystem.RemoveListener(typeof(TurretShootingMessage), ShootingDataListener);
		}
	}
}
