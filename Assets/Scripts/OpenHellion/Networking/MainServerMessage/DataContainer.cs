using System;

namespace OpenHellion.Networking.MainServerMessage
{
	public abstract class DataContainer
	{
		public override string ToString()
		{
			return Json.Serialize(this, Json.Formatting.None);
		}
	}
}
