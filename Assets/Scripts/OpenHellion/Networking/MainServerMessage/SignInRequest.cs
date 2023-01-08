using ZeroGravity;

namespace OpenHellion.Networking.MainServerMessage
{
	public class SignInRequest : NetworkMessage
	{
		public string playerId;
		public string version;
		public uint hash;
		public string joiningId;

		public override string GetDestination()
		{
			return "signin";
		}

	}
}
