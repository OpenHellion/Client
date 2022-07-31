using System;
using System.Collections.Generic;
using TeamUtility.IO;

namespace ZeroGravity.UI
{
	public class InputDefaultSaverJSON : IInputSaver
	{
		public void Save(SaveLoadParameters parameters)
		{
			throw new NotImplementedException();
		}

		public void Save(List<InputConfiguration> inputConfigurations, string defaultConfiguration)
		{
			Json.SerializeDataPath(inputConfigurations, "Resources/Data/ControlsDefault.json");
		}
	}
}
