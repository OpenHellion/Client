using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class RealtimeData
	{
		public long ParentGUID;

		public double[] Position;

		public double[] Velocity;
	}
}
