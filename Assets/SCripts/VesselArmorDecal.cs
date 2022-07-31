using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ThreeEyedGames;
using UnityEngine;
using ZeroGravity;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Objects;

public class VesselArmorDecal : MonoBehaviour
{
	public List<Decalicious> Decals;

	public SpaceObjectVessel ParentVessel;

	private SceneMachineryPartSlot ArmorSlot;

	[ColorUsage(false, true)]
	public Color NaniteCoreColor = Color.cyan;

	[ColorUsage(false, true)]
	public Color MilitaryNaniteCoreColor = Color.red;

	[CompilerGenerated]
	private static Func<SceneMachineryPartSlot, bool> _003C_003Ef__am_0024cache0;

	private void Start()
	{
		Decalicious[] componentsInChildren = GetComponentsInChildren<Decalicious>();
		foreach (Decalicious decalicious in componentsInChildren)
		{
			decalicious.Material = UnityEngine.Object.Instantiate(decalicious.Material);
			Decals.Add(decalicious);
		}
		if (Client.IsGameBuild)
		{
			ParentVessel = GetComponentInParent<GeometryRoot>().MainObject as SpaceObjectVessel;
		}
		SceneMachineryPartSlot[] machineryPartSlots = ParentVessel.VesselBaseSystem.MachineryPartSlots;
		if (_003C_003Ef__am_0024cache0 == null)
		{
			_003C_003Ef__am_0024cache0 = _003CStart_003Em__0;
		}
		ArmorSlot = machineryPartSlots.Where(_003C_003Ef__am_0024cache0).FirstOrDefault();
	}

	public void UpdateDecals()
	{
		float fade = 0f;
		Color value = Color.black;
		if (ArmorSlot.Item != null)
		{
			if ((ArmorSlot.Item as MachineryPart).PartType == MachineryPartType.NaniteCore)
			{
				value = NaniteCoreColor;
			}
			if ((ArmorSlot.Item as MachineryPart).PartType == MachineryPartType.MillitaryNaniteCore)
			{
				value = MilitaryNaniteCoreColor;
			}
			fade = ArmorSlot.Item.Health / ArmorSlot.Item.MaxHealth;
		}
		foreach (Decalicious decal in Decals)
		{
			decal.Fade = fade;
			decal.Material.SetColor("_EmissionColor", value);
		}
	}

	[CompilerGenerated]
	private static bool _003CStart_003Em__0(SceneMachineryPartSlot m)
	{
		return m.Scope == MachineryPartSlotScope.Armor;
	}
}
