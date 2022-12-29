using System.Collections.Generic;
using OpenHellion.Networking;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public class HurtTrigger : BaseSceneTrigger
	{
		public HurtType HurtType;

		[Tooltip("HP per sec.")]
		public float Damage;

		public bool InstantDamage;

		private bool playerInTrigger;

		private float duration;

		private float prevMessageTime;

		public override bool ExclusivePlayerLocking
		{
			get
			{
				return false;
			}
		}

		public override bool CameraMovementAllowed
		{
			get
			{
				return false;
			}
		}

		public override bool IsInteractable
		{
			get
			{
				return false;
			}
		}

		public override bool IsNearTrigger
		{
			get
			{
				return true;
			}
		}

		public override PlayerHandsCheckType PlayerHandsCheck
		{
			get
			{
				return PlayerHandsCheckType.DontCheck;
			}
		}

		public override List<ItemType> PlayerHandsItemType
		{
			get
			{
				return null;
			}
		}

		public override SceneTriggerType TriggerType
		{
			get
			{
				return SceneTriggerType.HurtTrigger;
			}
		}

		private void FixedUpdate()
		{
			if (playerInTrigger && !InstantDamage)
			{
				duration += Time.fixedDeltaTime;
				if ((double)(Time.time - prevMessageTime) > 0.5)
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
				NetworkController.Instance.SendToGameServer(new HurtPlayerMessage
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
