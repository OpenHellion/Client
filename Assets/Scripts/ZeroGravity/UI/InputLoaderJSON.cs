using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Luminosity.IO;
using OpenHellion.Util;
using UnityEngine;
using ZeroGravity.Data;

namespace ZeroGravity.UI
{
	public class InputLoaderJSON : IInputLoader
	{
		SaveData IInputLoader.Load()
		{
			Dbg.Log("Loading custom input...");

			// Get saved controls.
			List<ControlScheme> defaultControls = JsonSerialiser.LoadResource<List<ControlScheme>>("Data/ControlsDefault");
			List<ControlScheme> settingsControls;
			if (File.Exists(Path.Combine(Application.persistentDataPath, "Settings.json")))
			{
				SettingsData settingsData = JsonSerialiser.LoadPersistent<SettingsData>("Settings.json");
				settingsControls = (settingsData == null) ? defaultControls : settingsData.ControlsSettings.ControlSchemes;

				// Quality-check settings.
				if (settingsControls.Count > 0)
				{
					List<string> settingsControlsNames = new List<string>();
					List<string> defaultControlsNames = new List<string>();
					for (int i = 0; i < settingsControls[0].Actions.Count; i++)
					{
						settingsControlsNames.Add(settingsControls[0].Actions[i].Name);
					}
					for (int j = 0; j < defaultControls[0].Actions.Count; j++)
					{
						defaultControlsNames.Add(defaultControls[0].Actions[j].Name);
					}

					// Extra entries in the settings list.
					List<string> extraEntries = settingsControlsNames.Except(defaultControlsNames).ToList();
					if (extraEntries.Count > 0)
					{
						for (int k = 0; k < extraEntries.Count; k++)
						{
							for (int l = 0; l < settingsControls[0].Actions.Count; l++)
							{
								if (settingsControls[0].Actions[l].Name == extraEntries[k])
								{
									settingsControls[0].DeleteAction(settingsControls[0].Actions[l]);
								}
							}
						}
					}

					// Entries missing from the settings list.
					List<string> missingEntries = defaultControlsNames.Except(settingsControlsNames).ToList();
					if (missingEntries.Count > 0)
					{
						foreach (string item in missingEntries)
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
			}
			else
			{
				settingsControls = defaultControls;
			}

			// Apply changes.
			SaveData saveLoadParameters = new()
			{
				ControlSchemes = settingsControls,
				PlayerOneScheme = settingsControls[0].UniqueID
			};

			return saveLoadParameters;
		}

		public ControlScheme Load(string schemeName)
		{
			throw new NotImplementedException();
		}
	}
}
