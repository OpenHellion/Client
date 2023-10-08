using System;
using UnityEngine;

namespace ThreeEyedGames
{
	[ExecuteInEditMode]
	[AddComponentMenu("Decalicious/Decal Limit Group Manager")]
	public class DecalLimitGroupManager : MonoBehaviour
	{
		public static DecalLimitGroupManager Instance;

		public const int NumLimitGroups = 32;

		public DecalLimitGroup[] LimitGroups = new DecalLimitGroup[32];

		private void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
				return;
			}

			Debug.LogError("There cannot be more than one instance of DecalLimitGroupManager. Destroying.");
			UnityEngine.Object.DestroyImmediate(this);
		}

		private void OnEnable()
		{
			if (LimitGroups.Length != 32)
			{
				Array.Resize(ref LimitGroups, 32);
			}
		}
	}
}
