using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.LevelDesign;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.UI
{
	public class WarpCellUI : MonoBehaviour
	{
		public NavigationPanel Panel;

		public SceneMachineryPartSlot Slot;

		public bool IsSelected;

		public GameObject Selected;

		public Image FillerCurrent;

		public Image FillerRequired;

		public GameObject NoCellInSlot;

		public Text Value;

		private void Start()
		{
		}

		public void SelectWarpCell()
		{
			if (!(Slot.Item == null))
			{
				if (IsSelected)
				{
					IsSelected = false;
					UpdateUI();
				}
				else
				{
					IsSelected = true;
					UpdateUI();
				}
			}
		}

		public void UpdateUI()
		{
			if (Slot.Item != null)
			{
				Selected.SetActive(IsSelected);
				FillerCurrent.fillAmount = Slot.Item.Health / Slot.Item.MaxHealth;
				NoCellInSlot.SetActive(false);
				Value.text = FormatHelper.FormatValue(Slot.Item.Health, true) + "/" +
				             FormatHelper.FormatValue(Slot.Item.MaxHealth, true);
				if (!IsSelected)
				{
					FillerRequired.fillAmount = 0f;
				}
			}
			else
			{
				Selected.SetActive(false);
				FillerCurrent.fillAmount = 0f;
				NoCellInSlot.SetActive(true);
				Value.text = string.Empty;
			}
		}
	}
}
