using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public class DynamicSceneObject : MonoBehaviour
	{
		[HideInInspector] public GameObject PrefabObject;

		[HideInInspector] public MachineryPartType MachineryPartType;

		public DynaminObjectSpawnSettings[] SpawnSettings;

		public ItemType ItemType
		{
			get
			{
				Item componentInChildren = GetComponentInChildren<Item>();
				return (componentInChildren != null) ? componentInChildren.Type : ItemType.None;
			}
		}

		public Item Item => GetComponentInChildren<Item>();

		private void Awake()
		{
			if (Application.isPlaying && Application.isEditor)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}
}
