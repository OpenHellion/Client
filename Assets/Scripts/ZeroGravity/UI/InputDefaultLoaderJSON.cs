using System;
using System.Collections.Generic;
using Luminosity.IO;

namespace ZeroGravity.UI
{
	public class InputDefaultLoaderJSON : IInputLoader
	{
		SaveData IInputLoader.Load()
		{
			List<ControlScheme> list = Json.LoadResource<List<ControlScheme>>("Data/ControlsDefault");
			SaveData saveLoadParameters = new SaveData
			{
				ControlSchemes = list
			};
			return saveLoadParameters;
		}

		public ControlScheme Load(string schemeName)
		{
			throw new NotImplementedException();
		}
	}
}
