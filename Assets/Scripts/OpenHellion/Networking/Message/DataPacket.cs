using ZeroGravity;

namespace OpenHellion.Networking.Message
{
	public class DataPacket
	{
		public override string ToString()
		{
			return Json.Serialize(this, Json.Formatting.None);
		}
	}
}
