using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class DynamicObjectStatsMessage : NetworkData
	{
		public DynamicObjectInfo Info = new DynamicObjectInfo();

		public bool DestroyDynamicObject;

		public DynamicObjectAttachData AttachData;
	}
}
