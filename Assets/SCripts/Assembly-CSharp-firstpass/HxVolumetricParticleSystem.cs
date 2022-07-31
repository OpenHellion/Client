using UnityEngine;

[ExecuteInEditMode]
public class HxVolumetricParticleSystem : MonoBehaviour
{
	public enum ParticleBlendMode
	{
		Max = 0,
		Add = 1,
		Min = 2,
		Sub = 3
	}

	[Range(0f, 4f)]
	public float DensityStrength = 1f;

	private HxOctreeNode<HxVolumetricParticleSystem>.NodeObject octreeNode;

	[HideInInspector]
	public Renderer particleRenderer;

	public ParticleBlendMode BlendMode = ParticleBlendMode.Add;

	private Vector3 minBounds;

	private Vector3 maxBounds;

	private Bounds LastBounds;

	private void OnEnable()
	{
		particleRenderer = GetComponent<Renderer>();
		LastBounds = particleRenderer.bounds;
		minBounds = LastBounds.min;
		maxBounds = LastBounds.max;
		if (octreeNode == null)
		{
			HxVolumetricCamera.AllParticleSystems.Add(this);
			octreeNode = HxVolumetricCamera.AddParticleOctree(this, minBounds, maxBounds);
		}
	}

	public void UpdatePosition()
	{
		if (base.transform.hasChanged || true)
		{
			LastBounds = particleRenderer.bounds;
			minBounds = LastBounds.min;
			maxBounds = LastBounds.max;
			HxVolumetricCamera.ParticleOctree.Move(octreeNode, minBounds, maxBounds);
			base.transform.hasChanged = false;
		}
	}

	private void OnDisable()
	{
		if (octreeNode != null)
		{
			HxVolumetricCamera.AllParticleSystems.Remove(this);
			HxVolumetricCamera.RemoveParticletOctree(this);
			octreeNode = null;
		}
	}

	private void OnDestroy()
	{
		if (octreeNode != null)
		{
			HxVolumetricCamera.AllParticleSystems.Remove(this);
			HxVolumetricCamera.RemoveParticletOctree(this);
			octreeNode = null;
		}
	}
}
