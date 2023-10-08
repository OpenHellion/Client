using System;
using UnityEngine;

namespace ZeroGravity.Data
{
	[Serializable]
	public class DynaminObjectSpawnSettings : ISceneData
	{
		public SpawnSettingsCase Case;

		public string Tag;

		[Tooltip("-1 - don't respawn, \n0 or more - respawn delay in seconds")]
		public float RespawnTime = -1f;

		[Tooltip("-1 - spawn for sure, \n0-1 - spawn chance (0-100%)")]
		public float SpawnChance = -1f;

		[Tooltip("-1 - don't randomize health, \n0-1 - minimum health boundary (0-100% from Max Health HP)")]
		public float MinHealth = -1f;

		[Tooltip("-1 - don't randomize health, \n0-1 - maximum health boundary (0-100% from Max Health HP)")]
		public float MaxHealth = -1f;

		[Tooltip(
			"Valid for machinery parts only.\n0 - don't wear at all, \n0-1 - reduced wear rate, \n>1 - increased wear rate")]
		public float WearMultiplier = 1f;
	}
}
