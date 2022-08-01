using ZeroGravity.Data;

namespace ZeroGravity.LevelDesign
{
	public class SceneResourcesAutoTransferPoint : BaseSceneAttachPoint
	{
		public override BaseAttachPointData GetData()
		{
			ResourcesAutoTransferPointData data = new ResourcesAutoTransferPointData();
			FillBaseAPData(ref data);
			return data;
		}
	}
}
