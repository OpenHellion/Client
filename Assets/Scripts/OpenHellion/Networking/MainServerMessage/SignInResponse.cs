using ZeroGravity;

namespace OpenHellion.Networking.MainServerMessage
{
	public class SignInResponse : DataContainer
	{
		public ResponseResult result;
		public ServerData server;
		public string lastSignIn;
	}
}
