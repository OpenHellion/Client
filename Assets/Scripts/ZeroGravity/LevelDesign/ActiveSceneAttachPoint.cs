using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.LevelDesign
{
	public class ActiveSceneAttachPoint : SceneAttachPoint, IVesselSystemAccessory
	{
		[SerializeField] private VesselSystem _BaseVesselSystem;

		public override Item Item
		{
			get => base.Item;
			protected set
			{
				if (Item != value && Item != null && Item is IVesselSystemAccessory && BaseVesselSystem != null)
				{
					BaseVesselSystem.Accessories.Remove(Item as IVesselSystemAccessory);
				}

				if (value != null && value is IVesselSystemAccessory && BaseVesselSystem != null)
				{
					BaseVesselSystem.Accessories.Add(value as IVesselSystemAccessory);
					(value as IVesselSystemAccessory).BaseVesselSystemUpdated();
				}

				base.Item = value;
			}
		}

		public VesselSystem BaseVesselSystem
		{
			get => _BaseVesselSystem;
			set => _BaseVesselSystem = value;
		}

		protected override void Start()
		{
			base.Start();
			if (BaseVesselSystem == null)
			{
				BaseVesselSystem = ParentVessel.VesselBaseSystem;
			}
		}

		public void BaseVesselSystemUpdated()
		{
			if (Item != null && Item is IVesselSystemAccessory)
			{
				(Item as IVesselSystemAccessory).BaseVesselSystemUpdated();
			}
		}

		public override BaseAttachPointData GetData()
		{
			ActiveAttachPointData data = new ActiveAttachPointData();
			FillBaseAPData(ref data);
			return data;
		}
	}
}
