using System.Collections.Generic;
using System.Linq;
using ThreeEyedGames;
using UnityEngine;
using ZeroGravity.Data;

public class MiningPointVisual : MonoBehaviour
{
	public List<Decalicious> DecalList;

	public List<MeshRenderer> Rock1List;

	public List<MeshRenderer> Rock2List;

	public List<MeshRenderer> Rock3List;

	public List<MiningPointTypeObject> MiningPointTypes;

	public Light MiningPointLight;

	public void SetMaterials(ResourceType resourceType)
	{
		MiningPointTypeObject miningPointTypeObject =
			MiningPointTypes.FirstOrDefault((MiningPointTypeObject m) => m.ResourceType == resourceType);
		if (miningPointTypeObject is null)
		{
			return;
		}

		foreach (Decalicious decal in DecalList)
		{
			decal.Material = miningPointTypeObject.DecalMaterial;
		}

		foreach (MeshRenderer rock in Rock1List)
		{
			rock.material = miningPointTypeObject.Rock1Material;
		}

		foreach (MeshRenderer rock2 in Rock2List)
		{
			rock2.material = miningPointTypeObject.Rock1Material;
		}

		foreach (MeshRenderer rock3 in Rock3List)
		{
			rock3.material = miningPointTypeObject.Rock1Material;
		}

		MiningPointLight.color = miningPointTypeObject.LightColor;
	}
}
