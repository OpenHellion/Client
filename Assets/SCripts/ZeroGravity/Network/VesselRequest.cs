using ProtoBuf;
using ZeroGravity.Data;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class VesselRequest : NetworkData
	{
		public long GUID;

		public float Time;

		public GameScenes.SceneID RescueShipSceneID;

		public string RescueShipTag;
	}
}
