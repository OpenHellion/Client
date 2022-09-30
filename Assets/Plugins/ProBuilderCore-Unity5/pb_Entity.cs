using ProBuilder2.Common;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("")]
public class pb_Entity : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	private EntityType _entityType;

	public EntityType entityType => _entityType;

	public void Awake()
	{
		MeshRenderer component = GetComponent<MeshRenderer>();
		if ((bool)component)
		{
			switch (entityType)
			{
			case EntityType.Occluder:
				break;
			case EntityType.Detail:
				break;
			case EntityType.Trigger:
				component.enabled = false;
				break;
			case EntityType.Collider:
				component.enabled = false;
				break;
			}
		}
	}

	public void SetEntity(EntityType t)
	{
		_entityType = t;
	}
}
