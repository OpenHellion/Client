using UnityEngine;
using ZeroGravity.Network;
using ZeroGravity.ShipComponents;

public class SceneAnimatedSystemAccessory : MonoBehaviour, IVesselSystemAccessory
{
	[SerializeField] private VesselSystem _BaseVesselSystem;

	public Animator Animator;

	public VesselSystem BaseVesselSystem
	{
		get { return _BaseVesselSystem; }
		set { _BaseVesselSystem = value; }
	}

	private void Start()
	{
		Animator = GetComponent<Animator>();
		_BaseVesselSystem.Accessories.Add(this);
		BaseVesselSystemUpdated();
	}

	public void BaseVesselSystemUpdated()
	{
		if (BaseVesselSystem.Status == SystemStatus.Online)
		{
			Animator.SetBool("Online", true);
		}

		if (BaseVesselSystem.Status == SystemStatus.Offline)
		{
			Animator.SetBool("Online", false);
		}
	}

	private void OnDestroy()
	{
		if (BaseVesselSystem != null)
		{
			BaseVesselSystem.Accessories.Remove(this);
		}
	}
}
