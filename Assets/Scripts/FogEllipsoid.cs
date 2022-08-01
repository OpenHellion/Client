using UnityEngine;

[ExecuteInEditMode]
public class FogEllipsoid : MonoBehaviour
{
	public enum Blend
	{
		Additive = 0,
		Multiplicative = 1
	}

	public Blend m_Blend;

	public float m_Density = 1f;

	[MinValue(0f)]
	public float m_Radius = 1f;

	[MinValue(0f)]
	public float m_Stretch = 2f;

	[Range(0f, 1f)]
	public float m_Feather = 0.7f;

	[Range(0f, 1f)]
	public float m_NoiseAmount;

	public float m_NoiseSpeed = 1f;

	[MinValue(0f)]
	public float m_NoiseScale = 1f;

	private bool m_AddedToLightManager;

	private void AddToLightManager()
	{
		if (!m_AddedToLightManager)
		{
			m_AddedToLightManager = LightManager<FogEllipsoid>.Add(this);
		}
	}

	private void OnEnable()
	{
		AddToLightManager();
	}

	private void Update()
	{
		AddToLightManager();
	}

	private void OnDisable()
	{
		LightManager<FogEllipsoid>.Remove(this);
		m_AddedToLightManager = false;
	}

	private void OnDrawGizmosSelected()
	{
		Matrix4x4 identity = Matrix4x4.identity;
		Transform transform = base.transform;
		identity.SetTRS(transform.position, transform.rotation, new Vector3(1f, m_Stretch, 1f));
		Gizmos.matrix = identity;
		Gizmos.DrawWireSphere(Vector3.zero, m_Radius);
	}
}
