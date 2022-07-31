using System.Collections.Generic;
using ZeroGravity.Data;

namespace ZeroGravity
{
	public static class StaticData
	{
		private static Dictionary<short, DynamicObjectData> _DynamicObjectsDataList;

		public static Dictionary<short, DynamicObjectData> DynamicObjectsDataList
		{
			get
			{
				if (_DynamicObjectsDataList == null)
				{
					LoadData();
				}
				return _DynamicObjectsDataList;
			}
		}

		public static void LoadData()
		{
			List<DynamicObjectData> list = Json.LoadResource<List<DynamicObjectData>>("Data/DynamicObjects");
			_DynamicObjectsDataList = new Dictionary<short, DynamicObjectData>();
			foreach (DynamicObjectData item in list)
			{
				_DynamicObjectsDataList.Add(item.ItemID, item);
			}
		}
	}
}
