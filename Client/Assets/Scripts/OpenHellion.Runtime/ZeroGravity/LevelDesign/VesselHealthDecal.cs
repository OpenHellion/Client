using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public class VesselHealthDecal : MonoBehaviour
	{
		[NonSerialized] public SpaceObjectVessel ParentVessel;

		public List<DecalProjector> Decals = new List<DecalProjector>();

		private void Awake()
		{
			Decals.AddRange(GetComponentsInChildren<DecalProjector>().Where((m) => !Decals.Contains(m)));
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
			foreach (DecalProjector decal in Decals)
			{
				decal.fadeFactor = num;
				decal.gameObject.Activate(num > float.Epsilon);
			}
		}
	}
}
