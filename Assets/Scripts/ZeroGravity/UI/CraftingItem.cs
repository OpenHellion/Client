using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class CraftingItem : MonoBehaviour
	{
		public CargoPanel Panel;

		public SubSystemFabricator.CraftableItemData Data;

		public Image Icon;

		public Text Name;

		public Text TimeToCraft;

		public Transform ResourcesTransform;

		public CargoResourceForCraftingUI ResourceForCrafting;

		[CompilerGenerated] private static Func<KeyValuePair<ResourceType, float>, float> _003C_003Ef__am_0024cache0;

		private void Start()
		{
			string text = ((Data.CompoundType.Tier == 1) ? string.Empty : (" (" + Data.CompoundType.Tier + ")"));
			Name.text = Item.GetName(Data.CompoundType) + text;
			Name.color = Colors.Tier[Data.CompoundType.Tier];
			TimeToCraft.text = FormatHelper.Timer(GetTime());
			Icon.sprite = SpriteManager.Instance.GetSprite(Data.CompoundType);
			if (CanFabricate())
			{
				GetComponent<Image>().color = Colors.White;
			}
			else
			{
				GetComponent<Image>().color = Colors.Gray;
			}

			foreach (KeyValuePair<ResourceType, float> resource in Data.Resources)
			{
				CargoResourceForCraftingUI cargoResourceForCraftingUI =
					UnityEngine.Object.Instantiate(ResourceForCrafting, ResourcesTransform);
				cargoResourceForCraftingUI.transform.localScale = Vector3.one;
				cargoResourceForCraftingUI.gameObject.SetActive(true);
				cargoResourceForCraftingUI.Name.text = resource.Key.ToLocalizedString().ToUpper();
				cargoResourceForCraftingUI.Icon.sprite = SpriteManager.Instance.GetSprite(resource.Key);
				cargoResourceForCraftingUI.Value.text = FormatHelper.FormatValue(resource.Value);
			}
		}

		public float GetTime()
		{
			Dictionary<ResourceType, float> resources = Data.Resources;
			if (_003C_003Ef__am_0024cache0 == null)
			{
				_003C_003Ef__am_0024cache0 = _003CGetTime_003Em__0;
			}

			return resources.Sum(_003C_003Ef__am_0024cache0) * Panel.Fabricator.TimePerResourceUnit;
		}

		public void SelectItem()
		{
			Panel.CurrentCraftingItem = this;
			Panel.SetItemForCrafting();
		}

		public bool CanFabricate()
		{
			if (Panel.Fabricator.CargoResources.Resources != null)
			{
				return Panel.Fabricator.HasEnoughResources(Data.CompoundType.Type, Data.CompoundType.SubType,
					Data.CompoundType.PartType, Data.CompoundType.Tier);
			}

			return false;
		}

		[CompilerGenerated]
		private static float _003CGetTime_003Em__0(KeyValuePair<ResourceType, float> m)
		{
			return m.Value;
		}
	}
}
