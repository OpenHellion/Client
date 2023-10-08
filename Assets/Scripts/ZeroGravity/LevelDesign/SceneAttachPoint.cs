using ZeroGravity.Data;

namespace ZeroGravity.LevelDesign
{
	public class SceneAttachPoint : BaseSceneAttachPoint
	{
		public override string InteractionTip
		{
			get
			{
				if (StandardTip != 0)
				{
					return Localization.SlotFor + ": " + StandardTip.ToLocalizedString();
				}

				return base.InteractionTip;
			}
		}

		public override BaseAttachPointData GetData()
		{
			AttachPointData data = new AttachPointData();
			FillBaseAPData(ref data);
			return data;
		}

		public void PickUpItem()
		{
			Item.RequestPickUp();
		}
	}
}
