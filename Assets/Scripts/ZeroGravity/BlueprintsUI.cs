using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.Objects;
using ZeroGravity.UI;

namespace ZeroGravity
{
	public class BlueprintsUI : MonoBehaviour
	{
		[CompilerGenerated]
		private sealed class _003CInstantiateAll_003Ec__AnonStorey0
		{
			internal ItemCategory cat;

			internal bool _003C_003Em__0(DynamicObjectData m)
			{
				return Item.GetCraftingResources(m.CompoundType) != null && m.DefaultAuxData.Category == cat;
			}
		}

		public List<GameObject> Options;

		public Transform BlueprintsHolder;

		public GameObject BlueprintUI;

		public BlueprintItemUI Current;

		public Transform TiersHolder;

		public GameObject TierPrefab;

		public GameObject CurrentHolder;

		public Image Icon;

		public Text Name;

		public Text Description;

		public Text TiersDescription;

		public Text Researched;

		public Image ResFiller;

		[CompilerGenerated]
		private static Func<DynamicObjectData, bool> _003C_003Ef__am_0024cache0;

		[CompilerGenerated]
		private static Func<DynamicObjectData, int> _003C_003Ef__am_0024cache1;

		private void Start()
		{
		}

		private void Update()
		{
		}

		public void Toggle(bool val)
		{
			base.gameObject.SetActive(val);
			if (val)
			{
				SetScreen(0);
				Current = null;
				SetCurrent();
			}
		}

		public void SetScreen(int scr)
		{
			foreach (GameObject option in Options)
			{
				option.SetActive(false);
			}
			Options[scr].SetActive(true);
			switch (scr)
			{
			case 0:
				InstantiateAll(ItemCategory.Weapons);
				break;
			case 1:
				InstantiateAll(ItemCategory.Tools);
				break;
			case 2:
				InstantiateAll(ItemCategory.Utility);
				break;
			case 3:
				InstantiateAll(ItemCategory.Suits);
				break;
			case 4:
				InstantiateAll(ItemCategory.Medical);
				break;
			case 5:
				InstantiateAll(ItemCategory.Parts);
				break;
			case 6:
				InstantiateAll(ItemCategory.Containers);
				break;
			case 7:
				InstantiateAll(ItemCategory.Magazines);
				break;
			case 8:
				InstantiateAll(ItemCategory.General);
				break;
			default:
				InstantiateAll(ItemCategory.General);
				break;
			}
			CountResearched();
		}

		public void InstantiateAll(ItemCategory cat)
		{
			_003CInstantiateAll_003Ec__AnonStorey0 _003CInstantiateAll_003Ec__AnonStorey = new _003CInstantiateAll_003Ec__AnonStorey0();
			_003CInstantiateAll_003Ec__AnonStorey.cat = cat;
			BlueprintsHolder.DestroyAll<BlueprintItemUI>();
			foreach (DynamicObjectData item in StaticData.DynamicObjectsDataList.Values.Where(_003CInstantiateAll_003Ec__AnonStorey._003C_003Em__0))
			{
				ItemCompoundType compoundType = item.CompoundType;
				GameObject gameObject = UnityEngine.Object.Instantiate(BlueprintUI, BlueprintsHolder);
				gameObject.transform.localScale = Vector3.one;
				BlueprintItemUI component = gameObject.GetComponent<BlueprintItemUI>();
				component.Screen = this;
				component.gameObject.transform.localScale = Vector3.one;
				component.CompoundType = compoundType;
				component.InstantiateTiers(false);
			}
		}

		public void SetCurrent()
		{
			TiersHolder.DestroyAll(true);
			if (Current != null)
			{
				CurrentHolder.Activate(true);
				Name.text = Item.GetName(Current.CompoundType);
				Icon.sprite = Client.Instance.SpriteManager.GetSprite(Current.CompoundType);
				Description.text = Item.GetDescription(Current.CompoundType.Type, Current.CompoundType.SubType, Current.CompoundType.PartType);
				Current.InstantiateTiers(true);
			}
			else
			{
				CurrentHolder.Activate(false);
			}
		}

		public void CountResearched()
		{
			Dictionary<short, DynamicObjectData>.ValueCollection values = StaticData.DynamicObjectsDataList.Values;
			if (_003C_003Ef__am_0024cache0 == null)
			{
				_003C_003Ef__am_0024cache0 = _003CCountResearched_003Em__0;
			}
			IEnumerable<DynamicObjectData> source = values.Where(_003C_003Ef__am_0024cache0);
			if (_003C_003Ef__am_0024cache1 == null)
			{
				_003C_003Ef__am_0024cache1 = _003CCountResearched_003Em__1;
			}
			float num = source.Sum(_003C_003Ef__am_0024cache1);
			float num2 = MyPlayer.Instance.Blueprints.Count();
			Researched.text = num2 + " / " + num;
			ResFiller.fillAmount = num2 / num;
		}

		[CompilerGenerated]
		private static bool _003CCountResearched_003Em__0(DynamicObjectData m)
		{
			return Item.GetCraftingResources(m.CompoundType) != null;
		}

		[CompilerGenerated]
		private static int _003CCountResearched_003Em__1(DynamicObjectData m)
		{
			return (m.DefaultAuxData.TierMultipliers.Length < 2) ? 1 : m.DefaultAuxData.TierMultipliers.Length;
		}
	}
}
