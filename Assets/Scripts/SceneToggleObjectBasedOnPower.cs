using UnityEngine;
using ZeroGravity.Network;
using ZeroGravity.ShipComponents;

public class SceneToggleObjectBasedOnPower : MonoBehaviour, IVesselSystemAccessory
{
	[SerializeField]
	private VesselSystem _BaseVesselSystem;

	public bool Reverse;

	public VesselSystem BaseVesselSystem
	{
		get
		{
			return _BaseVesselSystem;
		}
		set
		{
			_BaseVesselSystem = value;
		}
	}

	private void Start()
	{
		_BaseVesselSystem.Accessories.Add(this);
		BaseVesselSystemUpdated();
	}

	public void BaseVesselSystemUpdated()
	{
		if (BaseVesselSystem.Status == SystemStatus.OnLine)
		{
			base.gameObject.SetActive(!Reverse);
		}
		else
		{
			base.gameObject.SetActive(Reverse);
		}
	}
}
