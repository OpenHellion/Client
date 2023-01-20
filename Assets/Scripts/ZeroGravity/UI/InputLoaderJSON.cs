using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Luminosity.IO;
using UnityEngine;
using ZeroGravity.Data;

namespace ZeroGravity.UI
{
	public class InputLoaderJSON : IInputLoader
	{
		private SettingsData SettingsData = new SettingsData();

		SaveData IInputLoader.Load()
		{
			List<ControlScheme> defaultControls = Json.LoadResource<SaveData>("Data/ControlsDefault").ControlSchemes;
			List<ControlScheme> settingsControls;
			if (File.Exists(Path.Combine(Application.persistentDataPath, "Settings.json")))
			{
				SettingsData = Json.LoadPersistent<SettingsData>("Settings.json");
				settingsControls = (SettingsData == null) ? defaultControls : SettingsData.ControlsSettings.InputConfigurations.ControlSchemes;
			}
			else
			{
				settingsControls = defaultControls;
			}
			if (settingsControls.Count > 0)
			{
				List<string> list3 = new List<string>();
				List<string> list4 = new List<string>();
				for (int i = 0; i < settingsControls[0].Actions.Count; i++)
				{
					list3.Add(settingsControls[0].Actions[i].Name);
				}
				for (int j = 0; j < defaultControls[0].Actions.Count; j++)
				{
					list4.Add(defaultControls[0].Actions[j].Name);
				}
				List<string> list5 = list4.Except(list3).ToList();
				List<string> list6 = list3.Except(list4).ToList();
				if (list6.Count > 0)
				{
					for (int k = 0; k < list6.Count; k++)
					{
						for (int l = 0; l < settingsControls[0].Actions.Count; l++)
						{
							if (settingsControls[0].Actions[l].Name == list6[k])
							{
								settingsControls[0].DeleteAction(settingsControls[0].Actions[l]);
							}
						}
					}
				}
				if (list5.Count > 0)
				{
					foreach (string item in list5)
					{
						for (int m = 0; m < defaultControls[0].Actions.Count; m++)
						{
							if (defaultControls[0].Actions[m].Name == item)
							{
								InputAction action = settingsControls[0].CreateNewAction(defaultControls[0].Actions[m].Name);
								foreach (InputBinding binding in defaultControls[0].Actions[m].Bindings)
								{
									action.CreateNewBinding(binding);
								}
							}
						}
					}
				}
			}
			SaveData saveLoadParameters = new SaveData
			{
				PlayerOneScheme = "KeyboardAndMouse",
				ControlSchemes = settingsControls
			};
			return saveLoadParameters;
		}

		public ControlScheme Load(string schemeName)
		{
			throw new NotImplementedException();
		}
	}
}
