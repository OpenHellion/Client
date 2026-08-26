using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity;
using ZeroGravity.ShipComponents;
using ZeroGravity.UI;

namespace OpenHellion.UI
{
	public class InWorldPanels : MonoBehaviour
	{
		[Title("Menus")] public PowerSupply PowerSupply;

		public LifeSupportPanel LifeSupport;

		public CargoPanel Cargo;

		public CryoPodUI Cryo;

		public SecurityScreen Security;

		public AirlockUI Airlock;

		public NavigationPanel Navigation;

		public DockingPanel Docking;

		public PilotOverlayUI Pilot;

		public PilotingOptionsUI PilotingOptions;

		public NameTagUI NameTagUI;

		public TextLogTerminal TextLogTerminal;

		public GameObject ToolTip;

		public TMP_Text TooltipHeading;

		public TMP_Text TooltipContent;

		public void LocalizePanels()
		{
			Text[] texts = GetComponentsInChildren<Text>(true);
			foreach (Text text in texts)
			{
				string value;
				if (Localization.PanelsLocalization.TryGetValue(text.name, out value))
				{
					text.text = value;
				}
			}

			TMP_Text[] tmpText = GetComponentsInChildren<TMP_Text>(true);
			foreach (TMP_Text text in tmpText)
			{
				string value;
				if (Localization.PanelsLocalization.TryGetValue(text.name, out value))
				{
					text.text = value;
				}
			}
		}

		public void Interact()
		{
			ToolTip.Activate(false);
			gameObject.SetActive(true);
		}

		public void Detach()
		{
			ToolTip.Activate(false);
			gameObject.SetActive(false);
		}
	}
}
