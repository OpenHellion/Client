using System;
using UnityEngine;

namespace ZeroGravity.Data
{
	[Serializable]
	public class SceneSpawnSettings : ISceneData
	{
		public SpawnSettingsCase Case;

		public string Tag;

		[Tooltip("-1 - don't randomize health, \n0-1 - minimum health boundary (0-100% from Max Health HP)")]
		public float MinHealth = -1f;

		[Tooltip("-1 - don't randomize health, \n0-1 - maximum health boundary (0-100% from Max Health HP)")]
		public float MaxHealth = -1f;
	}
}
