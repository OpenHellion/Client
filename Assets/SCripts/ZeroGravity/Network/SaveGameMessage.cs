using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class SaveGameMessage : NetworkData
	{
		public string FileName;

		public SaveFileAuxData AuxData;
	}
}
