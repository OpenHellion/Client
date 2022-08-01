using System;

namespace ZeroGravity.Data
{
	[Serializable]
	public class SystemSpawnSettings : ISceneData
	{
		public SpawnSettingsCase Case;

		public string Tag;

		public float ResourceRequirementMultiplier = 1f;
	}
}
