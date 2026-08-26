using System.Collections.Generic;
using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class DynamicObjectsInfoMessage : NetworkData
	{
		public List<DynamicObjectInfo> Infos;
	}
}
