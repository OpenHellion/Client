using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using OpenHellion;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Objects;

public class VesselArmorDecal : MonoBehaviour
{
	public List<DecalProjector> Decals;

	public SpaceObjectVessel ParentVessel;

	private SceneMachineryPartSlot _armorSlot;

	[ColorUsage(false, true)] public Color NaniteCoreColor = Color.cyan;

	[ColorUsage(false, true)] public Color MilitaryNaniteCoreColor = Color.red;

	private void Start()
	{
		foreach (DecalProjector decal in GetComponentsInChildren<DecalProjector>())
		{
			decal.material = Instantiate(decal.material);
			Decals.Add(decal);
		}

		ParentVessel = GetComponentInParent<GeometryRoot>().MainObject as SpaceObjectVessel;
		_armorSlot = ParentVessel.VesselBaseSystem.MachineryPartSlots.FirstOrDefault(m => m.Scope == MachineryPartSlotScope.Armor);
	}

	public void UpdateDecals()
	{
		float fade = 0f;
		Color value = Color.black;
		if (_armorSlot.Item != null)
		{
			if ((_armorSlot.Item as MachineryPart).PartType == MachineryPartType.NaniteCore)
			{
				value = NaniteCoreColor;
			}

			if ((_armorSlot.Item as MachineryPart).PartType == MachineryPartType.MillitaryNaniteCore)
			{
				value = MilitaryNaniteCoreColor;
			}

			fade = _armorSlot.Item.Health / _armorSlot.Item.MaxHealth;
		}

		foreach (DecalProjector decal in Decals)
		{
			decal.fadeFactor = fade;
			decal.material.SetColor("_EmissionColor", value);
		}
	}
}
