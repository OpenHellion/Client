using Luminosity.IO;
using OpenHellion.Util;

namespace ZeroGravity.UI
{
	public class InputDefaultSaverJSON : IInputSaver
	{
		public void Save(SaveData parameters)
		{
			JsonSerialiser.SerializeDataPath(parameters.ControlSchemes, "Resources/Data/ControlsDefault.json");
		}
	}
}
