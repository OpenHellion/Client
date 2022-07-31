using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class PartSlotUI : MonoBehaviour
	{
		public Image PartIcon;

		public Text NameText;

		public Text PartStatus;

		public Text Health;

		public Text DescriptionText;

		public SceneMachineryPartSlot PartSlot;

		public GameObject Panel;

		public void UpdateUI()
		{
			MachineryPartType machineryPartType = PartSlot.GetMachineryPartType();
			PartIcon.sprite = Client.Instance.SpriteManager.GetSprite(machineryPartType);
			NameText.text = machineryPartType.ToLocalizedString().ToUpper();
			if (PartSlot.Item == null)
			{
				PartIcon.color = Colors.Red;
				DescriptionText.text = Localization.InsertPartToImprove + " " + PartSlot.Scope.ToLocalizedString().ToLower() + ".";
				Health.gameObject.SetActive(false);
				PartStatus.text = Localization.Missing.ToUpper();
				PartStatus.color = Colors.Red;
			}
			else
			{
				PartIcon.color = Colors.White;
				DescriptionText.text = PartSlot.GetDescription() + FormatHelper.PartTier(PartSlot.Item as MachineryPart);
				float val = (PartSlot.Item as MachineryPart).Health / (PartSlot.Item as MachineryPart).MaxHealth;
				Health.gameObject.SetActive(true);
				Health.text = Localization.Health.ToUpper() + ": " + FormatHelper.Percentage(val);
				PartStatus.text = Localization.Tier.ToUpper() + ": " + (PartSlot.Item as MachineryPart).Tier;
				PartStatus.color = Colors.Tier[(PartSlot.Item as MachineryPart).Tier];
			}
		}
	}
}
