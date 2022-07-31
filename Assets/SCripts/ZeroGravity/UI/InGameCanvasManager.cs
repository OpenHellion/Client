using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.UI
{
	public class InGameCanvasManager : MonoBehaviour
	{
		public PowerSupply PowerSupply;

		public LifeSupportPanel LifeSupport;

		public CargoPanel Cargo;

		public CryoPodUI Cryo;

		public SecurityScreen Security;

		public AirlockUI Airlock;

		public NavigationPanel Navigation;

		public DockingPanel Docking;

		public PilotOverlayUI Pilot;

		public PilotingOptionsUI PilotingOptions;

		public MessageTerminal MessageTerminal;

		public NameTagUI NameTagUI;

		public TextLogTerminal TextLogTerminal;

		public GameObject ToolTip;

		public Text TooltipHeading;

		public Text TooltipContent;

		private void Start()
		{
		}

		public void LocalizePanels()
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

		private void Update()
		{
		}

		public void Interact()
		{
			ToolTip.Activate(false);
			base.gameObject.SetActive(true);
		}

		public void Detach()
		{
			ToolTip.Activate(false);
			base.gameObject.SetActive(false);
		}
	}
}
