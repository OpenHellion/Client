using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.Objects;
using ZeroGravity.UI;

namespace ZeroGravity
{
	public class GlossaryUI : MonoBehaviour
	{
		private List<AbstractGlossaryElement> allElements = new List<AbstractGlossaryElement>();

		public Dictionary<AbstractGlossaryElement, GlossaryElementUI> AllElements =
			new Dictionary<AbstractGlossaryElement, GlossaryElementUI>();

		public GameObject ElementUI;

		public List<GameObject> Options;

		[NonSerialized] public GlossaryElementUI Current;

		public GameObject CurrentHolder;

		public GameObject Equipment;

		public Transform EquipmentHolder;

		public GameObject Elements;

		public Transform ElementsHolder;

		public GameObject ElementsGroup;

		public Text Name;

		public Text Description;

		public Image Image;

		public Sprite DefaultImage;

		public GameObject Tiers;

		public Text Tier01;

		public Text Tier02;

		public Text Tier03;

		public Text Tier04;

		private void Awake()
		{
			allElements = Resources.LoadAll<AbstractGlossaryElement>("GlossaryElements").ToList();
			foreach (AbstractGlossaryElement allElement in allElements)
			{
				GameObject gameObject = Instantiate(ElementUI, ElementsHolder);
				gameObject.transform.localScale = Vector3.one;
				GlossaryElementUI elementUi = gameObject.GetComponent<GlossaryElementUI>();
				elementUi.Screen = this;
				elementUi.Element = allElement;
				elementUi.SetIcon();
				AllElements.Add(allElement, elementUi);
				if (elementUi.Element.Category == GlossaryCategory.Items)
				{
					DynamicObjectData dynamicObjectData =
						StaticData.DynamicObjectsDataList.Values.FirstOrDefault((DynamicObjectData m) =>
							m.CompoundType.Equals((elementUi.Element as GlossaryElementEquipment).CompoundType));
					if (dynamicObjectData != null)
					{
						elementUi.SubCategory = dynamicObjectData.DefaultAuxData.Category;
					}
				}
			}

			foreach (ItemCategory cat in AllElements.Values.Select((GlossaryElementUI m) => m.SubCategory).Distinct())
			{
				GameObject gameObject2 = Instantiate(ElementsGroup, EquipmentHolder);
				gameObject2.transform.localScale = Vector3.one;
				GlossaryEquipmentGroup component = gameObject2.GetComponent<GlossaryEquipmentGroup>();
				component.Name.text = cat.ToLocalizedString();
				foreach (KeyValuePair<AbstractGlossaryElement, GlossaryElementUI> item in AllElements.Where(
					         (KeyValuePair<AbstractGlossaryElement, GlossaryElementUI> m) =>
						         m.Value.SubCategory == cat))
				{
					if (item.Value.Element.Category == GlossaryCategory.Items)
					{
						item.Value.gameObject.transform.SetParent(component.Holder);
					}
					else
					{
						item.Value.gameObject.transform.SetParent(ElementsHolder);
					}
				}
			}
		}

		private void Start()
		{
		}

		public void Toggle(bool val, int option = 1)
		{
			base.gameObject.SetActive(val);
			if (val)
			{
				CurrentHolder.Activate(Current != null);
				SelectCategory(option);
			}
		}

		public void SelectCategory(int scr)
		{
			foreach (GameObject option in Options)
			{
				option.SetActive(value: false);
			}

			Options[scr - 1].SetActive(value: true);
			switch (scr)
			{
				case 1:
					ShowGroup(GlossaryCategory.Items);
					break;
				case 2:
					ShowGroup(GlossaryCategory.Systems);
					break;
				case 3:
					ShowGroup(GlossaryCategory.Modules);
					break;
				case 4:
					ShowGroup(GlossaryCategory.Story);
					break;
				case 5:
					ShowGroup(GlossaryCategory.Resources);
					break;
			}
		}

		public void ShowGroup(GlossaryCategory cat)
		{
			Equipment.Activate(cat == GlossaryCategory.Items);
			Elements.Activate(cat != GlossaryCategory.Items);
			foreach (KeyValuePair<AbstractGlossaryElement, GlossaryElementUI> allElement in AllElements)
			{
				allElement.Value.gameObject.Activate(allElement.Value.Element.Category == cat);
			}

			SelectElement();
		}

		public void SelectElement(GlossaryElementUI ElementUI = null)
		{
			Current = ElementUI;
			if (Current != null)
			{
				Name.text = Current.Element.Name;
				if (ElementUI.Element is GlossaryElementEquipment)
				{
					Description.text = Item.GetDescription((Current.Element as GlossaryElementEquipment).CompoundType);
					Text description = Description;
					description.text = description.text + "\n" + Current.Element.Description;
					UpdateTiers(Current.Element as GlossaryElementEquipment);
				}
				else
				{
					Description.text = Current.Element.Description;
					Tiers.Activate(value: false);
				}

				if (Current.Element.Image != null)
				{
					Image.sprite = Current.Element.Image;
				}
				else
				{
					Image.sprite = DefaultImage;
				}
			}

			CurrentHolder.Activate(Current != null);
		}

		public string GetDescription(ItemType itemType, GenericItemSubType subType, MachineryPartType partType)
		{
			string value = string.Empty;
			switch (itemType)
			{
				case ItemType.GenericItem:
					Localization.GenericItemsDescriptions.TryGetValue(subType, out value);
					break;
				case ItemType.MachineryPart:
					Localization.MachineryPartsDescriptions.TryGetValue(partType, out value);
					break;
				default:
					Localization.ItemsDescriptions.TryGetValue(itemType, out value);
					break;
			}

			return value ?? string.Empty;
		}

		public void UpdateTiers(GlossaryElementEquipment el)
		{
			Tiers.Activate(el.Tiers);
			Tier01.text = el.Tier1;
			Tier02.text = el.Tier2;
			Tier03.text = el.Tier3;
			Tier04.text = el.Tier4;
			Tier01.transform.parent.gameObject.Activate(el.Tier1 != string.Empty);
			Tier02.transform.parent.gameObject.Activate(el.Tier2 != string.Empty);
			Tier03.transform.parent.gameObject.Activate(el.Tier3 != string.Empty);
			Tier04.transform.parent.gameObject.Activate(el.Tier4 != string.Empty);
		}

		public void OpenElement(AbstractGlossaryElement ele)
		{
			SelectCategory((int)AllElements[ele].Element.Category);
			SelectElement(AllElements[ele]);
		}
	}
}
