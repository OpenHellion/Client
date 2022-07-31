using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class DropdownMenu : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CMakeDropDownItem_003Ec__AnonStorey0
	{
		internal DropDownItem dropItem;

		internal DropdownMenu _0024this;

		internal void _003C_003Em__0()
		{
			_0024this.UpdateDropDownButtonSize(dropItem);
		}
	}

	public GameObject DropDownItemPref;

	public GameObject ScrollViewContent;

	public List<DropDownItem> Items = new List<DropDownItem>();

	private void Start()
	{
		Items.Add(new DropDownItem
		{
			Heading = "Lokacija 1",
			Text = "Lorem Ipsum is simply dummy text of the printing and typesetting industry."
		});
		Items.Add(new DropDownItem
		{
			Heading = "Lokacija 2",
			Text = "Lorem Ipsum is simply dummy text of the printing and typesetting industry."
		});
		foreach (DropDownItem item in Items)
		{
			MakeDropDownItem(item);
		}
	}

	private void Update()
	{
	}

	public void OnClickDropDownButton()
	{
		if (ScrollViewContent.activeInHierarchy)
		{
			ScrollViewContent.SetActive(false);
		}
		else
		{
			ScrollViewContent.SetActive(true);
		}
	}

	private void UpdateDropDownButtonSize(DropDownItem dropItem)
	{
		OnClickDropDownButton();
	}

	private void MakeDropDownItem(DropDownItem dropItem)
	{
		_003CMakeDropDownItem_003Ec__AnonStorey0 _003CMakeDropDownItem_003Ec__AnonStorey = new _003CMakeDropDownItem_003Ec__AnonStorey0();
		_003CMakeDropDownItem_003Ec__AnonStorey.dropItem = dropItem;
		_003CMakeDropDownItem_003Ec__AnonStorey._0024this = this;
		GameObject gameObject = Object.Instantiate(DropDownItemPref);
		gameObject.SetActive(true);
		gameObject.transform.SetParent(base.transform.Find("Content").transform);
		gameObject.transform.Find("Padding/ManeuverSetText").GetComponent<Text>().text = _003CMakeDropDownItem_003Ec__AnonStorey.dropItem.Heading;
		gameObject.transform.Find("Padding/DestinationSetText").GetComponent<Text>().text = _003CMakeDropDownItem_003Ec__AnonStorey.dropItem.Text;
		gameObject.GetComponent<Button>().onClick.AddListener(_003CMakeDropDownItem_003Ec__AnonStorey._003C_003Em__0);
	}
}
