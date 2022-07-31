using ProtoBuf;
using ZeroGravity.Data;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class VesselSecurityAuthorizedPerson
	{
		public AuthorizedPersonRank Rank;

		public long GUID;

		public string SteamID;

		public string Name;
	}
}
