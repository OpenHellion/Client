using System;
using System.Collections.Generic;
using TeamUtility.IO;

namespace ZeroGravity.UI
{
	public class InputDefaultLoaderJSON : IInputLoader
	{
		public SaveLoadParameters Load()
		{
			List<InputConfiguration> list = new List<InputConfiguration>();
			list = Json.LoadResource<List<InputConfiguration>>("Data/ControlsDefault");
			SaveLoadParameters saveLoadParameters = new SaveLoadParameters();
			saveLoadParameters.inputConfigurations = list;
			return saveLoadParameters;
		}

		public InputConfiguration LoadSelective(string inputConfigName)
		{
			throw new NotImplementedException();
		}
	}
}
