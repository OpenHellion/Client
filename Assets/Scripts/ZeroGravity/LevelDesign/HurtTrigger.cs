using System.Collections.Generic;
using OpenHellion.Net;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public class HurtTrigger : BaseSceneTrigger
	{
		public HurtType HurtType;

		[Tooltip("HP per sec.")] public float Damage;

		public bool InstantDamage;

		private bool playerInTrigger;

		private float duration;

		private float prevMessageTime;

		public override bool ExclusivePlayerLocking => false;

		public override bool CameraMovementAllowed => false;

		public override bool IsInteractable => false;

		public override bool IsNearTrigger => true;

		public override PlayerHandsCheckType PlayerHandsCheck => PlayerHandsCheckType.DontCheck;

		public override List<ItemType> PlayerHandsItemType => null;

		public override SceneTriggerType TriggerType => SceneTriggerType.HurtTrigger;

		private void FixedUpdate()
		{
			if (playerInTrigger && !InstantDamage)
			{
				duration += Time.fixedDeltaTime;
				if (Time.time - prevMessageTime > 0.5)
				{
					prevMessageTime = Time.time;
					SendDamageMessage();
				}
			}
		}

		protected override void OnTriggerEnter(Collider coli)
		{
			base.OnTriggerEnter(coli);
			if (coli == MyPlayer.Instance.FpsController.GetCollider())
			{
				if (InstantDamage)
				{
					duration = 1f;
					SendDamageMessage();
				}
				else
				{
					duration = 0f;
					playerInTrigger = true;
				}
			}
		}

		protected override void OnTriggerExit(Collider coli)
		{
			base.OnTriggerExit(coli);
			if (coli == MyPlayer.Instance.FpsController.GetCollider())
			{
				playerInTrigger = false;
				SendDamageMessage();
			}
		}

		private void OnDisable()
		{
			if (playerInTrigger)
			{
				playerInTrigger = false;
				SendDamageMessage();
			}
		}

		private void SendDamageMessage()
		{
			if (!(duration <= float.Epsilon))
			{
				NetworkController.Send(new HurtPlayerMessage
				{
					Damage = new PlayerDamage
					{
						HurtType = HurtType,
						Amount = duration * Damage
					},
					Duration = duration
				});
				duration = 0f;
			}
		}
	}
}
