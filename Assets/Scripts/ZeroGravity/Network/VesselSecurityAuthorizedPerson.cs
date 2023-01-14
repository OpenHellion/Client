using ProtoBuf;
using ZeroGravity.Data;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class VesselSecurityAuthorizedPerson
	{
		public AuthorizedPersonRank Rank;

		public long GUID;

		public string PlayerNativeId;

		public string PlayerId;

		public string Name;
	}
}
