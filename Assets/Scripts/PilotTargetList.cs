using System.Collections.Generic;
using OpenHellion;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using ZeroGravity;
using ZeroGravity.ShipComponents;
using ZeroGravity.UI;

public class PilotTargetList : MonoBehaviour
{
	public Transform TargetListHolder;

	public TargetInListUI TargetListUI;

	public Text RadarRangeCurrent;

	public Text SelectionLabel;

	public GameObject NotActive;

	[FormerlySerializedAs("_worldState")] [SerializeField] private World _world;

	private PilotOverlayUI ParentPilot => _world.InWorldPanels.Pilot;

	private void Start()
	{
		NotActive.SetActive(true);
		SelectionLabel.text = Localization.Selection.ToUpper();
	}

	public void CreateTargetInList(TargetObject target)
	{
		TargetInListUI targetInListUI = Object.Instantiate(TargetListUI, TargetListHolder);
		targetInListUI.gameObject.transform.localScale = Vector3.one;
		targetInListUI.gameObject.SetActive(true);
		targetInListUI.Target = target;
		targetInListUI.AB = target.ArtificialBody;
		targetInListUI.Name.text = target.Name;
		targetInListUI.Icon.sprite = target.Icon;
	}

	public void UpdateTargetList()
	{
		List<TargetInListUI> list = new List<TargetInListUI>();
		TargetInListUI[] componentsInChildren = TargetListHolder.GetComponentsInChildren<TargetInListUI>(true);
		foreach (TargetInListUI item in componentsInChildren)
		{
			list.Add(item);
		}

		foreach (TargetInListUI item2 in list)
		{
			if (ParentPilot.SelectedTarget != null)
			{
				item2.Selected.SetActive(ParentPilot.SelectedTarget.ArtificialBody == item2.AB);
			}

			item2.Distance.text = FormatHelper.DistanceFormat(item2.Target.Distance);
		}

		GoToCurrentElement();
	}

	public void GoToCurrentElement()
	{
		int num = ParentPilot.AllTargets.IndexOf(ParentPilot.SelectedTarget);
		if (ParentPilot.AllTargets.Count > 6)
		{
			float y = 70f * num;
			TargetListHolder.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, y);
		}
		else if (ParentPilot.AllTargets.Count == 0)
		{
			TargetListHolder.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
		}
	}

	public void ToggleTargetList(bool toggle)
	{
		NotActive.SetActive(toggle);
	}
}
