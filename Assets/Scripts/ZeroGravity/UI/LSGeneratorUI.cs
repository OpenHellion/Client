using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.UI
{
	public class LSGeneratorUI : MonoBehaviour
	{
		[HideInInspector] public Generator Generator;

		public Text Name;

		public Text Status;

		public Text Consumption;

		public Text Output;

		public Text ToggleText;

		public GameObject IsOnline;

		public GameObject DisablePattern;

		public Image OxygenFiller;

		public Text OxygenValue;

		public Text OxygenChangeRate;

		public Image NitrogenFiller;

		public Text NitrogenValue;

		public Text NitrogenChangeRate;

		public Transform PartsTransform;

		public GameObject PartPref;

		public List<PartSlotUI> PowerPartsUI = new List<PartSlotUI>();

		private void Start()
		{
			Name.text = Generator.Type.ToLocalizedString().ToUpper();
			Text[] componentsInChildren = GetComponentsInChildren<Text>(true);
			foreach (Text text in componentsInChildren)
			{
				string value = null;
				if (Localization.PanelsLocalization.TryGetValue(text.name, out value))
				{
					text.text = value;
				}
			}

			UpdateContainers();
		}

		public void Toggle()
		{
			Generator.Toggle();
		}

		public void UpdateContainers()
		{
			ResourceContainer[] resourceContainers = Generator.ResourceContainers;
			foreach (ResourceContainer resourceContainer in resourceContainers)
			{
				if (resourceContainer.DistributionSystemType == DistributionSystemType.Oxygen)
				{
					OxygenChangeRate.text = Generator
						.GetResourceRequirement(resourceContainer.DistributionSystemType, true).ToString("0.0");
					OxygenValue.text = FormatHelper.CurrentMax(resourceContainer.Quantity, resourceContainer.Capacity);
					OxygenFiller.fillAmount = resourceContainer.Quantity / resourceContainer.Capacity;
				}

				if (resourceContainer.DistributionSystemType == DistributionSystemType.Nitrogen &&
				    Generator.Type == GeneratorType.Air)
				{
					NitrogenChangeRate.text = Generator
						.GetResourceRequirement(resourceContainer.DistributionSystemType, true).ToString("0.0");
					NitrogenValue.text =
						FormatHelper.CurrentMax(resourceContainer.Quantity, resourceContainer.Capacity);
					NitrogenFiller.fillAmount = resourceContainer.Quantity / resourceContainer.Capacity;
				}
			}
		}
	}
}
