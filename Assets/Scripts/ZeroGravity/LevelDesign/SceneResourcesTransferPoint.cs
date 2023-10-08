using System.Collections.Generic;
using ZeroGravity.Data;
using ZeroGravity.Objects;
using ZeroGravity.UI;

namespace ZeroGravity.LevelDesign
{
	public class SceneResourcesTransferPoint : BaseSceneAttachPoint
	{
		private CargoPanel cargoPanel;

		public override Item Item
		{
			get { return base.Item; }
			protected set { base.Item = value; }
		}

		public override string InteractionTip
		{
			get { return Localization.CargoAttachPoint; }
		}

		public override BaseAttachPointData GetData()
		{
			ResourcesTransferPointData data = new ResourcesTransferPointData();
			FillBaseAPData(ref data);
			return data;
		}

		protected override void Awake()
		{
			if (attachableTypesList == null || attachableTypesList.Count == 0)
			{
				attachableTypesList = new List<AttachPointTransformData>
				{
					new AttachPointTransformData
					{
						AttachPoint = base.transform,
						ItemType = ItemType.AltairPressurisedJetpack
					},
					new AttachPointTransformData
					{
						AttachPoint = base.transform,
						ItemType = ItemType.AltairHandDrillCanister
					}
				};
			}

			base.Awake();
		}

		public void Initialize(CargoPanel cargoUI)
		{
			cargoPanel = cargoUI;
		}
	}
}
