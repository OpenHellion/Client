using System.Collections.Generic;

namespace ZeroGravity.Data
{
	public class StationBlueprint
	{
		public class Structure
		{
			public int StructureID;

			public string StructureType;

			public List<DockingPort> DockingPorts;

			public bool? SystemsOnline;

			public bool? Invulnerable;

			public bool? DoorsLocked;

			public string Tag;

			public float? HealthMultiplier;

			public bool? DockingControlsDisabled;

			public GameScenes.SceneID SceneID
			{
				get
				{
					return NameGenerator.GetSceneID(StructureType);
				}
			}
		}

		public class DockingPort
		{
			public string PortName;

			public int OrderID;

			public int? DockedStructureID;

			public string DockedPortName;

			public bool Locked;
		}

		private static string configDir;

		public string Version;

		public string Name;

		public string LinkURI;

		public List<Structure> Structures;

		public double[] LocalPosition;

		public double[] LocalRotation;

		public bool Invulnerable = true;

		public bool? SystemsOnline;

		public bool? DoorsLocked;

		public float? HealthMultiplier;

		public bool? DockingControlsDisabled;
	}
}
