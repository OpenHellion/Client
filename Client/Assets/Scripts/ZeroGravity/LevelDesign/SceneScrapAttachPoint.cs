using ZeroGravity.Data;

namespace ZeroGravity.LevelDesign
{
	public class SceneScrapAttachPoint : SceneAttachPoint
	{
		public override BaseAttachPointData GetData()
		{
			ScrapAttachPointData data = new ScrapAttachPointData();
			FillBaseAPData(ref data);
			return data;
		}
	}
}
