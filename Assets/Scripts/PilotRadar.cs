using System.Collections.Generic;
using OpenHellion;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using ZeroGravity;
using ZeroGravity.UI;

public class PilotRadar : MonoBehaviour
{
	public Transform Root;

	public RadarShipElement RadarElement;

	public GameObject NotActive;

	public Text PilotingNotActiveLabel;

	[FormerlySerializedAs("_worldState")] [SerializeField] private World _world;

	private void Start()
	{
		PilotingNotActiveLabel.text = Localization.PilotingNotActive.ToUpper();
		NotActive.SetActive(true);
	}

	public void CreateRadarTarget(TargetObject target)
	{
		if (!(target.Distance > 10000f))
		{
			RadarShipElement radarShipElement = Instantiate(RadarElement, Root);
			radarShipElement.PilotP = _world.InWorldPanels.Pilot;
			radarShipElement.gameObject.transform.localScale = Vector3.one;
			radarShipElement.gameObject.SetActive(true);
			radarShipElement.Target = target;
			radarShipElement.AB = target.ArtificialBody;
			radarShipElement.Icon.sprite = target.Icon;
		}
	}

	public void UpdateRadar()
	{
		RadarShipElement[] radarElementsInChildren = Root.GetComponentsInChildren<RadarShipElement>(true);
		foreach (RadarShipElement radarElement in radarElementsInChildren)
		{
			if (_world.InWorldPanels.Pilot.SelectedTarget is { ArtificialBody: not null })
			{
				radarElement.Selected.SetActive(_world.InWorldPanels.Pilot.SelectedTarget.ArtificialBody ==
				                                radarElement.AB);
			}
			else
			{
				radarElement.Selected.SetActive(false);
			}
		}
	}

	public void ToggleRadarScreen(bool toggle)
	{
		NotActive.SetActive(toggle);
	}
}
