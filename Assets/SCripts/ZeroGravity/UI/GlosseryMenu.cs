using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	public class GlosseryMenu : MonoBehaviour
	{
		[CompilerGenerated]
		private sealed class _003CStart_003Ec__AnonStorey0
		{
			internal GlossaryGroupOption newGrpUI;

			internal GlosseryMenu _0024this;

			internal void _003C_003Em__0()
			{
				_0024this.ChangeGroup(newGrpUI);
			}
		}

		public Glossery Glossery;

		private GlosseryUI glosseryUI;

		private GlossaryGroupOption selectedGroup;

		[SerializeField]
		private Sprite missingSpritePlaceholder;

		[SerializeField]
		private Color normalButtonColor;

		[SerializeField]
		private Color pressedButtonColor;

		[SerializeField]
		private GameObject GroupButtonPref;

		public ScrollRect GroupScroll;

		public Transform GroupHolder;

		public GlossaryGroupUI GroupUI;

		public GlossaryItemUI ItemUI;

		[SerializeField]
		private GameObject TooltipPanel;

		[SerializeField]
		private Text TooltipText;

		[SerializeField]
		private GameObject LongDescriptionPanel;

		[SerializeField]
		private Image LongDescriptionImage;

		[SerializeField]
		private Text LongDescriptionName;

		[SerializeField]
		private Text LongDescriptionDescription;

		private void Start()
		{
			Glossery = Json.LoadResource<Glossery>("Data/Glossery");
			glosseryUI = new GlosseryUI();
			glosseryUI.Glossary = Glossery;
			glosseryUI.AllGroups = new List<GlossaryGroupOption>();
			foreach (GlosseryGroup glossaryGroup in Glossery.GlossaryGroups)
			{
				_003CStart_003Ec__AnonStorey0 _003CStart_003Ec__AnonStorey = new _003CStart_003Ec__AnonStorey0();
				_003CStart_003Ec__AnonStorey._0024this = this;
				GameObject gameObject = Object.Instantiate(GroupButtonPref, GroupButtonPref.transform.parent);
				gameObject.transform.Find("Text").GetComponent<Text>().text = glossaryGroup.GroupName;
				gameObject.SetActive(true);
				_003CStart_003Ec__AnonStorey.newGrpUI = new GlossaryGroupOption();
				_003CStart_003Ec__AnonStorey.newGrpUI.ButtonImage = gameObject.transform.Find("Active").GetComponent<Image>();
				_003CStart_003Ec__AnonStorey.newGrpUI.GlossaryGrp = glossaryGroup;
				_003CStart_003Ec__AnonStorey.newGrpUI.AllSubGroups = new List<GlossaryGroupUI>();
				glosseryUI.AllGroups.Add(_003CStart_003Ec__AnonStorey.newGrpUI);
				gameObject.GetComponent<Button>().onClick.AddListener(_003CStart_003Ec__AnonStorey._003C_003Em__0);
				foreach (GlosserySubGroup subGroup in glossaryGroup.SubGroups)
				{
					GlossaryGroupUI glossaryGroupUI = Object.Instantiate(GroupUI, GroupHolder);
					glossaryGroupUI.GlossarySubGrp = subGroup;
					glossaryGroupUI.Name.text = subGroup.SubGroupName.ToUpper();
					glossaryGroupUI.AllItems = new List<GlossaryItemUI>();
					glossaryGroupUI.gameObject.SetActive(false);
					_003CStart_003Ec__AnonStorey.newGrpUI.AllSubGroups.Add(glossaryGroupUI);
					foreach (GlosseryItem item in subGroup.Items)
					{
						GlossaryItemUI glossaryItemUI = Object.Instantiate(ItemUI, glossaryGroupUI.ItemsHolder);
						glossaryItemUI.gameObject.SetActive(true);
						glossaryItemUI.Menu = this;
						glossaryItemUI.GlossaryItm = item;
						glossaryGroupUI.AllItems.Add(glossaryItemUI);
						glossaryItemUI.ItemName.text = item.Name;
						Texture2D texture2D = Resources.Load("UI/GlosseryImages/" + item.ImagePath) as Texture2D;
						if (texture2D != null)
						{
							Sprite sprite = (glossaryItemUI.Sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0f, 0f), 1f, 0u, SpriteMeshType.Tight));
						}
						if (glossaryItemUI.Sprite != null)
						{
							glossaryItemUI.Image.sprite = glossaryItemUI.Sprite;
						}
						else
						{
							glossaryItemUI.Image.sprite = missingSpritePlaceholder;
						}
					}
				}
			}
			selectedGroup = glosseryUI.AllGroups[0];
			ChangeGroup(selectedGroup);
		}

		private void Update()
		{
			ShowTooltip();
		}

		public void ChangeGroup(GlossaryGroupOption groupUi)
		{
			if (selectedGroup != null)
			{
				foreach (GlossaryGroupUI allSubGroup in selectedGroup.AllSubGroups)
				{
					allSubGroup.gameObject.SetActive(false);
				}
				selectedGroup.ButtonImage.color = normalButtonColor;
			}
			selectedGroup = groupUi;
			foreach (GlossaryGroupUI allSubGroup2 in groupUi.AllSubGroups)
			{
				allSubGroup2.gameObject.SetActive(true);
			}
			selectedGroup.ButtonImage.color = pressedButtonColor;
			GroupScroll.normalizedPosition = new Vector2(0f, 1f);
			Canvas.ForceUpdateCanvases();
		}

		public void ShowTooltip()
		{
			GameObject gameObject = Client.Instance.InputModule.OverGameObject();
			if (gameObject != null)
			{
				GlossaryItemUI componentInParent = gameObject.GetComponentInParent<GlossaryItemUI>();
				if (componentInParent != null)
				{
					RectTransform component = TooltipPanel.GetComponent<RectTransform>();
					Vector2 localPoint = Vector2.zero;
					RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), Input.mousePosition, Client.Instance.CanvasManager.Canvas.worldCamera, out localPoint);
					component.transform.localPosition = localPoint;
					TooltipText.text = componentInParent.GlossaryItm.DescriptionShort;
					TooltipPanel.SetActive(true);
				}
				else
				{
					TooltipPanel.SetActive(false);
				}
			}
			else
			{
				TooltipPanel.SetActive(false);
			}
		}

		public void ShowLongDescription(GlossaryItemUI item)
		{
			LongDescriptionPanel.SetActive(true);
			LongDescriptionName.text = item.GlossaryItm.Name;
			if (item.Sprite != null)
			{
				LongDescriptionImage.sprite = item.Sprite;
			}
			else
			{
				LongDescriptionImage.sprite = missingSpritePlaceholder;
			}
			LongDescriptionDescription.text = item.GlossaryItm.DescriptionLong;
		}

		public void HideLongDescription()
		{
			LongDescriptionPanel.SetActive(false);
		}
	}
}
