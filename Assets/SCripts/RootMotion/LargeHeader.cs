using UnityEngine;

namespace RootMotion
{
	public class LargeHeader : PropertyAttribute
	{
		public string name;

		public string color = "white";

		public LargeHeader(string name)
		{
			this.name = name;
			color = "white";
		}

		public LargeHeader(string name, string color)
		{
			this.name = name;
			this.color = color;
		}
	}
}
