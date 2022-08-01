using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class CharacterTransformData
	{
		public float[] LocalPosition;

		public float[] LocalRotation;

		public float[] LocalVelocity;

		public float Timestamp;

		public float FreeLookX;

		public float FreeLookY;

		public float MouseLook;

		public float[] PlatformRelativePos;
	}
}
