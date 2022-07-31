using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.UI
{
	public class PowerSupplyPowerGenerator : MonoBehaviour
	{
		public Generator Generator;

		public Text Name;

		public Text Status;

		public Text PowerOutput;

		public Text Consumption;

		public Text Capacity;

		public Image Filler;

		public Text ToggleText;

		public GameObject IsOnline;

		public GameObject DisablePattern;

		public GameObject Warning;

		[HideInInspector]
		public ResourceContainer Container;

		public Transform PartsTransform;

		public GameObject PartPref;

		public List<PartSlotUI> PowerPartsUI = new List<PartSlotUI>();

		private void Start()
		{
			Text[] componentsInChildren = GetComponentsInChildren<Text>(true);
			foreach (Text text in componentsInChildren)
			{
				string value = null;
				if (Localization.PanelsLocalization.TryGetValue(text.name, out value))
				{
					text.text = value;
				}
			}
			Name.text = Generator.Type.ToLocalizedString().ToUpper();
		}

		public void Toggle()
		{
			Generator.Toggle();
		}
	}
}
