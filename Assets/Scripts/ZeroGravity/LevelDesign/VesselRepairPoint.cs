using System;
using System.Collections.Generic;
using System.Linq;
using OpenHellion.UI;
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
		[SerializeField] private int _InSceneID;

		public List<RepairPointParticleEffect> DamageEffects;

		public List<RepairPointMeshEffect> DamageMeshes;

		public List<RepairPointLight> LightEffects;

		[NonSerialized] public SpaceObjectVessel ParentVessel;

		public SceneTriggerRoom Room;

		[Space(10f)] public RepairPointDamageType DamageType;

		[Tooltip("Used only if Damage Type is 'System'")]
		public VesselComponent AffectedSystem;

		public bool External;

		[Range(0f, 1f)] public float MalfunctionThreshold;

		[Range(0f, 1f)] public float RepairThreshold;

		[Space(10f)] [Range(0f, 1f)] public float PrimaryDamageVisualThreshold;

		public GameObject PrimaryDamageVisual;

		[Space(10f)] public List<RepairPointParticleEffect> SecondaryDamageEffects;

		public List<RepairPointMeshEffect> SecondaryDamageMesh;

		public GameObject SecondaryDamageObject;

		[Space(10f)] [SerializeField] private float _MaxHealth;

		[SerializeField] private float _Health;

		private bool _firstStrike;

		private bool _secondaryDamageActive;

		private bool _prevSecondaryDamageActive;

		public bool SecondaryDamageActive
		{
			get
			{
				if (DamageType == RepairPointDamageType.Fire)
				{
					return _secondaryDamageActive && Room.AirQuality * Room.AirPressure >= 0.25f;
				}

				if (DamageType == RepairPointDamageType.Breach)
				{
					return _secondaryDamageActive && Room.AirPressure > float.Epsilon;
				}

				return _secondaryDamageActive;
			}
			set
			{
				_secondaryDamageActive = value;
				UpdateEffects();
			}
		}

		public float MaxHealth
		{
			get => _MaxHealth;
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
			get => _Health;
			set
			{
				if (_Health - value > _MaxHealth * 0.1f)
				{
					_firstStrike = true;
				}

				if (_Health != (_Health = value))
				{
					UpdateEffects();
				}
			}
		}

		public int InSceneID
		{
			get => _InSceneID;
			set => _InSceneID = value;
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
				if (component != null && item.gameObject.activeInHierarchy && !DamageEffects.Contains(component) &&
				    !SecondaryDamageEffects.Contains(component))
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

			ParentVessel = GetComponentInParent<GeometryRoot>().MainObject as SpaceObjectVessel;
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

			foreach (RepairPointParticleEffect item in SecondaryDamageEffects.Where((RepairPointParticleEffect m) =>
				         m.Effect != null))
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
				foreach (RepairPointParticleEffect particleEffect in DamageEffects.Where(
					         (RepairPointParticleEffect m) => m.Effect != null))
				{
					particleEffect.Stop();
				}

				foreach (RepairPointMeshEffect meshEffect in DamageMeshes.Where((RepairPointMeshEffect m) =>
					         m.Mesh != null))
				{
					meshEffect.Mesh.material.SetFloat("_Health", 1f);
				}

				{
					foreach (RepairPointLight lightEffect in LightEffects)
					{
						lightEffect.Light.intensity = 0f;
					}

					return;
				}
			}

			foreach (RepairPointParticleEffect particleEffect in DamageEffects.Where((RepairPointParticleEffect m) =>
				         m.Effect != null))
			{
				if (Room == null || !Room.UseGravity || Room.GravityForce == Vector3.zero)
				{
					particleEffect.SetGravity(gravity: false);
				}
				else
				{
					particleEffect.SetGravity(gravity: true);
				}

				if (particleEffect.PlayOnce)
				{
					if (_firstStrike)
					{
						particleEffect.SetIntensity(Damage);
						_firstStrike = false;
					}
				}
				else
				{
					particleEffect.SetIntensity(Damage);
				}
			}

			foreach (RepairPointMeshEffect meshEffect in
			         DamageMeshes.Where((RepairPointMeshEffect m) => m.Mesh != null))
			{
				meshEffect.Mesh.material.SetFloat("_Health", meshEffect.Curve.Evaluate(1f - Damage));
				if (!SecondaryDamageMesh.Contains(meshEffect))
				{
					meshEffect.Mesh.material.SetFloat("_SystemDamage", 0f);
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
					lightEffect.Light.intensity =
						lightEffect.IntensityCurve.Evaluate(Time.time % lightEffect.JitterLength /
						                                    lightEffect.JitterLength) * lightEffect.JitterIntensity *
						lightEffect.Curve.Evaluate(Damage);
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

				if (Room == null && (DamageType == RepairPointDamageType.Breach ||
				                     DamageType == RepairPointDamageType.Fire ||
				                     DamageType == RepairPointDamageType.Gravity))
				{
					throw new Exception("Room not set.");
				}
			}

			VesselRepairPointData vesselRepairPointData = new VesselRepairPointData
			{
				InSceneID = InSceneID,
				RoomID = Room != null ? Room.InSceneID : -1,
				DamageType = DamageType,
				AffectedSystemID = (AffectedSystem == null || DamageType != RepairPointDamageType.System)
					? -1
					: AffectedSystem.InSceneID,
				MalfunctionThreshold = MalfunctionThreshold,
				RepairThreshold = RepairThreshold,
				External = External
			};
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
				Gizmos.DrawIcon(transform.position, "RepairRegular", false);
			}

			if (DamageType == RepairPointDamageType.Breach)
			{
				Gizmos.DrawIcon(transform.position, "RepairBreach", false);
			}

			if (DamageType == RepairPointDamageType.Fire)
			{
				Gizmos.DrawIcon(transform.position, "RepairFire", false);
			}

			if (DamageType == RepairPointDamageType.System)
			{
				Gizmos.DrawIcon(transform.position, "RepairSystem", false);
			}
		}

		public void SetDetails(VesselRepairPointDetails repairPointDetails)
		{
			Health = repairPointDetails.Health;
			MaxHealth = repairPointDetails.MaxHealth;
			_prevSecondaryDamageActive = SecondaryDamageActive;
			SecondaryDamageActive = repairPointDetails.SecondaryDamageActive;
			bool isSecondaryDamageUnchanged = !_prevSecondaryDamageActive && SecondaryDamageActive;
			UpdateEffects();
			if (isSecondaryDamageUnchanged && MyPlayer.Instance.Parent is SpaceObjectVessel &&
			    MyPlayer.Instance.IsInVesselHierarchy(ParentVessel))
			{
				// Get affected system/ damage type as text.
				string damageText;
				if (DamageType is not RepairPointDamageType.System)
				{
					damageText = DamageType.ToLocalizedString();
				}
				else if (AffectedSystem is not Generator)
				{
					damageText = (AffectedSystem as SubSystem).Type.ToLocalizedString() + " " +
					             Localization.Failure.ToLower();
				}
				else
				{
					damageText = (AffectedSystem as Generator).Type.ToLocalizedString() + " " +
					             Localization.Failure.ToLower();
				}

				// Use the damage text and get a full alert message.
				string fullMessage;
				if (ParentVessel.DockedToVessel != null || ParentVessel.DockedVessels.Count is not 0)
				{
					fullMessage = string.Format(Localization.RepairPointMessageVesselRoom, damageText.ToUpper(),
						ParentVessel.VesselData.VesselRegistration,
						!External ? Room.RoomName.ToUpper() : Localization.Hull.ToUpper());
				}
				else
				{
					fullMessage = string.Format(Localization.RepairPointMessageRoom, damageText.ToUpper(),
						!External ? Room.RoomName.ToUpper() : Localization.Hull.ToUpper());
				}

				Debug.Log("Supposed to send notification: " + fullMessage);
				//_world.InGameGUI.Notification(fullMessage, InGameGUI.NotificationType.Alert);
			}
		}
	}
}
