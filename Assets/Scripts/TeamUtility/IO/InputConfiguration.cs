using System;
using System.Collections.Generic;

namespace TeamUtility.IO
{
	[Serializable]
	public sealed class InputConfiguration
	{
		public string name;

		public List<AxisConfiguration> axes;

		public bool isExpanded;

		public InputConfiguration()
			: this("New Configuration")
		{
		}

		public InputConfiguration(string name)
		{
			axes = new List<AxisConfiguration>();
			this.name = name;
			isExpanded = false;
		}

		public static InputConfiguration Duplicate(InputConfiguration source)
		{
			InputConfiguration inputConfiguration = new InputConfiguration();
			inputConfiguration.name = source.name;
			inputConfiguration.axes = new List<AxisConfiguration>(source.axes.Count);
			for (int i = 0; i < source.axes.Count; i++)
			{
				inputConfiguration.axes.Add(AxisConfiguration.Duplicate(source.axes[i]));
			}
			return inputConfiguration;
		}
	}
}
