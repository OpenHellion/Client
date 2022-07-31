using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ThreeEyedGames;
using UnityEngine;
using ZeroGravity.Data;

public class MiningPointVisual : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CSetMaterials_003Ec__AnonStorey0
	{
		internal ResourceType resourceType;

		internal bool _003C_003Em__0(MiningPointTypeObject m)
		{
			return m.ResourceType == resourceType;
		}
	}

	public List<Decalicious> DecalList;

	public List<MeshRenderer> Rock1List;

	public List<MeshRenderer> Rock2List;

	public List<MeshRenderer> Rock3List;

	public List<MiningPointTypeObject> MiningPointTypes;

	public Light MiningPointLight;

	public void SetMaterials(ResourceType resourceType)
	{
		_003CSetMaterials_003Ec__AnonStorey0 _003CSetMaterials_003Ec__AnonStorey = new _003CSetMaterials_003Ec__AnonStorey0();
		_003CSetMaterials_003Ec__AnonStorey.resourceType = resourceType;
		MiningPointTypeObject miningPointTypeObject = MiningPointTypes.FirstOrDefault(_003CSetMaterials_003Ec__AnonStorey._003C_003Em__0);
		if (miningPointTypeObject == null)
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
