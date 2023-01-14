using System.Collections;
using OpenHellion.Networking;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;
using ZeroGravity.UI;

namespace ZeroGravity.LevelDesign
{
	public class SceneItemRecycler : BaseSceneAttachPoint, IVesselSystemAccessory
	{
		public bool AutoRecycle;

		public RecycleMode RecycleMode;

		public float RecycleDelay;

		public HurtTrigger HurtTrigger;

		public RecyclerUI RecyclerUI;

		private bool recycle;

		public ParticleSystem RecycleParticles;

		public Animator RecycleLight;

		public Animator IdleAnimation;

		public SoundEffect IdleSound;

		public SoundEffect RecycleSound;

		private bool _Idle;

		[SerializeField]
		private VesselSystem _BaseVesselSystem;

		public bool Idle
		{
			get
			{
				return _Idle;
			}
			set
			{
				_Idle = value;
				UpdateStates();
			}
		}

		public VesselSystem BaseVesselSystem
		{
			get
			{
				return _BaseVesselSystem;
			}
			set
			{
				_BaseVesselSystem = value;
			}
		}

		public override BaseAttachPointData GetData()
		{
			ItemRecyclerAtachPointData data = new ItemRecyclerAtachPointData();
			FillBaseAPData(ref data);
			return data;
		}

		protected override void Start()
		{
			base.Start();
			if (BaseVesselSystem == null)
			{
				BaseVesselSystem = ParentVessel.VesselBaseSystem;
			}
			if (BaseVesselSystem != null)
			{
				BaseVesselSystem.Accessories.Add(this);
				BaseVesselSystemUpdated();
			}
		}

		public override bool CanAttachItemType(ItemType itemType, GenericItemSubType? generic = null, MachineryPartType? part = null, int? partTier = null)
		{
			if (attachableTypesList.Count == 0)
			{
				return true;
			}
			return base.CanAttachItemType(itemType, generic, part, partTier);
		}

		private IEnumerator DoRecycle()
		{
			yield return new WaitForSeconds(RecycleDelay);
			if (BaseVesselSystem.Status == SystemStatus.OnLine && !(Item == null))
			{
				NetworkController.Instance.SendToGameServer(new RecycleItemMessage
				{
					ID = new VesselObjectID(ParentVessel.GUID, base.InSceneID),
					GUID = Item.GUID,
					RecycleMode = RecycleMode
				});

				if (RecycleSound != null)
				{
					RecycleSound.Play();
				}
				if (RecycleParticles != null)
				{
					RecycleParticles.Play();
				}
				if (RecycleLight != null)
				{
					RecycleLight.SetTrigger("Recycle");
				}
			}
		}

		public void Recycle()
		{
			if (Executer == null || (Executer != null && Executer.IsMyPlayerTriggered))
			{
				recycle = true;
				if (BaseVesselSystem.Status == SystemStatus.OnLine)
				{
					StartCoroutine(DoRecycle());
				}
			}
		}

		public void BaseVesselSystemUpdated()
		{
			if (BaseVesselSystem.Status == SystemStatus.OnLine)
			{
				if (recycle)
				{
					StartCoroutine(DoRecycle());
				}
				if (AutoRecycle)
				{
					if (Item != null)
					{
						Shred(Item);
					}
					Collider.enabled = true;
				}
			}
			else if (AutoRecycle)
			{
				Collider.enabled = false;
			}
			UpdateStates();
			RecyclerUI.UpdateUI();
		}

		protected override void OnTriggerEnter(Collider other)
		{
			base.OnTriggerEnter(other);
			if (AutoRecycle)
			{
				DynamicObject component = other.GetComponent<DynamicObject>();
				if (component != null && component.Item != null)
				{
					Shred(component.Item);
				}
			}
		}

		private void Shred(Item item)
		{
			if (RecycleSound != null)
			{
				RecycleSound.Play();
			}
			RecyclerUI.ShowResults(item);
			NetworkController.Instance.SendToGameServer(new RecycleItemMessage
			{
				ID = new VesselObjectID(ParentVessel.GUID, base.InSceneID),
				GUID = item.GUID
			});

			if (RecycleParticles != null)
			{
				RecycleParticles.Play();
			}
		}

		protected override void OnAttach()
		{
			base.OnAttach();
			if (AutoRecycle && Item != null)
			{
				Shred(Item);
			}
			if (RecyclerUI != null && !AutoRecycle)
			{
				RecyclerUI.UpdateUI();
			}
		}

		protected override void OnDetach()
		{
			base.OnDetach();
			if (RecyclerUI != null && !AutoRecycle)
			{
				RecyclerUI.UpdateUI();
			}
		}

		public void UpdateStates()
		{
			if (BaseVesselSystem == null)
			{
				return;
			}
			if (BaseVesselSystem.Status == SystemStatus.OnLine && Idle)
			{
				if (IdleSound != null && !IdleSound.IsPlaying)
				{
					IdleSound.Play();
				}
				if (HurtTrigger != null)
				{
					HurtTrigger.gameObject.SetActive(true);
				}
				if (IdleAnimation != null)
				{
					IdleAnimation.enabled = true;
				}
			}
			else if (BaseVesselSystem.Status != SystemStatus.OnLine || !Idle)
			{
				if (IdleSound != null && IdleSound.IsPlaying)
				{
					IdleSound.Play(1);
				}
				if (HurtTrigger != null)
				{
					HurtTrigger.gameObject.SetActive(false);
				}
				if (IdleAnimation != null)
				{
					IdleAnimation.enabled = false;
				}
			}
		}
	}
}
