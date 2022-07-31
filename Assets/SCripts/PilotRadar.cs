using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity;
using ZeroGravity.UI;

public class PilotRadar : MonoBehaviour
{
	public Transform Root;

	public RadarShipElement RadarElement;

	public GameObject NotActive;

	public Text PilotingNotActiveLabel;

	private void Start()
	{
		PilotingNotActiveLabel.text = Localization.PilotingNotActive.ToUpper();
		NotActive.SetActive(true);
	}

	public void CreateRadarTarget(TargetObject target)
	{
		if (!(target.Distance > 10000f))
		{
			RadarShipElement radarShipElement = Object.Instantiate(RadarElement, Root);
			radarShipElement.gameObject.transform.localScale = Vector3.one;
			radarShipElement.gameObject.SetActive(true);
			radarShipElement.Target = target;
			radarShipElement.AB = target.AB;
			radarShipElement.Icon.sprite = target.Icon;
		}
	}

	public void UpdateRadar()
	{
		List<RadarShipElement> list = new List<RadarShipElement>();
		RadarShipElement[] componentsInChildren = Root.GetComponentsInChildren<RadarShipElement>(true);
		foreach (RadarShipElement item in componentsInChildren)
		{
			list.Add(item);
		}
		foreach (RadarShipElement item2 in list)
		{
			if (Client.Instance.InGamePanels.Pilot.SelectedTarget != null && Client.Instance.InGamePanels.Pilot.SelectedTarget.AB != null)
			{
				item2.Selected.SetActive(Client.Instance.InGamePanels.Pilot.SelectedTarget.AB == item2.AB);
			}
			else
			{
				item2.Selected.SetActive(false);
			}
		}
	}

	public void ToggleRadarScreen(bool toggle)
	{
		NotActive.SetActive(toggle);
	}
}
