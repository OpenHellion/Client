using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class RcsThrustStats
	{
		public float[] MoveTrust;

		public float[] RotationTrust;
	}
}
