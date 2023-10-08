using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.UI
{
	public class PowerSupplyCapacitor : MonoBehaviour
	{
		[HideInInspector] public GeneratorCapacitor Capacitor;

		public Text Value;

		public Image Filler;

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
		}
	}
}
