using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.UI;

namespace ZeroGravity
{
	public class InventoryGroupUI : MonoBehaviour
	{
		public InventoryUI Inventory;

		public Text Name;

		public Transform SlotHolder;

		public GameObject LootAllButton;

		public Text LootAllLabel;

		public List<AbstractSlotUI> List;

		public void Start()
		{
			LootAllLabel.text = Localization.LootAll.ToUpper();
		}

		public void Loot()
		{
			Inventory.LootAll(SlotHolder);
		}
	}
}
