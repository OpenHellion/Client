using OpenHellion;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Network;

namespace ZeroGravity.Objects
{
	public class Grenade : Item
	{
		[Space(25f)] private bool isActive;

		private bool isThrown;

		public Renderer[] Renderers;

		public SoundEffect GrenadeSounds;

		public float DetonationTime;

		private float timer;

		private bool actionCanceled;

		private float throwStartTime;

		private new void Start()
		{
			base.Start();
		}

		public override bool PrimaryFunction()
		{
			if (isActive || actionCanceled)
			{
				return false;
			}

			throwStartTime = Time.time;
			World.InGameGUI.ThrowingItemToggle(true);
			isActive = true;
			GrenadeSounds.Play(0);
			GrenadeSounds.Play(2);
			MyPlayer.Instance.animHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null,
				null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
				null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
				null, null, null, null, null, null, true);
			DynamicObject dynamicObj = DynamicObj;
			GrenadeStats statsData = new GrenadeStats
			{
				IsActive = true
			};
			dynamicObj.SendStatsMessage(null, statsData);
			return false;
		}

		public override void PrimaryReleased()
		{
			if (isActive && !isThrown)
			{
				RequestDrop(Mathf.Clamp(Time.time - throwStartTime - World.DROP_THRESHOLD, 0f,
					World.DROP_MAX_TIME) * 2f);
				MyPlayer.Instance.animHelper.SetParameter(null, null, null, null, null, null, null, null, null, null,
					null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
					null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
					null, null, null, null, null, null, null, null, null, false);
				isThrown = true;
			}
			else
			{
				actionCanceled = false;
			}

			World.InGameGUI.ThrowingItemToggle(false);
		}

		public override bool SecondaryFunction()
		{
			if (!isActive)
			{
				return false;
			}

			isActive = false;
			timer = 0f;
			actionCanceled = true;
			GrenadeSounds.Play(1);
			MyPlayer.Instance.animHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null,
				null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
				null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
				null, null, null, null, null, null, false);
			DynamicObject dynamicObj = DynamicObj;
			GrenadeStats statsData = new GrenadeStats
			{
				IsActive = isActive
			};
			dynamicObj.SendStatsMessage(null, statsData);
			return false;
		}

		public override void ChangeEquip(EquipType type, Player pl)
		{
			if (type != EquipType.Hands)
			{
				isThrown = false;
			}
		}

		public override bool Explode()
		{
			if (base.Explode())
			{
				MyPlayer.Instance.animHelper.SetParameter(null, null, null, null, null, null, null, null, null, null,
					null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
					null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
					null, null, null, null, null, null, null, null, null, false);
				Renderer[] renderers = Renderers;
				foreach (Renderer renderer in renderers)
				{
					renderer.enabled = false;
				}

				return true;
			}

			return false;
		}

		private void OnDrawGizmos()
		{
		}

		public override void ProcesStatsData(DynamicObjectStats dos)
		{
			base.ProcesStatsData(dos);
			GrenadeStats grenadeStats = dos as GrenadeStats;
			if (grenadeStats.IsActive.HasValue)
			{
				isActive = grenadeStats.IsActive.Value;
			}

			if (grenadeStats.Time.HasValue)
			{
				timer = DetonationTime - grenadeStats.Time.Value;
			}

			if (grenadeStats.Blast.HasValue && grenadeStats.Blast.Value)
			{
				Explode();
			}
		}

		public override DynamicObjectAuxData GetAuxData()
		{
			GrenadeData baseAuxData = GetBaseAuxData<GrenadeData>();
			baseAuxData.DetonationTime = DetonationTime;
			baseAuxData.IsActive = isActive;
			return baseAuxData;
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(base.transform.position, ExplosionRadius);
		}
	}
}
