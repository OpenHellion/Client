using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TeamUtility.IO;
using UnityEngine;
using ZeroGravity.Data;

namespace ZeroGravity.UI
{
	public class InputLoaderJSON : IInputLoader
	{
		private SettingsData SettingsData = new SettingsData();

		public SaveLoadParameters Load()
		{
			List<InputConfiguration> list = new List<InputConfiguration>();
			List<InputConfiguration> list2 = new List<InputConfiguration>();
			list2.Add(TeamUtility.IO.InputManager.GetInputConfiguration(PlayerID.One));
			list = Json.LoadResource<List<InputConfiguration>>("Data/ControlsDefault");
			if (File.Exists(Path.Combine(Application.persistentDataPath, "Settings.json")))
			{
				SettingsData = Json.LoadPersistent<SettingsData>("Settings.json");
				list2 = ((SettingsData == null) ? list : SettingsData.ControlsSettings.InputConfigurations);
			}
			else
			{
				list2 = list;
			}
			if (list2.Count > 0)
			{
				List<string> list3 = new List<string>();
				List<string> list4 = new List<string>();
				List<string> list5 = new List<string>();
				List<string> list6 = new List<string>();
				for (int i = 0; i < list2[0].axes.Count; i++)
				{
					list3.Add(list2[0].axes[i].name);
				}
				for (int j = 0; j < list[0].axes.Count; j++)
				{
					list4.Add(list[0].axes[j].name);
				}
				list5 = list4.Except(list3).ToList();
				list6 = list3.Except(list4).ToList();
				if (list6.Count > 0)
				{
					for (int k = 0; k < list6.Count; k++)
					{
						for (int l = 0; l < list2[0].axes.Count; l++)
						{
							if (list2[0].axes[l].name == list6[k])
							{
								list2[0].axes.Remove(list2[0].axes[l]);
							}
						}
					}
				}
				if (list5.Count > 0)
				{
					foreach (string item in list5)
					{
						for (int m = 0; m < list[0].axes.Count; m++)
						{
							if (list[0].axes[m].name == item)
							{
								list2[0].axes.Add(list[0].axes[m]);
							}
						}
					}
				}
			}
			SaveLoadParameters saveLoadParameters = new SaveLoadParameters();
			saveLoadParameters.playerOneDefault = "KeyboardAndMouse";
			saveLoadParameters.inputConfigurations = list2;
			return saveLoadParameters;
		}

		public InputConfiguration LoadSelective(string inputConfigName)
		{
			throw new NotImplementedException();
		}
	}
}
