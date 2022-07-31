using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class NavigationMapDetailsMessage : NetworkData
	{
		public NavigationMapDetails NavMapDetails;
	}
}
