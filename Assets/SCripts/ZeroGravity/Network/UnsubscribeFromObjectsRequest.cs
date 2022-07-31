using System.Collections.Generic;
using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class UnsubscribeFromObjectsRequest : NetworkData
	{
		public List<long> GUIDs;
	}
}
