using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ThreeEyedGames;
using UnityEngine;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public class VesselHealthDecal : MonoBehaviour
	{
		[NonSerialized]
		public SpaceObjectVessel ParentVessel;

		public List<Decalicious> Decals = new List<Decalicious>();

		private void Awake()
		{
			Decals.AddRange(GetComponentsInChildren<Decalicious>().ToList().Where(_003CAwake_003Em__0));
		}

		public void UpdateDecals()
		{
			if (ParentVessel == null)
			{
				ParentVessel = GetComponentInParent<SpaceObjectVessel>();
				if (ParentVessel == null)
				{
					return;
				}
			}
			float num = 1f - ParentVessel.Health / ParentVessel.MaxHealth;
			foreach (Decalicious decal in Decals)
			{
				decal.Fade = num;
				decal.gameObject.Activate(num > float.Epsilon);
			}
		}

		[CompilerGenerated]
		private bool _003CAwake_003Em__0(Decalicious m)
		{
			return !Decals.Contains(m);
		}
	}
}
