using System.Collections.Generic;
using System.Linq;
using ThreeEyedGames;
using UnityEngine;
using OpenHellion;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Objects;

public class VesselArmorDecal : MonoBehaviour
{
	public List<Decalicious> Decals;

	public SpaceObjectVessel ParentVessel;

	private SceneMachineryPartSlot _armorSlot;

	[ColorUsage(false, true)] public Color NaniteCoreColor = Color.cyan;

	[ColorUsage(false, true)] public Color MilitaryNaniteCoreColor = Color.red;

	private void Start()
	{
		Decalicious[] componentsInChildren = GetComponentsInChildren<Decalicious>();
		foreach (Decalicious decalicious in componentsInChildren)
		{
			decalicious.Material = Object.Instantiate(decalicious.Material);
			Decals.Add(decalicious);
		}

		ParentVessel = GetComponentInParent<GeometryRoot>().MainObject as SpaceObjectVessel;
		_armorSlot = ParentVessel.VesselBaseSystem.MachineryPartSlots
			.Where((SceneMachineryPartSlot m) => m.Scope == MachineryPartSlotScope.Armor).FirstOrDefault();
	}

	public void UpdateDecals()
	{
		float fade = 0f;
		Color value = Color.black;
		if (_armorSlot.Item is not null)
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

		foreach (Decalicious decal in Decals)
		{
			decal.Fade = fade;
			decal.Material.SetColor("_EmissionColor", value);
		}
	}
}
