using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class BlueprintItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
	{
		public BlueprintsUI Screen;

		[NonSerialized] public ItemCompoundType CompoundType;

		public Image Icon;

		public Image TierColor;

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (Screen.Current != this)
			{
				base.gameObject.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
				Screen.Current = this;
				Screen.SetCurrent();
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (Screen.Current == this)
			{
				base.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
				Screen.Current = null;
				Screen.SetCurrent();
			}
		}

		private void Start()
		{
			Icon.sprite = SpriteManager.Instance.GetSprite(CompoundType);
		}

		public void InstantiateTiers(bool inst)
		{
			DynamicObjectData dynamicObjectData =
				StaticData.DynamicObjectsDataList.Values.FirstOrDefault(_003CInstantiateTiers_003Em__0);
			int num = ((dynamicObjectData.DefaultAuxData.TierMultipliers == null ||
			            dynamicObjectData.DefaultAuxData.TierMultipliers.Length < 1)
				? 1
				: dynamicObjectData.DefaultAuxData.TierMultipliers.Length);
			float num2 = 0f;
			for (int i = 0; i < num; i++)
			{
				ItemCompoundType itemCompoundType = ObjectCopier.Copy(CompoundType);
				itemCompoundType.Tier = i + 1;
				if (inst)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(Screen.TierPrefab, Screen.TiersHolder);
					gameObject.transform.localScale = Vector3.one;
					gameObject.GetComponent<Image>().color = Colors.Tier[i + 1];
					gameObject.GetComponentInChildren<Text>().text = itemCompoundType.Tier.ToString();
					Color color = gameObject.GetComponent<Image>().color;
					if (MyPlayer.Instance.Blueprints.Contains(itemCompoundType))
					{
						num2 += 1f;
						color.a = 1f;
						gameObject.GetComponent<Image>().color = color;
						gameObject.GetComponentInChildren<Text>().color = Colors.White;
					}
					else
					{
						color.a = 0.5f;
						gameObject.GetComponent<Image>().color = color;
						gameObject.GetComponentInChildren<Text>().color = Colors.Gray;
					}

					if (num2 == (float)num)
					{
						Screen.TiersDescription.text = Localization.AllTiersUnlocked.ToUpper();
					}
					else
					{
						Screen.TiersDescription.text = Localization.FurtherResearchRequired.ToUpper();
					}
				}
				else
				{
					if (MyPlayer.Instance.Blueprints.Contains(itemCompoundType))
					{
						num2 += 1f;
					}

					TierColor.fillAmount = num2 / (float)num;
					if (num2 == 0f)
					{
						Icon.color = Colors.BlueprintIcon;
					}
					else
					{
						Icon.color = Colors.White;
					}
				}
			}
		}

		[CompilerGenerated]
		private bool _003CInstantiateTiers_003Em__0(DynamicObjectData m)
		{
			return m.CompoundType.Equals(CompoundType);
		}
	}
}
