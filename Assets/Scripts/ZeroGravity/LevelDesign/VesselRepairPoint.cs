using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Math;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;
using ZeroGravity.UI;

namespace ZeroGravity.LevelDesign
{
	public class VesselRepairPoint : MonoBehaviour, ISceneObject
	{
		[SerializeField]
		private int _InSceneID;

		public List<RepairPointParticleEffect> DamageEffects;

		public List<RepairPointMeshEffect> DamageMeshes;

		public List<RepairPointLight> LightEffects;

		[NonSerialized]
		public SpaceObjectVessel ParentVessel;

		public SceneTriggerRoom Room;

		[Space(10f)]
		public RepairPointDamageType DamageType;

		[Tooltip("Used only if Damage Type is 'System'")]
		public VesselComponent AffectedSystem;

		public bool External;

		[Range(0f, 1f)]
		public float MalfunctionThreshold;

		[Range(0f, 1f)]
		public float RepairThreshold;

		[Space(10f)]
		[Range(0f, 1f)]
		public float PrimaryDamageVisualThreshold;

		public GameObject PrimaryDamageVisual;

		[Space(10f)]
		public List<RepairPointParticleEffect> SecondaryDamageEffects;

		public List<RepairPointMeshEffect> SecondaryDamageMesh;

		public GameObject SecondaryDamageObject;

		[Space(10f)]
		[SerializeField]
		private float _MaxHealth;

		[SerializeField]
		private float _Health;

		private bool firstStrike;

		private bool _SecondaryDamageActive;

		private bool prevSecondaryDamageActive;

		public bool SecondaryDamageActive
		{
			get
			{
				if (DamageType == RepairPointDamageType.Fire)
				{
					return _SecondaryDamageActive && Room.AirQuality * Room.AirPressure >= 0.25f;
				}
				if (DamageType == RepairPointDamageType.Breach)
				{
					return _SecondaryDamageActive && Room.AirPressure > float.Epsilon;
				}
				return _SecondaryDamageActive;
			}
			set
			{
				_SecondaryDamageActive = value;
				UpdateEffects();
			}
		}

		public float MaxHealth
		{
			get
			{
				return _MaxHealth;
			}
			set
			{
				if (_MaxHealth != (_MaxHealth = value))
				{
					UpdateEffects();
				}
			}
		}

		public float Health
		{
			get
			{
				return _Health;
			}
			set
			{
				if (_Health - value > _MaxHealth * 0.1f)
				{
					firstStrike = true;
				}
				if (_Health != (_Health = value))
				{
					UpdateEffects();
				}
			}
		}

		public int InSceneID
		{
			get
			{
				return _InSceneID;
			}
			set
			{
				_InSceneID = value;
			}
		}

		public float Damage => 1f - MathHelper.Clamp(Health / (MaxHealth * 0.98f), 0f, 1f);

		private void OnEnable()
		{
			UpdateEffects();
		}

		private void Awake()
		{
			foreach (Transform item in base.transform)
			{
				RepairPointParticleEffect component = item.gameObject.GetComponent<RepairPointParticleEffect>();
				if (component != null && item.gameObject.activeInHierarchy && !DamageEffects.Contains(component) && !SecondaryDamageEffects.Contains(component))
				{
					DamageEffects.Add(item.gameObject.GetComponent<RepairPointParticleEffect>());
				}
				RepairPointMeshEffect component2 = item.gameObject.GetComponent<RepairPointMeshEffect>();
				if (component2 != null && item.gameObject.activeInHierarchy && !DamageMeshes.Contains(component2))
				{
					DamageMeshes.Add(item.gameObject.GetComponent<RepairPointMeshEffect>());
				}
				RepairPointLight component3 = item.gameObject.GetComponent<RepairPointLight>();
				if (component3 != null && item.gameObject.activeInHierarchy && !LightEffects.Contains(component3))
				{
					LightEffects.Add(item.gameObject.GetComponent<RepairPointLight>());
				}
			}
			if (Client.IsGameBuild)
			{
				ParentVessel = GetComponentInParent<GeometryRoot>().MainObject as SpaceObjectVessel;
			}
			foreach (RepairPointParticleEffect damageEffect in DamageEffects)
			{
				damageEffect.Stop();
			}
			foreach (RepairPointMeshEffect damageMesh in DamageMeshes)
			{
				damageMesh.Mesh.material.SetFloat("_Health", 1f);
				damageMesh.Mesh.material.SetFloat("_SystemDamage", 0f);
			}
			foreach (RepairPointLight lightEffect in LightEffects)
			{
				lightEffect.Light.intensity = 0f;
			}
		}

		private void Update()
		{
			UpdateLightFlicker();
			if (!Client.IsGameBuild)
			{
				UpdateEffects();
			}
		}

		public void UpdateEffects()
		{
			if (PrimaryDamageVisual != null)
			{
				PrimaryDamageVisual.Activate(Health / MaxHealth < PrimaryDamageVisualThreshold);
			}
			if (SecondaryDamageObject != null)
			{
				SecondaryDamageObject.Activate(SecondaryDamageActive);
			}
			foreach (RepairPointParticleEffect item in SecondaryDamageEffects.Where((RepairPointParticleEffect m) => m.Effect != null))
			{
				if (SecondaryDamageActive)
				{
					if (Room == null || !Room.UseGravity || Room.GravityForce == Vector3.zero)
					{
						item.SetGravity(gravity: false);
					}
					else
					{
						item.SetGravity(gravity: true);
					}
					item.SetIntensity(Damage);
				}
				else
				{
					item.Stop();
				}
			}
			foreach (RepairPointMeshEffect item2 in DamageMeshes.Where((RepairPointMeshEffect m) => m.Mesh != null))
			{
				if (SecondaryDamageActive)
				{
					item2.Mesh.material.SetFloat("_SystemDamage", Damage);
				}
				else
				{
					item2.Mesh.material.SetFloat("_SystemDamage", 0f);
				}
			}
			if (DamageEffects.Count == 0 && DamageMeshes.Count == 0)
			{
				return;
			}
			if (Damage <= float.Epsilon)
			{
				foreach (RepairPointParticleEffect item3 in DamageEffects.Where((RepairPointParticleEffect m) => m.Effect != null))
				{
					item3.Stop();
				}
				foreach (RepairPointMeshEffect item4 in DamageMeshes.Where((RepairPointMeshEffect m) => m.Mesh != null))
				{
					item4.Mesh.material.SetFloat("_Health", 1f);
				}
				{
					foreach (RepairPointLight lightEffect in LightEffects)
					{
						lightEffect.Light.intensity = 0f;
					}
					return;
				}
			}
			foreach (RepairPointParticleEffect item5 in DamageEffects.Where((RepairPointParticleEffect m) => m.Effect != null))
			{
				if (Room == null || !Room.UseGravity || Room.GravityForce == Vector3.zero)
				{
					item5.SetGravity(gravity: false);
				}
				else
				{
					item5.SetGravity(gravity: true);
				}
				if (item5.PlayOnce)
				{
					if (firstStrike)
					{
						item5.SetIntensity(Damage);
						firstStrike = false;
					}
				}
				else
				{
					item5.SetIntensity(Damage);
				}
			}
			foreach (RepairPointMeshEffect item6 in DamageMeshes.Where((RepairPointMeshEffect m) => m.Mesh != null))
			{
				item6.Mesh.material.SetFloat("_Health", item6.Curve.Evaluate(1f - Damage));
				if (!SecondaryDamageMesh.Contains(item6))
				{
					item6.Mesh.material.SetFloat("_SystemDamage", 0f);
				}
			}
		}

		public void UpdateLightFlicker()
		{
			if (!(Damage > float.Epsilon))
			{
				return;
			}
			foreach (RepairPointLight lightEffect in LightEffects)
			{
				if (lightEffect.Curve.Evaluate(Damage) > 0f)
				{
					lightEffect.Light.intensity = lightEffect.IntensityCurve.Evaluate(Time.time % lightEffect.JitterLength / lightEffect.JitterLength) * lightEffect.JitterIntensity * lightEffect.Curve.Evaluate(Damage);
				}
				else
				{
					lightEffect.Light.intensity = 0f;
				}
			}
		}

		public VesselRepairPointData GetData()
		{
			if (DamageType == RepairPointDamageType.System && AffectedSystem == null)
			{
				throw new Exception("Affected system not set.");
			}
			if (Room == null)
			{
				SceneTriggerRoom componentInParent = GetComponentInParent<SceneTriggerRoom>();
				if (componentInParent != null)
				{
					Room = GetComponentInParent<SceneTriggerRoom>();
				}
				if (Room == null && (DamageType == RepairPointDamageType.Breach || DamageType == RepairPointDamageType.Fire || DamageType == RepairPointDamageType.Gravity))
				{
					throw new Exception("Room not set.");
				}
			}
			VesselRepairPointData vesselRepairPointData = new VesselRepairPointData();
			vesselRepairPointData.InSceneID = InSceneID;
			vesselRepairPointData.RoomID = ((!(Room == null)) ? Room.InSceneID : (-1));
			vesselRepairPointData.DamageType = DamageType;
			vesselRepairPointData.AffectedSystemID = ((!(AffectedSystem != null) || DamageType != RepairPointDamageType.System) ? (-1) : AffectedSystem.InSceneID);
			vesselRepairPointData.MalfunctionThreshold = MalfunctionThreshold;
			vesselRepairPointData.RepairThreshold = RepairThreshold;
			vesselRepairPointData.External = External;
			return vesselRepairPointData;
		}

		private void Start()
		{
			if (Room != null)
			{
				Room.AddBehaviourScript(this);
			}
			UpdateEffects();
		}

		private void OnDrawGizmos()
		{
			if (DamageType == RepairPointDamageType.None)
			{
				Gizmos.DrawIcon(base.transform.position, "RepairRegular");
			}
			if (DamageType == RepairPointDamageType.Breach)
			{
				Gizmos.DrawIcon(base.transform.position, "RepairBreach");
			}
			if (DamageType == RepairPointDamageType.Fire)
			{
				Gizmos.DrawIcon(base.transform.position, "RepairFire");
			}
			if (DamageType == RepairPointDamageType.System)
			{
				Gizmos.DrawIcon(base.transform.position, "RepairSystem");
			}
		}

		public void SetDetails(VesselRepairPointDetails vrpd)
		{
			Health = vrpd.Health;
			MaxHealth = vrpd.MaxHealth;
			prevSecondaryDamageActive = SecondaryDamageActive;
			SecondaryDamageActive = vrpd.SecondaryDamageActive;
			bool flag = !prevSecondaryDamageActive && SecondaryDamageActive;
			UpdateEffects();
			if (flag && MyPlayer.Instance.Parent is SpaceObjectVessel && MyPlayer.Instance.IsInVesselHierarchy(ParentVessel))
			{
				string text = ((DamageType != RepairPointDamageType.System) ? DamageType.ToLocalizedString() : ((!(AffectedSystem is Generator)) ? ((AffectedSystem as SubSystem).Type.ToLocalizedString() + " " + Localization.Failure.ToLower()) : ((AffectedSystem as Generator).Type.ToLocalizedString() + " " + Localization.Failure.ToLower())));
				string msg = ((!(ParentVessel.DockedToVessel == null) || ParentVessel.DockedVessels.Count != 0) ? string.Format(Localization.RepairPointMessageVesselRoom, text.ToUpper(), ParentVessel.VesselData.VesselRegistration, (!External) ? Room.RoomName.ToUpper() : Localization.Hull.ToUpper()) : string.Format(Localization.RepairPointMessageRoom, text.ToUpper(), (!External) ? Room.RoomName.ToUpper() : Localization.Hull.ToUpper()));
				Client.Instance.CanvasManager.CanvasUI.Notification(msg, CanvasUI.NotificationType.Alert);
			}
		}
	}
}
