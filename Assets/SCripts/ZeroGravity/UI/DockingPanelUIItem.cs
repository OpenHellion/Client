using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class DockingPanelUIItem : MonoBehaviour
	{
		public Text NameText;

		public Text DistanceText;

		public GameObject NameSelected;

		public Ship Ship;

		private bool isSelected;

		private string distance = string.Empty;

		public bool IsSelected
		{
			get
			{
				return isSelected;
			}
			set
			{
				isSelected = value;
				NameSelected.SetActive(isSelected);
				if (isSelected)
				{
					DistanceText.color = Colors.Orange;
				}
				else
				{
					DistanceText.color = Colors.White;
				}
			}
		}

		public string Distance
		{
			get
			{
				return distance;
			}
			set
			{
				distance = value;
				DistanceText.text = value;
			}
		}
	}
}
