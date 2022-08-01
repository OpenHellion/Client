using System.Collections.Generic;
using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class PlayerRespawnResponse : NetworkData
	{
		public long GUID;

		public int SpawnPointID;

		public List<CorpseDetails> Corpses;
	}
}
