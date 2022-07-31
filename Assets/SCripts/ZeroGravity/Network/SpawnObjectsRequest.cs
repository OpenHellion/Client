using System.Collections.Generic;
using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class SpawnObjectsRequest : NetworkData
	{
		public List<long> GUIDs;
	}
}
