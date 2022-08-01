using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

		[CompilerGenerated]
		private static Func<RepairPointParticleEffect, bool> _003C_003Ef__am_0024cache0;

		[CompilerGenerated]
		private static Func<RepairPointMeshEffect, bool> _003C_003Ef__am_0024cache1;

		[CompilerGenerated]
		private static Func<RepairPointParticleEffect, bool> _003C_003Ef__am_0024cache2;

		[CompilerGenerated]
		private static Func<RepairPointMeshEffect, bool> _003C_003Ef__am_0024cache3;

		[CompilerGenerated]
		private static Func<RepairPointParticleEffect, bool> _003C_003Ef__am_0024cache4;

		[CompilerGenerated]
		private static Func<RepairPointMeshEffect, bool> _003C_003Ef__am_0024cache5;

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

		public float Damage
		{
			get
			{
				return 1f - MathHelper.Clamp(Health / (MaxHealth * 0.98f), 0f, 1f);
			}
		}

		private void OnEnable()
		{
			UpdateEffects();
		}

		private void Awake()
		{
			IEnumerator enumerator = base.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Transform transform = (Transform)enumerator.Current;
					RepairPointParticleEffect component = transform.gameObject.GetComponent<RepairPointParticleEffect>();
					if (component != null && transform.gameObject.activeInHierarchy && !DamageEffects.Contains(component) && !SecondaryDamageEffects.Contains(component))
					{
						DamageEffects.Add(transform.gameObject.GetComponent<RepairPointParticleEffect>());
					}
					RepairPointMeshEffect component2 = transform.gameObject.GetComponent<RepairPointMeshEffect>();
					if (component2 != null && transform.gameObject.activeInHierarchy && !DamageMeshes.Contains(component2))
					{
						DamageMeshes.Add(transform.gameObject.GetComponent<RepairPointMeshEffect>());
					}
					RepairPointLight component3 = transform.gameObject.GetComponent<RepairPointLight>();
					if (component3 != null && transform.gameObject.activeInHierarchy && !LightEffects.Contains(component3))
					{
						LightEffects.Add(transform.gameObject.GetComponent<RepairPointLight>());
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
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
			List<RepairPointParticleEffect> secondaryDamageEffects = SecondaryDamageEffects;
			if (_003C_003Ef__am_0024cache0 == null)
			{
				_003C_003Ef__am_0024cache0 = _003CUpdateEffects_003Em__0;
			}
			foreach (RepairPointParticleEffect item in secondaryDamageEffects.Where(_003C_003Ef__am_0024cache0))
			{
				if (SecondaryDamageActive)
				{
					if (Room == null || !Room.UseGravity || Room.GravityForce == Vector3.zero)
					{
						item.SetGravity(false);
					}
					else
					{
						item.SetGravity(true);
					}
					item.SetIntensity(Damage);
				}
				else
				{
					item.Stop();
				}
			}
			List<RepairPointMeshEffect> damageMeshes = DamageMeshes;
			if (_003C_003Ef__am_0024cache1 == null)
			{
				_003C_003Ef__am_0024cache1 = _003CUpdateEffects_003Em__1;
			}
			foreach (RepairPointMeshEffect item2 in damageMeshes.Where(_003C_003Ef__am_0024cache1))
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
				List<RepairPointParticleEffect> damageEffects = DamageEffects;
				if (_003C_003Ef__am_0024cache2 == null)
				{
					_003C_003Ef__am_0024cache2 = _003CUpdateEffects_003Em__2;
				}
				foreach (RepairPointParticleEffect item3 in damageEffects.Where(_003C_003Ef__am_0024cache2))
				{
					item3.Stop();
				}
				List<RepairPointMeshEffect> damageMeshes2 = DamageMeshes;
				if (_003C_003Ef__am_0024cache3 == null)
				{
					_003C_003Ef__am_0024cache3 = _003CUpdateEffects_003Em__3;
				}
				foreach (RepairPointMeshEffect item4 in damageMeshes2.Where(_003C_003Ef__am_0024cache3))
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
			List<RepairPointParticleEffect> damageEffects2 = DamageEffects;
			if (_003C_003Ef__am_0024cache4 == null)
			{
				_003C_003Ef__am_0024cache4 = _003CUpdateEffects_003Em__4;
			}
			foreach (RepairPointParticleEffect item5 in damageEffects2.Where(_003C_003Ef__am_0024cache4))
			{
				if (Room == null || !Room.UseGravity || Room.GravityForce == Vector3.zero)
				{
					item5.SetGravity(false);
				}
				else
				{
					item5.SetGravity(true);
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
			List<RepairPointMeshEffect> damageMeshes3 = DamageMeshes;
			if (_003C_003Ef__am_0024cache5 == null)
			{
				_003C_003Ef__am_0024cache5 = _003CUpdateEffects_003Em__5;
			}
			foreach (RepairPointMeshEffect item6 in damageMeshes3.Where(_003C_003Ef__am_0024cache5))
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

		[CompilerGenerated]
		private static bool _003CUpdateEffects_003Em__0(RepairPointParticleEffect m)
		{
			return m.Effect != null;
		}

		[CompilerGenerated]
		private static bool _003CUpdateEffects_003Em__1(RepairPointMeshEffect m)
		{
			return m.Mesh != null;
		}

		[CompilerGenerated]
		private static bool _003CUpdateEffects_003Em__2(RepairPointParticleEffect m)
		{
			return m.Effect != null;
		}

		[CompilerGenerated]
		private static bool _003CUpdateEffects_003Em__3(RepairPointMeshEffect m)
		{
			return m.Mesh != null;
		}

		[CompilerGenerated]
		private static bool _003CUpdateEffects_003Em__4(RepairPointParticleEffect m)
		{
			return m.Effect != null;
		}

		[CompilerGenerated]
		private static bool _003CUpdateEffects_003Em__5(RepairPointMeshEffect m)
		{
			return m.Mesh != null;
		}
	}
}
