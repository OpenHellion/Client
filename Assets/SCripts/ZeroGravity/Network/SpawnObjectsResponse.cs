using System.Collections.Generic;
using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class SpawnObjectsResponse : NetworkData
	{
		public List<SpawnObjectResponseData> Data = new List<SpawnObjectResponseData>();
	}
}
