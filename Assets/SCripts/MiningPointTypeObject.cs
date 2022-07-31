using UnityEngine;
using ZeroGravity.Data;

[CreateAssetMenu(fileName = "MiningPointType", menuName = "Helpers/MiningPointType")]
public class MiningPointTypeObject : ScriptableObject
{
	public ResourceType ResourceType;

	public Material DecalMaterial;

	public Material Rock1Material;

	public Material Rock2Material;

	public Material Rock3Material;

	public Color LightColor;
}
