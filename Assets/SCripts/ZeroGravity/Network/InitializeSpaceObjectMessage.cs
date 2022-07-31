using System.Collections.Generic;
using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class InitializeSpaceObjectMessage : NetworkData
	{
		public long GUID;

		public List<DynamicObjectDetails> DynamicObjects;

		public List<CorpseDetails> Corpses;

		public List<CharacterDetails> Characters;

		public InitializeSpaceObjectAuxData AuxData;
	}
}
