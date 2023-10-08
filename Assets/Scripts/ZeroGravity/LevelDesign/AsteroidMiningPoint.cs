using System;
using UnityEngine;
using UnityEngine.Playables;
using ZeroGravity.Data;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using OpenHellion.Net;

namespace ZeroGravity.LevelDesign
{
	public class AsteroidMiningPoint : MonoBehaviour, ISceneObject
	{
		[SerializeField] private int _InSceneID;

		[SerializeField] private float size = 1f;

		[NonSerialized] public SpaceObjectVessel ParentVessel;

		public ResourceType ResourceType;

		public float MaxQuantity;

		public float Quantity;

		public GameObject GasBurst;

		public PlayableDirector PlayableDirector;

		public MiningPointVisual MiningPointVisual;

		public int InSceneID
		{
			get { return _InSceneID; }
			set { _InSceneID = value; }
		}

		public void SetDetails(AsteroidMiningPointDetails ampd)
		{
			if (ampd.MaxQuantity <= float.Epsilon)
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}

			if (ResourceType != ampd.ResourceType)
			{
				ResourceType = ampd.ResourceType;
				MiningPointVisual.SetMaterials(ResourceType);
			}

			MaxQuantity = ampd.MaxQuantity;
			HurtTrigger[] componentsInChildren = GetComponentsInChildren<HurtTrigger>(true);
			foreach (HurtTrigger hurtTrigger in componentsInChildren)
			{
				hurtTrigger.Damage *= ParentVessel.Orbit.Parent.CelestialBody.AsteroidGasBurstDmgMultiplier;
			}

			SetValues(ampd.Quantity, null);
		}

		public AsteroidMiningPointData GetData()
		{
			AsteroidMiningPointData asteroidMiningPointData = new AsteroidMiningPointData();
			asteroidMiningPointData.InSceneID = InSceneID;
			asteroidMiningPointData.Size = size;
			asteroidMiningPointData.Position = base.transform.localPosition.ToArray();
			return asteroidMiningPointData;
		}

		private void Awake()
		{
			EventSystem.AddListener(typeof(MiningPointStatsMessage), MiningPointStatsMessageListener);
		}

		private void MiningPointStatsMessageListener(NetworkData data)
		{
			MiningPointStatsMessage miningPointStatsMessage = data as MiningPointStatsMessage;
			if (miningPointStatsMessage.ID.VesselGUID == ParentVessel.GUID &&
			    miningPointStatsMessage.ID.InSceneID == InSceneID)
			{
				SetValues(miningPointStatsMessage.Quantity, miningPointStatsMessage.GasBurst);
			}
		}

		private void SetValues(float? quantity, bool? gasBurst)
		{
			try
			{
				if (quantity.HasValue)
				{
					Quantity = quantity.Value;
					if (PlayableDirector != null)
					{
						PlayableDirector.time = (1f - Quantity / MaxQuantity) * 1.6666f;
						PlayableDirector.Evaluate();
					}

					MiningPointVisual.MiningPointLight.intensity = Quantity / MaxQuantity * 0.5f;
				}

				if (gasBurst.HasValue && gasBurst.Value && GasBurst != null)
				{
					GasBurst.gameObject.SetActive(true);
				}
			}
			catch (Exception ex)
			{
				Dbg.Error(ex.Message, ex.StackTrace);
			}
		}

		private void OnDestroy()
		{
			EventSystem.RemoveListener(typeof(MiningPointStatsMessage), MiningPointStatsMessageListener);
		}
	}
}
