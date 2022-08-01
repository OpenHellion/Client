using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using ZeroGravity.Network;

namespace ZeroGravity.LevelDesign
{
	public class SpawnObjectsWithChanceScene : MonoBehaviour, ISceneObject
	{
		public List<GameObjectChancePair> ListToPickFrom = new List<GameObjectChancePair>();

		[SerializeField]
		private int _inSceneID;

		[CompilerGenerated]
		private static Func<GameObjectChancePair, float> _003C_003Ef__am_0024cache0;

		public int InSceneID
		{
			get
			{
				return _inSceneID;
			}
			set
			{
				_inSceneID = value;
			}
		}

		private void Awake()
		{
			List<GameObjectChancePair> listToPickFrom = ListToPickFrom;
			if (_003C_003Ef__am_0024cache0 == null)
			{
				_003C_003Ef__am_0024cache0 = _003CAwake_003Em__0;
			}
			listToPickFrom.OrderBy(_003C_003Ef__am_0024cache0);
		}

		private int ReturnIndexBasedOnChance(float chance)
		{
			for (int i = 0; i < ListToPickFrom.Count; i++)
			{
				if (chance < ListToPickFrom[i].Chance)
				{
					return i;
				}
			}
			return -1;
		}

		public void SetDetails(SpawnObjectsWithChanceDetails details)
		{
			int num = ReturnIndexBasedOnChance(details.Chance);
			if (num != -1 && details.InSceneID == InSceneID)
			{
				for (int i = 0; i < ListToPickFrom.Count; i++)
				{
					ListToPickFrom[i].Object.SetActive(i == num);
				}
			}
		}

		[CompilerGenerated]
		private static float _003CAwake_003Em__0(GameObjectChancePair m)
		{
			return m.Chance;
		}
	}
}
