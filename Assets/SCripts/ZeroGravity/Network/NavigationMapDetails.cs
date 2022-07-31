using System;
using System.Collections.Generic;
using ProtoBuf;

namespace ZeroGravity.Network
{
	[Serializable]
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class NavigationMapDetails
	{
		public List<UnknownMapObjectDetails> Unknown;
	}
}
